using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using ChinhDo.Transactions.FileManager;
using log4net;

using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN.IO
{
    // This type represents the mapping of installed files across multiple game instances.
    // We use it to simplify the type of InstalledWhere, since the outer dictionary is grouped by volume.
    using NestedDict = Dictionary<(string identifier, ModuleVersion version),
                                  (GameInstance instance, InstalledModule instMod)[]>;

    /// <summary>
    /// Provides services for deduplicating installed files across multiple game instances.
    /// </summary>
    public class InstalledFilesDeduplicator
    {
        /// <summary>
        /// Creates a new instance of the InstalledFilesDeduplicator class.
        /// This constructor is used when no destination instance is specified,
        /// which is intended for deduplicating files across all instances.
        /// </summary>
        /// <param name="instances">Full list of instances that might already have the files we need</param>
        /// <param name="repoData">RepositoryDataManager needed to instantiate registries</param>
        public InstalledFilesDeduplicator(IEnumerable<GameInstance> instances,
                                          RepositoryDataManager     repoData)
        {
            log.DebugFormat("Checking for duplicate installed files from {0}...",
                            string.Join(", ", instances.Select(inst => inst.Name)));
            InstalledWhere = instances.Where(inst => Directory.Exists(inst.GameDir()))
                                      .ZipBy(goodInsts => HardLink.GetDeviceIdentifiers(goodInsts.Select(inst => inst.GameDir())))
                                      .GroupBy(tuple => tuple.Second ?? 0UL,
                                               tuple => tuple.First)
                                      .ToDictionary(grp => grp.Key,
                                                    grp => GetInstalledDict(repoData, grp));
            log.Debug("Done checking for duplicate installed files.");
        }

        /// <summary>
        /// Get single dictionary of all installed modules grouped by (identifier, version)
        /// and containing the instances where they are installed.
        /// </summary>/param>
        /// <param name="repoData">RepositoryDataManager needed to instantiate registries</param>
        /// <param name="instances">Sequence of instances that share a volume</param>
        /// <returns>Dictionary representing this group's installed files, grouped by mod</returns>
        private static NestedDict GetInstalledDict(RepositoryDataManager     repoData,
                                                   IEnumerable<GameInstance> instances)
            => instances.SelectMany(inst => RegistryManager.ReadOnlyRegistry(inst, repoData)
                                                           ?.InstalledModules
                                                            .Select(im => (instance: inst, instMod: im))
                                                           ?? Enumerable.Empty<(GameInstance instance, InstalledModule instMod)>())
                        .GroupBy(tuple => (tuple.instMod.identifier,
                                           tuple.instMod.Module.version),
                                 tuple => (tuple.instance,
                                           tuple.instMod))
                        .ToDictionary(grp => grp.Key,
                                      grp => grp.ToArray());

        /// <summary>
        /// Creates a new instance of the InstalledFilesDeduplicator class.
        /// This constructor is used when the destination instance is specified,
        /// which is intended for installing new files.
        /// </summary>
        /// <param name="destinationInstance">The instance where we want to deduplicate files</param>
        /// <param name="instances">Full list of instances that might already have the files we need</param>
        /// <param name="repoData">RepositoryDataManager needed to instantiate registries</param>
        public InstalledFilesDeduplicator(GameInstance              destinationInstance,
                                          IEnumerable<GameInstance> instances,
                                          RepositoryDataManager     repoData)
            : this(HardLink.GetDeviceIdentifiers(instances.Prepend(destinationInstance)
                                                          .Select(inst => inst.GameDir())).ToArray()
                   is ulong?[] devIds
                       ? instances.Zip(devIds.Skip(1))
                                  .Where(tuple => tuple.First != destinationInstance
                                                  && tuple.Second == devIds[0])
                                  .Select(tuple => tuple.First)
                                  .Where(inst => inst.game == destinationInstance.game)
                       : Enumerable.Empty<GameInstance>(),
                   repoData)
        {
        }

        /// <summary>
        /// Returns a dictionary of all installed files for a given module, from all instances, grouped by file name.
        /// Only files larger than 128 KiB are included.
        /// The dictionary contains the relative path as the key and an array of absolute paths as the value.
        /// </summary>
        /// <param name="identifier">Identifier of the module</param>
        /// <param name="version">Version of the module</param>
        /// <returns>Mapping from relative path to array of absolute paths</returns>
        public Dictionary<string, string[]> ModuleCandidateDuplicates(string        identifier,
                                                                      ModuleVersion version)
            => InstalledWhere.Values
                             .SelectMany(dict => (dict.TryGetValue((identifier, version),
                                                                   out (GameInstance instance, InstalledModule instMod)[]? sources)
                                                     ? sources.SelectMany(DupEntries)
                                                     : Enumerable.Empty<(string relPath, string absPath)>()))
                             .GroupBy(tuple => tuple.relPath,
                                      tuple => tuple.absPath)
                             .ToDictionary(grp => grp.Key,
                                           grp => grp.ToArray());

        /// <summary>
        /// Reduces the disk space used by all game instances
        /// by replacing duplicate installed files with hard links.
        /// Only files larger than 128 KiB are considered for deduplication.
        /// The method will prompt the user for confirmation before proceeding.
        /// </summary>
        /// <param name="user">Handler for messaging and interaction</param>
        public void DeduplicateAll(IUser user)
        {
            log.Debug("Grouping duplicate installed files...");
            var fileGroups = InstalledWhere.Values
                                           .SelectMany(dict => dict.Values
                                                                   .Where(dups => dups.Length > 1)
                                                                   .SelectMany(GetDuplicateGroups))
                                           .ToArray();
            log.DebugFormat("Got {0} duplicate installed file groups...", fileGroups.Length);
            if (fileGroups.Length > 0)
            {
                var dupCount  = fileGroups.Sum(grp => grp.Count() - 1);
                var dupSize   = fileGroups.Sum(grp => grp.Skip(1)
                                                         .Sum(absPath => new FileInfo(absPath).Length));
                int doneCount = 0;
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Found duplicate installed files:");
                    log.DebugFormat("{0}", string.Join(Environment.NewLine,
                                                       fileGroups.SelectMany(grp => grp)));
                }
                if (user.RaiseYesNoDialog(string.Format(Properties.Resources.FoundDuplicatesConfirmation,
                                                        dupCount)))
                {
                    log.DebugFormat("Deduplicating all...");
                    using (var tx = CkanTransaction.CreateTransactionScope())
                    {
                        var file_transaction = new TxFileManager();
                        foreach (var grp in fileGroups)
                        {
                            var target = grp.First();
                            foreach (var linkPath in grp.Skip(1))
                            {
                                file_transaction.Delete(linkPath);
                                // If this fails (e.g., if the files are on different volumes), it throws,
                                // the transaction is aborted, and all changes are reverted.
                                HardLink.Create(target, linkPath);
                                ++doneCount;
                            }
                            user.RaiseProgress(string.Format(Properties.Resources.DeduplicatedRelPath,
                                                             grp.Count() - 1,
                                                             Platform.FormatPath(grp.Key)),
                                               100 * doneCount / dupCount);
                        }
                    }
                    user.RaiseMessage(Properties.Resources.DoneDeduplicatingFiles,
                                      CkanModule.FmtSize(dupSize));
                }
                else
                {
                    throw new CancelledActionKraken();
                }
            }
            else
            {
                user.RaiseMessage(Properties.Resources.NoDuplicatesFound);
            }
        }

        private static IEnumerable<IGrouping<string, string>> GetDuplicateGroups((GameInstance instance, InstalledModule instMod)[] tuples)
            => tuples.SelectMany(DupEntries)
                     .ZipBy(tuples => HardLink.GetFileIdentifiers(tuples.Select(tuple => tuple.absPath)))
                     .DistinctBy(tuple => tuple.Second)
                     .Select(tuple => tuple.First)
                     .ZipBy(tuples => HardLink.GetLinkCounts(tuples.Select(tuple => tuple.absPath)))
                     .OrderByDescending(tuple => tuple.Second)
                     .Select(tuple => tuple.First)
                     .GroupBy(tuple => tuple.relPath,
                              tuple => tuple.absPath)
                     .Where(grp => grp.Count() > 1);

        public static void CreateOrCopy(FileInfo      target,
                                        string        linkPath,
                                        TxFileManager file_transaction)
        {
            if (target.Length >= MinDedupFileSize)
            {
                // If >=128KiB, try making a hard link, then fall back to copying if it fails
                log.DebugFormat("Creating a hard link from {0} to {1}...", target, linkPath);
                HardLink.CreateOrCopy(target.FullName, linkPath, file_transaction);
            }
            else
            {
                // If <128KiB, just copy the file
                log.DebugFormat("Copying from {0} to {1}...", target, linkPath);
                file_transaction.Copy(target.FullName, linkPath, false);
            }
        }

        /// <summary>
        /// Minimum file size for deduplication.
        /// Files smaller than this are not considered for deduplication.
        /// The idea is that deduplicating is most effective for larger files,
        /// and smaller files are more likely to be edited by the user after installation.
        /// </summary>
        private const long MinDedupFileSize = 128 * 1024;

        /// <summary>
        /// Returns sequence of tuples containing the relative path and absolute path of files installed
        /// for an instance and module.
        /// Intended for other code to check for duplicates.
        /// </summary>
        /// <param name="instance">Game instance for getting absolute paths</param>
        /// <param name="instMod">Instaleld module to check for duplicate installed files</param>
        /// <returns>Sequence of relpath, abspath pairs</returns>
        private static IEnumerable<(string relPath, string absPath)> DupEntries(GameInstance instance, InstalledModule instMod)
            => instMod.Files.Select(relPath => (relPath,
                                                absPath: instance.ToAbsoluteGameDir(relPath)))
                            .Where(tuple => File.Exists(tuple.absPath)
                                            && new FileInfo(tuple.absPath).Length >= MinDedupFileSize);

        /// <summary>
        /// Same as above but accepts a tuple.
        /// </summary>
        /// <param name="tuple">Game instance and installed module</param>
        /// <returns>Sequence of relpath, abspath pairs</returns>
        private static IEnumerable<(string relPath, string absPath)> DupEntries((GameInstance instance, InstalledModule instMod) tuple)
            => DupEntries(tuple.instance, tuple.instMod);

        /// <summary>
        /// All installed modules for all instances, grouped by volume, then by (identifier, version).
        /// The value is an array of tuples containing the instance and the installed module.
        /// </summary>
        private readonly Dictionary<ulong, NestedDict> InstalledWhere;

        private static readonly ILog log = LogManager.GetLogger(typeof(InstalledFilesDeduplicator));
    }
}
