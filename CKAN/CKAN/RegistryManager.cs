using System.Collections.Generic;
using System.IO;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{
    public class RegistryManager
    {
        private static readonly Dictionary<string, RegistryManager> singleton =
            new Dictionary<string, RegistryManager>();

        private static readonly ILog log = LogManager.GetLogger(typeof (RegistryManager));
        private readonly string path;
        public Registry registry;

        // We require our constructor to be private so we can
        // enforce this being an instance (via Instance() above)
        private RegistryManager(string path)
        {
            this.path = path;
            LoadOrCreate();
        }

        public static RegistryManager Instance(string path = null)
        {
            if (path == null)
            {
                path = DefaultRegistry();
                log.DebugFormat("Using default CKAN registry at {0}", path);
            }
            else
            {
                log.DebugFormat("Using suppied CKAN registry at {0}", path);
            }

            if (! singleton.ContainsKey(path))
            {
                log.Debug("RegistryManager not yet active, loading...");
                singleton[path] = new RegistryManager(path);
            }

            return singleton[path];
        }

        // Default registry location
        private static string DefaultRegistry()
        {
            return Path.Combine(KSP.CkanDir(), "registry.json");
        }

        public void Load()
        {
            string json = File.ReadAllText(path);
            registry = JsonConvert.DeserializeObject<Registry>(json);
            log.DebugFormat("Loaded CKAN registry at {0}", path);
        }

        public void LoadOrCreate()
        {
            try
            {
                Load();
            }
            catch (FileNotFoundException)
            {
                Create();
                Load();
            }
            catch (DirectoryNotFoundException)
            {
                Create();
                Load();
            }
        }

        private void Create()
        {
            registry = Registry.Empty();
            log.DebugFormat("Creating new CKAN registry at {0}", path);
            Save();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(registry);
        }

        public void Save()
        {
            log.DebugFormat("Saving CKAN registry at {0}", path);
            string directoryPath = Path.GetDirectoryName(path);

            if (directoryPath == null)
            {
                log.DebugFormat("Failed to save registry, invalid path: {0}", path);
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(path, Serialize());
        }
    }
}