using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using ChinhDo.Transactions.FileManager;
using Autofac;

using CKAN.Extensions;
using CKAN.Versioning;
using CKAN.Configuration;
using CKAN.Games;
using System.Threading;

namespace CKAN
{
    public struct InstallableFile
    {
        public ZipEntry source;
        public string   destination;
        public bool     makedir;
    }

    public class ModuleInstaller
    {
        public IUser User { get; set; }

        public event Action<CkanModule, long, long>?      InstallProgress;
        public event Action<InstalledModule, long, long>? RemoveProgress;
        public event Action<CkanModule>?                  OneComplete;

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        private readonly GameInstance      instance;
        private readonly NetModuleCache    Cache;
        private readonly string?           userAgent;
        private readonly CancellationToken cancelToken;

        // Constructor
        public ModuleInstaller(GameInstance      inst,
                               NetModuleCache    cache,
                               IUser             user,
                               string?           userAgent   = null,
                               CancellationToken cancelToken = default)
        {
            User     = user;
            Cache    = cache;
            instance = inst;
            this.userAgent = userAgent;
            this.cancelToken = cancelToken;
            log.DebugFormat("Creating ModuleInstaller for {0}", instance.GameDir());
        }

        #region Downloading

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public string Download(CkanModule module, string filename)
        {
            User.RaiseProgress(string.Format(Properties.Resources.ModuleInstallerDownloading, module.download), 0);
            return Download(module, filename, userAgent, Cache);
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public static string Download(CkanModule module, string filename, string? userAgent, NetModuleCache cache)
        {
            log.Info("Downloading " + filename);

            string tmp_file = Net.Download((module.download ?? Enumerable.Empty<Uri>())
                .OrderBy(u => u,
                         new PreferredHostUriComparer(
                             ServiceLocator.Container.Resolve<IConfiguration>().PreferredHosts))
                .First(),
                userAgent);

            return cache.Store(module, tmp_file, new ProgressImmediate<long>(bytes => {}), filename, true);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Checks the CKAN cache first.
        /// </summary>
        public string CachedOrDownload(CkanModule module, string? filename = null)
            => CachedOrDownload(module, userAgent, Cache, filename);

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks provided cache first.
        /// </summary>
        public static string CachedOrDownload(CkanModule module, string? userAgent, NetModuleCache cache, string? filename = null)
        {
            filename ??= CkanModule.StandardName(module.identifier, module.version);

            var full_path = cache.GetCachedFilename(module);
            if (full_path == null)
            {
                return Download(module, filename, userAgent, cache);
            }

            log.DebugFormat("Using {0} (cached)", filename);
            return full_path;
        }

        #endregion

        #region Installation

        /// <summary>
        ///     Installs all modules given a list of identifiers as a transaction. Resolves dependencies.
        ///     This *will* save the registry at the end of operation.
        ///
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Propagates a CancelledActionKraken if the user cancelled the install.
        /// </summary>
        public void InstallList(ICollection<CkanModule>     modules,
                                RelationshipResolverOptions options,
                                RegistryManager             registry_manager,
                                ref HashSet<string>?        possibleConfigOnlyDirs,
                                string?                     userAgent     = null,
                                IDownloader?                downloader    = null,
                                bool                        ConfirmPrompt = true)
        {
            if (modules.Count == 0)
            {
                User.RaiseProgress(Properties.Resources.ModuleInstallerNothingToInstall, 100);
                return;
            }
            var resolver = new RelationshipResolver(modules, null, options,
                                                    registry_manager.registry,
                                                    instance.game, instance.VersionCriteria());
            var modsToInstall = resolver.ModList().ToList();
            // Alert about attempts to install DLC before downloading or installing anything
            if (modsToInstall.Any(m => m.IsDLC))
            {
                throw new BadCommandKraken(Properties.Resources.ModuleInstallerDLC);
            }

            // Make sure we have enough space to install this stuff
            var installBytes = modsToInstall.Sum(m => m.install_size);
            CKANPathUtils.CheckFreeSpace(new DirectoryInfo(instance.GameDir()),
                                         installBytes,
                                         Properties.Resources.NotEnoughSpaceToInstall);

            var cached    = new List<CkanModule>();
            var downloads = new List<CkanModule>();
            User.RaiseMessage(Properties.Resources.ModuleInstallerAboutToInstall);
            User.RaiseMessage("");
            foreach (var module in modsToInstall)
            {
                User.RaiseMessage(" * {0}", Cache.DescribeAvailability(module));
                if (!module.IsMetapackage && !Cache.IsMaybeCachedZip(module))
                {
                    downloads.Add(module);
                }
                else
                {
                    cached.Add(module);
                }
            }
            if (ConfirmPrompt && !User.RaiseYesNoDialog(Properties.Resources.ModuleInstallerContinuePrompt))
            {
                throw new CancelledActionKraken(Properties.Resources.ModuleInstallerUserDeclined);
            }

            var downloadBytes = CkanModule.GroupByDownloads(downloads)
                                          .Sum(grp => grp.First().download_size);
            var rateCounter = new ByteRateCounter()
            {
                Size      = downloadBytes + installBytes,
                BytesLeft = downloadBytes + installBytes,
            };
            rateCounter.Start();
            long downloadedBytes = 0;
            long installedBytes  = 0;
            if (downloads.Count > 0)
            {
                downloader ??= new NetAsyncModulesDownloader(User, Cache, userAgent, cancelToken);
                downloader.OverallDownloadProgress += brc =>
                {
                    downloadedBytes = downloadBytes - brc.BytesLeft;
                    rateCounter.BytesLeft = downloadBytes - downloadedBytes
                                          + installBytes  - installedBytes;
                    User.RaiseProgress(rateCounter);
                };
            }

            // We're about to install all our mods; so begin our transaction.
            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                var gameDir = new DirectoryInfo(instance.GameDir());
                long modInstallCompletedBytes = 0;
                foreach (var mod in ModsInDependencyOrder(resolver, cached, downloads, downloader))
                {
                    // Re-check that there's enough free space in case game dir and cache are on same drive
                    CKANPathUtils.CheckFreeSpace(gameDir, mod.install_size,
                                                 Properties.Resources.NotEnoughSpaceToInstall);
                    Install(mod, resolver.IsAutoInstalled(mod),
                            registry_manager.registry,
                            ref possibleConfigOnlyDirs,
                            new ProgressImmediate<long>(bytes =>
                            {
                                InstallProgress?.Invoke(mod, mod.install_size - bytes, mod.install_size);
                                installedBytes = modInstallCompletedBytes + bytes;
                                rateCounter.BytesLeft = downloadBytes - downloadedBytes
                                                      + installBytes  - installedBytes;
                                User.RaiseProgress(rateCounter);
                            }));
                    modInstallCompletedBytes += mod.install_size;
                }
                rateCounter.Stop();

                User.RaiseProgress(Properties.Resources.ModuleInstallerUpdatingRegistry, 90);
                registry_manager.Save(!options.without_enforce_consistency);

                User.RaiseProgress(Properties.Resources.ModuleInstallerCommitting, 95);
                transaction.Complete();
            }

            EnforceCacheSizeLimit(registry_manager.registry, Cache);
            User.RaiseProgress(Properties.Resources.ModuleInstallerDone, 100);
        }

        private static IEnumerable<CkanModule> ModsInDependencyOrder(RelationshipResolver    resolver,
                                                                     ICollection<CkanModule> cached,
                                                                     ICollection<CkanModule> toDownload,
                                                                     IDownloader?            downloader)

            => ModsInDependencyOrder(resolver, cached,
                                     downloader != null && toDownload.Count > 0
                                         ? downloader.ModulesAsTheyFinish(cached, toDownload)
                                         : null);

        private static IEnumerable<CkanModule> ModsInDependencyOrder(RelationshipResolver     resolver,
                                                                     ICollection<CkanModule>  cached,
                                                                     IEnumerable<CkanModule>? downloading)
        {
            var waiting = new HashSet<CkanModule>();
            var done    = new HashSet<CkanModule>();
            if (downloading != null)
            {
                foreach (var newlyCached in downloading)
                {
                    waiting.Add(newlyCached);
                    foreach (var m in OnePass(resolver, waiting, done))
                    {
                        yield return m;
                    }
                }
            }
            else
            {
                waiting.UnionWith(cached);
                foreach (var m in OnePass(resolver, waiting, done))
                {
                    yield return m;
                }
            }
        }

        private static IEnumerable<CkanModule> OnePass(RelationshipResolver resolver,
                                                       HashSet<CkanModule>  waiting,
                                                       HashSet<CkanModule>  done)
        {
            while (true)
            {
                var newlyDone = waiting.Where(m => resolver.ReadyToInstall(m, done))
                                       .OrderBy(m => m.identifier)
                                       .ToArray();
                if (newlyDone.Length == 0)
                {
                    // No mods ready to install
                    break;
                }
                foreach (var m in newlyDone)
                {
                    waiting.Remove(m);
                    done.Add(m);
                    yield return m;
                }
            }
        }

        /// <summary>
        ///     Install our mod from the filename supplied.
        ///     If no file is supplied, we will check the cache or throw FileNotFoundKraken.
        ///     Does *not* resolve dependencies; this does the heavy lifting.
        ///     Does *not* save the registry.
        ///     Do *not* call this directly, use InstallList() instead.
        ///
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Throws a FileNotFoundKraken if we can't find the downloaded module.
        ///
        /// TODO: The name of this and InstallModule() need to be made more distinctive.
        /// </summary>
        private void Install(CkanModule           module,
                             bool                 autoInstalled,
                             Registry             registry,
                             ref HashSet<string>? possibleConfigOnlyDirs,
                             IProgress<long>?     progress)
        {
            CheckKindInstallationKraken(module);
            var version = registry.InstalledVersion(module.identifier);

            // TODO: This really should be handled by higher-up code.
            if (version is not null and not UnmanagedModuleVersion)
            {
                User.RaiseMessage(Properties.Resources.ModuleInstallerAlreadyInstalled,
                                  module.name, version);
                return;
            }

            string? filename = null;
            if (!module.IsMetapackage)
            {
                // Find ZIP in the cache if we don't already have it.
                filename ??= Cache.GetCachedFilename(module);

                // If we *still* don't have a file, then kraken bitterly.
                if (filename == null)
                {
                    throw new FileNotFoundKraken(null,
                                                 string.Format(Properties.Resources.ModuleInstallerZIPNotInCache,
                                                               module));
                }
            }

            User.RaiseMessage(Properties.Resources.ModuleInstallerInstallingMod,
                              module.name);

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                // Install all the things!
                var files = InstallModule(module, filename, registry,
                                          ref possibleConfigOnlyDirs, progress);

                // Register our module and its files.
                registry.RegisterModule(module, files, instance, autoInstalled);

                // Finish our transaction, but *don't* save the registry; we may be in an
                // intermediate, inconsistent state.
                // This is fine from a transaction standpoint, as we may not have an enclosing
                // transaction, and if we do, they can always roll us back.
                transaction.Complete();
            }

            User.RaiseMessage(Properties.Resources.ModuleInstallerInstalledMod,
                              module.name);

            // Fire our callback that we've installed a module, if we have one.
            OneComplete?.Invoke(module);
        }

        /// <summary>
        /// Check if the given module is a DLC:
        /// if it is, throws a BadCommandKraken.
        /// </summary>
        private static void CheckKindInstallationKraken(CkanModule module)
        {
            if (module.IsDLC)
            {
                throw new BadCommandKraken(Properties.Resources.ModuleInstallerDLC);
            }
        }

        /// <summary>
        /// Installs the module from the zipfile provided.
        /// Returns a list of files installed.
        /// Propagates a DllLocationMismatchKraken if the user has a bad manual install.
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a CancelledActionKraken if the user decides not to overwite unowned files.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// </summary>
        private List<string> InstallModule(CkanModule           module,
                                           string?              zip_filename,
                                           Registry             registry,
                                           ref HashSet<string>? possibleConfigOnlyDirs,
                                           IProgress<long>?     moduleProgress)
        {
            var createdPaths = new List<string>();
            if (module.IsMetapackage || zip_filename == null)
            {
                // It's OK to include metapackages in changesets,
                // but there's no work to do for them
                return createdPaths;
            }
            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                var filters = ServiceLocator.Container.Resolve<IConfiguration>().GlobalInstallFilters
                    .Concat(instance.InstallFilters)
                    .ToHashSet();
                var files = FindInstallableFiles(module, zipfile, instance)
                    .Where(instF => !filters.Any(filt =>
                                        instF.destination != null
                                        && instF.destination.Contains(filt))
                                    // Skip the file if it's a ckan file, these should never be copied to GameData
                                    && !IsInternalCkan(instF.source))
                    .ToList();

                try
                {
                    var dll = registry.DllPath(module.identifier);
                    if (dll is not null && !string.IsNullOrEmpty(dll))
                    {
                        // Find where we're installing identifier.optionalversion.dll
                        // (file name might not be an exact match with manually installed)
                        var dllFolders = files
                            .Select(f => instance.ToRelativeGameDir(f.destination))
                            .Where(relPath => instance.DllPathToIdentifier(relPath) == module.identifier)
                            .Select(Path.GetDirectoryName)
                            .ToHashSet();
                        // Make sure that the DLL is actually included in the install
                        // (NearFutureElectrical, NearFutureElectrical-Core)
                        if (dllFolders.Count > 0 && registry.FileOwner(dll) == null)
                        {
                            if (!dllFolders.Contains(Path.GetDirectoryName(dll)))
                            {
                                // Manually installed DLL is somewhere else where we're not installing files,
                                // probable bad install, alert user and abort
                                throw new DllLocationMismatchKraken(dll, string.Format(
                                    Properties.Resources.ModuleInstallerBadDLLLocation, module.identifier, dll));
                            }
                            // Delete the manually installed DLL transaction-style because we believe we'll be replacing it
                            var toDelete = instance.ToAbsoluteGameDir(dll);
                            log.DebugFormat("Deleting manually installed DLL {0}", toDelete);
                            TxFileManager file_transaction = new TxFileManager();
                            file_transaction.Snapshot(toDelete);
                            file_transaction.Delete(toDelete);
                        }
                    }

                    // Look for overwritable files if session is interactive
                    if (!User.Headless)
                    {
                        var conflicting = FindConflictingFiles(zipfile, files, registry).Memoize();
                        if (conflicting.Any())
                        {
                            var fileMsg = conflicting
                                .OrderBy(c => c.Value)
                                .Aggregate("", (a, b) =>
                                    $"{a}\r\n- {instance.ToRelativeGameDir(b.Key.destination)}  ({(b.Value ? Properties.Resources.ModuleInstallerFileSame : Properties.Resources.ModuleInstallerFileDifferent)})");
                            if (User.RaiseYesNoDialog(string.Format(
                                Properties.Resources.ModuleInstallerOverwrite, module.name, fileMsg)))
                            {
                                DeleteConflictingFiles(conflicting.Select(f => f.Key));
                            }
                            else
                            {
                                throw new CancelledActionKraken(string.Format(
                                    Properties.Resources.ModuleInstallerOverwriteCancelled, module.name));
                            }
                        }
                    }
                    long installedBytes = 0;
                    var fileProgress = new ProgressImmediate<long>(bytes => moduleProgress?.Report(installedBytes + bytes));
                    foreach (InstallableFile file in files)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            throw new CancelledActionKraken();
                        }
                        log.DebugFormat("Copying {0}", file.source.Name);
                        var path = CopyZipEntry(zipfile, file.source, file.destination, file.makedir,
                                                fileProgress);
                        installedBytes += file.source.Size;
                        if (path != null)
                        {
                            createdPaths.Add(path);
                            if (file.source.IsDirectory && possibleConfigOnlyDirs != null)
                            {
                                possibleConfigOnlyDirs.Remove(file.destination);
                            }
                        }
                    }
                    log.InfoFormat("Installed {0}", module);
                }
                catch (FileExistsKraken kraken)
                {
                    // Decorate the kraken with our module and re-throw
                    kraken.filename = instance.ToRelativeGameDir(kraken.filename);
                    kraken.installingModule = module;
                    kraken.owningModule = registry.FileOwner(kraken.filename);
                    throw;
                }
            }
            return createdPaths;
        }

        public static bool IsInternalCkan(ZipEntry ze)
            => ze.Name.EndsWith(".ckan", StringComparison.OrdinalIgnoreCase);

        #region File overwrites

        /// <summary>
        /// Find files in the given list that are already installed and unowned.
        /// Note, this compares files on demand; Memoize for performance!
        /// </summary>
        /// <param name="zip">Zip file that we are installing from</param>
        /// <param name="files">Files that we want to install for a module</param>
        /// <param name="registry">Registry to check for file ownership</param>
        /// <returns>
        /// List of pairs: Key = file, Value = true if identical, false if different
        /// </returns>
        private IEnumerable<KeyValuePair<InstallableFile, bool>> FindConflictingFiles(ZipFile zip, IEnumerable<InstallableFile> files, Registry registry)
        {
            foreach (InstallableFile file in files)
            {
                if (!file.source.IsDirectory
                    && File.Exists(file.destination)
                    && registry.FileOwner(instance.ToRelativeGameDir(file.destination)) == null)
                {
                    log.DebugFormat("Comparing {0}", file.destination);
                    using (Stream zipStream = zip.GetInputStream(file.source))
                    using (FileStream curFile = new FileStream(file.destination, FileMode.Open, FileAccess.Read))
                    {
                        yield return new KeyValuePair<InstallableFile, bool>(
                            file,
                            file.source.Size == curFile.Length
                                && StreamsEqual(zipStream, curFile)
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Compare the contents of two streams
        /// </summary>
        /// <param name="s1">First stream to compare</param>
        /// <param name="s2">Second stream to compare</param>
        /// <returns>
        /// true if both streams contain same bytes, false otherwise
        /// </returns>
        private static bool StreamsEqual(Stream s1, Stream s2)
        {
            const int bufLen = 1024;
            byte[] bytes1 = new byte[bufLen];
            byte[] bytes2 = new byte[bufLen];
            int bytesChecked = 0;
            while (true)
            {
                int bytesFrom1 = s1.Read(bytes1, 0, bufLen);
                int bytesFrom2 = s2.Read(bytes2, 0, bufLen);
                if (bytesFrom1 == 0 && bytesFrom2 == 0)
                {
                    // Boths streams finished, all bytes are equal
                    return true;
                }
                if (bytesFrom1 != bytesFrom2)
                {
                    // One ended early, not equal.
                    log.DebugFormat("Read {0} bytes from stream1 and {1} bytes from stream2", bytesFrom1, bytesFrom2);
                    return false;
                }
                for (int i = 0; i < bytesFrom1; ++i)
                {
                    if (bytes1[i] != bytes2[i])
                    {
                        log.DebugFormat("Byte {0} doesn't match", bytesChecked + i);
                        // Bytes don't match, not equal.
                        return false;
                    }
                }
                bytesChecked += bytesFrom1;
            }
        }

        /// <summary>
        /// Remove files that the user chose to overwrite, so
        /// the installer can replace them.
        /// Uses a transaction so they can be undeleted if the install
        /// fails at a later stage.
        /// </summary>
        /// <param name="files">The files to overwrite</param>
        private static void DeleteConflictingFiles(IEnumerable<InstallableFile> files)
        {
            TxFileManager file_transaction = new TxFileManager();
            foreach (InstallableFile file in files)
            {
                log.DebugFormat("Trying to delete {0}", file.destination);
                file_transaction.Delete(file.destination);
            }
        }

        #endregion

        #region Find files

        /// <summary>
        /// Given a module and an open zipfile, return all the files that would be installed
        /// for this module.
        ///
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        ///
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        /// </summary>
        public static List<InstallableFile> FindInstallableFiles(CkanModule module, ZipFile zipfile, GameInstance ksp)
        {
            var files = new List<InstallableFile>();

            try
            {
                // Use the provided stanzas, or use the default install stanza if they're absent.
                if (module.install != null && module.install.Length != 0)
                {
                    foreach (ModuleInstallDescriptor stanza in module.install)
                    {
                        files.AddRange(stanza.FindInstallableFiles(zipfile, ksp));
                    }
                }
                else
                {
                    files.AddRange(ModuleInstallDescriptor
                        .DefaultInstallStanza(ksp.game, module.identifier)
                        .FindInstallableFiles(zipfile, ksp));
                }
            }
            catch (BadMetadataKraken kraken)
            {
                // Decorate our kraken with the current module, as the lower-level
                // methods won't know it.
                kraken.module = module;
                throw;
            }

            return files;
        }

        /// <summary>
        /// Given a module and a path to a zipfile, returns all the files that would be installed
        /// from that zip for this module.
        ///
        /// This *will* throw an exception if the file does not exist.
        ///
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        ///
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        /// </summary>
        // TODO: Document which exception!
        public static List<InstallableFile> FindInstallableFiles(CkanModule module, string zip_filename, GameInstance ksp)
        {
            // `using` makes sure our zipfile gets closed when we exit this block.
            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                log.DebugFormat("Searching {0} using {1} as module", zip_filename, module);
                return FindInstallableFiles(module, zipfile, ksp);
            }
        }

        /// <summary>
        /// Returns contents of an installed module
        /// </summary>
        public static IEnumerable<(string path, bool dir, bool exists)> GetModuleContents(
                GameInstance                instance,
                IReadOnlyCollection<string> installed,
                HashSet<string>             filters)
            => GetModuleContents(instance, installed,
                                 installed.SelectMany(f => f.TraverseNodes(Path.GetDirectoryName)
                                                            .Skip(1)
                                                            .Where(s => s.Length > 0)
                                                            .Select(CKANPathUtils.NormalizePath))
                                          .ToHashSet(),
                                 filters);

        private static IEnumerable<(string path, bool dir, bool exists)> GetModuleContents(
                GameInstance                instance,
                IReadOnlyCollection<string> installed,
                HashSet<string>             parents,
                HashSet<string>             filters)
            => installed.Where(f => !filters.Any(filt => f.Contains(filt)))
                        .GroupBy(parents.Contains)
                        .SelectMany(grp =>
                            grp.Select(p => (path:   p,
                                             dir:    grp.Key,
                                             exists: grp.Key ? Directory.Exists(instance.ToAbsoluteGameDir(p))
                                                             : File.Exists(instance.ToAbsoluteGameDir(p)))));

        /// <summary>
        /// Returns the module contents if and only if we have it
        /// available in our cache, empty sequence otherwise.
        ///
        /// Intended for previews.
        /// </summary>
        public static IEnumerable<(string path, bool dir, bool exists)> GetModuleContents(
                NetModuleCache  Cache,
                GameInstance    instance,
                CkanModule      module,
                HashSet<string> filters)
            => (Cache.GetCachedFilename(module) is string filename
                    ? GetModuleContents(instance,
                                        Utilities.DefaultIfThrows(
                                            () => FindInstallableFiles(module, filename, instance)),
                                        filters)
                    : null)
               ?? Enumerable.Empty<(string path, bool dir, bool exists)>();

        private static IEnumerable<(string path, bool dir, bool exists)>? GetModuleContents(
                GameInstance                  instance,
                IEnumerable<InstallableFile>? installable,
                HashSet<string>               filters)
            => installable?.Where(instF => !filters.Any(filt => instF.destination != null
                                                                && instF.destination.Contains(filt)))
                           .Select(f => (path:   instance.ToRelativeGameDir(f.destination),
                                         dir:    f.source.IsDirectory,
                                         exists: true));

        #endregion

        /// <summary>
        /// Copy the entry from the opened zipfile to the path specified.
        /// </summary>
        /// <returns>
        /// Path of file or directory that was created.
        /// May differ from the input fullPath!
        /// </returns>
        internal static string? CopyZipEntry(ZipFile          zipfile,
                                             ZipEntry         entry,
                                             string           fullPath,
                                             bool             makeDirs,
                                             IProgress<long>? progress)
        {
            var file_transaction = new TxFileManager();

            if (entry.IsDirectory)
            {
                // Skip if we're not making directories for this install.
                if (!makeDirs)
                {
                    log.DebugFormat("Skipping '{0}', we don't make directories for this path", fullPath);
                    return null;
                }

                // Windows silently trims trailing spaces, get the path it will actually use
                fullPath = Path.GetDirectoryName(Path.Combine(fullPath, "DUMMY")) is string p
                    ? CKANPathUtils.NormalizePath(p)
                    : fullPath;

                log.DebugFormat("Making directory '{0}'", fullPath);
                file_transaction.CreateDirectory(fullPath);
            }
            else
            {
                log.DebugFormat("Writing file '{0}'", fullPath);

                // ZIP format does not require directory entries
                if (makeDirs && Path.GetDirectoryName(fullPath) is string d)
                {
                    log.DebugFormat("Making parent directory '{0}'", d);
                    file_transaction.CreateDirectory(d);
                }

                // We don't allow for the overwriting of files. See #208.
                if (file_transaction.FileExists(fullPath))
                {
                    throw new FileExistsKraken(fullPath, string.Format(Properties.Resources.ModuleInstallerFileExists, fullPath));
                }

                // Snapshot whatever was there before. If there's nothing, this will just
                // remove our file on rollback. We still need this even though we won't
                // overwite files, as it ensures deletion on rollback.
                file_transaction.Snapshot(fullPath);

                try
                {
                    // It's a file! Prepare the streams
                    using (var zipStream = zipfile.GetInputStream(entry))
                    using (var writer = File.Create(fullPath))
                    {
                        // Windows silently changes paths ending with spaces, get the name it actually used
                        fullPath = CKANPathUtils.NormalizePath(writer.Name);
                        // 4k is the block size on practically every disk and OS.
                        byte[] buffer = new byte[4096];
                        progress?.Report(0);
                        StreamUtils.Copy(zipStream, writer, buffer,
                                         // This doesn't fire at all if the interval never elapses
                                         (sender, e) =>
                                         {
                                             progress?.Report(e.Processed);
                                         },
                                         UnzipProgressInterval,
                                         entry, "CopyZipEntry");
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw new DirectoryNotFoundKraken("", ex.Message, ex);
                }
            }
            // Usually, this is the path we're given.
            // Sometimes it has trailing spaces trimmed by the OS.
            return fullPath;
        }

        private static readonly TimeSpan UnzipProgressInterval = TimeSpan.FromMilliseconds(200);

        #endregion

        #region Uninstallation

        /// <summary>
        /// Uninstalls all the mods provided, including things which depend upon them.
        /// This *DOES* save the registry.
        /// Preferred over Uninstall.
        /// </summary>
        public void UninstallList(IEnumerable<string>  mods,
                                  ref HashSet<string>? possibleConfigOnlyDirs,
                                  RegistryManager      registry_manager,
                                  bool                 ConfirmPrompt = true,
                                  List<CkanModule>?    installing    = null)
        {
            mods = mods.Memoize();
            // Pre-check, have they even asked for things which are installed?

            foreach (string mod in mods.Where(mod => registry_manager.registry.InstalledModule(mod) == null))
            {
                throw new ModNotInstalledKraken(mod);
            }

            var instDlc = mods
                .Select(registry_manager.registry.InstalledModule)
                .OfType<InstalledModule>()
                .FirstOrDefault(m => m.Module.IsDLC);
            if (instDlc != null)
            {
                throw new ModuleIsDLCKraken(instDlc.Module);
            }

            // Find all the things which need uninstalling.
            var revdep = mods
                .Union(registry_manager.registry.FindReverseDependencies(
                    mods.Except(installing?.Select(m => m.identifier) ?? Array.Empty<string>())
                        .ToList(),
                    installing))
                .ToList();

            var goners = revdep.Union(
                    registry_manager.registry.FindRemovableAutoInstalled(
                        registry_manager.registry.InstalledModules
                            .Where(im => !revdep.Contains(im.identifier))
                            .Concat(installing?.Select(m => new InstalledModule(null, m, Array.Empty<string>(), false)) ?? Array.Empty<InstalledModule>())
                            .ToList(),
                        instance.game, instance.StabilityToleranceConfig, instance.VersionCriteria())
                    .Select(im => im.identifier))
                .ToList();

            // If there is nothing to uninstall, skip out.
            if (goners.Count == 0)
            {
                return;
            }

            User.RaiseMessage(Properties.Resources.ModuleInstallerAboutToRemove);
            User.RaiseMessage("");

            foreach (var module in goners.Select(registry_manager.registry.InstalledModule)
                                         .OfType<InstalledModule>())
            {
                User.RaiseMessage(" * {0} {1}", module.Module.name, module.Module.version);
            }

            if (ConfirmPrompt && !User.RaiseYesNoDialog(Properties.Resources.ModuleInstallerContinuePrompt))
            {
                throw new CancelledActionKraken(Properties.Resources.ModuleInstallerRemoveAborted);
            }

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                var registry = registry_manager.registry;
                long removeBytes = goners.Select(registry.InstalledModule)
                                         .OfType<InstalledModule>()
                                         .Sum(m => m.Module.install_size);
                var rateCounter = new ByteRateCounter()
                {
                    Size      = removeBytes,
                    BytesLeft = removeBytes,
                };
                rateCounter.Start();

                long modRemoveCompletedBytes = 0;
                foreach (string ident in goners)
                {
                    if (registry.InstalledModule(ident) is InstalledModule instMod)
                    {
                        User.RaiseMessage(Properties.Resources.ModuleInstallerRemovingMod,
                                          registry.InstalledModule(ident)?.Module.name
                                                                         ?? ident);
                        Uninstall(ident, ref possibleConfigOnlyDirs, registry,
                                  new ProgressImmediate<long>(bytes =>
                                  {
                                      RemoveProgress?.Invoke(instMod,
                                                             instMod.Module.install_size - bytes,
                                                             instMod.Module.install_size);
                                      rateCounter.BytesLeft = removeBytes - (modRemoveCompletedBytes + bytes);
                                      User.RaiseProgress(rateCounter);
                                  }));
                        modRemoveCompletedBytes += instMod?.Module.install_size ?? 0;
                        User.RaiseMessage(Properties.Resources.ModuleInstallerRemovedMod,
                                          registry.InstalledModule(ident)?.Module.name
                                                                         ?? ident);
                    }
                }

                // Enforce consistency if we're not installing anything,
                // otherwise consistency will be enforced after the installs
                registry_manager.Save(installing == null);

                transaction.Complete();
            }

            User.RaiseProgress(Properties.Resources.ModuleInstallerDone, 100);
        }

        /// <summary>
        /// Uninstall the module provided. For internal use only.
        /// Use UninstallList for user queries, it also does dependency handling.
        /// This does *NOT* save the registry.
        /// </summary>
        /// <param name="identifier">Identifier of module to uninstall</param>
        /// <param name="possibleConfigOnlyDirs">Directories that the user might want to remove after uninstall</param>
        /// <param name="registry">Registry to use</param>
        /// <param name="progress">Progress to report</param>
        private void Uninstall(string               identifier,
                               ref HashSet<string>? possibleConfigOnlyDirs,
                               Registry             registry,
                               IProgress<long>      progress)
        {
            var file_transaction = new TxFileManager();

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                var instMod = registry.InstalledModule(identifier);

                if (instMod == null)
                {
                    log.ErrorFormat("Trying to uninstall {0} but it's not installed", identifier);
                    throw new ModNotInstalledKraken(identifier);
                }

                // Walk our registry to find all files for this mod.
                var modFiles = instMod.Files.ToArray();

                // We need case insensitive path matching on Windows
                var directoriesToDelete = new HashSet<string>(Platform.PathComparer);

                // Files that Windows refused to delete due to locking (probably)
                var undeletableFiles = new List<string>();

                long bytesDeleted = 0;
                foreach (string relPath in modFiles)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        throw new CancelledActionKraken();
                    }

                    string absPath = instance.ToAbsoluteGameDir(relPath);

                    try
                    {
                        if (File.GetAttributes(absPath)
                                .HasFlag(FileAttributes.Directory))
                        {
                            directoriesToDelete.Add(absPath);
                        }
                        else
                        {
                            // Add this file's directory to the list for deletion if it isn't already there.
                            // Helps clean up directories when modules are uninstalled out of dependency order
                            // Since we check for directory contents when deleting, this should purge empty
                            // dirs, making less ModuleManager headaches for people.
                            if (Path.GetDirectoryName(absPath) is string p)
                            {
                                directoriesToDelete.Add(p);
                            }

                            bytesDeleted += new FileInfo(absPath).Length;
                            progress.Report(bytesDeleted);
                            log.DebugFormat("Removing {0}", relPath);
                            file_transaction.Delete(absPath);
                        }
                    }
                    catch (FileNotFoundException exc)
                    {
                        log.Debug("Ignoring missing file while deleting", exc);
                    }
                    catch (DirectoryNotFoundException exc)
                    {
                        log.Debug("Ignoring missing directory while deleting", exc);
                    }
                    catch (IOException)
                    {
                        // "The specified file is in use."
                        undeletableFiles.Add(relPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // "The caller does not have the required permission."
                        // "The file is an executable file that is in use."
                        undeletableFiles.Add(relPath);
                    }
                    catch (Exception exc)
                    {
                        // We don't consider this problem serious enough to abort and revert,
                        // so treat it as a "--verbose" level log message.
                        log.InfoFormat("Failure in locating file {0}: {1}", absPath, exc.Message);
                    }
                }

                if (undeletableFiles.Count > 0)
                {
                    throw new FailedToDeleteFilesKraken(identifier, undeletableFiles);
                }

                // Remove from registry.
                registry.DeregisterModule(instance, identifier);

                // Our collection of directories may leave empty parent directories.
                directoriesToDelete = AddParentDirectories(directoriesToDelete);

                // Sort our directories from longest to shortest, to make sure we remove child directories
                // before parents. GH #78.
                foreach (string directory in directoriesToDelete.OrderByDescending(dir => dir.Length))
                {
                    log.DebugFormat("Checking {0}...", directory);
                    // It is bad if any of this directories gets removed
                    // So we protect them
                    // A few string comparisons will be cheaper than hitting the disk, so do this first
                    if (instance.game.IsReservedDirectory(instance, directory))
                    {
                        log.DebugFormat("Directory {0} is reserved, skipping", directory);
                        continue;
                    }

                    // See what's left in this folder and what we can do about it
                    GroupFilesByRemovable(instance.ToRelativeGameDir(directory),
                                          registry, modFiles, instance.game,
                                          (Directory.Exists(directory)
                                              ? Directory.EnumerateFileSystemEntries(directory, "*", SearchOption.AllDirectories)
                                              : Enumerable.Empty<string>())
                                           .Select(instance.ToRelativeGameDir)
                                           .ToArray(),
                                          out string[] removable,
                                          out string[] notRemovable);

                    // Delete the auto-removable files and dirs
                    foreach (var absPath in removable.Select(instance.ToAbsoluteGameDir))
                    {
                        if (File.Exists(absPath))
                        {
                            log.DebugFormat("Attempting transaction deletion of file {0}", absPath);
                            file_transaction.Delete(absPath);
                        }
                        else if (Directory.Exists(absPath))
                        {
                            log.DebugFormat("Attempting deletion of directory {0}", absPath);
                            try
                            {
                                Directory.Delete(absPath);
                            }
                            catch
                            {
                                // There might be files owned by other mods, oh well
                                log.DebugFormat("Failed to delete {0}", absPath);
                            }
                        }
                    }

                    if (notRemovable.Length < 1)
                    {
                        // We *don't* use our file_transaction to delete files here, because
                        // it fails if the system's temp directory is on a different device
                        // to KSP. However we *can* safely delete it now we know it's empty,
                        // because the TxFileMgr *will* put it back if there's a file inside that
                        // needs it.
                        //
                        // This works around GH #251.
                        // The filesystem boundary bug is described in https://transactionalfilemgr.codeplex.com/workitem/20

                        log.DebugFormat("Removing {0}", directory);
                        Directory.Delete(directory);
                    }
                    else if (notRemovable.Except(possibleConfigOnlyDirs?.Select(instance.ToRelativeGameDir)
                                                 ?? Enumerable.Empty<string>())
                                         // Can't remove if owned by some other mod
                                         .Any(relPath => registry.FileOwner(relPath) != null
                                                         || modFiles.Contains(relPath)))
                    {
                        log.InfoFormat("Not removing directory {0}, it's not empty", directory);
                    }
                    else
                    {
                        log.DebugFormat("Directory {0} contains only non-registered files, ask user about it later: {1}",
                                        directory,
                                        string.Join(", ", notRemovable));
                        possibleConfigOnlyDirs ??= new HashSet<string>(Platform.PathComparer);
                        possibleConfigOnlyDirs.Add(directory);
                    }
                }
                log.InfoFormat("Removed {0}", identifier);
                transaction.Complete();
            }
        }

        internal static void GroupFilesByRemovable(string       relRoot,
                                                   Registry     registry,
                                                   string[]     alreadyRemoving,
                                                   IGame        game,
                                                   string[]     relPaths,
                                                   out string[] removable,
                                                   out string[] notRemovable)
        {
            if (relPaths.Length < 1)
            {
                removable    = Array.Empty<string>();
                notRemovable = Array.Empty<string>();
                return;
            }
            log.DebugFormat("Getting contents of {0}", relRoot);
            var contents = relPaths
                // Split into auto-removable and not-removable
                // Removable must not be owned by other mods
                .GroupBy(f => registry.FileOwner(f) == null
                              // Also skip owned by this module since it's already deregistered
                              && !alreadyRemoving.Contains(f)
                              // Must have a removable dir name somewhere in path AFTER main dir
                              && f[relRoot.Length..]
                                  .Split('/')
                                  .Where(piece => !string.IsNullOrEmpty(piece))
                                  .Any(piece => game.AutoRemovableDirs.Contains(piece)))
                .ToDictionary(grp => grp.Key,
                              grp => grp.OrderByDescending(f => f.Length)
                                        .ToArray());
            removable    = contents.TryGetValue(true,  out string[]? val1) ? val1 : Array.Empty<string>();
            notRemovable = contents.TryGetValue(false, out string[]? val2) ? val2 : Array.Empty<string>();
            log.DebugFormat("Got removable: {0}",    string.Join(", ", removable));
            log.DebugFormat("Got notRemovable: {0}", string.Join(", ", notRemovable));
        }

        /// <summary>
        /// Takes a collection of directories and adds all parent directories within the GameData structure.
        /// </summary>
        /// <param name="directories">The collection of directory path strings to examine</param>
        public HashSet<string> AddParentDirectories(HashSet<string> directories)
        {
            var gameDir = CKANPathUtils.NormalizePath(instance.GameDir());
            return directories
                .Where(dir => !string.IsNullOrWhiteSpace(dir))
                // Normalize all paths before deduplicate
                .Select(CKANPathUtils.NormalizePath)
                // Remove any duplicate paths
                .Distinct()
                .SelectMany(dir =>
                {
                    var results = new HashSet<string>(Platform.PathComparer);
                    // Adding in the DirectorySeparatorChar fixes attempts on Windows
                    // to parse "X:" which resolves to Environment.CurrentDirectory
                    var dirInfo = new DirectoryInfo(
                        dir.EndsWith("/") ? dir : dir + Path.DirectorySeparatorChar);

                    // If this is a parentless directory (Windows)
                    // or if the Root equals the current directory (Mono)
                    if (dirInfo.Parent == null || dirInfo.Root == dirInfo)
                    {
                        return results;
                    }

                    if (!dir.StartsWith(gameDir, Platform.PathComparison))
                    {
                        dir = CKANPathUtils.ToAbsolute(dir, gameDir);
                    }

                    // Remove the system paths, leaving the path under the instance directory
                    var relativeHead = CKANPathUtils.ToRelative(dir, gameDir);
                    // Don't try to remove GameRoot
                    if (!string.IsNullOrEmpty(relativeHead))
                    {
                        var pathArray = relativeHead.Split('/');
                        var builtPath = "";
                        foreach (var path in pathArray)
                        {
                            builtPath += path + '/';
                            results.Add(CKANPathUtils.ToAbsolute(builtPath, gameDir));
                        }
                    }

                    return results;
                })
                .Where(dir => !instance.game.IsReservedDirectory(instance, dir))
                .ToHashSet();
        }

        #endregion

        #region AddRemove

        /// <summary>
        /// Adds and removes the listed modules as a single transaction.
        /// No relationships will be processed.
        /// This *will* save the registry.
        /// </summary>
        /// <param name="possibleConfigOnlyDirs">Directories that the user might want to remove after uninstall</param>
        /// <param name="registry_manager">Registry to use</param>
        /// <param name="resolver">Relationship resolver to use</param>
        /// <param name="add">Modules to add</param>
        /// <param name="autoInstalled">true or false for each item in `add`</param>
        /// <param name="remove">Modules to remove</param>
        /// <param name="downloader">Downloader to use</param>
        /// <param name="enforceConsistency">Whether to enforce consistency</param>
        private void AddRemove(ref HashSet<string>?          possibleConfigOnlyDirs,
                               RegistryManager               registry_manager,
                               RelationshipResolver          resolver,
                               ICollection<CkanModule>       add,
                               IDictionary<CkanModule, bool> autoInstalled,
                               ICollection<InstalledModule>  remove,
                               IDownloader                   downloader,
                               bool                          enforceConsistency)
        {
            using (var tx = CkanTransaction.CreateTransactionScope())
            {
                var groups = add.GroupBy(m => m.IsMetapackage || Cache.IsCached(m));
                var cached = groups.FirstOrDefault(grp => grp.Key)?.ToArray()
                                                                  ?? Array.Empty<CkanModule>();
                var toDownload = groups.FirstOrDefault(grp => !grp.Key)?.ToArray()
                                                                       ?? Array.Empty<CkanModule>();

                long removeBytes     = remove.Sum(m => m.Module.install_size);
                long removedBytes    = 0;
                long downloadBytes   = toDownload.Sum(m => m.download_size);
                long downloadedBytes = 0;
                long installBytes    = add.Sum(m => m.install_size);
                long installedBytes  = 0;
                var rateCounter = new ByteRateCounter()
                {
                    Size      = removeBytes + downloadBytes + installBytes,
                    BytesLeft = removeBytes + downloadBytes + installBytes,
                };
                rateCounter.Start();

                downloader.OverallDownloadProgress += brc =>
                {
                    downloadedBytes = downloadBytes - brc.BytesLeft;
                    rateCounter.BytesLeft = removeBytes   - removedBytes
                                          + downloadBytes - downloadedBytes
                                          + installBytes  - installedBytes;
                    User.RaiseProgress(rateCounter);
                };
                var toInstall = ModsInDependencyOrder(resolver, cached, toDownload, downloader);

                long modRemoveCompletedBytes = 0;
                foreach (var instMod in remove)
                {
                    Uninstall(instMod.Module.identifier,
                              ref possibleConfigOnlyDirs,
                              registry_manager.registry,
                              new ProgressImmediate<long>(bytes =>
                              {
                                  RemoveProgress?.Invoke(instMod,
                                                         instMod.Module.install_size - bytes,
                                                         instMod.Module.install_size);
                                  removedBytes = modRemoveCompletedBytes + bytes;
                                  rateCounter.BytesLeft = removeBytes   - removedBytes
                                                        + downloadBytes - downloadedBytes
                                                        + installBytes  - installedBytes;
                                  User.RaiseProgress(rateCounter);
                              }));
                     modRemoveCompletedBytes += instMod.Module.install_size;
                }

                var gameDir = new DirectoryInfo(instance.GameDir());
                long modInstallCompletedBytes = 0;
                foreach (var mod in toInstall)
                {
                    CKANPathUtils.CheckFreeSpace(gameDir, mod.install_size,
                                                 Properties.Resources.NotEnoughSpaceToInstall);
                    Install(mod,
                            // For upgrading, new modules are dependencies and should be marked auto-installed,
                            // for replacing, new modules are the replacements and should not be marked auto-installed
                            remove?.FirstOrDefault(im => im.Module.identifier == mod.identifier)
                                  ?.AutoInstalled
                                  ?? autoInstalled[mod],
                            registry_manager.registry,
                            ref possibleConfigOnlyDirs,
                            new ProgressImmediate<long>(bytes =>
                            {
                                InstallProgress?.Invoke(mod,
                                                        mod.install_size - bytes,
                                                        mod.install_size);
                                installedBytes = modInstallCompletedBytes + bytes;
                                rateCounter.BytesLeft = removeBytes   - removedBytes
                                                      + downloadBytes - downloadedBytes
                                                      + installBytes  - installedBytes;
                                User.RaiseProgress(rateCounter);
                            }));
                    modInstallCompletedBytes += mod.install_size;
                }

                registry_manager.Save(enforceConsistency);
                tx.Complete();
                EnforceCacheSizeLimit(registry_manager.registry, Cache);
            }
        }

        /// <summary>
        /// Upgrades or installs the mods listed to the specified versions for the user's KSP.
        /// Will *re-install* or *downgrade* (with a warning) as well as upgrade.
        /// Throws ModuleNotFoundKraken if a module is not installed.
        /// </summary>
        public void Upgrade(ICollection<CkanModule> modules,
                            IDownloader             downloader,
                            ref HashSet<string>?    possibleConfigOnlyDirs,
                            RegistryManager         registry_manager,
                            bool                    enforceConsistency   = true,
                            bool                    ConfirmPrompt        = true)
        {
            var registry = registry_manager.registry;

            var resolver = new RelationshipResolver(
                modules,
                modules.Select(m => registry.InstalledModule(m.identifier)?.Module)
                       .OfType<CkanModule>(),
                RelationshipResolverOptions.DependsOnlyOpts(instance.StabilityToleranceConfig),
                registry,
                instance.game, instance.VersionCriteria());
            modules = resolver.ModList().ToArray();
            var autoInstalled = modules.ToDictionary(m => m, resolver.IsAutoInstalled);

            User.RaiseMessage(Properties.Resources.ModuleInstallerAboutToUpgrade);
            User.RaiseMessage("");

            // Our upgrade involves removing everything that's currently installed, then
            // adding everything that needs installing (which may involve new mods to
            // satisfy dependencies). We always know the list passed in is what we need to
            // install, but we need to calculate what needs to be removed.
            var to_remove = new List<InstalledModule>();

            // Let's discover what we need to do with each module!
            foreach (CkanModule module in modules)
            {
                var installed_mod = registry.InstalledModule(module.identifier);

                if (installed_mod == null)
                {
                    if (!Cache.IsMaybeCachedZip(module)
                        && Cache.GetInProgressFileName(module) is FileInfo inProgressFile)
                    {
                        if (inProgressFile.Exists)
                        {
                            User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeInstallingResuming,
                                module.name, module.version,
                                string.Join(", ", PrioritizedHosts(module.download)),
                                CkanModule.FmtSize(module.download_size - inProgressFile.Length));
                        }
                        else
                        {
                            User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeInstallingUncached,
                                module.name, module.version,
                                string.Join(", ", PrioritizedHosts(module.download)),
                                CkanModule.FmtSize(module.download_size));
                        }
                    }
                    else
                    {
                        User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeInstallingCached,
                            module.name, module.version);
                    }
                }
                else
                {
                    // Module already installed. We'll need to remove it first.
                    to_remove.Add(installed_mod);

                    CkanModule installed = installed_mod.Module;
                    if (installed.version.IsEqualTo(module.version))
                    {
                        User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeReinstalling,
                            module.name, module.version);
                    }
                    else if (installed.version.IsGreaterThan(module.version))
                    {
                        User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeDowngrading,
                            module.name, installed.version, module.version);
                    }
                    else
                    {
                        if (!Cache.IsMaybeCachedZip(module)
                            && Cache.GetInProgressFileName(module) is FileInfo inProgressFile)
                        {
                            if (inProgressFile.Exists)
                            {
                                User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeUpgradingResuming,
                                    module.name, installed.version, module.version,
                                    string.Join(", ", PrioritizedHosts(module.download)),
                                    CkanModule.FmtSize(module.download_size - inProgressFile.Length));
                            }
                            else
                            {
                                User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeUpgradingUncached,
                                    module.name, installed.version, module.version,
                                    string.Join(", ", PrioritizedHosts(module.download)),
                                    CkanModule.FmtSize(module.download_size));
                            }
                        }
                        else
                        {
                            User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeUpgradingCached,
                                module.name, installed.version, module.version);
                        }
                    }
                }
            }

            var removingIdents = to_remove.Select(im => im.identifier).ToHashSet();
            var autoRemoving = registry
                .FindRemovableAutoInstalled(
                    // Conjure the future state of the installed modules list after upgrading
                    registry.InstalledModules
                            .Where(im => !removingIdents.Contains(im.identifier))
                            .Concat(modules.Select(m => new InstalledModule(null, m, Array.Empty<string>(), false)))
                            .ToList(),
                    instance.game, instance.StabilityToleranceConfig, instance.VersionCriteria())
                .ToList();
            if (autoRemoving.Count > 0)
            {
                foreach (var im in autoRemoving)
                {
                    User.RaiseMessage(Properties.Resources.ModuleInstallerUpgradeAutoRemoving,
                                      im.Module.name, im.Module.version);
                }
                to_remove.AddRange(autoRemoving);
            }

            if (ConfirmPrompt && !User.RaiseYesNoDialog(Properties.Resources.ModuleInstallerContinuePrompt))
            {
                throw new CancelledActionKraken(Properties.Resources.ModuleInstallerUpgradeUserDeclined);
            }

            AddRemove(ref possibleConfigOnlyDirs,
                      registry_manager,
                      resolver,
                      modules,
                      autoInstalled,
                      to_remove,
                      downloader,
                      enforceConsistency);
            User.RaiseProgress(Properties.Resources.ModuleInstallerDone, 100);
        }

        /// <summary>
        /// Enacts listed Module Replacements to the specified versions for the user's KSP.
        /// Will *re-install* or *downgrade* (with a warning) as well as upgrade.
        /// </summary>
        /// <exception cref="DependenciesNotSatisfiedKraken">Thrown if a dependency for a replacing module couldn't be satisfied.</exception>
        /// <exception cref="ModuleNotFoundKraken">Thrown if a module that should be replaced is not installed.</exception>
        public void Replace(IEnumerable<ModuleReplacement> replacements,
                            RelationshipResolverOptions    options,
                            IDownloader                    downloader,
                            ref HashSet<string>?           possibleConfigOnlyDirs,
                            RegistryManager                registry_manager,
                            bool                           enforceConsistency = true)
        {
            replacements = replacements.Memoize();
            log.Debug("Using Replace method");
            List<CkanModule> modsToInstall = new List<CkanModule>();
            var modsToRemove = new List<InstalledModule>();
            foreach (ModuleReplacement repl in replacements)
            {
                modsToInstall.Add(repl.ReplaceWith);
                log.DebugFormat("We want to install {0} as a replacement for {1}", repl.ReplaceWith.identifier, repl.ToReplace.identifier);
            }

            // Our replacement involves removing the currently installed mods, then
            // adding everything that needs installing (which may involve new mods to
            // satisfy dependencies).

            // Let's discover what we need to do with each module!
            foreach (ModuleReplacement repl in replacements)
            {
                string ident = repl.ToReplace.identifier;
                var installedMod = registry_manager.registry.InstalledModule(ident);

                if (installedMod == null)
                {
                    log.WarnFormat("Wait, {0} is not actually installed?", ident);
                    //Maybe ModuleNotInstalled ?
                    if (registry_manager.registry.IsAutodetected(ident))
                    {
                        throw new ModuleNotFoundKraken(ident,
                            repl.ToReplace.version.ToString(),
                            string.Format(Properties.Resources.ModuleInstallerReplaceAutodetected, ident));
                    }

                    throw new ModuleNotFoundKraken(ident,
                        repl.ToReplace.version.ToString(),
                        string.Format(Properties.Resources.ModuleInstallerReplaceNotInstalled, ident, repl.ReplaceWith.identifier));
                }
                else
                {
                    // Obviously, we need to remove the mod we are replacing
                    modsToRemove.Add(installedMod);

                    log.DebugFormat("Ok, we are removing {0}", repl.ToReplace.identifier);
                    //Check whether our Replacement target is already installed
                    var installed_replacement = registry_manager.registry.InstalledModule(repl.ReplaceWith.identifier);

                    // If replacement is not installed, we've already added it to modsToInstall above
                    if (installed_replacement != null)
                    {
                        //Module already installed. We'll need to treat it as an upgrade.
                        log.DebugFormat("It turns out {0} is already installed, we'll upgrade it.", installed_replacement.identifier);
                        modsToRemove.Add(installed_replacement);

                        CkanModule installed = installed_replacement.Module;
                        if (installed.version.IsEqualTo(repl.ReplaceWith.version))
                        {
                            log.InfoFormat("{0} is already at the latest version, reinstalling to replace {1}", repl.ReplaceWith.identifier, repl.ToReplace.identifier);
                        }
                        else if (installed.version.IsGreaterThan(repl.ReplaceWith.version))
                        {
                            log.WarnFormat("Downgrading {0} from {1} to {2} to replace {3}", repl.ReplaceWith.identifier, repl.ReplaceWith.version, repl.ReplaceWith.version, repl.ToReplace.identifier);
                        }
                        else
                        {
                            log.InfoFormat("Upgrading {0} to {1} to replace {2}", repl.ReplaceWith.identifier, repl.ReplaceWith.version, repl.ToReplace.identifier);
                        }
                    }
                    else
                    {
                        log.InfoFormat("Replacing {0} with {1} {2}", repl.ToReplace.identifier, repl.ReplaceWith.identifier, repl.ReplaceWith.version);
                    }
                }
            }
            var resolver = new RelationshipResolver(modsToInstall, null, options, registry_manager.registry,
                                                    instance.game, instance.VersionCriteria());
            var resolvedModsToInstall = resolver.ModList().ToList();

            AddRemove(ref possibleConfigOnlyDirs,
                      registry_manager,
                      resolver,
                      resolvedModsToInstall,
                      resolvedModsToInstall.ToDictionary(m => m, m => false),
                      modsToRemove,
                      downloader,
                      enforceConsistency);
            User.RaiseProgress(Properties.Resources.ModuleInstallerDone, 100);
        }

        #endregion

        public static IEnumerable<string> PrioritizedHosts(IEnumerable<Uri>? urls)
            => urls?.OrderBy(u => u, new PreferredHostUriComparer(ServiceLocator.Container.Resolve<IConfiguration>().PreferredHosts))
                    .Select(dl => dl.Host)
                    .Distinct()
                   ?? Enumerable.Empty<string>();

        #region Recommendations

        /// <summary>
        /// Looks for optional related modules that could be installed alongside the given modules
        /// </summary>
        /// <param name="instance">Game instance to use</param>
        /// <param name="sourceModules">Modules to check for relationships</param>
        /// <param name="toInstall">Modules already being installed, to be omitted from search</param>
        /// <param name="registry">Registry to use</param>
        /// <param name="recommendations">Modules that are recommended to install</param>
        /// <param name="suggestions">Modules that are suggested to install</param>
        /// <param name="supporters">Modules that support other modules we're installing</param>
        /// <returns>
        /// true if anything found, false otherwise
        /// </returns>
        public static bool FindRecommendations(GameInstance                                          instance,
                                               ICollection<CkanModule>                               sourceModules,
                                               ICollection<CkanModule>                               toInstall,
                                               Registry                                              registry,
                                               out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                                               out Dictionary<CkanModule, List<string>>              suggestions,
                                               out Dictionary<CkanModule, HashSet<string>>           supporters)
        {
            log.DebugFormat("Finding recommendations for: {0}", string.Join(", ", sourceModules));
            var crit     = instance.VersionCriteria();
            var resolver = new RelationshipResolver(sourceModules.Where(m => !m.IsDLC),
                                                    null,
                                                    RelationshipResolverOptions.KitchenSinkOpts(instance.StabilityToleranceConfig),
                                                    registry, instance.game, crit);
            var recommenders = resolver.Dependencies().ToHashSet();
            log.DebugFormat("Recommenders: {0}", string.Join(", ", recommenders));

            var checkedRecs = resolver.Recommendations(recommenders)
                                      .Where(m => resolver.ReasonsFor(m)
                                                          .Any(r => r is SelectionReason.Recommended { ProvidesIndex: 0 }))
                                      .ToHashSet();
            var conflicting = new RelationshipResolver(toInstall.Concat(checkedRecs), null,
                                                       RelationshipResolverOptions.ConflictsOpts(instance.StabilityToleranceConfig),
                                                       registry, instance.game, crit)
                                  .ConflictList.Keys;
            // Don't check recommendations that conflict with installed or installing mods
            checkedRecs.ExceptWith(conflicting);

            recommendations = resolver.Recommendations(recommenders)
                                      .ToDictionary(m => m,
                                                    m => new Tuple<bool, List<string>>(
                                                             checkedRecs.Contains(m),
                                                             resolver.ReasonsFor(m)
                                                                     .OfType<SelectionReason.Recommended>()
                                                                     .Where(r => recommenders.Contains(r.Parent))
                                                                     .Select(r => r.Parent)
                                                                     .OfType<CkanModule>()
                                                                     .Select(m => m.identifier)
                                                                     .ToList()));
            suggestions = resolver.Suggestions(recommenders,
                                               recommendations.Keys.ToList())
                                  .ToDictionary(m => m,
                                                m => resolver.ReasonsFor(m)
                                                             .OfType<SelectionReason.Suggested>()
                                                             .Where(r => recommenders.Contains(r.Parent))
                                                             .Select(r => r.Parent)
                                                             .OfType<CkanModule>()
                                                             .Select(m => m.identifier)
                                                             .ToList());

            var opts = RelationshipResolverOptions.DependsOnlyOpts(instance.StabilityToleranceConfig);
            supporters = resolver.Supporters(recommenders,
                                             recommenders.Concat(recommendations.Keys)
                                                         .Concat(suggestions.Keys))
                                 .Where(kvp => CanInstall(toInstall.Append(kvp.Key).ToList(),
                                                          opts, registry, instance.game, crit))
                                 .ToDictionary();

            return recommendations.Count > 0
                || suggestions.Count > 0
                || supporters.Count > 0;
        }

        /// <summary>
        /// Determine whether there is any way to install the given set of mods.
        /// Handles virtual dependencies, including recursively.
        /// </summary>
        /// <param name="opts">Installer options</param>
        /// <param name="toInstall">Mods we want to install</param>
        /// <param name="registry">Registry of instance into which we want to install</param>
        /// <param name="game">Game instance</param>
        /// <param name="crit">Game version criteria</param>
        /// <returns>
        /// True if it's possible to install these mods, false otherwise
        /// </returns>
        public static bool CanInstall(List<CkanModule>            toInstall,
                                      RelationshipResolverOptions opts,
                                      IRegistryQuerier            registry,
                                      IGame                       game,
                                      GameVersionCriteria         crit)
        {
            string request = string.Join(", ", toInstall.Select(m => m.identifier));
            try
            {
                var installed = toInstall.Select(m => registry.InstalledModule(m.identifier)?.Module)
                                         .OfType<CkanModule>();
                var resolver = new RelationshipResolver(toInstall, installed, opts, registry, game, crit);

                var resolverModList = resolver.ModList(false).ToList();
                if (resolverModList.Count >= toInstall.Count(m => !m.IsMetapackage))
                {
                    // We can install with no further dependencies
                    string recipe = string.Join(", ", resolverModList.Select(m => m.identifier));
                    log.Debug($"Installable: {request}: {recipe}");
                    return true;
                }
                else
                {
                    log.DebugFormat("Can't install {0}: {1}", request, string.Join("; ", resolver.ConflictDescriptions));
                    return false;
                }
            }
            catch (TooManyModsProvideKraken k)
            {
                // One of the dependencies is virtual
                foreach (var mod in k.modules)
                {
                    // Try each option recursively to see if any are successful
                    if (CanInstall(toInstall.Append(mod).ToList(), opts, registry, game, crit))
                    {
                        // Child call will emit debug output, so we don't need to here
                        return true;
                    }
                }
                log.Debug($"Can't install {request}: Can't install provider of {k.requested}");
            }
            catch (InconsistentKraken k)
            {
                log.Debug($"Can't install {request}: {k.ShortDescription}");
            }
            catch (Exception ex)
            {
                log.Debug($"Can't install {request}: {ex.Message}");
            }
            return false;
        }

        #endregion

        private static void EnforceCacheSizeLimit(Registry registry, NetModuleCache Cache)
        {
            // Purge old downloads if we're over the limit
            IConfiguration winReg = ServiceLocator.Container.Resolve<IConfiguration>();
            if (winReg.CacheSizeLimit.HasValue)
            {
                Cache.EnforceSizeLimit(winReg.CacheSizeLimit.Value, registry);
            }
        }

        /// <summary>
        /// Remove prepending v V. Version_ etc
        /// </summary>
        public static string StripV(string version)
            => Regex.Match(version, @"^(?<num>\d\:)?[vV]+(ersion)?[_.]*(?<ver>\d.*)$") is Match match
               && match.Success
                   ? match.Groups["num"].Value + match.Groups["ver"].Value
                   : version;

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version that might contain an epoch</param>
        public static string StripEpoch(ModuleVersion version)
            => StripEpoch(version.ToString());

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string StripEpoch(string version)
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            => epochMatch.IsMatch(version)
                ? epochReplace.Replace(version, @"$2")
                : version;

        /// <summary>
        /// As above, but includes the original in parentheses
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string WithAndWithoutEpoch(string version)
            => epochMatch.IsMatch(version)
                ? $"{epochReplace.Replace(version, @"$2")} ({version})"
                : version;

        private static readonly Regex epochMatch   = new Regex(@"^[0-9][0-9]*:[^:]+$", RegexOptions.Compiled);
        private static readonly Regex epochReplace = new Regex(@"^([^:]+):([^:]+)$",   RegexOptions.Compiled);
    }
}
