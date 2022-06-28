using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using ChinhDo.Transactions.FileManager;
using Autofac;
using CKAN.Extensions;
using CKAN.Versioning;
using CKAN.Configuration;

namespace CKAN
{
    public delegate void ModuleInstallerReportModInstalled(CkanModule module);

    public struct InstallableFile
    {
        public ZipEntry source;
        public string destination;
        public bool makedir;
    }

    public class ModuleInstaller
    {
        public IUser User { get; set; }

        // To allow the ModuleInstaller to work on multiple KSP instances, keep a list of each ModuleInstaller and return the correct one upon request.
        private static SortedList<string, ModuleInstaller> instances = new SortedList<string, ModuleInstaller>();

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        private GameInstance ksp;

        private NetModuleCache Cache;

        public ModuleInstallerReportModInstalled onReportModInstalled = null;

        // Constructor
        public ModuleInstaller(GameInstance ksp, NetModuleCache cache, IUser user)
        {
            User = user;
            Cache = cache;
            this.ksp = ksp;
            log.DebugFormat("Creating ModuleInstaller for {0}", ksp.GameDir());
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public string Download(CkanModule module, string filename)
        {
            User.RaiseProgress(String.Format("Downloading \"{0}\"", module.download), 0);
            return Download(module, filename, Cache);
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public static string Download(CkanModule module, string filename, NetModuleCache cache)
        {
            log.Info("Downloading " + filename);

            string tmp_file = Net.Download(module.download);

            return cache.Store(module, tmp_file, filename, true);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks the CKAN cache first.
        /// </summary>
        public string CachedOrDownload(CkanModule module, string filename = null)
        {
            return CachedOrDownload(module, Cache, filename);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks provided cache first.
        /// </summary>
        public static string CachedOrDownload(CkanModule module, NetModuleCache cache, string filename = null)
        {
            if (filename == null)
            {
                filename = CkanModule.StandardName(module.identifier, module.version);
            }

            string full_path = cache.GetCachedZip(module);
            if (full_path == null)
            {
                return Download(module, filename, cache);
            }

            log.DebugFormat("Using {0} (cached)", filename);
            return full_path;
        }

        public void InstallList(List<string> modules, RelationshipResolverOptions options, RegistryManager registry_manager, ref HashSet<string> possibleConfigOnlyDirs, IDownloader downloader = null)
        {
            var resolver = new RelationshipResolver(modules, null, options, registry_manager.registry, ksp.VersionCriteria());
            // Only pass the CkanModules of the parameters, so we can tell which are auto-installed,
            // and relationships of metapackages, since metapackages aren't included in the RR modlist.
            var list = resolver.ModList().Where(
                m =>
                {
                    var reason = resolver.ReasonFor(m);
                    return reason is SelectionReason.UserRequested || (reason.Parent?.IsMetapackage ?? false);
                }).ToList();
            InstallList(list, options, registry_manager, ref possibleConfigOnlyDirs, downloader);
        }

        /// <summary>
        ///     Installs all modules given a list of identifiers as a transaction. Resolves dependencies.
        ///     This *will* save the registry at the end of operation.
        ///
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Propagates a CancelledActionKraken if the user cancelled the install.
        /// </summary>
        public void InstallList(ICollection<CkanModule> modules, RelationshipResolverOptions options, RegistryManager registry_manager, ref HashSet<string> possibleConfigOnlyDirs, IDownloader downloader = null, bool ConfirmPrompt = true)
        {
            // TODO: Break this up into smaller pieces! It's huge!
            if (modules.Count == 0)
            {
                User.RaiseProgress("Nothing to install.", 100);
                return;
            }
            var resolver = new RelationshipResolver(modules, null, options, registry_manager.registry, ksp.VersionCriteria());
            var modsToInstall = resolver.ModList().ToList();
            List<CkanModule> downloads = new List<CkanModule>();

            // TODO: All this user-stuff should be happening in another method!
            // We should just be installing mods as a transaction.

            User.RaiseMessage("About to install:\r\n");

            foreach (CkanModule module in modsToInstall)
            {
                if (!Cache.IsMaybeCachedZip(module))
                {
                    User.RaiseMessage(" * {0} {1} ({2}, {3})",
                        module.name,
                        module.version,
                        module.download.Host,
                        CkanModule.FmtSize(module.download_size)
                    );
                    downloads.Add(module);
                }
                else
                {
                    User.RaiseMessage(" * {0} {1} (cached)", module.name, module.version);
                }
            }

            if (ConfirmPrompt && !User.RaiseYesNoDialog("Continue?"))
            {
                throw new CancelledActionKraken("User declined install list");
            }

            if (downloads.Count > 0)
            {
                if (downloader == null)
                {
                    downloader = new NetAsyncModulesDownloader(User, Cache);
                }

                downloader.DownloadModules(downloads);
            }

            // We're about to install all our mods; so begin our transaction.
            using (TransactionScope transaction = CkanTransaction.CreateTransactionScope())
            {
                for (int i = 0; i < modsToInstall.Count; i++)
                {
                    // The post-install steps start at 70%, so count up to 60% for installation
                    int percent_complete = (i * 60) / modsToInstall.Count;

                    User.RaiseProgress(String.Format("Installing mod \"{0}\"", modsToInstall[i]),
                                         percent_complete);

                    Install(modsToInstall[i], resolver.IsAutoInstalled(modsToInstall[i]), registry_manager.registry, ref possibleConfigOnlyDirs);
                }

                User.RaiseProgress("Updating registry", 70);

                registry_manager.Save(!options.without_enforce_consistency);

                User.RaiseProgress("Committing filesystem changes", 80);

                transaction.Complete();

            }

            EnforceCacheSizeLimit(registry_manager.registry);

            if (!options.without_enforce_consistency)
            {
                // We can scan GameData as a separate transaction. Installing the mods
                // leaves everything consistent, and this is just gravy. (And ScanGameData
                // acts as a Tx, anyway, so we don't need to provide our own.)
                User.RaiseProgress("Rescanning GameData", 90);
                log.Debug("Scanning after install");
                ksp.Scan();
            }

            User.RaiseProgress("Done!", 100);
        }

        /// <summary>
        /// Returns the module contents if and only if we have it
        /// available in our cache. Returns null, otherwise.
        ///
        /// Intended for previews.
        /// </summary>
        public IEnumerable<string> GetModuleContentsList(CkanModule module)
        {
            string filename = Cache.GetCachedFilename(module);

            if (filename == null)
            {
                return null;
            }

            try
            {
                return FindInstallableFiles(module, filename, ksp)
                    .Select(x => ksp.ToRelativeGameDir(x.destination));
            }
            catch (ZipException)
            {
                return null;
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
        private void Install(CkanModule module, bool autoInstalled, Registry registry, ref HashSet<string> possibleConfigOnlyDirs, string filename = null)
        {
            CheckKindInstallationKraken(module);

            ModuleVersion version = registry.InstalledVersion(module.identifier);

            // TODO: This really should be handled by higher-up code.
            if (version != null && !(version is UnmanagedModuleVersion))
            {
                User.RaiseMessage("    {0} {1} already installed, skipped", module.identifier, version);
                return;
            }

            // Find ZIP in the cache if we don't already have it.
            filename = filename ?? Cache.GetCachedZip(module);

            // If we *still* don't have a file, then kraken bitterly.
            if (filename == null)
            {
                throw new FileNotFoundKraken(
                    null,
                    String.Format("Trying to install {0}, but it's not downloaded or download is corrupted", module)
                );
            }

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                // Install all the things!
                IEnumerable<string> files = InstallModule(module, filename, registry, ref possibleConfigOnlyDirs);

                // Register our module and its files.
                registry.RegisterModule(module, files, ksp, autoInstalled);

                // Finish our transaction, but *don't* save the registry; we may be in an
                // intermediate, inconsistent state.
                // This is fine from a transaction standpoint, as we may not have an enclosing
                // transaction, and if we do, they can always roll us back.
                transaction.Complete();
            }

            // Fire our callback that we've installed a module, if we have one.
            onReportModInstalled?.Invoke(module);

        }

        /// <summary>
        /// Check if the given module is a metapackage:
        /// if it is, throws a BadCommandKraken.
        /// </summary>
        private static void CheckKindInstallationKraken(CkanModule module)
        {
            if (module.IsMetapackage)
            {
                throw new BadCommandKraken("Metapackages cannot be installed!");
            }
            if (module.IsDLC)
            {
                throw new BadCommandKraken("DLC cannot be installed!");
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
        private IEnumerable<string> InstallModule(CkanModule module, string zip_filename, Registry registry, ref HashSet<string> possibleConfigOnlyDirs)
        {
            CheckKindInstallationKraken(module);
            var createdPaths = new List<string>();

            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                var filters = ServiceLocator.Container.Resolve<IConfiguration>().GlobalInstallFilters
                    .Concat(ksp.InstallFilters)
                    .ToHashSet();
                var files = FindInstallableFiles(module, zipfile, ksp)
                    .Where(instF => !filters.Any(filt =>
                        instF.destination.Contains(filt)))
                    .ToList();

                try
                {
                    var dll = registry.DllPath(module.identifier);
                    if (!string.IsNullOrEmpty(dll))
                    {
                        // Find where we're installing identifier.optionalversion.dll
                        // (file name might not be an exact match with manually installed)
                        var dllFolders = files.Where(f =>
                                Path.GetFileName(f.destination).StartsWith(module.identifier)
                                    && f.destination.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                            .Select(f => Path.GetDirectoryName(ksp.ToRelativeGameDir(f.destination)))
                            .ToHashSet();
                        if (!dllFolders.Contains(Path.GetDirectoryName(dll)))
                        {
                            // Manually installed DLL is somewhere else where we're not installing files,
                            // probable bad install, alert user and abort
                            throw new DllLocationMismatchKraken(dll, $"DLL for module {module.identifier} found at {dll}, but it's not where CKAN would install it. Aborting to prevent multiple copies of the same mod being installed. To install this module, uninstall it manually and try again.");
                        }
                        // Delete the manually installed DLL transaction-style because we believe we'll be replacing it
                        var toDelete = ksp.ToAbsoluteGameDir(dll);
                        log.DebugFormat("Deleting manually installed DLL {0}", toDelete);
                        TxFileManager file_transaction = new TxFileManager();
                        file_transaction.Snapshot(toDelete);
                        file_transaction.Delete(toDelete);
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
                                    $"{a}\r\n- {ksp.ToRelativeGameDir(b.Key.destination)}  ({(b.Value ? "same" : "DIFFERENT")})");
                            if (User.RaiseYesNoDialog($"Module {module.name} wants to overwrite the following manually installed files:\r\n{fileMsg}\r\n\r\nOverwrite?"))
                            {
                                DeleteConflictingFiles(conflicting.Select(f => f.Key));
                            }
                            else
                            {
                                throw new CancelledActionKraken($"Not overwriting manually installed files, can't install {module.name}.");
                            }
                        }
                    }
                    foreach (InstallableFile file in files)
                    {
                        log.DebugFormat("Copying {0}", file.source.Name);
                        createdPaths.Add(CopyZipEntry(zipfile, file.source, file.destination, file.makedir));
                        if (file.source.IsDirectory && possibleConfigOnlyDirs != null)
                        {
                            possibleConfigOnlyDirs.Remove(file.destination);
                        }
                    }
                    log.InfoFormat("Installed {0}", module);
                }
                catch (FileExistsKraken kraken)
                {
                    // Decorate the kraken with our module and re-throw
                    kraken.filename = ksp.ToRelativeGameDir(kraken.filename);
                    kraken.installingModule = module;
                    kraken.owningModule = registry.FileOwner(kraken.filename);
                    throw;
                }
            }
            return createdPaths.Where(p => p != null);
        }

        /// <summary>
        /// Find files in the given list that are already installed and unowned.
        /// Note, this compares files on demand; Memoize for performance!
        /// </summary>
        /// <param name="files">Files that we want to install for a module</param>
        /// <returns>
        /// List of pairs: Key = file, Value = true if identical, false if different
        /// </returns>
        private IEnumerable<KeyValuePair<InstallableFile, bool>> FindConflictingFiles(ZipFile zip, IEnumerable<InstallableFile> files, Registry registry)
        {
            foreach (InstallableFile file in files)
            {
                if (!file.source.IsDirectory
                    && File.Exists(file.destination)
                    && registry.FileOwner(ksp.ToRelativeGameDir(file.destination)) == null)
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
        private bool StreamsEqual(Stream s1, Stream s2)
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
        private void DeleteConflictingFiles(IEnumerable<InstallableFile> files)
        {
            TxFileManager file_transaction = new TxFileManager();
            foreach (InstallableFile file in files)
            {
                log.DebugFormat("Trying to delete {0}", file.destination);
                file_transaction.Delete(file.destination);
            }
        }

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
        /// Copy the entry from the opened zipfile to the path specified.
        /// </summary>
        /// <returns>
        /// Path of file or directory that was created.
        /// May differ from the input fullPath!
        /// </returns>
        internal static string CopyZipEntry(ZipFile zipfile, ZipEntry entry, string fullPath, bool makeDirs)
        {
            TxFileManager file_transaction = new TxFileManager();

            if (entry.IsDirectory)
            {
                // Skip if we're not making directories for this install.
                if (!makeDirs)
                {
                    log.DebugFormat("Skipping '{0}', we don't make directories for this path", fullPath);
                    return null;
                }

                // Windows silently trims trailing spaces, get the path it will actually use
                fullPath = CKANPathUtils.NormalizePath(Path.GetDirectoryName(
                    Path.Combine(fullPath, "DUMMY")));

                log.DebugFormat("Making directory '{0}'", fullPath);
                file_transaction.CreateDirectory(fullPath);
            }
            else
            {
                log.DebugFormat("Writing file '{0}'", fullPath);

                // ZIP format does not require directory entries
                if (makeDirs)
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    log.DebugFormat("Making parent directory '{0}'", directory);
                    file_transaction.CreateDirectory(directory);
                }

                // We don't allow for the overwriting of files. See #208.
                if (file_transaction.FileExists(fullPath))
                {
                    throw new FileExistsKraken(fullPath, string.Format("Trying to write '{0}' but it already exists.", fullPath));
                }

                // Snapshot whatever was there before. If there's nothing, this will just
                // remove our file on rollback. We still need this even though we won't
                // overwite files, as it ensures deletion on rollback.
                file_transaction.Snapshot(fullPath);

                try
                {
                    // It's a file! Prepare the streams
                    using (Stream zipStream = zipfile.GetInputStream(entry))
                    using (FileStream writer = File.Create(fullPath))
                    {
                        // Windows silently changes paths ending with spaces, get the name it actually used
                        fullPath = CKANPathUtils.NormalizePath(writer.Name);
                        // 4k is the block size on practically every disk and OS.
                        byte[] buffer = new byte[4096];
                        StreamUtils.Copy(zipStream, writer, buffer);
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

        /// <summary>
        /// Uninstalls all the mods provided, including things which depend upon them.
        /// This *DOES* save the registry.
        /// Preferred over Uninstall.
        /// </summary>
        public void UninstallList(
            IEnumerable<string> mods, ref HashSet<string> possibleConfigOnlyDirs,
            RegistryManager registry_manager, bool ConfirmPrompt = true, IEnumerable<CkanModule> installing = null
        )
        {
            mods = mods.Memoize();
            // Pre-check, have they even asked for things which are installed?

            foreach (string mod in mods.Where(mod => registry_manager.registry.InstalledModule(mod) == null))
            {
                throw new ModNotInstalledKraken(mod);
            }

            var instDlc = mods
                .Select(ident => registry_manager.registry.InstalledModule(ident))
                .FirstOrDefault(m => m.Module.IsDLC);
            if (instDlc != null)
            {
                throw new ModuleIsDLCKraken(instDlc.Module);
            }

            // Find all the things which need uninstalling.
            IEnumerable<string> revdep = mods
                .Union(registry_manager.registry.FindReverseDependencies(
                    mods.Except(installing?.Select(m => m.identifier) ?? new string[] {}),
                    installing
                )).Memoize();
            var goners = revdep.Union(
                    registry_manager.registry.FindRemovableAutoInstalled(
                        registry_manager.registry.InstalledModules
                            .Where(im => !revdep.Contains(im.identifier))
                            .Concat(installing?.Select(m => new InstalledModule(null, m, new string[0], false)) ?? new InstalledModule[0]))
                    .Select(im => im.identifier))
                .ToList();

            // If there us nothing to uninstall, skip out.
            if (!goners.Any())
            {
                return;
            }

            User.RaiseMessage("About to remove:\r\n");

            foreach (string mod in goners)
            {
                InstalledModule module = registry_manager.registry.InstalledModule(mod);
                User.RaiseMessage(" * {0} {1}", module.Module.name, module.Module.version);
            }

            if (ConfirmPrompt && !User.RaiseYesNoDialog("Continue?"))
            {
                throw new CancelledActionKraken("Mod removal aborted at user request");
            }

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                int step = 0;
                foreach (string mod in goners)
                {
                    int percent_complete = (step++ * 100) / goners.Count;
                    User.RaiseProgress($"Removing {mod}...", percent_complete);
                    Uninstall(mod, ref possibleConfigOnlyDirs, registry_manager.registry);
                }

                // Enforce consistency if we're not installing anything,
                // otherwise consistency will be enforced after the installs
                registry_manager.Save(installing == null);

                transaction.Complete();
            }

            User.RaiseProgress("Done!", 100);
        }

        /// <summary>
        /// Uninstall the module provided. For internal use only.
        /// Use UninstallList for user queries, it also does dependency handling.
        /// This does *NOT* save the registry.
        /// </summary>
        /// <param name="modName">Identifier of module to uninstall</param>
        /// <param name="possibleConfigOnlyDirs">Directories that the user might want to remove after uninstall</param>
        private void Uninstall(string modName, ref HashSet<string> possibleConfigOnlyDirs, Registry registry)
        {
            TxFileManager file_transaction = new TxFileManager();

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                InstalledModule mod = registry.InstalledModule(modName);

                if (mod == null)
                {
                    log.ErrorFormat("Trying to uninstall {0} but it's not installed", modName);
                    throw new ModNotInstalledKraken(modName);
                }

                // Walk our registry to find all files for this mod.
                IEnumerable<string> files = mod.Files;

                // We need case insensitive path matching on Windows
                var directoriesToDelete = Platform.IsWindows
                    ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>();

                foreach (string file in files)
                {
                    string path = ksp.ToAbsoluteGameDir(file);

                    try
                    {
                        FileAttributes attr = File.GetAttributes(path);

                        // [This is] bitwise math. Basically, attr is some binary value with one bit meaning
                        // "this is a directory". The bitwise and & operator will return a binary value where
                        // only the bits that are on (1) in both the operands are turned on. In this case
                        // doing a bitwise and operation against attr and the FileAttributes.Directory value
                        // will return the value of FileAttributes.Directory if the Directory file attribute
                        // bit is turned on. See en.wikipedia.org/wiki/Bitwise_operation for a better
                        // explanation. – Kyle Trauberman Aug 30 '12 at 21:28
                        // (https://stackoverflow.com/questions/1395205/better-way-to-check-if-path-is-a-file-or-a-directory)
                        // This is the fastest way to do this test.
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if (!directoriesToDelete.Contains(path))
                            {
                                directoriesToDelete.Add(path);
                            }
                        }
                        else
                        {
                            // Add this file's directory to the list for deletion if it isn't already there.
                            // Helps clean up directories when modules are uninstalled out of dependency order
                            // Since we check for directory contents when deleting, this should purge empty
                            // dirs, making less ModuleManager headaches for people.
                            var directoryName = Path.GetDirectoryName(path);
                            if (!(directoriesToDelete.Contains(directoryName)))
                            {
                                directoriesToDelete.Add(directoryName);
                            }

                            log.DebugFormat("Removing {0}", file);
                            file_transaction.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        // XXX: This is terrible, we're catching all exceptions.
                        // We don't consider this problem serious enough to abort and revert,
                        // so treat it as a "--verbose" level log message.
                        log.InfoFormat("Failure in locating file {0} : {1}", path, ex.Message);
                    }
                }

                // Remove from registry.
                registry.DeregisterModule(ksp, modName);

                // Our collection of directories may leave empty parent directories.
                directoriesToDelete = AddParentDirectories(directoriesToDelete);

                // Sort our directories from longest to shortest, to make sure we remove child directories
                // before parents. GH #78.
                foreach (string directory in directoriesToDelete.OrderBy(dir => dir.Length).Reverse())
                {
                    log.DebugFormat("Checking {0}...", directory);
                    // It is bad if any of this directories gets removed
                    // So we protect them
                    // A few string comparisons will be cheaper than hitting the disk, so do this first
                    if (ksp.game.IsReservedDirectory(ksp, directory))
                    {
                        log.DebugFormat("Directory {0} is reserved, skipping", directory);
                        continue;
                    }

                    var contents = Directory
                        .EnumerateFileSystemEntries(directory, "*", SearchOption.AllDirectories)
                        .Select(f => ksp.ToRelativeGameDir(f))
                        .Memoize();
                    log.DebugFormat("Got contents: {0}", string.Join(", ", contents));
                    var owners = contents.Select(f => registry.FileOwner(f));
                    log.DebugFormat("Got owners: {0}", string.Join(", ", owners));
                    if (!contents.Any())
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
                    else if (contents.All(f => registry.FileOwner(f) == null))
                    {
                        log.DebugFormat("Directory {0} contains only non-registered files, ask user about it later", directory);
                        if (possibleConfigOnlyDirs == null)
                        {
                            possibleConfigOnlyDirs = new HashSet<string>();
                        }
                        possibleConfigOnlyDirs.Add(directory);
                    }
                    else
                    {
                        log.InfoFormat("Not removing directory {0}, it's not empty", directory);
                    }
                }
                log.InfoFormat("Removed {0}", modName);
                transaction.Complete();
            }
        }

        /// <summary>
        /// Takes a collection of directories and adds all parent directories within the GameData structure.
        /// </summary>
        /// <param name="directories">The collection of directory path strings to examine</param>
        public HashSet<string> AddParentDirectories(HashSet<string> directories)
        {
            if (directories == null || directories.Count == 0)
            {
                return new HashSet<string>();
            }

            var gameDir = CKANPathUtils.NormalizePath(ksp.GameDir());
            return directories
                .Where(dir => !string.IsNullOrWhiteSpace(dir))
                // Normalize all paths before deduplicate
                .Select(CKANPathUtils.NormalizePath)
                // Remove any duplicate paths
                .Distinct()
                .SelectMany(dir =>
                {
                    var results = new HashSet<string>();
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

                    if (!dir.StartsWith(gameDir, StringComparison.CurrentCultureIgnoreCase))
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
                .Where(dir => !ksp.game.IsReservedDirectory(ksp, dir))
                .ToHashSet();
        }

        #region AddRemove

        /// <summary>
        /// Adds and removes the listed modules as a single transaction.
        /// No relationships will be processed.
        /// This *will* save the registry.
        /// </summary>
        /// <param name="add">Add.</param>
        /// <param name="remove">Remove.</param>
        public void AddRemove(ref HashSet<string> possibleConfigOnlyDirs, RegistryManager registry_manager, IEnumerable<CkanModule> add = null, IEnumerable<InstalledModule> remove = null, bool enforceConsistency = true)
        {
            // TODO: We should do a consistency check up-front, rather than relying
            // upon our registry catching inconsistencies at the end.

            using (var tx = CkanTransaction.CreateTransactionScope())
            {
                remove = remove.Memoize();
                add    = add.Memoize();
                int totSteps = (remove?.Count() ?? 0)
                             + (add?.Count()    ?? 0);
                int step = 0;
                foreach (InstalledModule instMod in remove)
                {
                    // The post-install steps start at 80%, so count up to 70% for installation
                    int percent_complete = (step++ * 70) / totSteps;
                    User.RaiseProgress($"Removing \"{instMod.Module}\"", percent_complete);
                    Uninstall(instMod.Module.identifier, ref possibleConfigOnlyDirs, registry_manager.registry);
                }

                foreach (CkanModule module in add)
                {
                    var previous = remove?.FirstOrDefault(im => im.Module.identifier == module.identifier);
                    int percent_complete = (step++ * 70) / totSteps;
                    User.RaiseProgress($"Installing \"{module}\"", percent_complete);
                    Install(module, previous?.AutoInstalled ?? false, registry_manager.registry, ref possibleConfigOnlyDirs);
                }

                User.RaiseProgress("Updating registry", 80);
                registry_manager.Save(enforceConsistency);

                User.RaiseProgress("Committing filesystem changes", 90);
                tx.Complete();

                EnforceCacheSizeLimit(registry_manager.registry);
            }
        }

        /// <summary>
        /// Upgrades the mods listed to the latest versions for the user's KSP.
        /// Will *re-install* with warning even if an upgrade is not available.
        /// Throws ModuleNotFoundKraken if module is not installed, or not available.
        /// </summary>
        public void Upgrade(IEnumerable<string> identifiers, IDownloader netAsyncDownloader, ref HashSet<string> possibleConfigOnlyDirs, RegistryManager registry_manager, bool enforceConsistency = true)
        {
            // When upgrading, we are removing these mods first and install them again afterwards (but in different versions).
            // So the list of identifiers of modulesToRemove and modulesToInstall is the same,
            // RelationshipResolver take care of finding the right CkanModule for each identifier.
            List<string> identifierList = identifiers.ToList();
            var resolver = new RelationshipResolver(
                identifierList,
                identifierList,
                RelationshipResolver.DependsOnlyOpts(),
                registry_manager.registry, ksp.VersionCriteria()
            );
            Upgrade(resolver.ModList(), netAsyncDownloader, ref possibleConfigOnlyDirs, registry_manager, enforceConsistency);
        }

        /// <summary>
        /// Upgrades or installs the mods listed to the specified versions for the user's KSP.
        /// Will *re-install* or *downgrade* (with a warning) as well as upgrade.
        /// Throws ModuleNotFoundKraken if a module is not installed.
        /// </summary>
        public void Upgrade(IEnumerable<CkanModule> modules, IDownloader netAsyncDownloader, ref HashSet<string> possibleConfigOnlyDirs, RegistryManager registry_manager, bool enforceConsistency = true, bool resolveRelationships = false, bool ConfirmPrompt = true)
        {
            modules = modules.Memoize();

            if (resolveRelationships)
            {
                var resolver = new RelationshipResolver(
                    modules,
                    modules.Select(m => registry_manager.registry.InstalledModule(m.identifier)?.Module).Where(m => m != null),
                    RelationshipResolver.DependsOnlyOpts(),
                    registry_manager.registry,
                    ksp.VersionCriteria()
                );
                modules = resolver.ModList();
            }

            User.RaiseMessage("About to upgrade:\r\n");

            // Our upgrade involves removing everything that's currently installed, then
            // adding everything that needs installing (which may involve new mods to
            // satisfy dependencies). We always know the list passed in is what we need to
            // install, but we need to calculate what needs to be removed.
            var to_remove = new List<InstalledModule>();

            // Let's discover what we need to do with each module!
            foreach (CkanModule module in modules)
            {
                InstalledModule installed_mod = registry_manager.registry.InstalledModule(module.identifier);

                if (installed_mod == null)
                {
                    if (!Cache.IsMaybeCachedZip(module))
                    {
                        User.RaiseMessage(" * Install: {0} {1} ({2}, {3})",
                            module.name,
                            module.version,
                            module.download.Host,
                            CkanModule.FmtSize(module.download_size)
                        );
                    }
                    else
                    {
                        User.RaiseMessage(" * Install: {0} {1} (cached)",
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
                        User.RaiseMessage(" * Re-install: {0} {1}",
                            module.name, module.version);
                    }
                    else if (installed.version.IsGreaterThan(module.version))
                    {
                        User.RaiseMessage(" * Downgrade: {0} from {1} to {2}",
                            module.name, installed.version, module.version);
                    }
                    else
                    {
                        if (!Cache.IsMaybeCachedZip(module))
                        {
                            User.RaiseMessage(" * Upgrade: {0} {1} to {2} ({3}, {4})",
                                module.name,
                                installed.version,
                                module.version,
                                module.download.Host,
                                CkanModule.FmtSize(module.download_size)
                            );
                        }
                        else
                        {
                            User.RaiseMessage(" * Upgrade: {0} {1} to {2} (cached)",
                                module.name, installed.version, module.version);
                        }
                    }
                }
            }

            if (ConfirmPrompt && !User.RaiseYesNoDialog("Continue?"))
            {
                throw new CancelledActionKraken("User declined upgrade list");
            }

            // Start by making sure we've downloaded everything.
            DownloadModules(modules, netAsyncDownloader);

            AddRemove(
                ref possibleConfigOnlyDirs,
                registry_manager,
                modules,
                to_remove,
                enforceConsistency
            );
            User.RaiseProgress("Done!", 100);
        }

        /// <summary>
        /// Enacts listed Module Replacements to the specified versions for the user's KSP.
        /// Will *re-install* or *downgrade* (with a warning) as well as upgrade.
        /// </summary>
        /// <exception cref="DependencyNotSatisfiedKraken">Thrown if a dependency for a replacing module couldn't be satisfied.</exception>
        /// <exception cref="ModuleNotFoundKraken">Thrown if a module that should be replaced is not installed.</exception>
        public void Replace(IEnumerable<ModuleReplacement> replacements, RelationshipResolverOptions options, IDownloader netAsyncDownloader, ref HashSet<string> possibleConfigOnlyDirs, RegistryManager registry_manager, bool enforceConsistency = true)
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
            // Start by making sure we've downloaded everything.
            DownloadModules(modsToInstall, netAsyncDownloader);

            // Our replacement involves removing the currently installed mods, then
            // adding everything that needs installing (which may involve new mods to
            // satisfy dependencies).


            // Let's discover what we need to do with each module!
            foreach (ModuleReplacement repl in replacements)
            {
                string ident = repl.ToReplace.identifier;
                InstalledModule installedMod = registry_manager.registry.InstalledModule(ident);

                if (installedMod == null)
                {
                    log.DebugFormat("Wait, {0} is not actually installed?", installedMod.identifier);
                    //Maybe ModuleNotInstalled ?
                    if (registry_manager.registry.IsAutodetected(ident))
                    {
                        throw new ModuleNotFoundKraken(ident, repl.ToReplace.version.ToString(), String.Format("Can't replace {0} as it was not installed by CKAN. \r\n Please remove manually before trying to install it.", ident));
                    }

                    throw new ModuleNotFoundKraken(ident, repl.ToReplace.version.ToString(), String.Format("Can't replace {0} as it is not installed. Please attempt to install {1} instead.", ident, repl.ReplaceWith.identifier));
                }
                else
                {
                    // Obviously, we need to remove the mod we are replacing
                    modsToRemove.Add(installedMod);

                    log.DebugFormat("Ok, we are removing {0}", repl.ToReplace.identifier);
                    //Check whether our Replacement target is already installed
                    InstalledModule installed_replacement = registry_manager.registry.InstalledModule(repl.ReplaceWith.identifier);

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
            var resolver = new RelationshipResolver(modsToInstall, null, options, registry_manager.registry, ksp.VersionCriteria());
            var resolvedModsToInstall = resolver.ModList().ToList();
            AddRemove(
                ref possibleConfigOnlyDirs,
                registry_manager,
                resolvedModsToInstall,
                modsToRemove,
                enforceConsistency
            );
            User.RaiseProgress("Done!", 100);
        }

        #endregion

        /// <summary>
        /// Makes sure all the specified mods are downloaded.
        /// </summary>
        private void DownloadModules(IEnumerable<CkanModule> mods, IDownloader downloader)
        {
            List<CkanModule> downloads = mods.Where(module => !Cache.IsCachedZip(module)).ToList();

            if (downloads.Count > 0)
            {
                downloader.DownloadModules(downloads);
            }
        }

        /// <summary>
        /// Looks for optional related modules that could be installed alongside the given modules
        /// </summary>
        /// <param name="sourceModules">Modules to check for relationships</param>
        /// <param name="toInstall">Modules already being installed, to be omitted from search</param>
        /// <param name="recommendations">Modules that are recommended to install</param>
        /// <param name="suggestions">Modules that are suggested to install</param>
        /// <param name="supporters">Modules that support other modules we're installing</param>
        /// <returns>
        /// true if anything found, false otherwise
        /// </returns>
        public bool FindRecommendations(
            HashSet<CkanModule> sourceModules,
            HashSet<CkanModule> toInstall,
            Registry registry,
            out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
            out Dictionary<CkanModule, List<string>> suggestions,
            out Dictionary<CkanModule, HashSet<string>> supporters
        )
        {
            Dictionary<CkanModule, List<string>> dependersIndex = getDependersIndex(sourceModules, registry, toInstall);
            var instList = toInstall.ToList();
            recommendations = new Dictionary<CkanModule, Tuple<bool, List<string>>>();
            suggestions = new Dictionary<CkanModule, List<string>>();
            supporters = new Dictionary<CkanModule, HashSet<string>>();
            foreach (CkanModule mod in sourceModules.Where(m => m.recommends != null))
            {
                foreach (RelationshipDescriptor rel in mod.recommends)
                {
                    List<CkanModule> providers = rel.LatestAvailableWithProvides(
                        registry,
                        ksp.VersionCriteria()
                    );
                    int i = 0;
                    foreach (CkanModule provider in providers)
                    {
                        if (!registry.IsInstalled(provider.identifier)
                            && !toInstall.Any(m => m.identifier == provider.identifier)
                            && dependersIndex.TryGetValue(provider, out List<string> dependers)
                            && (provider.IsDLC || CanInstall(RelationshipResolver.DependsOnlyOpts(),
                                instList.Concat(new List<CkanModule>() { provider }).ToList(), registry)))
                        {
                            dependersIndex.Remove(provider);
                            recommendations.Add(
                                provider,
                                new Tuple<bool, List<string>>(
                                    !provider.IsDLC && (i == 0 || provider.identifier == (rel as ModuleRelationshipDescriptor)?.name),
                                    dependers)
                            );
                            ++i;
                        }
                    }
                }
            }
            foreach (CkanModule mod in sourceModules.Where(m => m.suggests != null))
            {
                foreach (RelationshipDescriptor rel in mod.suggests)
                {
                    List<CkanModule> providers = rel.LatestAvailableWithProvides(
                        registry,
                        ksp.VersionCriteria()
                    );
                    foreach (CkanModule provider in providers)
                    {
                        if (!registry.IsInstalled(provider.identifier)
                            && !toInstall.Any(m => m.identifier == provider.identifier)
                            && dependersIndex.TryGetValue(provider, out List<string> dependers)
                            && (provider.IsDLC || CanInstall(RelationshipResolver.DependsOnlyOpts(),
                                instList.Concat(new List<CkanModule>() { provider }).ToList(), registry)))
                        {
                            dependersIndex.Remove(provider);
                            suggestions.Add(provider, dependers);
                        }
                    }
                }
            }

            // Find installable modules with "supports" relationships
            var candidates = registry.CompatibleModules(ksp.VersionCriteria())
                .Where(mod => !registry.IsInstalled(mod.identifier)
                    && !toInstall.Any(m => m.identifier == mod.identifier))
                .Where(m => m?.supports != null)
                .Except(recommendations.Keys)
                .Except(suggestions.Keys);
            // Find each module that "supports" something we're installing
            foreach (CkanModule mod in candidates)
            {
                foreach (RelationshipDescriptor rel in mod.supports)
                {
                    if (rel.MatchesAny(sourceModules, null, null))
                    {
                        var name = (rel as ModuleRelationshipDescriptor)?.name;
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (supporters.TryGetValue(mod, out HashSet<string> others))
                            {
                                others.Add(name);
                            }
                            else
                            {
                                supporters.Add(mod, new HashSet<string>() { name });
                            }
                        }
                    }
                }
            }
            supporters.RemoveWhere(kvp => !CanInstall(
                RelationshipResolver.DependsOnlyOpts(),
                instList.Concat(new List<CkanModule>() { kvp.Key }).ToList(),
                registry
            ));

            return recommendations.Any() || suggestions.Any() || supporters.Any();
        }

        // Build up the list of who recommends what
        private Dictionary<CkanModule, List<string>> getDependersIndex(
            IEnumerable<CkanModule> sourceModules,
            IRegistryQuerier        registry,
            HashSet<CkanModule>     toExclude
        )
        {
            Dictionary<CkanModule, List<string>> dependersIndex = new Dictionary<CkanModule, List<string>>();
            foreach (CkanModule mod in sourceModules)
            {
                foreach (List<RelationshipDescriptor> relations in new List<List<RelationshipDescriptor>>() { mod.recommends, mod.suggests })
                {
                    if (relations != null)
                    {
                        foreach (RelationshipDescriptor rel in relations)
                        {
                            List<CkanModule> providers = rel.LatestAvailableWithProvides(
                                registry,
                                ksp.VersionCriteria()
                            );
                            foreach (CkanModule provider in providers)
                            {
                                if (!registry.IsInstalled(provider.identifier)
                                    && !toExclude.Any(m => m.identifier == provider.identifier))
                                {
                                    if (dependersIndex.TryGetValue(provider, out List<string> dependers))
                                    {
                                        // Add the dependent mod to the list of reasons this dependency is shown.
                                        dependers.Add(mod.identifier);
                                    }
                                    else
                                    {
                                        // Add a new entry if this provider isn't listed yet.
                                        dependersIndex.Add(provider, new List<string>() { mod.identifier });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dependersIndex;
        }

        /// <summary>
        /// Determine whether there is any way to install the given set of mods.
        /// Handles virtual dependencies, including recursively.
        /// </summary>
        /// <param name="opts">Installer options</param>
        /// <param name="toInstall">Mods we want to install</param>
        /// <param name="registry">Registry of instance into which we want to install</param>
        /// <returns>
        /// True if it's possible to install these mods, false otherwise
        /// </returns>
        public bool CanInstall(
            RelationshipResolverOptions opts,
            List<CkanModule>            toInstall,
            IRegistryQuerier            registry
        )
        {
            string request = toInstall.Select(m => m.identifier).Aggregate((a, b) => $"{a}, {b}");
            try
            {
                RelationshipResolver resolver = new RelationshipResolver(
                    toInstall,
                    toInstall.Select(m => registry.InstalledModule(m.identifier)?.Module).Where(m => m != null),
                    opts, registry, ksp.VersionCriteria()
                );

                if (resolver.ModList().Count() >= toInstall.Count(m => !m.IsMetapackage))
                {
                    // We can install with no further dependencies
                    string recipe = resolver.ModList()
                        .Select(m => m.identifier)
                        .Aggregate((a, b) => $"{a}, {b}");
                    log.Debug($"Installable: {request}: {recipe}");
                    return true;
                }
                else
                {
                    string problems = resolver.ConflictList.Values
                        .Aggregate((a, b) => $"{a}, {b}");
                    log.Debug($"Can't install {request}: {problems}");
                    return false;
                }
            }
            catch (TooManyModsProvideKraken k)
            {
                // One of the dependencies is virtual
                foreach (CkanModule mod in k.modules)
                {
                    // Try each option recursively to see if any are successful
                    if (CanInstall(opts, toInstall.Concat(new List<CkanModule>() { mod }).ToList(), registry))
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

        /// <summary>
        /// Import a list of files into the download cache, with progress bar and
        /// interactive prompts for installation and deletion.
        /// </summary>
        /// <param name="files">Set of files to import</param>
        /// <param name="user">Object for user interaction</param>
        /// <param name="installMod">Function to call to mark a mod for installation</param>
        /// <param name="allowDelete">True to ask user whether to delete imported files, false to leave the files as is</param>
        public void ImportFiles(HashSet<FileInfo> files, IUser user, Action<CkanModule> installMod, Registry registry, bool allowDelete = true)
        {
            HashSet<CkanModule> installable = new HashSet<CkanModule>();
            List<FileInfo>      deletable   = new List<FileInfo>();
            // Get the mapping of known hashes to modules
            Dictionary<string, List<CkanModule>> index = registry.GetSha1Index();
            int i = 0;
            foreach (FileInfo f in files)
            {
                int percent = i * 100 / files.Count;
                user.RaiseProgress($"Importing {f.Name}... ({percent}%)", percent);
                // Calc SHA-1 sum
                string sha1 = Cache.GetFileHashSha1(f.FullName);
                // Find SHA-1 sum in registry (potentially multiple)
                if (index.ContainsKey(sha1))
                {
                    deletable.Add(f);
                    List<CkanModule> matches = index[sha1];
                    foreach (CkanModule mod in matches)
                    {
                        if (mod.IsCompatibleKSP(ksp.VersionCriteria()))
                        {
                            installable.Add(mod);
                        }
                        if (Cache.IsMaybeCachedZip(mod))
                        {
                            user.RaiseMessage("Already cached: {0}", f.Name);
                        }
                        else
                        {
                            user.RaiseMessage($"Importing {mod.identifier} {StripEpoch(mod.version)}...");
                            Cache.Store(mod, f.FullName);
                        }
                    }
                }
                else
                {
                    user.RaiseMessage("Not found in index: {0}", f.Name);
                }
                ++i;
            }
            if (installable.Count > 0 && user.RaiseYesNoDialog($"Install {installable.Count} compatible imported mods in game instance {ksp.Name} ({ksp.GameDir()})?"))
            {
                // Install the imported mods
                foreach (CkanModule mod in installable)
                {
                    installMod(mod);
                }
            }
            if (allowDelete && deletable.Count > 0 && user.RaiseYesNoDialog($"Import complete. Delete {deletable.Count} old files?"))
            {
                // Delete old files
                foreach (FileInfo f in deletable)
                {
                    f.Delete();
                }
            }

            EnforceCacheSizeLimit(registry);
        }

        private void EnforceCacheSizeLimit(Registry registry)
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
        {
            Match match = Regex.Match(version, @"^(?<num>\d\:)?[vV]+(ersion)?[_.]*(?<ver>\d.*)$");

            if (match.Success)
                return match.Groups["num"].Value + match.Groups["ver"].Value;
            else
                return version;
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version that might contain an epoch</param>
        public static string StripEpoch(ModuleVersion version)
        {
            return StripEpoch(version.ToString());
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string StripEpoch(string version)
        {
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            return epochMatch.IsMatch(version)
                ? epochReplace.Replace(version, @"$2")
                : version;
        }

        /// <summary>
        /// As above, but includes the original in parentheses
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string WithAndWithoutEpoch(string version)
        {
            return epochMatch.IsMatch(version)
                ? $"{epochReplace.Replace(version, @"$2")} ({version})"
                : version;
        }

        private static readonly Regex epochMatch   = new Regex(@"^[0-9][0-9]*:[^:]+$", RegexOptions.Compiled);
        private static readonly Regex epochReplace = new Regex(@"^([^:]+):([^:]+)$",   RegexOptions.Compiled);
    }
}
