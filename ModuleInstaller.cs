using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Transactions;
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

        private NetAsyncDownloader downloader;
        private bool lastDownloadSuccessful;
        public ModuleInstallerReportModInstalled onReportModInstalled = null;
        public ModuleInstallerReportProgress onReportProgress = null;
        private bool installCanceled = false; // Used for inter-thread communication.

        // Our own cache is that of the KSP instance we're using.
        public Cache Cache
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
            this.registry_manager = RegistryManager.Instance(ksp.CkanDir());
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
        public static string Download(Uri url, string filename, Cache cache)
        {
            log.Info("Downloading " + filename);

            string full_path = cache.CachePath(filename);

            return Net.Download(url, full_path);
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
        public static string CachedOrDownload(string identifier, Version version, Uri url, Cache cache, string filename = null)
        {
            if (filename == null)
            {
                filename = CkanModule.StandardName(identifier, version);
            }

            string full_path = cache.CachedFile(filename);

            if (full_path != null)
            {
                log.DebugFormat("Using {0} (cached)", filename);
                return full_path;
            }

            return Download(url, filename, cache);
        }

        /// <summary>
        /// Downloads all the modules specified to the cache.
        /// </summary>
        public NetAsyncDownloader DownloadAsync(CkanModule[] modules)
        {
            var urls = new Uri[modules.Length];
            var fullPaths = new string[modules.Length];

            for (int i = 0; i < modules.Length; i++)
            {
                fullPaths[i] = ksp.Cache.CachePath(modules[i]);
                urls[i] = modules[i].download;
            }

            downloader = new NetAsyncDownloader(urls, fullPaths);

            if (onReportProgress != null)
            {
                downloader.onProgressReport = (percent, bytesPerSecond, bytesLeft) =>
                    onReportProgress(
                        String.Format("{0} kbps - downloading - {1} MiB left", bytesPerSecond/1024, bytesLeft/1024/1024),
                        percent);
            }

            downloader.onCompleted = (_uris, strings, errors) => OnDownloadsComplete(_uris, fullPaths, modules, errors);

            return downloader;
        }

        /// <summary>
        /// Downloads all the modules specified to the cache.
        /// </summary>
        public NetAsyncDownloader DownloadAsync(List<CkanModule> modules)
        {
            var mod_array = new CkanModule[modules.Count];
            modules.CopyTo(mod_array);
            return DownloadAsync(mod_array);
        }

        /// <summary>
        ///     Installs all modules given a list of identifiers. Resolves dependencies.
        ///     The function initializes a filesystem transaction, then installs all cached mods
        ///     this ensures we don't waste time and bandwidth if there is an issue with any of the cached archives.
        ///     After this we try to download the rest of the mods (asynchronously) and install them.
        ///     Finally, only if everything is successful, we commit the transaction.
        /// </summary>
        //
        // TODO: Break this up into smaller pieces! It's huge!
        public void InstallList(List<string> modules, RelationshipResolverOptions options, bool downloadOnly = false)
        {
            using (TransactionScope transaction = new TransactionScope())
            {

                installCanceled = false; // Can be set by another thread

                var resolver = new RelationshipResolver(modules, options, registry_manager.registry);
                List<CkanModule> modsToInstall = resolver.ModList();
                List<CkanModule> downloads = new List<CkanModule> (); 

                User.WriteLine("About to install...\n");

                foreach (CkanModule module in modsToInstall)
                {
                    if (Cache.IsCached(module))
                    {
                        User.WriteLine(" * {0} (cached)", module);
                    }
                    else
                    {
                        User.WriteLine(" * {0}", module);
                        downloads.Add(module);
                    }
                }

                bool ok = User.YesNo("\nContinue?", FrontEndType.CommandLine);

                if (!ok)
                {
                    log.Debug("Halting install at user request");
                    return;
                }

                User.WriteLine(""); // Just to look tidy.

                // TODO: Is this really where we want to be setting this?
                lastDownloadSuccessful = true;

                if (installCanceled)
                {
                    log.Warn("Pre-download halted at user request");
                    return;
                }

                if (downloads.Count > 0)
                {
                    downloader = DownloadAsync(downloads);
                    downloader.StartDownload();

                    lock (downloader)
                    {
                        Monitor.Wait(downloader);
                    }
                }

                if (installCanceled)
                {
                    log.Warn("Download halted at user request");
                    return;
                }

                if (lastDownloadSuccessful && !downloadOnly && modsToInstall.Count > 0)
                {
                    for (int i = 0; i < modsToInstall.Count; i++)
                    {
                        int percentComplete = (i * 100) / modsToInstall.Count;
                        if (onReportProgress != null)
                        {
                            onReportProgress(String.Format("Installing mod \"{0}\"", modsToInstall[i]),
                                             percentComplete);
                        }

                        Install(modsToInstall[i]);
                    }

                    transaction.Complete();
                    return;
                }
             
                transaction.Dispose(); // Rollback
            }
        }

        /// <summary>Call this to cancel the installs being performed by other threads</summary>
        public void CancelInstall()
        {
            if (downloader != null)
            {
                downloader.CancelDownload();
            }

            installCanceled = true;
        }

        private void OnDownloadsComplete(Uri[] urls, string[] filenames, CkanModule[] modules, Exception[] errors)
        {
            bool noErrors = false;

            if (urls != null)
            {
                noErrors = true;
                
                for (int i = 0; i < errors.Length; i++)
                {
                    if (errors[i] != null)
                    {
                        noErrors = false;
                        User.Error("Failed to download \"{0}\" - error: {1}", urls[i], errors[i].Message);
                    }
                }
            }

            lastDownloadSuccessful = noErrors;

            lock (downloader)
            {
                Monitor.Pulse(downloader);
            }
        }

        /// <summary>
        /// Returns the module contents if and only if we have it
        /// available in our cache. Returns null, otherwise.
        ///
        /// Intended for previews.
        /// </summary>

        public List<string> GetModuleContentsList(CkanModule module)
        {
            string filename = Cache.CachedFile(module);

            if (filename == null)
            {
                return null;
            }

            List<InstallableFile> contents = FindInstallableFiles(module, filename, ksp);

            var pretty_filenames = new List<string> ();

            foreach (var entry in contents)
            {
                string path = entry.destination;

                pretty_filenames.Add(path);
            }

            return pretty_filenames;
        }

        /// <summary>
        ///     Install our mod from the filename supplied.
        ///     If no file is supplied, we will check the cache or download it.
        ///     Does *not* resolve dependencies; this actually does the heavy listing.
        ///     Use InstallList() for requests from the user.
        /// 
        /// </summary>
        // 
        // TODO: The name of this and InstallModule() need to be made more distinctive.

        internal void Install(CkanModule module, string filename = null)
        {
            using (var transaction = new TransactionScope())
            {


                User.WriteLine(module.identifier + ":\n");

                Version version = registry_manager.registry.InstalledVersion(module.identifier);

                // TODO: This really should be handled by higher-up code.
                if (version != null)
                {
                    User.WriteLine("    {0} {1} already installed, skipped", module.identifier, version);
                    return;
                }

                // Fetch our file if we don't already have it.
                if (filename == null)
                {
                    filename = CachedOrDownload(module);
                }

                // We'll need our registry to record which files we've installed.
                Registry registry = registry_manager.registry;

                // And a list of files to record them to.
                var module_files = new Dictionary<string, InstalledModuleFile>();

                // Install all the things!
                InstallModule(module, filename, module_files);

                // Register our files.
                registry.RegisterModule(new InstalledModule(module_files, module, DateTime.Now));

                // Done! Save our registry changes!
                // This is fine from a transaction standpoint, as we may not have an enclosing
                // transaction, and if we do, they can always roll us back.
                registry_manager.Save();

                transaction.Complete();
            }

            // Fire our callback that we've installed a module, if we have one.
            if (onReportModInstalled != null)
            {
                onReportModInstalled(module);
            }

        }

        /// <summary>
        /// Returns the sha1 sum of the given filename.
        /// Returns null if passed a directory.
        /// Throws an exception on failure to access the file.
        /// </summary>
        internal string Sha1Sum(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }

            SHA1 hasher = new SHA1CryptoServiceProvider();

            // Even if we throw an exception, the using block here makes sure
            // we close our file.
            using (var fh = File.OpenRead(path))
            {
                string sha1 = BitConverter.ToString(hasher.ComputeHash(fh));
                fh.Close();
                return sha1;
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

            string ident_filter = @"(^|/|\\)" + Regex.Escape(identifier) + @"$";

            // Let's find that directory
            foreach (ZipEntry entry in zipfile)
            {
                string directory = Path.GetDirectoryName(entry.Name);

                // If this looks like what we're after, remember it.
                if (Regex.IsMatch(directory, ident_filter, RegexOptions.IgnoreCase ))
                {
                    candidate_set.Add(directory);
                }
            }

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
        /// Install the component described in the stanza.
        /// Modifies the supplied module_files to contain the files installed.
        /// This method should be avoided, as it may be removed in the future.
        /// </summary>
        internal void InstallComponent(ModuleInstallDescriptor stanza, string zip_filename,
            Dictionary<string, InstalledModuleFile> module_files)
        {

            // TODO: Can we deprecate this code now please?
            log.Warn("Soon to be deprecated method InstallComponent called");

            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                List<InstallableFile> files = FindInstallableFiles(stanza, zipfile, ksp);

                foreach (var file in files) {

                    // TODO: I'd like to replace this with a log.Info
                    User.WriteLine("    * Copying " + file.source.Name);

                    CopyZipEntry(zipfile, file.source, file.destination, file.makedir);

                    module_files.Add(file.destination, new InstalledModuleFile
                    {
                        sha1_sum = Sha1Sum(file.destination)
                    });
                }
            }
        }

        /// <summary>
        /// Installs the module from the zipfile provided, updating the supplied list of installed files provided.
        /// 
        /// Propagates up a BadMetadataKraken if our install metadata is bad.
        /// </summary>
        internal void InstallModule(CkanModule module, string zip_filename, Dictionary<string, InstalledModuleFile> installed_files)
        {
            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                List<InstallableFile> files = FindInstallableFiles(module, zipfile, ksp);

                foreach (var file in files)
                {
                    log.InfoFormat("Copying {0}", file.source.Name);
                    CopyZipEntry(zipfile, file.source, file.destination, file.makedir);
                    installed_files.Add(file.destination, new InstalledModuleFile
                    {
                        sha1_sum = Sha1Sum(file.destination)
                    });
                }
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
        internal static List<InstallableFile> FindInstallableFiles(ModuleInstallDescriptor stanza, ZipFile zipfile, KSP ksp)
        {
            string installDir;
            bool makeDirs;
            var files = new List<InstallableFile> ();

            if (stanza.install_to == "GameData")
            {
                installDir = ksp == null ? null : ksp.GameData();
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

                    // Strip off everything up to GameData/Ships
                    // TODO: There's got to be a nicer way of doing path resolution.
                    outputName = Regex.Replace(outputName, @"^/?(.*(GameData|Ships)/)?", "", RegexOptions.IgnoreCase);

                    string full_path = Path.Combine(installDir, outputName);

                    // Make the path pretty, and of course the prettiest paths use Unix separators. ;)
                    full_path = full_path.Replace('\\', '/');

                    // Update our file info with the install location
                    file_info.destination = full_path;
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

                // Snapshot whatever was there before. If there's nothing, this will just
                // remove our file on rollback.
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

        public List<string> FindReverseDependencies(string modName)
        {
            var reverseDependencies = new List<string>();

            // loop through all installed modules
            foreach (var keyValue in registry_manager.registry.installed_modules)
            {
                Module mod = keyValue.Value.source_module;
                bool isDependency = false;

                if (mod.depends != null)
                {
                    foreach (RelationshipDescriptor dependency in mod.depends)
                    {
                        if (dependency.name == modName)
                        {
                            isDependency = true;
                            break;
                        }
                    }
                }

                if (isDependency)
                {
                    reverseDependencies.Add(mod.identifier);
                }
            }

            return reverseDependencies;
        }

        /// <summary>
        /// Uninstall the module provided.
        /// </summary>
         
        // TODO: Remove second arg; shouldn't we *always* uninstall dependencies?
        public void Uninstall(string modName, bool uninstallDependencies)
        {
            using (var transaction = new TransactionScope())
            {

                if (!registry_manager.registry.IsInstalled(modName))
                {
                    // TODO: This could indicates a logic error somewhere;
                    // change to a kraken, the calling code can always catch it
                    // if it expects that it may try to uninstall a module twice.
                    log.ErrorFormat("Trying to uninstall {0} but it's not installed", modName);
                    return;
                }

                // Find all mods that depend on this one
                if (uninstallDependencies)
                {
                    List<string> reverseDependencies = FindReverseDependencies(modName);
                    foreach (string reverseDependency in reverseDependencies)
                    {
                        Uninstall(reverseDependency, uninstallDependencies);
                    }
                }

                // Walk our registry to find all files for this mod.
                Dictionary<string, InstalledModuleFile> files =
                    registry_manager.registry.installed_modules[modName].installed_files;

                var directoriesToDelete = new HashSet<string>();

                foreach (string file in files.Keys)
                {
                    string path = Path.Combine(ksp.GameDir(), file);

                    try
                    {
                        FileAttributes attr = File.GetAttributes(path);

                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            directoriesToDelete.Add(path);
                        }
                        else
                        {
                            User.WriteLine("Removing {0}", file);
                            file_transaction.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO: Report why.
                        log.ErrorFormat("Failure in locating file {0} : {1}", path, ex.Message);
                    }
                }

                // Remove from registry.

                registry_manager.registry.DeregisterModule(modName);
                registry_manager.Save();

                foreach (string directory in directoriesToDelete)
                {
                    if (!Directory.GetFiles(directory).Any())
                    {
                        try
                        {
                            file_transaction.Delete(directory);
                        }
                        catch (Exception)
                        {
                            // TODO: Report why.
                            User.WriteLine("Couldn't delete directory {0}", directory);
                        }
                    }
                }
                transaction.Complete();
            }

            return;
        }
    }
}
