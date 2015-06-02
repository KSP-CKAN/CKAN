using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChinhDo.Transactions;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        private readonly KSP ksp;

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
            try
            {
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

        /// <summary>
        /// Returns the currently installed modules in json format suitable for outputting to a ckan file.
        /// Defaults to using depends and with version numbers.
        /// </summary>
        /// <param name="recommmends">If the json should use a recommends field instead of depends</param>
        /// <param name="with_versions">If version numbers should be included</param>
        /// <returns>String containing a valid ckan file</returns>
        public string CurrentInstallAsCKAN(bool recommmends, bool with_versions)
        {
            return SerializeCurrentInstall(recommmends, with_versions);
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

            AscertainDefaultRepo();
        }

        private void Create()
        {
            registry = Registry.Empty();
            log.DebugFormat("Creating new CKAN registry at {0}", path);
            Save();
        }

        private void AscertainDefaultRepo()
        {
            SortedDictionary<string, Repository> repositories = registry.Repositories;

            if (repositories == null)
            {
                repositories = new SortedDictionary<string, Repository>();
            }

            if (repositories.Count == 0)
            {
                repositories.Add(Repository.default_ckan_repo_name,
                    new Repository(Repository.default_ckan_repo_name, Repository.default_ckan_repo_uri));
            }

            registry.Repositories = repositories;
        }

        private string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 1;
                writer.IndentChar = '\t';

                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, registry);
            }

            return sw + Environment.NewLine;
        }

        private string SerializeCurrentInstall(bool recommmends = false, bool with_versions = true)
        {
            // TODO how do we obtain the name of the current KSP instance?
            string kspInstanceName = "default";
            string name = "installed-" + kspInstanceName;

            var installed = new JObject();
            installed["kind"] = "metapackage";
            installed["abstract"] = "A list of modules installed on the " + kspInstanceName + " KSP instance";
            installed["name"] = name;
            installed["license"] = "unknown";
            installed["version"] = DateTime.UtcNow.ToString("yyyy.MM.dd.hh.mm.ss");
            installed["identifier"] = name;
            installed["spec_version"] = "v1.6";

            var mods = new JArray();
            foreach (var mod in registry.Installed()
                .Where(mod => !(mod.Value is ProvidesVersion || mod.Value is DllVersion)))
            {
                var module = new JObject();
                module["name"] = mod.Key;
                if (with_versions)
                {
                    module["version"] = mod.Value.ToString();
                }
                mods.Add(module);
            }

            installed[recommmends ? "recommends" : "depends"] = mods;

            var sw = new StringWriter(new StringBuilder());
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 1;
                writer.IndentChar = '\t';

                new JsonSerializer().Serialize(writer, installed);
            }

            return sw + Environment.NewLine;
        }

        public void Save(bool enforce_consistency = true)
        {
            log.DebugFormat("Saving CKAN registry at {0}", path);

            if (enforce_consistency)
            {
                // No saving the registry unless it's in a sane state.
                registry.CheckSanity();
            }

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

            // TODO how do we obtain the name of the current KSP instance?
            string kspInstanceName = "default";
            string installedModsPath = Path.Combine(directoryPath, "installed-" + kspInstanceName + ".ckan");
            file_transaction.WriteAllText(installedModsPath, SerializeCurrentInstall());
        }
    }
}