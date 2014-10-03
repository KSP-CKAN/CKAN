using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CKAN
{
    public class RegistryManager
    {
        string path;
        public Registry registry; 
        static Dictionary<string, RegistryManager> singleton = new Dictionary<string, RegistryManager> ();

        public static RegistryManager Instance (string path = null) {

            if (path == null) {
                path = defaultRegistry ();
            }

            // If we've already got a registry for this, then return it.
            if (! singleton.ContainsKey (path)) {
                singleton [path] = new RegistryManager (path);
            }

            return singleton [path];
        }

        // We require our constructor to be private so we can
        // enforce this being an instance.
        private RegistryManager (string path)
        {

            this.path = path;
            this.load_or_create ();

            singleton [path] = this;
        }

        // Default registry location
        private static string defaultRegistry() {
            return Path.Combine (KSP.ckanDir(), "registry.json");
        }

        public void load() {
            string json = System.IO.File.ReadAllText(path);
            registry = JsonConvert.DeserializeObject<Registry>(json);
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
            save ();
        }

        public string serialise () {
            return JsonConvert.SerializeObject (registry);
        }

        public void save () {
            System.IO.File.WriteAllText(path, serialise ());
        }
    }
}
