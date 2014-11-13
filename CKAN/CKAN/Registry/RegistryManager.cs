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

        // The only reason we have a KSP field is so we can pass it to the registry
        // when deserialising, and *it* only needs it to do registry upgrades.
        // We could get rid of all of this if we declare we no longer wish to support
        // older registry formats.
        private KSP ksp;

        public Registry registry;

        // We require our constructor to be private so we can
        // enforce this being an instance (via Instance() above)
        private RegistryManager(string path, KSP ksp)
        {
            this.ksp = ksp;

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
        /// Returns an instance of the registry manager for the KSP install.
        /// The file `registry.json` is assumed.
        /// </summary>
        public static RegistryManager Instance(KSP ksp)
        {
            string directory = ksp.CkanDir();
            if (!singleton.ContainsKey(directory))
            {
                log.DebugFormat("Preparing to load registry at {0}", directory);
                singleton[directory] = new RegistryManager(directory, ksp);
            }

            return singleton[directory];
        }

        private void Load()
        {

            // Our registry needs to know our KSP install when upgrading from older
            // registry formats. This lets us encapsulate that to make it available
            // after deserialisation.
            var settings = new JsonSerializerSettings
            {
                Context = new System.Runtime.Serialization.StreamingContext(
                    System.Runtime.Serialization.StreamingContextStates.Other,
                    ksp
                )
            };

            string json = File.ReadAllText(path);
            registry = JsonConvert.DeserializeObject<Registry>(json, settings);
            log.DebugFormat("Loaded CKAN registry at {0}", path);
        }

        private void LoadOrCreate()
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

        private string Serialize()
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
                log.ErrorFormat("Failed to save registry, invalid path: {0}", path);
                throw new DirectoryNotFoundKraken(path, "Can't find a directory in " + path);
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            file_transaction.WriteAllText(path, Serialize());
        }
    }
}