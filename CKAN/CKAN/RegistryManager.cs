using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using log4net;

namespace CKAN
{
    public class RegistryManager
    {
        string path;
        public Registry registry; 
        static Dictionary<string, RegistryManager> singleton = new Dictionary<string, RegistryManager> ();
        static readonly ILog log = LogManager.GetLogger(typeof(RegistryManager));

        public static RegistryManager Instance (string path = null) {

            if (path == null) {
                path = defaultRegistry ();
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
        private RegistryManager (string path)
        {
            this.path = path;
            this.load_or_create ();
        }

        // Default registry location
        private static string defaultRegistry() {
            return Path.Combine (KSP.ckanDir(), "registry.json");
        }

        public void load() {
            string json = System.IO.File.ReadAllText(path);
            registry = JsonConvert.DeserializeObject<Registry>(json);
            log.DebugFormat("Loaded CKAN registry at {0}", path);
        }

        public void load_or_create() {
            try {
                load ();
            }
            catch (System.IO.FileNotFoundException) {
                create ();
                load ();
            }
        }

        void create() {
            registry = Registry.empty ();
            log.DebugFormat ("Creating new CKAN registry at {0}", path);
            save ();
        }

        public string serialise () {
            return JsonConvert.SerializeObject (registry);
        }

        public void save () {
            log.DebugFormat ("Saving CKAN registry at {0}", path);
            System.IO.File.WriteAllText(path, serialise ());
        }
    }
}
