using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using ChinhDo.Transactions.FileManager;
using log4net;
using Newtonsoft.Json;

using CKAN.Configuration;
using CKAN.Versioning;

namespace CKAN
{
    public class RegistryManager : IDisposable
    {
        private static readonly Dictionary<string, RegistryManager> registryCache =
            new Dictionary<string, RegistryManager>();

        private static readonly ILog log = LogManager.GetLogger(typeof(RegistryManager));
        private readonly string path;
        public readonly string lockfilePath;
        private FileStream?   lockfileStream = null;
        private StreamWriter? lockfileWriter = null;

        private readonly GameInstance gameInstance;

        public Registry registry;

        /// <summary>
        /// If loading the registry failed, the parsing error text, else null.
        /// </summary>
        public string? previousCorruptedMessage;

        /// <summary>
        /// If loading the registry failed, the location to which we moved it, else null.
        /// </summary>
        public string? previousCorruptedPath;

        private static string InstanceRegistryLockPath(string ckanDirPath)
            => Path.Combine(ckanDirPath, "registry.locked");

        public static bool IsInstanceMaybeLocked(string ckanDirPath)
            => File.Exists(InstanceRegistryLockPath(ckanDirPath));

        // We require our constructor to be private so we can
        // enforce this being an instance (via Instance() above)
        private RegistryManager(string                          path,
                                GameInstance                    inst,
                                RepositoryDataManager           repoData,
                                IReadOnlyCollection<Repository> initialRepositories)
        {
            gameInstance = inst;

            this.path    = Path.Combine(path, "registry.json");
            lockfilePath = InstanceRegistryLockPath(path);

            // Create a lock for this registry, so we cannot touch it again.
            if (!GetLock())
            {
                log.DebugFormat("Unable to acquire registry lock: {0}", lockfilePath);
                throw new RegistryInUseKraken(lockfilePath);
            }

            try
            {
                Load(repoData, initialRepositories);
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
                // Only log an error for this if user-interactive,
                // automated tools do not care that no one picked a Scatterer config
                if (gameInstance.User.Headless)
                {
                    log.InfoFormat("Loaded registry with inconsistencies:\r\n\r\n{0}", kraken.Message);
                }
                else
                {
                    log.ErrorFormat("Loaded registry with inconsistencies:\r\n\r\n{0}", kraken.Message);
                }
            }
        }

        #region destruction

        // See http://stackoverflow.com/a/538238/19422 for an awesome explanation of
        // what's going on here.

        /// <summary>
        /// Releases all resource used by the <see cref="RegistryManager"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="RegistryManager"/>. The
        /// <see cref="Dispose()"/> method leaves the <see cref="RegistryManager"/> in an unusable state. After
        /// calling <see cref="Dispose()"/>, you must release all references to the <see cref="RegistryManager"/> so
        /// the garbage collector can reclaim the memory that the <see cref="RegistryManager"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable IDE0060
        protected void Dispose(bool safeToAlsoFreeManagedObjects)
        #pragma warning restore IDE0060
        {
            // Right now we just release our lock, and leave everything else
            // to the GC, but if we were implementing the full pattern we'd also
            // free managed (.NET core) objects when called with a true value here.

            ReleaseLock();
            var directory = gameInstance.CkanDir();
            if (!registryCache.ContainsKey(directory))
            {
                log.DebugFormat("Registry not in cache at {0}", directory);
                return;
            }

            log.DebugFormat("Dispose of registry at {0}", directory);
            registryCache.Remove(directory);
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
            if (IsInstanceMaybeLocked(gameInstance.CkanDir()))
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
                if (int.TryParse(contents, out int pid))
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
        /// Returns an instance of the registry manager for the game instance.
        /// The file `registry.json` is assumed.
        /// </summary>
        public static RegistryManager Instance(GameInstance                     inst,
                                               RepositoryDataManager            repoData,
                                               IReadOnlyCollection<Repository>? repositories = null)
        {
            string directory = inst.CkanDir();
            if (!registryCache.ContainsKey(directory))
            {
                log.DebugFormat("Preparing to load registry at {0}", directory);
                registryCache[directory] = new RegistryManager(directory, inst, repoData,
                                                               repositories ?? Array.Empty<Repository>());
            }

            return registryCache[directory];
        }

        public static void DisposeInstance(GameInstance inst)
        {
            if (registryCache.TryGetValue(inst.CkanDir(), out RegistryManager? regMgr))
            {
                regMgr.Dispose();
            }
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

        [MemberNotNull(nameof(registry))]
        private void Load(RepositoryDataManager           repoData,
                          IReadOnlyCollection<Repository> repositories)
        {
            try
            {
                registry = LoadRegistry(gameInstance, repoData);
                AscertainDefaultRepo();
            }
            catch (IOException exc) when (exc is FileNotFoundException or DirectoryNotFoundException)
            {
                Create(repoData, repositories);
            }
            catch (RegistryVersionNotSupportedKraken kraken)
            {
                // Throw a new one with the full path, since Registry doesn't know it
                throw new RegistryVersionNotSupportedKraken(
                    kraken.requestVersion,
                    string.Format(Properties.Resources.RegistryManagerRegistryVersionNotSupported,
                                  Platform.FormatPath(path)));
            }
            catch (JsonException exc)
            {
                previousCorruptedMessage = exc.Message;
                previousCorruptedPath    = path + "_CORRUPTED_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                log.ErrorFormat("{0} is corrupted, archiving to {1}: {2}",
                    path, previousCorruptedPath, previousCorruptedMessage);
                File.Move(path, previousCorruptedPath);
                Create(repoData, repositories);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Uncaught exception loading registry: {0}", ex.ToString());
                throw;
            }
        }

        public static Registry? ReadOnlyRegistry(GameInstance          inst,
                                                 RepositoryDataManager repoData)
            => Utilities.DefaultIfThrows(() => registryCache.TryGetValue(inst.CkanDir(),
                                                                         out RegistryManager? regMgr)
                                                   ? regMgr.registry
                                                   : LoadRegistry(inst, repoData));

        private static Registry LoadRegistry(GameInstance          inst,
                                             RepositoryDataManager repoData)
            => Registry.FromJson(inst, repoData,
                                 File.ReadAllText(Path.Combine(inst.CkanDir(), "registry.json")));

        [MemberNotNull(nameof(registry))]
        private void Create(RepositoryDataManager   repoData,
                            IEnumerable<Repository> repositories)
        {
            log.InfoFormat("Creating new CKAN registry at {0}", path);
            registry = new Registry(repoData, repositories);
            AscertainDefaultRepo();
            ScanUnmanagedFiles();
        }

        private void AscertainDefaultRepo()
        {
            if (registry.Repositories.Count == 0)
            {
                log.InfoFormat("Fabricating repository: {0}", gameInstance.game.DefaultRepositoryURL);
                var repo = Repository.DefaultGameRepo(gameInstance.game);
                registry.RepositoriesSet(new SortedDictionary<string, Repository>
                {
                    { repo.name, repo }
                });
            }
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

        public void Save(bool enforce_consistency = true)
        {
            TxFileManager file_transaction = new TxFileManager();

            log.InfoFormat("Saving CKAN registry at {0}", path);

            if (enforce_consistency)
            {
                // No saving the registry unless it's in a sane state.
                registry.CheckSanity();
            }

            var directoryPath = Path.GetDirectoryName(path);

            if (directoryPath == null)
            {
                log.ErrorFormat("Failed to save registry, invalid path: {0}", path);
                throw new DirectoryNotFoundKraken(path, string.Format(
                    Properties.Resources.RegistryManagerDirectoryNotFound, path));
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            file_transaction.WriteAllText(path, Serialize());

            ExportInstalled(
                Path.Combine(directoryPath, LatestInstalledExportFilename()),
                false, true
            );
            if (!Directory.Exists(gameInstance.InstallHistoryDir()))
            {
                Directory.CreateDirectory(gameInstance.InstallHistoryDir());
            }
            ExportInstalled(
                Path.Combine(gameInstance.InstallHistoryDir(), HistoricInstalledExportFilename()),
                false, true
            );
        }

        public string LatestInstalledExportFilename() => $"{Properties.Resources.RegistryManagerExportFilenamePrefix}-{gameInstance.SanitizedName}.ckan";
        public string HistoricInstalledExportFilename() => $"{Properties.Resources.RegistryManagerExportFilenamePrefix}-{gameInstance.SanitizedName}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.ckan";

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

        private string SerializeCurrentInstall(bool recommends = false, bool with_versions = true)
        {
            var pack = GenerateModpack(recommends, with_versions);
            return pack.ToJson();
        }

        /// <summary>
        /// Create a CkanModule object that represents the currently installed
        /// mod list as a metapackage.
        /// </summary>
        /// <param name="recommends">If true, put the mods in the recommends relationship, otherwise use depends</param>
        /// <param name="with_versions">If true, set the installed mod versions in the relationships</param>
        /// <returns>
        /// The CkanModule object
        /// </returns>
        public CkanModule GenerateModpack(bool recommends = false, bool with_versions = true)
        {
            string gameInstanceName = gameInstance.Name;
            string name      = string.Format(Properties.Resources.ModpackName, gameInstanceName);
            var    crit      = gameInstance.VersionCriteria();
            var    minAndMax = crit.MinAndMax;
            var module = new CkanModule(
                new ModuleVersion("v1.6"),
                Identifier.Sanitize(name),
                name,
                string.Format(Properties.Resources.RegistryManagerDefaultModpackAbstract, gameInstanceName),
                null,
                new List<string>()  { Environment.UserName   },
                new List<License>() { License.UnknownLicense },
                new ModuleVersion(DateTime.UtcNow.ToString("yyyy.MM.dd.hh.mm.ss")),
                null,
                ModuleKind.metapackage)
            {
                ksp_version_min       = minAndMax.Lower.AsInclusiveLower().WithoutBuild,
                ksp_version_max       = minAndMax.Upper.AsInclusiveUpper().WithoutBuild,
                download_content_type = typeof(CkanModule).GetTypeInfo()
                                            ?.GetDeclaredField("download_content_type")
                                            ?.GetCustomAttribute<DefaultValueAttribute>()
                                            ?.Value?.ToString(),
                release_date          = DateTime.Now,
            };

            var mods = registry.InstalledModules
                               .Where(inst => !inst.Module.IsDLC && !inst.AutoInstalled
                                              && IsAvailable(inst, gameInstance.StabilityToleranceConfig))
                               .Select(inst => inst.Module)
                               .ToHashSet();
            // Sort dependencies before dependers
            var resolver = new RelationshipResolver(mods, null,
                                                    RelationshipResolverOptions.ConflictsOpts(gameInstance.StabilityToleranceConfig),
                                                    registry, gameInstance.game, gameInstance.VersionCriteria());
            var rels = resolver.ModList()
                               .Intersect(mods)
                               .Select(with_versions ? (Func<CkanModule, RelationshipDescriptor>)
                                                       RelationshipWithVersion
                                                     : RelationshipWithoutVersion)
                               .ToList();

            if (recommends)
            {
                module.recommends = rels;
            }
            else
            {
                module.depends    = rels;
            }
            module.spec_version = SpecVersionAnalyzer.MinimumSpecVersion(module);

            return module;
        }

        private bool IsAvailable(InstalledModule inst, StabilityToleranceConfig stabilityTolerance)
        {
            try
            {
                var avail = registry.LatestAvailable(inst.identifier, stabilityTolerance, null);
                return true;
            }
            catch
            {
                // Skip unavailable modules (custom .ckan files)
                return false;
            }
        }

        private RelationshipDescriptor RelationshipWithVersion(CkanModule mod)
            => new ModuleRelationshipDescriptor()
            {
                name    = mod.identifier,
                version = mod.version,
            };

        private RelationshipDescriptor RelationshipWithoutVersion(CkanModule mod)
            => new ModuleRelationshipDescriptor()
            {
                name = mod.identifier,
            };

        /// <summary>
        /// Scans the game folder for DLL data and updates the registry.
        /// This operates as a transaction.
        /// </summary>
        /// <returns>
        /// True if found anything different, false if same as before
        /// </returns>
        public bool ScanUnmanagedFiles()
        {
            log.Info(Properties.Resources.GameInstanceScanning);
            using (var tx = CkanTransaction.CreateTransactionScope())
            {
                var modFolders = Enumerable.Repeat(gameInstance.game.PrimaryModDirectoryRelative, 1)
                                           .Concat(gameInstance.game.AlternateModDirectoriesRelative)
                                           .Select(mf => $"{mf}/")
                                           .ToArray();
                var stockFolders = gameInstance.game.StockFolders
                                                    // Folders outside GameData won't be scanned
                                                    .Where(sf => modFolders.Any(mf => sf.StartsWith(mf)))
                                                    // Precalculate the full prefix once for all files
                                                    .Select(f => $"{f}/")
                                                    .ToArray();
                var dlls = modFolders.Select(gameInstance.ToAbsoluteGameDir)
                                     .Where(Directory.Exists)
                                     // EnumerateFiles is *case-sensitive* in its pattern, which causes
                                     // DLL files to be missed under Linux; we have to pick .dll, .DLL, or scanning
                                     // GameData *twice*.
                                     //
                                     // The least evil is to walk it once, and filter it ourselves.
                                     .SelectMany(absDir => Directory.EnumerateFiles(absDir, "*",
                                                                                    SearchOption.AllDirectories))
                                     .Where(file => file.EndsWith(".dll",
                                                                  StringComparison.CurrentCultureIgnoreCase))
                                     .Select(gameInstance.ToRelativeGameDir)
                                     .Where(relPath => !stockFolders.Any(f => relPath.StartsWith(f)))
                                     .GroupBy(gameInstance.DllPathToIdentifier)
                                     .OfType<IGrouping<string, string>>()
                                     .ToDictionary(grp => grp.Key,
                                                   grp => grp.First());
                log.DebugFormat("Registering DLLs: {0}", string.Join(", ", dlls.Values));
                var dllChanged = registry.SetDlls(dlls);

                var dlcChanged = ScanDlc();

                log.Debug("Scan completed, committing transaction");
                tx.Complete();

                return dllChanged || dlcChanged;
            }
        }

        /// <summary>
        /// Look for DLC installed in GameData
        /// </summary>
        /// <returns>
        /// True if not the same list as last scan, false otherwise
        /// </returns>
        public bool ScanDlc()
            => registry.SetDlcs(TestDlcScan(Path.Combine(gameInstance.CkanDir(), "dlc"))
                                .Concat(WellKnownDlcScan())
                                .ToDictionary());

        private static IEnumerable<KeyValuePair<string, UnmanagedModuleVersion>> TestDlcScan(string dlcDir)
            => (Directory.Exists(dlcDir)
                       ? Directory.EnumerateFiles(dlcDir, "*.dlc",
                                                  SearchOption.TopDirectoryOnly)
                       : Enumerable.Empty<string>())
                   .Select(f => new KeyValuePair<string, UnmanagedModuleVersion>(
                       $"{Path.GetFileNameWithoutExtension(f)}-DLC",
                       new UnmanagedModuleVersion(File.ReadAllText(f).Trim())));

        private IEnumerable<KeyValuePair<string, UnmanagedModuleVersion>> WellKnownDlcScan()
            => gameInstance.game.DlcDetectors
                .Select(d => d.IsInstalled(gameInstance, out string? identifier, out UnmanagedModuleVersion? version)
                                 && identifier is not null && version is not null
                             ? new KeyValuePair<string, UnmanagedModuleVersion>(identifier, version)
                             : (KeyValuePair<string, UnmanagedModuleVersion>?)null)
                .OfType<KeyValuePair<string, UnmanagedModuleVersion>>();
    }
}
