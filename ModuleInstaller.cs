using System.Runtime.InteropServices;

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

    public class ModuleInstaller {
        RegistryManager registry_manager = RegistryManager.Instance();
        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        public ModuleInstallerReportProgress onReportProgress = null;

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

        public string CachedOrDownload(CkanModule module, string filename = null) {
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

        public string CachePath(string file) {
            return Path.Combine (KSP.DownloadCacheDir (), file);
        }

        public void InstallList(List<string> modules, RelationshipResolverOptions options) {
            var resolver = new RelationshipResolver (modules, options);

            User.WriteLine ("About to install...\n");

            foreach (CkanModule module in resolver.ModList()) {
                User.WriteLine (" * {0} {1}", module.identifier, module.version);
            }

            bool ok = User.YesNo ("\nContinue?");

            if (!ok) {
                log.Debug ("Halting install at user request");
                return;
            }

            User.WriteLine (""); // Just to look tidy.

            foreach (CkanModule module in resolver.ModList ()) {
                Install (module);
            }

        }

        /// <summary>
        /// Install our mod from the filename supplied.
        /// If no file is supplied, we will fetch() it first.
        /// 
        /// Does *not* resolve dependencies; this actually does the heavy listing.
        /// Use InstallList() for requests from the user.
        /// </summary>

        void Install (CkanModule module, string filename = null) {

            if (onReportProgress != null)
            {
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
                filename = CachedOrDownload (module);
            }

            // We'll need our registry to record which files we've installed.
            Registry registry = registry_manager.registry;

            // And a list of files to record them to.
            Dictionary<string, InstalledModuleFile> module_files = new Dictionary<string, InstalledModuleFile> ();

            // Open our zip file for processing
            ZipFile zipfile = new ZipFile (File.OpenRead (filename));

            int counter = 0;
            // Walk through our install instructions.
            foreach (dynamic stanza in module.install) {

                if (onReportProgress != null)
                {
                    onReportProgress(String.Format("Installing \"{0}\"", module.name), (counter * 100) / module.install.Count());
                }

                InstallComponent (stanza, zipfile, module_files);
                counter++;
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

            return;

        }

        string Sha1Sum (string path) {
            if (System.IO.Path.GetFileName(path).Length == 0)
            {
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
            } else {
                // What is the best exception to use here??
                throw new Exception ("Unknown install location: " + stanza.install_to);
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
                    sha1_sum = Sha1Sum (fullPath),
                });
            }

            return;
        }

        void CopyZipEntry (ZipFile zipfile, ZipEntry entry, string fullPath, bool makeDirs) {

            if (entry.IsDirectory) {

                // Skip if we're not making directories for this install.
                if (! makeDirs) {
                    return;
                }

                log.DebugFormat("Making directory {0}",fullPath);
                Directory.CreateDirectory (fullPath);
            } else {

                log.DebugFormat("Writing file {0}", fullPath);

                // Sometimes there are zipfiles that don't contain entries for the
                // directories their files are in. No, I understand either, but
                // the result is we have to make sure our directories exist, just in case.
                if (makeDirs) {
                    string directory = Path.GetDirectoryName (fullPath);
                    Directory.CreateDirectory (directory);
                }

                // It's a file! Prepare the streams
                Stream zipStream = zipfile.GetInputStream (entry);
                FileStream output = File.Create (fullPath);

                // Copy
                zipStream.CopyTo (output);

                // Tidy up.
                zipStream.Close ();
                output.Close ();
            }

            return;
        }

        public List<string> FindReverseDependencies(string modName) {
            var rootMod = registry_manager.registry.installed_modules[modName].source_module;

            List<string> reverseDependencies = new List<string>();

            foreach (var keyValue in registry_manager.registry.installed_modules) {
                var mod = keyValue.Value.source_module;
                bool isDependency = false;

                if (mod.depends != null)
                {
                    foreach (dynamic dependency in mod.depends)
                    {
                        if (dependency.name == modName)
                        {
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

        public void Uninstall(string modName) {
            // Find all mods that depend on this one
            var reverseDependencies = FindReverseDependencies(modName);
            foreach (var reverseDependency in reverseDependencies) {
                Uninstall(reverseDependency);
            }

            // Walk our registry to find all files for this mod.
            Dictionary<string, InstalledModuleFile> files = registry_manager.registry.installed_modules [modName].installed_files;

            foreach (string file in files.Keys) {
                string path = Path.Combine (KSP.GameDir (), file);
                try
                {
                    FileAttributes attr = File.GetAttributes(path);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {

                        // TODO: Actually prune empty directories
                        if (!System.IO.Directory.GetFiles(path).Any())
                        {
                            System.IO.Directory.Delete(path);
                        }

                        User.WriteLine("Skipping directory {0}", file);
                    }
                    else
                    {
                        User.WriteLine("Removing {0}", file);
                        File.Delete(path);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
               
            }

            // Remove from registry.

            registry_manager.registry.DeregisterModule (modName);
            registry_manager.Save ();

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