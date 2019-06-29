using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ChinhDo.Transactions.FileManager;
using CKAN.DLC;
using CKAN.Versioning;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    public class RegistryManager : IDisposable
    {
        private static readonly Dictionary<string, RegistryManager> registryCache =
            new Dictionary<string, RegistryManager>();

        private static readonly ILog log = LogManager.GetLogger(typeof (RegistryManager));
        private readonly string path;
        public readonly string lockfilePath;
        private FileStream lockfileStream = null;
        private StreamWriter lockfileWriter = null;


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

            this.path    = Path.Combine(path, "registry.json");
            lockfilePath = Path.Combine(path, "registry.locked");

            // Create a lock for this registry, so we cannot touch it again.
            if (!GetLock())
            {
                log.DebugFormat("Unable to acquire registry lock: {0}", lockfilePath);
                throw new RegistryInUseKraken(lockfilePath);
            }

            try
            {
                LoadOrCreate();
            }
            catch
            {
                // Clean up the lock file
                Dispose(false);
                throw;
            }

            // We don't cause an inconsistency error to stop the registry from being loaded,
            // because then the user can't do anything to correct it. However we're
            // sure as hell going to complain if we spot one!
            try
            {
                registry.CheckSanity();
            }
            catch (InconsistentKraken kraken)
            {
                log.ErrorFormat("Loaded registry with inconsistencies:\r\n\r\n{0}", kraken.InconsistenciesPretty);
            }
        }

        #region destruction

        // See http://stackoverflow.com/a/538238/19422 for an awesome explanation of
        // what's going on here.

        /// <summary>
        /// Releases all resource used by the <see cref="CKAN.RegistryManager"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CKAN.RegistryManager"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CKAN.RegistryManager"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="CKAN.RegistryManager"/> so
        /// the garbage collector can reclaim the memory that the <see cref="CKAN.RegistryManager"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool safeToAlsoFreeManagedObjects)
        {
            // Right now we just release our lock, and leave everything else
            // to the GC, but if we were implementing the full pattern we'd also
            // free managed (.NET core) objects when called with a true value here.

            ReleaseLock();
            var directory = ksp.CkanDir();
            if (!registryCache.ContainsKey(directory))
            {
                log.DebugFormat("Registry not in cache at {0}", directory);
                return;
            }

            log.DebugFormat("Dispose of registry at {0}", directory);
            if (!registryCache.Remove(directory))
            {
                throw new RegistryInUseKraken(directory);
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="CKAN.RegistryManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~RegistryManager()
        {
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// If the lock file exists, it contains the id of the owning process.
        /// If there is no process with that id, then the lock file is stale.
        /// If there IS a process with that id, there are two possibilities:
        ///   1. It's actually the CKAN process that owns the lock
        ///   2. It's some other process that got the same id by coincidence
        /// If #1, it's definitely not stale.
        /// If #2, it's stale, but we don't know that.
        /// Since we can't tell the difference between #1 and #2, we need to
        /// keep the lock file.
        /// If we encounter any other errors (permissions, corrupt file, etc.),
        /// then we need to keep the file.
        /// </summary>
        private void CheckStaleLock()
        {
            log.DebugFormat("Checking for stale lock file at {0}", lockfilePath);
            if (File.Exists(lockfilePath))
            {
                log.DebugFormat("Lock file found at {0}", lockfilePath);
                string contents;
                try
                {
                    contents = File.ReadAllText(lockfilePath);
                }
                catch
                {
                    // If we can't read the file, we can't check whether it's stale.
                    log.DebugFormat("Lock file unreadable at {0}", lockfilePath);
                    return;
                }
                log.DebugFormat("Lock file contents: {0}", contents);
                Int32 pid;
                if (Int32.TryParse(contents, out pid))
                {
                    // File contains a valid integer.
                    try
                    {
                        // Try to find the corresponding process.
                        log.DebugFormat("Looking for process with ID: {0}", pid);
                        Process.GetProcessById(pid);
                        // If no exception is thrown, then a process with this id
                        // is running, and it's not safe to delete the lock file.
                        // We are done.
                    }
                    catch (ArgumentException)
                    {
                        // ArgumentException means the process doesn't exist,
                        // so the lock file is stale and we can delete it.
                        try
                        {
                            log.DebugFormat("Deleting stale lock file at {0}", lockfilePath);
                            File.Delete(lockfilePath);
                        }
                        catch
                        {
                            // If we can't delete the file, then all this was for naught,
                            // but at least we haven't crashed.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to lock the registry by creating a lock file.
        /// </summary>
        /// <returns><c>true</c>, if lock was gotten, <c>false</c> otherwise.</returns>
        public bool GetLock()
        {
            try
            {
                CheckStaleLock();

                log.DebugFormat("Trying to create lock file: {0}", lockfilePath);

                lockfileStream = new FileStream(lockfilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 512, FileOptions.DeleteOnClose);

                // Write the current process ID to the file.
                lockfileWriter = new StreamWriter(lockfileStream);
                lockfileWriter.Write(Process.GetCurrentProcess().Id);
                lockfileWriter.Flush();
                // The lock file is now locked and open.
                log.DebugFormat("Lock file created: {0}", lockfilePath);
            }
            catch (IOException)
            {
                log.DebugFormat("Failed to create lock file: {0}", lockfilePath);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Release the lock by deleting the file, but only if we managed to create the file.
        /// </summary>
        public void ReleaseLock()
        {
            // We have to dispose our writer first, otherwise it cries when
            // it finds the stream is already disposed.
            if (lockfileWriter != null)
            {
                log.DebugFormat("Disposing of lock file writer at {0}", lockfilePath);
                lockfileWriter.Dispose();
                lockfileWriter = null;
            }

            // Disposing the writer also disposes the underlying stream,
            // but we're extra tidy just in case.
            if (lockfileStream != null)
            {
                log.DebugFormat("Disposing of lock file stream at {0}", lockfilePath);
                lockfileStream.Dispose();
                lockfileStream = null;
            }

        }

        /// <summary>
        /// Returns an instance of the registry manager for the KSP install.
        /// The file `registry.json` is assumed.
        /// </summary>
        public static RegistryManager Instance(KSP ksp)
        {
            string directory = ksp.CkanDir();
            if (!registryCache.ContainsKey(directory))
            {
                log.DebugFormat("Preparing to load registry at {0}", directory);
                registryCache[directory] = new RegistryManager(directory, ksp);
            }

            return registryCache[directory];
        }

        /// <summary>
        /// Call Dispose on all the registry managers in the cache.
        /// Useful for exiting without Dispose-related exceptions.
        /// Note that this also REMOVES these entries from the cache.
        /// </summary>
        public static void DisposeAll()
        {
            foreach (RegistryManager rm in new List<RegistryManager>(registryCache.Values))
            {
                rm.Dispose();
            }
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

            log.DebugFormat("Trying to load registry from {0}", path);
            string json = File.ReadAllText(path);
            log.Debug("Registry JSON loaded; parsing...");
            // A 0-byte registry.json file loads as null without exceptions
            registry = JsonConvert.DeserializeObject<Registry>(json, settings)
                ?? Registry.Empty();
            log.Debug("Registry loaded and parsed");
            ScanDlc();
            log.InfoFormat("Loaded CKAN registry at {0}", path);
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
            catch (Exception ex)
            {
                log.ErrorFormat("Uncaught exception loading registry: {0}", ex.ToString());
                throw;
            }

            AscertainDefaultRepo();
        }

        private void Create()
        {
            log.InfoFormat("Creating new CKAN registry at {0}", path);
            registry = Registry.Empty();
            Save();
        }

        private void AscertainDefaultRepo()
        {
            var repositories = registry.Repositories ?? new SortedDictionary<string, Repository>();

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
                writer.Indentation = 0;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, registry);
            }

            return sw + Environment.NewLine;
        }

        private string SerializeCurrentInstall(bool recommmends = false, bool with_versions = true)
        {
            string kspInstanceName = ksp.Name;
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
                .Where(mod => !(mod.Value is ProvidesModuleVersion || mod.Value is UnmanagedModuleVersion)))
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
            TxFileManager file_transaction = new TxFileManager();

            log.InfoFormat("Saving CKAN registry at {0}", path);

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

            ExportInstalled(
                Path.Combine(directoryPath, $"installed-{ksp.Name}.ckan"),
                false, true
            );
            if (!Directory.Exists(ksp.InstallHistoryDir()))
            {
                Directory.CreateDirectory(ksp.InstallHistoryDir());
            }
            ExportInstalled(
                Path.Combine(
                    ksp.InstallHistoryDir(),
                    $"installed-{ksp.Name}-{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.ckan"
                ),
                false, true
            );
        }

        /// <summary>
        /// Save a custom .ckan file that contains all the currently
        /// installed mods as dependencies.
        /// </summary>
        /// <param name="path">Desired location of file</param>
        /// <param name="recommends">True to save the mods as recommended, false for depends</param>
        /// <param name="with_versions">True to include the mod versions in the file, false to omit them</param>
        public void ExportInstalled(string path, bool recommends, bool with_versions)
        {
            TxFileManager file_transaction = new TxFileManager();

            string serialized = SerializeCurrentInstall(recommends, with_versions);
            file_transaction.WriteAllText(path, serialized);
        }

        /// <summary>
        /// Look for DLC installed in GameData
        /// </summary>
        /// <returns>
        /// True if not the same list as last scan, false otherwise
        /// </returns>
        public bool ScanDlc()
        {
            var dlc = new Dictionary<string, UnmanagedModuleVersion>(registry.InstalledDlc);
            UnmanagedModuleVersion foundVer;
            bool changed = false;

            registry.ClearDlc();

            var testDlc = TestDlcScan();
            foreach (var i in testDlc)
            {
                if (!changed
                    && (!dlc.TryGetValue(i.Key, out foundVer)
                        || foundVer != i.Value))
                {
                    changed = true;
                }
                registry.RegisterDlc(i.Key, i.Value);
            }

            var wellKnownDlc = WellKnownDlcScan();
            foreach (var i in wellKnownDlc)
            {
                if (!changed
                    && (!dlc.TryGetValue(i.Key, out foundVer)
                        || foundVer != i.Value))
                {
                    changed = true;
                }
                registry.RegisterDlc(i.Key, i.Value);
            }
            
            // Check if anything got removed
            if (!changed)
            {
                foreach (var i in dlc)
                {
                    if (!registry.InstalledDlc.TryGetValue(i.Key, out foundVer)
                        || foundVer != i.Value)
                    {
                        changed = true;
                        break;
                    }
                }
            }
            return changed;
        }

        private Dictionary<string, UnmanagedModuleVersion> TestDlcScan()
        {
            var dlc = new Dictionary<string, UnmanagedModuleVersion>();

            var dlcDirectory = Path.Combine(ksp.CkanDir(), "dlc");
            if (Directory.Exists(dlcDirectory))
            {
                foreach (var f in Directory.EnumerateFiles(dlcDirectory, "*.dlc", SearchOption.TopDirectoryOnly))
                {
                    var id = $"{Path.GetFileNameWithoutExtension(f)}-DLC";
                    var ver = File.ReadAllText(f).Trim();

                    dlc[id] = new UnmanagedModuleVersion(ver);
                }
            }

            return dlc;
        }

        private Dictionary<string, UnmanagedModuleVersion> WellKnownDlcScan()
        {
            var dlc = new Dictionary<string, UnmanagedModuleVersion>();

            var detectors = new IDlcDetector[] { new BreakingGroundDlcDetector(), new MakingHistoryDlcDetector() };

            foreach (var d in detectors)
            {
                if (d.IsInstalled(ksp, out var identifier, out var version))
                {
                    dlc[identifier] = version ?? new UnmanagedModuleVersion(null);
                }
            }

            return dlc;
        }
    }
}
