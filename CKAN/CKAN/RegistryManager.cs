namespace CKAN {

    using System;
    using System.IO;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using log4net;

    public class RegistryManager {

        string path;
        public Registry registry; 
        static Dictionary<string, RegistryManager> singleton = new Dictionary<string, RegistryManager> ();
        static readonly ILog log = LogManager.GetLogger(typeof(RegistryManager));

        public static RegistryManager Instance (string path = null) {

            if (path == null) {
                path = DefaultRegistry ();
                log.DebugFormat ("Using default CKAN registry at {0}", path);
            } else {
                log.DebugFormat ("Using suppied CKAN registry at {0}", path);
            }

            if (! singleton.ContainsKey (path)) {
                log.Debug ("RegistryManager not yet active, loading...");
                singleton [path] = new RegistryManager (path);
            }

            return singleton [path];
        }

        // We require our constructor to be private so we can
        // enforce this being an instance (via Instance() above)
        private RegistryManager (string path) {
            this.path = path;
            this.LoadOrCreate ();
        }

        // Default registry location
        private static string DefaultRegistry() {
            return Path.Combine (KSP.CkanDir(), "registry.json");
        }

        public void Load() {
            string json = System.IO.File.ReadAllText(path);
            registry = JsonConvert.DeserializeObject<Registry>(json);
            log.DebugFormat("Loaded CKAN registry at {0}", path);
        }

        public void LoadOrCreate() {
            try {
                Load ();
            }
            catch (System.IO.FileNotFoundException) {
                Create ();
                Load ();
            }
        }

        void Create() {
            registry = Registry.Empty ();
            log.DebugFormat ("Creating new CKAN registry at {0}", path);
            Save ();
        }

        public string Serialise () {
            return JsonConvert.SerializeObject (registry);
        }

        public void Save () {
            log.DebugFormat ("Saving CKAN registry at {0}", path);
            System.IO.File.WriteAllText(path, Serialise ());
        }
    }
}
