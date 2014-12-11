using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ChinhDo.Transactions;
using log4net;

namespace CKAN
{
    public delegate void ModuleInstallerReportProgress(string message, int progress);

    public delegate void ModuleInstallerReportModInstalled(CkanModule module);

    public struct InstallableFile 
    {
        public ZipEntry source;
        public string destination;
        public bool makedir;
    }

    public class ModuleInstaller
    {
        private static ModuleInstaller _Instance;

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));
        private static readonly TxFileManager file_transaction = new TxFileManager ();

        private RegistryManager registry_manager;
        private KSP ksp;

        public ModuleInstallerReportModInstalled onReportModInstalled = null;
        public ModuleInstallerReportProgress onReportProgress = null;

        // Our own cache is that of the KSP instance we're using.
        public NetFileCache Cache
        {
            get
            {
                return ksp.Cache;
            }
        }

        // Constructor
        private ModuleInstaller(KSP ksp)
        {
            this.ksp = ksp;
            this.registry_manager = RegistryManager.Instance(ksp);
            log.DebugFormat("Creating ModuleInstaller for {0}", ksp.GameDir());
        }

        // TODO: It'd be really lovely if this wasn't a singleton. It prevents code that
        // wishes to deal with multiple KSP installs.
        //
        // It would be totally fine to have this be an instance based upon KSP path, mind.
        public static ModuleInstaller Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ModuleInstaller( KSPManager.CurrentInstance);
                }

                return _Instance;
            }
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public string Download(Uri url, string filename)
        {
            if (onReportProgress != null)
            {
                onReportProgress(String.Format("Downloading \"{0}\"", url), 0);
            }

            return Download(url, filename, this.Cache);
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public static string Download(Uri url, string filename, NetFileCache cache)
        {
            log.Info("Downloading " + filename);

            string tmp_file = Net.Download(url);

            return cache.Store(url, tmp_file, filename, move: true);
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
            return CachedOrDownload(module.identifier, module.version, module.download, this.Cache, filename);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks the CKAN cache first.
        /// </summary>
        public string CachedOrDownload(string identifier, Version version, Uri url, string filename = null)
        {
            return CachedOrDownload(identifier, version, url, this.Cache, filename);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks provided cache first.
        /// </summary>
        public static string CachedOrDownload(string identifier, Version version, Uri url, NetFileCache cache, string filename = null)
        {
            if (filename == null)
            {
                filename = CkanModule.StandardName(identifier, version);
            }

            string full_path = cache.GetCachedZip(url);
            if (full_path == null)
            {
                return Download(url, filename, cache);
            }

            log.DebugFormat("Using {0} (cached)", filename);
            return full_path;
        }

        /// <summary>
        ///     Installs all modules given a list of identifiers as a transaction. Resolves dependencies.
        ///     This *will* save the registry at the end of operation.
        /// 
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Propagates a CancelledActionKraken if the user cancelled the install.
        /// </summary>
        //
        // TODO: Break this up into smaller pieces! It's huge!
        public void InstallList(
            List<string> modules,
            RelationshipResolverOptions options,
            NetAsyncDownloader downloader = null
        )
        {
            onReportProgress = onReportProgress ?? ((message, progress) => { });

            var resolver = new RelationshipResolver(modules, options, registry_manager.registry);
            List<CkanModule> modsToInstall = resolver.ModList();
            List<CkanModule> downloads = new List<CkanModule> (); 

            // TODO: All this user-stuff should be happening in another method!
            // We should just be installing mods as a transaction.

            User.WriteLine("About to install...\n");

            foreach (CkanModule module in modsToInstall)
            {
                if (!ksp.Cache.IsCachedZip(module.download))
                {
                    User.WriteLine(" * {0}", module);
                    downloads.Add(module);
                }
                else
                {
                    User.WriteLine(" * {0} (cached)", module);
                }
            }

            bool ok = User.YesNo("\nContinue?", FrontEndType.CommandLine);

            if (!ok)
            {
                throw new CancelledActionKraken("User declined install list");
            }

            User.WriteLine(""); // Just to look tidy.

            if (downloads.Count > 0)
            {
                if (downloader == null)
                {
                    downloader = new NetAsyncDownloader();
                }
                
                downloader.DownloadModules(ksp.Cache, downloads, onReportProgress);
            }

            // We're about to install all our mods; so begin our transaction.
            using (CkanTransaction transaction = new CkanTransaction())
            {
                for (int i = 0; i < modsToInstall.Count; i++)
                {
                    int percentComplete = (i * 100) / modsToInstall.Count;
                    
                    onReportProgress(String.Format("Installing mod \"{0}\"", modsToInstall[i]),
                                         percentComplete);

                    Install(modsToInstall[i]);
                }

                onReportProgress("Updating registry", 70);

                registry_manager.Save();

                onReportProgress("Commiting filesystem changes", 80);

                transaction.Complete();

            }

            // We can scan GameData as a separate transaction. Installing the mods
            // leaves everything consistent, and this is just gravy. (And ScanGameData
            // acts as a Tx, anyway, so we don't need to provide our own.)

            onReportProgress("Rescanning GameData", 90);

            ksp.ScanGameData();

            onReportProgress("Done!", 100);
        }

        /// <summary>
        /// Returns the module contents if and only if we have it
        /// available in our cache. Returns null, otherwise.
        ///
        /// Intended for previews.
        /// </summary>
        // TODO: Return files relative to GameRoot
        public IEnumerable<string> GetModuleContentsList(CkanModule module)
        {
            string filename = ksp.Cache.GetCachedZip(module.download);

            if (filename == null)
            {
                return null;
            }

            return FindInstallableFiles(module, filename, ksp)
                .Select(x => x.destination);
        }

        /// <summary>
        ///     Install our mod from the filename supplied.
        ///     If no file is supplied, we will check the cache or throw FileNotFoundKraken.
        ///     Does *not* resolve dependencies; this actually does the heavy listing.
        ///     Does *not* save the registry.
        ///     Do *not* call this directly, use InstallList() instead.
        /// 
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Throws a FileNotFoundKraken if we can't find the downloaded module.
        /// 
        /// </summary>
        // 
        // TODO: The name of this and InstallModule() need to be made more distinctive.

        private void Install(CkanModule module, string filename = null)
        {
            Version version = registry_manager.registry.InstalledVersion(module.identifier);

            // TODO: This really should be handled by higher-up code.
            if (version != null)
            {
                User.WriteLine("    {0} {1} already installed, skipped", module.identifier, version);
                return;
            }

            // Find our in the cache if we don't already have it.
            filename = filename ?? this.Cache.GetCachedZip(module.download);

            // If we *still* don't have a file, then kraken bitterly.
            if (filename == null)
            {
                throw new FileNotFoundKraken(
                    null, 
                    String.Format("Trying to install {0}, but it's not downloaded", module)
                );
            }

            // We'll need our registry to record which files we've installed.
            Registry registry = registry_manager.registry;

            using (var transaction = new CkanTransaction())
            {
                // Install all the things!
                IEnumerable<string> files = InstallModule(module, filename);

                // Register our module and its files.
                registry.RegisterModule(module, files, ksp);

                // Finish our transaction, but *don't* save the registry; we may be in an
                // intermediate, inconsistent state.
                // This is fine from a transaction standpoint, as we may not have an enclosing
                // transaction, and if we do, they can always roll us back.
                transaction.Complete();
            }

            // Fire our callback that we've installed a module, if we have one.
            if (onReportModInstalled != null)
            {
                onReportModInstalled(module);
            }

        }
            
        /// <summary>
        /// Returns a default install stanza for the module provided. This finds the topmost
        /// directory which matches the module identifier, and generates a stanza that
        /// installs that into GameData.
        /// 
        /// Throws a FileNotFoundKraken() if unable to locate a suitable directory.
        /// </summary>
        internal static ModuleInstallDescriptor GenerateDefaultInstall(string identifier, ZipFile zipfile)
        {
            var stanza = new ModuleInstallDescriptor();
            stanza.install_to = "GameData";

            // Candidate top-level directories.
            var candidate_set = new HashSet<string>();

            // Match *only* things with our module identifier as a directory.
            // We can't just look for directories, because some zipfiles
            // don't include entries for directories, but still include entries
            // for the files they contain.

            string ident_filter = @"(?:^|/)" + Regex.Escape(identifier) + @"$";

            // Let's find that directory
            foreach (ZipEntry entry in zipfile)
            {
                string directory = Path.GetDirectoryName(entry.Name);

                // Normalise our path.
                directory = directory.Replace('\\', '/');
                directory = Regex.Replace(directory, "/$", "");

                // If this looks like what we're after, remember it.
                if (Regex.IsMatch(directory, ident_filter, RegexOptions.IgnoreCase))
                {
                    candidate_set.Add(directory);
                }
            }

            // Sort to have shortest first. It's not *quite* top-level directory order,
            // but it's good enough for now.
            var candidates = new List<string>(candidate_set);
            candidates.Sort((a,b) => a.Length.CompareTo(b.Length));

            if (candidates.Count == 0)
            {
                throw new FileNotFoundKraken(
                    identifier,
                    String.Format("Could not find {0} directory in zipfile to install", identifier)
                );
            }

            // Fill in our stanza!
            stanza.file = candidates[0];
            return stanza;
        }

        /// <summary>
        /// Installs the module from the zipfile provided.
        /// Returns a list of files installed.
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// </summary>
        private IEnumerable<string> InstallModule(CkanModule module, string zip_filename)
        {
            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                IEnumerable<InstallableFile> files = FindInstallableFiles(module, zipfile, ksp);

                try
                {
                    foreach (InstallableFile file in files)
                    {
                        log.InfoFormat("Copying {0}", file.source.Name);
                        CopyZipEntry(zipfile, file.source, file.destination, file.makedir);
                    }
                }
                catch (FileExistsKraken kraken)
                {
                    // Decorate the kraken with our module and re-throw
                    kraken.filename = ksp.ToRelativeGameDir(kraken.filename);
                    kraken.installing_module = module;
                    kraken.owning_module = registry_manager.registry.FileOwner(kraken.filename);
                    throw;
                }

                return files.Select(x => x.destination);
            }
        }

        /// <summary>
        /// Given a stanza and an open zipfile, returns all files that would be installed
        /// for this stanza.
        /// 
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        /// 
        /// Throws a BadInstallLocationKraken if the install stanza targets an
        /// unknown install location (eg: not GameData, Ships, etc)
        /// 
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        /// </summary>
        /// <exception cref="BadInstallLocationKraken">Thrown when the installation path is not valid according to the spec.</exception>
        internal static List<InstallableFile> FindInstallableFiles(ModuleInstallDescriptor stanza, ZipFile zipfile, KSP ksp)
        {
            string installDir;
            bool makeDirs;
            var files = new List<InstallableFile> ();

            // Normalize the path before doing everything else
            stanza.install_to = KSPPathUtils.NormalizePath(stanza.install_to);
  
            if (stanza.install_to == "GameData" || stanza.install_to.StartsWith("GameData/"))
            {
                // The installation path can be either "GameData" or a sub-directory of "GameData"
                // but it cannot contain updirs
                if (stanza.install_to.Contains("/../") || stanza.install_to.EndsWith("/.."))
                    throw new BadInstallLocationKraken("Invalid installation path: " + stanza.install_to);

                string subDir = stanza.install_to.Substring("GameData".Length);    // remove "GameData"
                subDir = subDir.StartsWith("/") ? subDir.Substring(1) : subDir;    // remove a "/" at the beginning, if present
                
                // Add the extracted subdirectory to the path of KSP's GameData
                installDir = ksp == null ? null : (KSPPathUtils.NormalizePath(ksp.GameData() + "/" + subDir));
                makeDirs = true;
            }
            else if (stanza.install_to == "Ships")
            {
                installDir = ksp == null ? null : ksp.Ships();
                makeDirs = false; // Don't allow directory creation in ships directory
            }
            else if (stanza.install_to == "Tutorial")
            {
                installDir = ksp == null ? null : ksp.Tutorial();
                makeDirs = true;
            }
            else if (stanza.install_to == "GameRoot")
            {
                installDir = ksp == null ? null : ksp.GameDir();
                makeDirs = false;
            }
            else
            {
                throw new BadInstallLocationKraken("Unknown install_to " + stanza.install_to);
            }

            // O(N^2) solution, as we're walking the zipfile for each stanza.
            // Surely there's a better way, although this is fast enough we may not care.

            foreach (ZipEntry entry in zipfile)
            {
                // Skips things not prescribed by our install stanza.
                if (! stanza.IsWanted(entry.Name)) {
                    continue;
                }

                // Prepare our file info.
                InstallableFile file_info = new InstallableFile (); 
                file_info.source = entry;
                file_info.makedir = makeDirs;
                file_info.destination = null;

                // If we have a place to install it, fill that in...
                if (installDir != null)
                {
                    // Get the full name of the file.
                    string outputName = entry.Name;

                    // Update our file info with the install location
                    file_info.destination = TransformOutputName(stanza.file, outputName, installDir);
                }

                files.Add(file_info);
            }

            // If we have no files, then something is wrong! (KSP-CKAN/CKAN#93)
            if (files.Count == 0)
            {
                // We have null as the first argument here, because we don't know which module we're installing
                throw new BadMetadataKraken(null, String.Format("No files found in {0} to install!", stanza.file));
            }

            return files;
        }

        /// <summary>
        /// Transforms the name of the output. This will strip the leading directories from the stanza file from
        /// output name and then combine it with the installDir.
        /// EX: "kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData" will be transformed 
        /// to "GameData/kOS/Plugins/kOS.dll"
        /// </summary>
        /// <returns>The output name.</returns>
        /// <param name="file">The file directive of the stanza.</param>
        /// <param name="outputName">The name of the file to transform.</param>
        /// <param name="installDir">The installation dir where the file should end up with.</param>
        internal static string TransformOutputName(string file, string outputName, string installDir)
        {
            string leadingPathToRemove = KSPPathUtils.GetLeadingPathElements(file);

            // Special-casing, if stanza.file is just "GameData" or "Ships", strip it.
            // TODO: Do we need to do anything special for tutorials or GameRoot?
            if (
                leadingPathToRemove == string.Empty &&
                (file == "GameData" || file == "Ships")
            )
            {
                leadingPathToRemove = file;
            }

            // If there's a leading path to remove, then we have some extra work that
            // needs doing...
            if (leadingPathToRemove != string.Empty)
            {
                string leadingRegEx = "^" + Regex.Escape(leadingPathToRemove) + "/";
                if (!Regex.IsMatch(outputName, leadingRegEx))
                {
                    throw new BadMetadataKraken(null,
                        String.Format("Output file name ({0}) not matching leading path of stanza.file ({1})",
                            outputName, leadingRegEx
                        )
                    );
                }
                // Strip off leading path name
                outputName = Regex.Replace(outputName, leadingRegEx, "");
            }
 
            // Return our snipped, normalised, and ready to go output filename!
            return KSPPathUtils.NormalizePath(
                Path.Combine(installDir, outputName)
            );
        }

        /// <summary>
        /// Given a module and an open zipfile, return all the files that would be installed
        /// for this module.
        /// 
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        /// 
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        /// </summary>
        public static List<InstallableFile> FindInstallableFiles(CkanModule module, ZipFile zipfile, KSP ksp)
        {
            var files = new List<InstallableFile> ();

            try
            {
                // Use the provided stanzas, or use the default install stanza if they're absent.
                if (module.install != null && module.install.Length != 0)
                {
                    foreach (ModuleInstallDescriptor stanza in module.install)
                    {
                        files.AddRange(FindInstallableFiles(stanza, zipfile, ksp));
                    }
                }
                else
                {
                    ModuleInstallDescriptor default_stanza = GenerateDefaultInstall(module.identifier, zipfile);
                    files.AddRange(FindInstallableFiles(default_stanza, zipfile, ksp));
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
        public static List<InstallableFile> FindInstallableFiles(CkanModule module, string zip_filename, KSP ksp)
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
        internal static void CopyZipEntry(ZipFile zipfile, ZipEntry entry, string fullPath, bool makeDirs)
        {
            if (entry.IsDirectory)
            {
                // Skip if we're not making directories for this install.
                if (!makeDirs)
                {
                    log.DebugFormat ("Skipping {0}, we don't make directories for this path", fullPath);
                    return;
                }

                log.DebugFormat("Making directory {0}", fullPath);
                file_transaction.CreateDirectory(fullPath);
            }
            else
            {
                log.DebugFormat("Writing file {0}", fullPath);

                // Sometimes there are zipfiles that don't contain entries for the
                // directories their files are in. No, I understand either, but
                // the result is we have to make sure our directories exist, just in case.
                if (makeDirs)
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    file_transaction.CreateDirectory(directory);
                }

                // We don't allow for the overwriting of files. See #208.
                if (File.Exists (fullPath))
                {
                    throw new FileExistsKraken(fullPath, string.Format("Trying to write {0} but it already exists.", fullPath));
                }

                // Snapshot whatever was there before. If there's nothing, this will just
                // remove our file on rollback. We still need this even thought we won't
                // overwite files, as it ensures deletiion on rollback.
                file_transaction.Snapshot(fullPath);

                // It's a file! Prepare the streams
                using (Stream zipStream = zipfile.GetInputStream(entry))
                using (FileStream writer = File.Create(fullPath))
                {
                    // 4k is the block size on practically every disk and OS.
                    byte[] buffer = new byte[4096];
                    StreamUtils.Copy(zipStream, writer, buffer);
                }
            }

            return;
        }

        /// <summary>
        /// Uninstalls all the mods provided, including things which depend upon them.
        /// This *DOES* save the registry.
        /// Preferred over Uninstall.
        /// </summary>
        public void UninstallList(IEnumerable<string> mods)
        {
            // Pre-check, have they even asked for things which are installed?

            foreach (string mod in mods)
            {
                if (registry_manager.registry.InstalledModule(mod) == null)
                {
                    throw new ModNotInstalledKraken(mod);
                }
            }

            // Find all the things which need uninstalling.
            IEnumerable<string> goners = registry_manager.registry.FindReverseDependencies(mods);

            User.WriteLine("About to remove:\n");

            foreach (string mod in goners)
            {
                User.WriteLine(" * {0}", mod);
            }

            bool ok = User.YesNo("\nContinue?", FrontEndType.CommandLine);

            if (!ok)
            {
                User.WriteLine("Mod removal aborted at user request.");
                return;
            }

            using (var transaction = new CkanTransaction())
            {
                foreach (string mod in goners)
                {
                    User.WriteLine("Removing {0}...", mod);
                    Uninstall(mod);
                }

                registry_manager.Save();

                transaction.Complete();
            }

            User.WriteLine("Done!");
        }

        public void UninstallList(string mod)
        {
            var list = new List<string>();
            list.Add(mod);
            UninstallList(list);
        }

        /// <summary>
        /// Uninstall the module provided. For internal use only.
        /// Use UninstallList for user queries, it also does dependency handling.
        /// This does *NOT* save the registry.
        /// </summary>
         
        private void Uninstall(string modName)
        {
            using (var transaction = new CkanTransaction())
            {
                InstalledModule mod = registry_manager.registry.InstalledModule(modName);

                if (mod == null)
                {
                    log.ErrorFormat("Trying to uninstall {0} but it's not installed", modName);
                    throw new ModNotInstalledKraken(modName);
                }

                // Walk our registry to find all files for this mod.
                IEnumerable<string> files = mod.Files;

                var directoriesToDelete = new HashSet<string>();

                foreach (string file in files)
                {
                    string path = ksp.ToAbsoluteGameDir(file);

                    try
                    {
                        FileAttributes attr = File.GetAttributes(path);

                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            directoriesToDelete.Add(path);
                        }
                        else
                        {
                            log.InfoFormat("Removing {0}", file);
                            file_transaction.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        // XXX: This is terrible, we're catching all exceptions.
                        log.ErrorFormat("Failure in locating file {0} : {1}", path, ex.Message);
                    }
                }

                // Remove from registry.

                registry_manager.registry.DeregisterModule(ksp, modName);

                // Sort our directories from longest to shortest, to make sure we remove child directories
                // before parents. GH #78.
                foreach (string directory in directoriesToDelete.OrderBy(dir => dir.Length).Reverse())
                {
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                    {

                        // We *don't* use our file_transaction to delete files here, because
                        // it fails if the system's temp directory is on a different device
                        // to KSP. However we *can* safely delete it now we know it's empty,
                        // because the TxFileMgr *will* put it back if there's a file inside that
                        // needs it.
                        //
                        // This works around GH #251.
                        // The filesystem boundry bug is described in https://transactionalfilemgr.codeplex.com/workitem/20

                        log.InfoFormat("Removing {0}", directory);
                        Directory.Delete(directory);
                    }
                    else
                    {
                        User.WriteLine("Not removing directory {0}, it's not empty", directory);
                    }
                }
                transaction.Complete();
            }

            return;
        }

        #region AddRemove

        /// <summary>
        /// Adds and removes the listed modules as a single transaction.
        /// No relationships will be processed.
        /// This *will* save the registry.
        /// </summary>
        /// <param name="add">Add.</param>
        /// <param name="remove">Remove.</param>
        public void AddRemove(IEnumerable<CkanModule> add = null, IEnumerable<string> remove = null)
        {

            // TODO: We should do a consistency check up-front, rather than relying
            // upon our registry catching inconsistencies at the end.

            // TODO: Download our files.

            using (var tx = new CkanTransaction())
            {

                foreach (string identifier in remove)
                {
                    Uninstall(identifier);
                }

                foreach (CkanModule module in add)
                {
                    Install(module);
                }

                this.registry_manager.Save();

                tx.Complete();
            }
        }

        /// <summary>
        /// Upgrades the mods listed to the latest versions for the user's KSP.
        /// Will *re-install* with warning even if an upgrade is not available.
        /// Throws ModuleNotFoundKraken if module is not installed, or not available.
        /// </summary>
        public void Upgrade(IEnumerable<string> identifiers)
        {
            List<CkanModule> upgrades = new List<CkanModule>();

            foreach (string ident in identifiers)
            {
                CkanModule latest = registry_manager.registry.LatestAvailable(
                                        ident, this.ksp.Version()
                                    );

                if (latest == null)
                {
                    throw new ModuleNotFoundKraken(
                        ident,
                        "Can't upgrade {0}, no modules available", ident
                    );
                }

                upgrades.Add(latest);
            }

            Upgrade(upgrades);
        }

        /// <summary>
        /// Upgrades the mods listed to the specified versions for the user's KSP.
        /// Will *re-install* or *downgrade* (with a warning) as well as upgrade.
        /// Throws ModuleNotFoundKraken if a module is not installed.
        /// </summary>
        public void Upgrade(IEnumerable<CkanModule> modules)
        {
            // Start by making sure we've downloaded everything.
            DownloadModules(modules);

            foreach (CkanModule module in modules)
            {
                string ident = module.identifier;
                Module installed = registry_manager.registry.InstalledModule(ident).Module;

                if (installed == null)
                {
                    throw new ModuleNotFoundKraken(
                        ident,
                        "Can't upgrade {0}, it is not installed", ident
                    );
                }

                if (installed.version.IsEqualTo(module.version))
                {
                    log.WarnFormat("{0} is already at the latest version, reinstalling", installed.identifier);
                }
                else if (installed.version.IsGreaterThan(module.version))
                {
                    log.WarnFormat("Downgrading {0} from {1} to {2}", ident, installed.version, module.version);
                }
                else
                {
                    log.InfoFormat("Upgrading {0} to {1}", ident, module.version);
                }
            }

            AddRemove(
                modules,
                modules.Select(x => x.identifier)
            );
        }

        #endregion

        /// <summary>
        /// Makes sure all the specified mods are downloaded.
        /// </summary>
        private void DownloadModules(IEnumerable<CkanModule> mods)
        {
            List<CkanModule> downloads = new List<CkanModule> ();

            foreach (CkanModule module in mods)
            {
                if (!ksp.Cache.IsCachedZip(module.download))
                {
                    downloads.Add(module);
                }
            }

            if (downloads.Count > 0)
            {
                var downloader = new NetAsyncDownloader();

                downloader.DownloadModules(ksp.Cache, downloads, onReportProgress);
            }
        }

        /// <summary>
        /// Don't use this. Use Registry.FindReverseDependencies instead.
        /// This method may be deprecated in the future.
        /// </summary>
        // Here for now to keep the GUI happy.
        public HashSet<string> FindReverseDependencies(string module)
        {
            return registry_manager.registry.FindReverseDependencies(module);
        }

    }
}
