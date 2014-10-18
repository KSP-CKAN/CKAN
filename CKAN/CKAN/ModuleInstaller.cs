using System.Runtime.InteropServices;
using System.Threading;

namespace CKAN {

    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using System.Text.RegularExpressions;
    using log4net;

    public delegate void ModuleInstallerReportProgress(string message, int progress);

    public delegate void ModuleInstallerReportModInstalled(CkanModule module);

    public class ModuleInstaller {
        RegistryManager registry_manager = RegistryManager.Instance();
        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        public ModuleInstallerReportProgress onReportProgress = null;
        public ModuleInstallerReportModInstalled onReportModInstalled = null;

        private NetAsyncDownloader downloader = null;

        private FilesystemTransaction currentTransaction = null;

        /// <summary>
        /// Download the given mod. Returns the filename it was saved to.
        ///
        /// If no filename is provided, the standard_name() will be used.
        ///
        /// </summary>
        /// <param name="filename">Filename.</param>
        public string Download (CkanModule module, string filename = null) {

            // Generate a standard filename if none is provided.
            if (filename == null) {
                filename = module.StandardName ();
            }

            User.WriteLine ("    * Downloading " + filename + "...");

            string full_path = Path.Combine (KSP.DownloadCacheDir(), filename);

            if (onReportProgress != null) {
                onReportProgress(String.Format("Downloading \"{0}\"", module.download), 0);
            }

            return Net.Download (module.download, full_path);
        }

        public NetAsyncDownloader DownloadAsync(CkanModule[] modules, string[] filenames = null)
        {
            Uri[] urls = new Uri[modules.Length];
            string[] fullPaths = new string[modules.Length];

            for (int i = 0; i < modules.Length; i++) {
                fullPaths[i] = Path.Combine(KSP.DownloadCacheDir(), filenames[i]);
                urls[i] = modules[i].download;
            }

            downloader = new NetAsyncDownloader(urls, fullPaths);

            if (onReportProgress != null)
            {
                downloader.onProgressReport = (percent, bytesPerSecond, bytesLeft) =>
       onReportProgress(String.Format("{0} kbps - downloading - {1} MiB left", bytesPerSecond / 1024, bytesLeft / 1024 / 1024), percent);
            }

            downloader.onCompleted = (uris, strings, errors) => OnDownloadsComplete(urls, fullPaths, modules, errors);

            return downloader;
        }

        public string CachedOrDownload (CkanModule module, string filename = null) {
            if (filename == null) {
                filename = module.StandardName ();
            }

            string fullPath = CachePath (filename);

            if (File.Exists (fullPath)) {
                User.WriteLine ("    * Using {0} (cached)", filename);
                return fullPath;
            }

            return Download (module, filename);
        }

        public bool IsCached(string filename, out string fullPath)
        {
            fullPath = CachePath(filename);

            if (File.Exists(fullPath)) {
                return true;
            }

            return false;
        }

        public string CachePath (string file) {
            return Path.Combine (KSP.DownloadCacheDir (), file);
        }

        /// <summary>
        /// Installs all modules given a list of identifiers. Resolves dependencies.
        /// The function initializes a filesystem transaction, then installs all cached mods
        /// this ensures we don't waste time and bandwidth if there is an issue with any of the cached archives
        /// After this we try to download the rest of the mods (asynchronously) and install them
        /// Finally, only if everything is successful, we commit the transaction
        /// </summary>
        public void InstallList (List<string> modules, RelationshipResolverOptions options) {
            currentTransaction = new FilesystemTransaction();

            var resolver = new RelationshipResolver (modules, options);

            User.WriteLine ("About to install...\n");

            foreach (CkanModule module in resolver.ModList()) {
                User.WriteLine (" * {0} {1}", module.identifier, module.version);
            }

            bool ok = User.YesNo ("\nContinue?", FrontEndType.CommandLine);

            if (!ok) {
                log.Debug ("Halting install at user request");
                return;
            }

            User.WriteLine (""); // Just to look tidy.

            int counter = 0;
            var modList = resolver.ModList ();

            List<CkanModule> notCached = new List<CkanModule>();

            foreach (CkanModule module in modList) {
                string fullPath;
                if (IsCached(module.StandardName(), out fullPath)) {
                    Install(module, fullPath);

                    counter++;
                    if (onReportProgress != null) {
                        var percentDone = (counter * 100) / modList.Count();
                        onReportProgress(String.Format("Installing \"{0}\"", module.name), percentDone);
                    }
                }
                else
                {
                    notCached.Add(module);
                }
            }

            if (!notCached.Any())
            {
                currentTransaction.Commit();
                return;
            }

            CkanModule[] modulesToDownload = new CkanModule[notCached.Count];
            string[] modulesToDownloadPaths = new string[notCached.Count];

            for (int i = 0; i < notCached.Count; i++) {
                modulesToDownload[i] = notCached[i];
                modulesToDownloadPaths[i] = CachePath(notCached[i].StandardName());
            }

            downloader = DownloadAsync(modulesToDownload, modulesToDownloadPaths);
            downloader.StartDownload();

            lock (downloader)
            {
                Monitor.Wait(downloader);
            }
        }

        private void OnDownloadsComplete(Uri[] urls, string[] filenames, CkanModule[] modules, Exception[] errors)
        {
            bool noErrors = true;

            for (int i = 0; i < errors.Length; i++) {
                if (errors[i] != null)
                {
                    noErrors = false;
                    User.Error("Failed to download \"{0}\" - error: {1}", urls[i], errors[i].Message);
                }
            }

            if (noErrors)
            {
                for (int i = 0; i < urls.Length; i++) {
                    Install(modules[i], filenames[i]);
                }

                currentTransaction.Commit();
            }

            lock (downloader)
            {
                Monitor.Pulse(downloader);
            }
        }

        /// <summary>
        /// Install our mod from the filename supplied.
        /// If no file is supplied, we will fetch() it first.
        /// 
        /// Does *not* resolve dependencies; this actually does the heavy listing.
        /// Use InstallList() for requests from the user.
        /// </summary>

        private void Install (CkanModule module, string filename = null) {

            if (onReportProgress != null) {
                onReportProgress(String.Format("Installing \"{0}\"", module.name), 0);
            }

            User.WriteLine (module.identifier + ":\n");

            Version version = registry_manager.registry.InstalledVersion (module.identifier);

            if (version != null) {
                // TODO: Check if we can upgrade!
                User.WriteLine("    {0} {1} already installed, skipped", module.identifier, version);
                return;
            }

            // Fetch our file if we don't already have it.
            if (filename == null) {
                filename = CachedOrDownload(module);
            }

            // We'll need our registry to record which files we've installed.
            Registry registry = registry_manager.registry;

            // And a list of files to record them to.
            Dictionary<string, InstalledModuleFile> module_files = new Dictionary<string, InstalledModuleFile> ();

            ZipFile zipfile = null;

            // Open our zip file for processing
            try {
                zipfile = new ZipFile(File.OpenRead(filename));
            }
            catch (Exception) {
                User.Error("Failed to open archive \"{0}\"", filename);
                return;
            }

            // Walk through our install instructions.
            foreach (dynamic stanza in module.install) {
                InstallComponent (stanza, zipfile, module_files);
            }

            // Register our files.
            registry.RegisterModule (new InstalledModule (module_files, module, DateTime.Now));

            // Handle bundled mods, if we have them.
            if (module.bundles != null) {

                foreach (dynamic stanza in module.bundles) {
                    BundledModule bundled = new BundledModule (stanza);

                    Version ver = registry_manager.registry.InstalledVersion (bundled.identifier);

                    if (ver != null) {
                        User.WriteLine (
                            "{0} {1} already installed, skipping bundled version {2}",
                            bundled.identifier, ver, bundled.version
                        );
                        continue;
                    }

                    // Not installed, so let's get about installing it!
                    Dictionary<string, InstalledModuleFile> installed_files = new Dictionary<string, InstalledModuleFile> ();

                    InstallComponent (stanza, zipfile, installed_files);

                    registry.RegisterModule (new InstalledModule (installed_files, bundled, DateTime.Now));
                }
            }

            // Done! Save our registry changes!
            registry_manager.Save();

            if (onReportModInstalled != null) {
                onReportModInstalled(module);
            }
        }

        string Sha1Sum (string path) {
            if (System.IO.Path.GetFileName(path).Length == 0) {
                return null;
            }

            SHA1 hasher = new SHA1CryptoServiceProvider();

            try {
                return BitConverter.ToString(hasher.ComputeHash (File.OpenRead (path)));
            }
            catch {
                return null;
            };
        }

        void InstallComponent (dynamic stanza, ZipFile zipfile, Dictionary<string, InstalledModuleFile> module_files) {
            string fileToInstall = (string)stanza.file;

            User.WriteLine ("    * Installing " + fileToInstall);

            string installDir;
            bool makeDirs;

            if (stanza.install_to == "GameData") {
                installDir = KSP.GameData ();
                makeDirs = true;
            } else if (stanza.install_to == "Ships") {
                installDir = KSP.Ships ();
                makeDirs = false; // Don't allow directory creation in ships directory
            }
            else if (stanza.install_to == "Tutorial")
            {
                installDir = Path.Combine(Path.Combine(KSP.GameDir(), "saves"), "training");
                makeDirs = true;
            }
            else if (stanza.install_to == "GameRoot")
            {
                installDir = KSP.GameDir();
                makeDirs = false;
            } else {
                // What is the best exception to use here??
                throw new Exception("Unknown install location: " + stanza.install_to);
            }

            // User.WriteLine("InstallDir is "+installDir);

            // Is there a better way to extract a tree?
            string filter = "^" + stanza.file + "(/|$)";

            // O(N^2) solution, as we're walking the zipfile for each stanza.
            // Surely there's a better way, although this is fast enough we may not care.

            foreach (ZipEntry entry in zipfile) {

                // Skip things we don't want.
                if (! Regex.IsMatch (entry.Name, filter)) {
                    continue;
                }

                // SKIP the file if it's a .CKAN file, these should never be copied to GameData.
                if (Regex.IsMatch (entry.Name, ".CKAN", RegexOptions.IgnoreCase)) {
                    continue;
                }

                // Get the full name of the file.
                string outputName = entry.Name;

                // Strip off everything up to GameData/Ships
                // TODO: There's got to be a nicer way of doing path resolution.
                outputName = Regex.Replace (outputName, @"^/?(.*(GameData|Ships)/)?", "");

                // Aww hell yes, let's write this file out!

                string fullPath = Path.Combine (installDir, outputName);
                // User.WriteLine (fullPath);

                CopyZipEntry (zipfile, entry, fullPath, makeDirs);
                User.WriteLine("    * Copying " + entry.ToString());

                module_files.Add (Path.Combine((string) stanza.install_to, outputName), new InstalledModuleFile {
                    sha1_sum = ""//Sha1Sum (currentTransaction.OpenFile(fullPath).TemporaryPath)
                });
            }

            return;
        }

        void CopyZipEntry (ZipFile zipfile, ZipEntry entry, string fullPath, bool makeDirs) {

            if (entry.IsDirectory) {

                // Skip if we're not making directories for this install.
                if (!makeDirs) {
                    return;
                }

                log.DebugFormat("Making directory {0}",fullPath);
                currentTransaction.CreateDirectory(fullPath);
            } else {

                log.DebugFormat("Writing file {0}", fullPath);

                // Sometimes there are zipfiles that don't contain entries for the
                // directories their files are in. No, I understand either, but
                // the result is we have to make sure our directories exist, just in case.
                if (makeDirs) {
                    string directory = Path.GetDirectoryName (fullPath);
                    currentTransaction.CreateDirectory(directory);
                }

                // It's a file! Prepare the streams
                Stream zipStream = zipfile.GetInputStream (entry);

                var file = currentTransaction.OpenFileWrite(fullPath);
                FileStream output = file.Stream;
                // Copy
                zipStream.CopyTo (output);

                // Tidy up.
                zipStream.Close ();
                output.Close ();
            }

            return;
        }

        public List<string> FindReverseDependencies(string modName) {
            List<string> reverseDependencies = new List<string>();

            // loop through all installed modules
            foreach (var keyValue in registry_manager.registry.installed_modules) {
                var mod = keyValue.Value.source_module;
                bool isDependency = false;

                if (mod.depends != null) {
                    foreach (dynamic dependency in mod.depends) {
                        if (dependency.name == modName) {
                            isDependency = true;
                            break;
                        }
                    }
                }

                if (isDependency) {
                    reverseDependencies.Add(mod.identifier);
                }
            }

            return reverseDependencies;
        }

        public void Uninstall(string modName, bool uninstallDependencies) {
            if (!registry_manager.registry.IsInstalled(modName))
            {
                User.Error("Trying to uninstall {0} but it's not installed", modName);
                return;
            }
            
            // Find all mods that depend on this one
            if (uninstallDependencies)
            {
                var reverseDependencies = FindReverseDependencies(modName);
                foreach (var reverseDependency in reverseDependencies)
                {
                    Uninstall(reverseDependency, uninstallDependencies);
                }
            }
            
            // Walk our registry to find all files for this mod.
            Dictionary<string, InstalledModuleFile> files = registry_manager.registry.installed_modules [modName].installed_files;

            HashSet<string> directoriesToDelete = new HashSet<string>();

            foreach (string file in files.Keys) {
                string path = Path.Combine (KSP.GameDir (), file);

                try {
                    FileAttributes attr = File.GetAttributes(path);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                        directoriesToDelete.Add(path);
                    }  else {
                        User.WriteLine("Removing {0}", file);
                        File.Delete(path);
                    }
                }
                catch (Exception) {
                    continue;
                }
            }

            // Remove from registry.

            registry_manager.registry.DeregisterModule (modName);
            registry_manager.Save();

            foreach (var directory in directoriesToDelete) {
                if (!System.IO.Directory.GetFiles(directory).Any()) {
                    try {
                        Directory.Delete(directory);
                    } catch (Exception) {
                        User.WriteLine("Couldn't delete directory {0}", directory);
                    }
                }
            }

            // And we're done! :)
            return;
        }
    }


    public class ModuleNotFoundException : Exception {
        public string module;
        public string version;

        // TODO: Is there a way to set the stringify version of this?
        public ModuleNotFoundException (string mod, string ver = null) {
            module = mod;
            version = ver;
        }
    }
}
