using System.Collections.Generic;
using System.IO;
using log4net;
using Newtonsoft.Json;
using ChinhDo.Transactions;

namespace CKAN
{
    public class RegistryManager
    {
        private static readonly Dictionary<string, RegistryManager> singleton =
            new Dictionary<string, RegistryManager>();

        private static readonly ILog log = LogManager.GetLogger(typeof (RegistryManager));
        private readonly string path;
        private readonly TxFileManager file_transaction = new TxFileManager();

        public Registry registry;

        // We require our constructor to be private so we can
        // enforce this being an instance (via Instance() above)
        private RegistryManager(string path)
        {
            this.path = Path.Combine(path, "registry.json");
            LoadOrCreate();

            // We don't cause an inconsistency error to stop the registry from being loaded,
            // because then the user can't do anything to correct it. However we're
            // sure as hell going to complain if we spot one!
            try {
                registry.CheckSanity();
            }
            catch (InconsistentKraken kraken)
            {
                log.ErrorFormat("Loaded registry with inconsistencies:\n\n{0}", kraken.InconsistenciesPretty);
            }
        }

        /// <summary>
        /// Returns an instance of the registry manager for the given path.
        /// The file `registry.json` is assumed.
        /// </summary>
        public static RegistryManager Instance(string directory)
        {
            log.DebugFormat("Using suppied CKAN registry at {0}", directory);

            if (!singleton.ContainsKey(directory))
            {
                log.Debug("RegistryManager not yet active, loading...");
                singleton[directory] = new RegistryManager(directory);
            }

            return singleton[directory];
        }

        /// <summary>
        /// Returns the registry manager for the supplied KSP instance.
        /// </summary>
        public static RegistryManager Instance(KSP ksp)
        {
            return Instance(ksp.CkanDir());
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

            // No saving the registry unless it's in a sane state.
            registry.CheckSanity();

            string directoryPath = Path.GetDirectoryName(path);

            if (directoryPath == null)
            {
                log.DebugFormat("Failed to save registry, invalid path: {0}", path);
                // TODO: Throw a friggin exception!
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            file_transaction.WriteAllText(path, Serialize());
        }
    }
}