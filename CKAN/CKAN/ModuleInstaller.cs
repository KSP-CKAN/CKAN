using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

namespace CKAN
{
    public delegate void ModuleInstallerReportProgress(string message, int progress);

    public delegate void ModuleInstallerReportModInstalled(CkanModule module);

    internal struct InstallableFile 
    {
        public ZipEntry source;
        public string destination;
        public bool makedir;
    }

    public class ModuleInstaller
    {
        private static ModuleInstaller _Instance;

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));
        private RegistryManager registry_manager;

        private FilesystemTransaction currentTransaction;
        private NetAsyncDownloader downloader;
        private bool lastDownloadSuccessful;
        public ModuleInstallerReportModInstalled onReportModInstalled = null;
        public ModuleInstallerReportProgress onReportProgress = null;
        private bool installCanceled = false; // Used for inter-thread communication.

        private ModuleInstaller()
        {
        }

        public static ModuleInstaller Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ModuleInstaller();
                    _Instance.registry_manager = RegistryManager.Instance(KSPManager.CurrentInstance.CkanDir());
                }

                return _Instance;
            }
        }

        /// <summary>
        ///     Download the given mod. Returns the filename it was saved to.
        ///     If no filename is provided, the standard_name() will be used.
        /// </summary>
        /// <param name="filename">Filename.</param>
        public string Download(Uri url, string filename)
        {
            User.WriteLine("    * Downloading " + filename + "...");

            string full_path = Path.Combine(KSPManager.CurrentInstance.DownloadCacheDir(), filename);

            if (onReportProgress != null)
            {
                onReportProgress(String.Format("Downloading \"{0}\"", url), 0);
            }

            return Net.Download(url, full_path);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        /// 
        /// If no filename is provided, the module's standard name will be used.
        /// In all caches, the CachePath() location is checked for the filename before downloading.
        /// </summary>
        public string CachedOrDownload(CkanModule module, string filename = null)
        {
            return CachedOrDownload(module.identifier, module.version, module.download, filename);
        }

        public string CachedOrDownload(string identifier, Version version, Uri url, string filename = null)
        {
            if (filename == null)
            {
                filename = CkanModule.StandardName(identifier, version);
            }

            string fullPath = CachePath(filename);

            if (File.Exists(fullPath))
            {
                log.DebugFormat("Using {0} (cached)", filename);
                return fullPath;
            }

            return Download(url, filename);
        }

        public NetAsyncDownloader DownloadAsync(CkanModule[] modules, string[] filenames = null)
        {
            var urls = new Uri[modules.Length];
            var fullPaths = new string[modules.Length];

            for (int i = 0; i < modules.Length; i++)
            {
                fullPaths[i] = Path.Combine(KSPManager.CurrentInstance.DownloadCacheDir(), filenames[i]);
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
        /// Returns true if the given module is present in our cache.
        /// </summary>
        //
        // TODO: Update this if we start caching by URL (GH #111)
        public static bool IsCached(CkanModule module)
        {
            string filename = CkanModule.StandardName(module.identifier, module.version);
            string path = CachePath(filename);
            if (File.Exists(path))
            {
                return true;
            }

            return false;
        }

        public bool IsCached(string filename, out string fullPath)
        {
            fullPath = CachePath(filename);

            if (File.Exists(fullPath))
            {
                return true;
            }

            return false;
        }

        public static string CachePath(string file)
        {
            return Path.Combine(KSPManager.CurrentInstance.DownloadCacheDir(), file);
        }

        /// <summary>
        ///     Installs all modules given a list of identifiers. Resolves dependencies.
        ///     The function initializes a filesystem transaction, then installs all cached mods
        ///     this ensures we don't waste time and bandwidth if there is an issue with any of the cached archives
        ///     After this we try to download the rest of the mods (asynchronously) and install them
        ///     Finally, only if everything is successful, we commit the transaction
        /// </summary>
        //
        // TODO: Break this up into smaller pieces! It's huge!
        public void InstallList(List<string> modules, RelationshipResolverOptions options, bool downloadOnly = false)
        {
            installCanceled = false; // Can be set by another thread
            currentTransaction = new FilesystemTransaction(KSPManager.CurrentInstance.TempDir());

            if (onReportProgress != null)
            {
                currentTransaction.onProgressReport += (message, percent) => onReportProgress(message, percent);
            }

            var resolver = new RelationshipResolver(modules, options, registry_manager.registry);

            User.WriteLine("About to install...\n");

            foreach (CkanModule module in resolver.ModList())
            {
                User.WriteLine(" * {0} {1}", module.identifier, module.version);
            }

            bool ok = User.YesNo("\nContinue?", FrontEndType.CommandLine);

            if (!ok)
            {
                log.Debug("Halting install at user request");
                return;
            }

            User.WriteLine(""); // Just to look tidy.

            List<CkanModule> modList = resolver.ModList();

            var notCached = new List<CkanModule>();
            var cached = new List<KeyValuePair<CkanModule, string>>();

            foreach (CkanModule module in modList)
            {
                string fullPath;
                if (IsCached(module.StandardName(), out fullPath))
                {
                    cached.Add(new KeyValuePair<CkanModule, string>(module, fullPath));
                }
                else
                {
                    notCached.Add(module);
                }
            }

            var modulesToDownload = new CkanModule[notCached.Count];
            var modulesToDownloadPaths = new string[notCached.Count];

            for (int i = 0; i < notCached.Count; i++)
            {
                modulesToDownload[i] = notCached[i];
                modulesToDownloadPaths[i] = CachePath(notCached[i].StandardName());
            }

            lastDownloadSuccessful = true;

            if (installCanceled)
            {
                return;
            }

            if (modulesToDownload.Length > 0)
            {
                downloader = DownloadAsync(modulesToDownload, modulesToDownloadPaths);
                downloader.StartDownload();

                lock (downloader)
                {
                    Monitor.Wait(downloader);
                }
            }

            if (installCanceled)
            {
                return;
            }

            var modsToInstall = new List<KeyValuePair<CkanModule, string>>();
            for (int i = 0; i < modulesToDownload.Length; i++)
            {
                modsToInstall.Add(new KeyValuePair<CkanModule, string>(modulesToDownload[i], modulesToDownloadPaths[i]));;
            }

            foreach (var pair in cached)
            {
                modsToInstall.Add(new KeyValuePair<CkanModule, string>(pair.Key, pair.Value));
            }

            if (lastDownloadSuccessful && !downloadOnly && modsToInstall.Count > 0)
            {
                for (int i = 0; i < modsToInstall.Count; i++)
                {
                    int percentComplete = (i * 100) / modsToInstall.Count;
                    if (onReportProgress != null)
                    {
                        onReportProgress(String.Format("Installing mod \"{0}\"", modsToInstall[i].Key.name),
                            percentComplete);
                    }

                    Install(modsToInstall[i].Key, modsToInstall[i].Value);
                }

                currentTransaction.Commit();
                return;
            }
         
            currentTransaction.Rollback();
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

            if (!IsCached(module))
            {
                return null;
            }

            string filename = CachedOrDownload(module);

            ZipFile zipfile = null;

            // Open our zip file for processing
            try
            {
                zipfile = new ZipFile(File.OpenRead(filename));
            }
            catch (Exception)
            {
                User.Error("Failed to open archive \"{0}\". Try clearing the cache.", filename);
                return null;
            }

            var contents = new List<InstallableFile> ();

            foreach (ModuleInstallDescriptor stanza in module.install)
            {
                contents.AddRange( FindInstallableFiles(stanza, zipfile) );
            }
            
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
        ///     If no file is supplied, we will fetch() it first.
        ///     Does *not* resolve dependencies; this actually does the heavy listing.
        ///     Use InstallList() for requests from the user.
        /// 
        ///     XXX: This provides no way to check if the install failed,
        ///     it *should* throw an exception if it does.
        /// </summary>
        private void Install(CkanModule module, string filename = null)
        {
            User.WriteLine(module.identifier + ":\n");

            Version version = registry_manager.registry.InstalledVersion(module.identifier);

            if (version != null)
            {
                // TODO: Check if we can upgrade!
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

            ZipFile zipfile = null;

            // Open our zip file for processing
            try
            {
                zipfile = new ZipFile(File.OpenRead(filename));
            }
            catch (Exception)
            {
                // TODO: I'm not sure we want to just be returing here
                // on error. A failed install is enough of a reason to
                // bail out entirely. This should be throwing an exception.
                User.Error("Failed to open archive \"{0}\"", filename);
                return;
            }

            if (module.install == null || module.install.Length == 0)
            {
                log.DebugFormat("No install stanzas found for {0}, using defaults", module);

                // This throws a FileNotFoundKraken on failure. We intentionally
                // don't catch it, because that's an irrecoverable error.
                var stanza = GenerateDefaultInstall(module.identifier, zipfile);
                InstallComponent(stanza, zipfile, module_files);
            }
            else
            {
                // Walk through our install instructions.
                foreach (ModuleInstallDescriptor stanza in module.install)
                {
                    InstallComponent(stanza, zipfile, module_files);
                }
            }

            // Register our files.
            registry.RegisterModule(new InstalledModule(module_files, module, DateTime.Now));

            // Handle bundled mods, if we have them.
            if (module.bundles != null)
            {
                foreach (BundledModuleDescriptor stanza in module.bundles)
                {
                    var bundled = new BundledModule(stanza);

                    Version ver = registry_manager.registry.InstalledVersion(bundled.identifier);

                    if (ver != null)
                    {
                        User.WriteLine(
                            "{0} {1} already installed, skipping bundled version {2}",
                            bundled.identifier, ver, bundled.version
                            );
                        continue;
                    }

                    // Not installed, so let's get about installing it!
                    var installed_files = new Dictionary<string, InstalledModuleFile>();

                    InstallComponent(stanza, zipfile, installed_files);

                    registry.RegisterModule(new InstalledModule(installed_files, bundled, DateTime.Now));
                }
            }

            // Done! Save our registry changes!
            registry_manager.Save();

            if (onReportModInstalled != null)
            {
                onReportModInstalled(module);
            }
        }

        private string Sha1Sum(string path)
        {
            if (Path.GetFileName(path).Length == 0)
            {
                return null;
            }

            SHA1 hasher = new SHA1CryptoServiceProvider();

            try
            {
                return BitConverter.ToString(hasher.ComputeHash(File.OpenRead(path)));
            }
            catch
            {
                return null;
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
            stanza.description = "Default install (autogenerated)";
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
        /// </summary>
        private void InstallComponent(InstallableDescriptor stanza, ZipFile zipfile,
            Dictionary<string, InstalledModuleFile> module_files)
        {
            List<InstallableFile> files = FindInstallableFiles(stanza, zipfile);

            foreach (var file in files) {

                User.WriteLine("    * Copying " + file.source.Name);

                CopyZipEntry(zipfile, file.source, file.destination, file.makedir);

                // TODO: We really should be computing sha1sums again!
                module_files.Add(file.destination, new InstalledModuleFile
                {
                    sha1_sum = "" //Sha1Sum (currentTransaction.OpenFile(fullPath).TemporaryPath)
                });
            }
        }

        /// <summary>
        /// Returns a list of files to be installed from the given stanza.
        ///
        /// Throws a BadInstallLocationKraken if the install stanza targets an
        /// unknown install location.
        /// </summary>
        internal List<InstallableFile> FindInstallableFiles(InstallableDescriptor stanza, ZipFile zipfile)
        {
            string installDir;
            bool makeDirs;
            var files = new List<InstallableFile> ();

            if (stanza.install_to == "GameData")
            {
                installDir = KSPManager.CurrentInstance.GameData();
                makeDirs = true;
            }
            else if (stanza.install_to == "Ships")
            {
                installDir = KSPManager.CurrentInstance.Ships();
                makeDirs = false; // Don't allow directory creation in ships directory
            }
            else if (stanza.install_to == "Tutorial")
            {
                installDir = Path.Combine(Path.Combine(KSPManager.CurrentInstance.GameDir(), "saves"), "training");
                makeDirs = true;
            }
            else if (stanza.install_to == "GameRoot")
            {
                installDir = KSPManager.CurrentInstance.GameDir();
                makeDirs = false;
            }
            else
            {
                throw new BadInstallLocationKraken("Unknown install_to " + stanza.install_to);
            }

            // Is there a better way to extract a tree?
            string filter = "^" + stanza.file + "(/|$)";

            // O(N^2) solution, as we're walking the zipfile for each stanza.
            // Surely there's a better way, although this is fast enough we may not care.

            foreach (ZipEntry entry in zipfile)
            {
                // Skip things we don't want.
                if (!Regex.IsMatch(entry.Name, filter))
                {
                    continue;
                }

                // SKIP the file if it's a .CKAN file, these should never be copied to GameData.
                if (Regex.IsMatch(entry.Name, ".CKAN", RegexOptions.IgnoreCase))
                {
                    continue;
                }

                // Get the full name of the file.
                string outputName = entry.Name;

                // Strip off everything up to GameData/Ships
                // TODO: There's got to be a nicer way of doing path resolution.
                outputName = Regex.Replace(outputName, @"^/?(.*(GameData|Ships)/)?", "", RegexOptions.IgnoreCase);

                string full_path = Path.Combine(installDir, outputName);

                // Make the path pretty, and of course the prettiest paths use Unix separators. ;)
                full_path = full_path.Replace('\\', '/');

                InstallableFile file_info = new InstallableFile (); 
                file_info.source = entry;
                file_info.destination = full_path;
                file_info.makedir = makeDirs;

                files.Add(file_info);
            }

            return files;
        }

        private void CopyZipEntry(ZipFile zipfile, ZipEntry entry, string fullPath, bool makeDirs)
        {
            if (entry.IsDirectory)
            {
                // Skip if we're not making directories for this install.
                if (!makeDirs)
                {
                    return;
                }

                log.DebugFormat("Making directory {0}", fullPath);
                currentTransaction.CreateDirectory(fullPath);
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
                    currentTransaction.CreateDirectory(directory);
                }

                // It's a file! Prepare the streams
                Stream zipStream = zipfile.GetInputStream(entry);

                TransactionalFileWriter file = currentTransaction.OpenFileWrite(fullPath);
                FileStream output = file.Stream;

                // Copy
                zipStream.CopyTo(output);

                // Tidy up.
                zipStream.Close();
                output.Close();
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

        public void Uninstall(string modName, bool uninstallDependencies)
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
                string path = Path.Combine(KSPManager.CurrentInstance.GameDir(), file);

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
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
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
                        Directory.Delete(directory);
                    }
                    catch (Exception)
                    {
                        User.WriteLine("Couldn't delete directory {0}", directory);
                    }
                }
            }

            return;
        }
    }
}
