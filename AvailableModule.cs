namespace CKAN {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Utility class to track version -> module mappings
    /// </summary>
    public class AvailableModule {
        public Dictionary<string, CkanModule> versions = new Dictionary<string, CkanModule> ();

        public AvailableModule() { }

        public void Add(CkanModule module) {
            versions [module.version] = module;
        }

        /// <summary>
        /// Return the most recent release of a module.
        /// </summary>
        public CkanModule Latest() {
            string[] version_strings = versions.Keys.ToArray ();

            List<Version> all_versions = new List<Version> ();

            // Convert our simple strings into a list of versions
            foreach (string v in version_strings) {
                all_versions.Add (new Version (v));
            }

            // Now, sort them...

            all_versions.Sort();

            // Return the last one!
            return versions[all_versions[all_versions.Count-1].ToString()];
        }
    }
}

