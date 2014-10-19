namespace CKAN {
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using log4net;

    /// <summary>
    /// Class for downloading the CKAN meta-info itself.
    /// </summary>
    public class Repo {

        static readonly ILog log = LogManager.GetLogger (typeof(Repo));

        // Right now we only use the default repo, but in the future we'll
        // want to let users add their own.
        public static readonly string default_ckan_repo = "https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip";

        /// <summary>
        /// Download and update the local CKAN meta-info.
        /// 
        /// Optionally takes a URL to the zipfile repo to download.
        /// 
        /// Returns the number of unique modules updated.
        /// </summary>

        static public int Update(string repo = null) {
            RegistryManager registry_manager = RegistryManager.Instance();

            // Use our default repo, unless we've been told otherwise.
            if (repo == null) {
                repo = default_ckan_repo;
            }

            log.InfoFormat ("Downloading {0}", repo);

            string repo_file = Net.Download (repo);

            // Open our zip file for processing
            ZipFile zipfile = new ZipFile (File.OpenRead (repo_file));

            // Clear our list of known modules.
            registry_manager.registry.ClearAvailable ();

            // Walk the archive, looking for .ckan files.
            string filter = @"\.ckan$";

            foreach (ZipEntry entry in zipfile) {

                string filename = entry.Name;

                // Skip things we don't want.
                if (! Regex.IsMatch (filename, filter)) {
                    log.DebugFormat ("Skipping archive entry {0}", filename);
                    continue;
                }

                log.DebugFormat ("Reading CKAN data from {0}", filename);

                // Read each file into a string.
                string metadata_json = new StreamReader(zipfile.GetInputStream(entry)).ReadToEnd();

                log.Debug ("Converting from JSON...");

                CkanModule module = CkanModule.from_string (metadata_json);

                log.InfoFormat ("Found {0} version {1}", module.identifier, module.version);

                // Hooray! Now save it in our registry.

                registry_manager.registry.AddAvailable (module);
            }

            // Save our changes!
            registry_manager.Save ();

            // Clean up!
            zipfile.Close ();
            File.Delete (repo_file);

            // Return how many we got!
            return registry_manager.registry.Available ().Count;

        }
    }
}