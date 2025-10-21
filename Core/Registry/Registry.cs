using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using System.Transactions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

using Autofac;
using Newtonsoft.Json;
using log4net;

using CKAN.IO;
using CKAN.Configuration;
using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// This is the CKAN registry. All the modules that we have installed
    /// are contained in here.
    /// </summary>

    // TODO: It would be *great* for the registry to have a 'dirty' bit, that records if
    // anything has changed. But that would involve catching access to a lot of the data
    // structures we pass back, and we're not doing that yet.

    public class Registry : IEnlistmentNotification, IRegistryQuerier
    {
        [JsonIgnore] private const int LATEST_REGISTRY_VERSION = 3;
        [JsonIgnore] private static readonly ILog log = LogManager.GetLogger(typeof(Registry));

        [JsonProperty] private int registry_version;

        // name => Repository
        [JsonProperty("sorted_repositories")]
        private SortedDictionary<string, Repository>? repositories;

        // name => relative path
        [JsonProperty]
        private Dictionary<string, string> installed_dlls;

        [JsonProperty]
        [JsonConverter(typeof(JsonParallelDictionaryConverter<InstalledModule>))]
        private readonly Dictionary<string, InstalledModule> installed_modules;

        // filename (case insensitive on Windows) => module
        [JsonProperty]
        private IDictionary<string, string> installed_files;

        /// <summary>
        /// Returns all the activated registries.
        /// ReadOnly to ensure calling code can't make changes that
        /// should invalidate the available mod caches.
        /// </summary>
        [JsonIgnore]
        public ReadOnlyDictionary<string, Repository> Repositories
            => new ReadOnlyDictionary<string, Repository>(repositories
                                                          ?? new SortedDictionary<string, Repository>());

        /// <summary>
        /// Wrapper around assignment to this.repositories that invalidates
        /// available mod caches
        /// </summary>
        /// <param name="value">The repositories dictionary to replace our current one</param>
        public void RepositoriesSet(SortedDictionary<string, Repository> value)
        {
            EnlistWithTransaction();
            InvalidateAvailableModCaches();
            repositories = value;
        }

        /// <summary>
        /// Wrapper around this.repositories.Clear() that invalidates
        /// available mod caches
        /// </summary>
        public void RepositoriesClear()
        {
            EnlistWithTransaction();
            InvalidateAvailableModCaches();
            repositories?.Clear();
        }

        /// <summary>
        /// Wrapper around this.repositories.Add() that invalidates
        /// available mod caches
        /// </summary>
        /// <param name="repo"></param>
        public void RepositoriesAdd(Repository repo)
        {
            EnlistWithTransaction();
            InvalidateAvailableModCaches();
            if (repo.name != null)
            {
                repositories?.Add(repo.name, repo);
            }
        }

        /// <summary>
        /// Wrapper around this.repositories.Remove() that invalidates
        /// available mod caches
        /// </summary>
        /// <param name="name"></param>
        public void RepositoriesRemove(string name)
        {
            EnlistWithTransaction();
            InvalidateAvailableModCaches();
            repositories?.Remove(name);
        }

        /// <summary>
        /// Returns all the installed modules
        /// </summary>
        [JsonIgnore] public IReadOnlyCollection<InstalledModule> InstalledModules
            => installed_modules.Values;

        /// <summary>
        /// Returns the names of installed DLLs.
        /// </summary>
        [JsonIgnore] public IReadOnlyCollection<string> InstalledDlls
            => installed_dlls.Keys;

        /// <summary>
        /// Returns the file path of a DLL.
        /// null if not found.
        /// </summary>
        public string? DllPath(string identifier)
            => installed_dlls.TryGetValue(identifier, out string? path)
                ? path
                : null;

        /// <summary>
        /// A map between module identifiers and versions for official DLC that are installed.
        /// </summary>
        [JsonIgnore] public IDictionary<string, UnmanagedModuleVersion> InstalledDlc
            => installedDlc ??= installed_modules.Values
                .Where(im => im.Module.IsDLC)
                .Select(im => im.Module.version is UnmanagedModuleVersion unmVer
                              ? (KeyValuePair<string, UnmanagedModuleVersion>?)
                                new KeyValuePair<string, UnmanagedModuleVersion>(
                                    im.Module.identifier, unmVer)
                              : null)
                .OfType<KeyValuePair<string, UnmanagedModuleVersion>>()
                .ToDictionary();

        /// <summary>
        /// Find installed modules that are not compatible with the given versions
        /// </summary>
        /// <param name="crit">Version criteria against which to check modules</param>
        /// <returns>
        /// Installed modules that are incompatible, if any
        /// </returns>
        public IEnumerable<InstalledModule> IncompatibleInstalled(GameVersionCriteria crit)
            => installed_modules.Values
                    .Where(im => !im.Module.IsCompatible(crit)
                        && !(GetModuleByVersion(im.identifier, im.Module.version)?.IsCompatible(crit)
                            ?? false));

        #region Registry Upgrades

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext context)
        {
            if (registry_version > LATEST_REGISTRY_VERSION)
            {
                throw new RegistryVersionNotSupportedKraken(
                    registry_version,
                    string.Format(Properties.Resources.RegistryManagerRegistryVersionNotSupported,
                                  "registry.json"));
            }

            // Older registries didn't have the installed_files list, so we create one
            // if absent.
            if (installed_files == null)
            {
                log.Warn("Older registry format detected, adding installed files manifest...");
                ReindexInstalled();
            }

            // If we have no registry version at all, then we're from the pre-release period.
            // We would check for a null here, but ints *can't* be null.
            if (registry_version < 1)
            {
                log.Warn("Older registry format detected, normalising paths...");

                // Our context is our game instance.
                if (context.Context is GameInstance inst)
                {
                    installed_files = installed_files.ToDictionary(
                                          kvp => Path.IsPathRooted(kvp.Key)
                                                     ? inst.ToRelativeGameDir(kvp.Key)
                                                     // Already relative.
                                                     : CKANPathUtils.NormalizePath(kvp.Key),
                                          kvp => kvp.Value,
                                          // We need case insensitive path matching on Windows
                                          Platform.PathComparer);
                    // Now update all our module file manifests.
                    foreach (var module in installed_modules.Values)
                    {
                        module.Renormalise(inst);
                    }
                }

                // Our installed dlls have contained relative paths since forever,
                // and the next `ckan scan` will fix them anyway. (We can't scan here,
                // because that needs a registry, and we chicken-egg.)

                log.Warn("Registry upgrade complete");
            }
            else if (Platform.IsWindows)
            {
                // We need case insensitive path matching on Windows
                // (already done when replacing this object in the above block, hence the 'else')
                installed_files = new Dictionary<string, string>(installed_files, Platform.PathComparer);
            }

            // Fix control lock, which previously was indexed with an invalid identifier.
            if (registry_version < 2)
            {
                const string old_ident = "001ControlLock";
                const string new_ident = "ControlLock";

                if (installed_modules.TryGetValue("001ControlLock", out InstalledModule? control_lock_entry))
                {
                    if (context.Context is not GameInstance inst)
                    {
                        throw new Kraken("Internal bug: No KSP instance provided on registry deserialisation");
                    }

                    log.WarnFormat("Older registry detected. Reindexing {0} as {1}. This may take a moment.", old_ident, new_ident);

                    // Remove old record.
                    installed_modules.Remove(old_ident);

                    // Extract the old module metadata
                    CkanModule control_lock_mod = control_lock_entry.Module;

                    // Change to the correct ident.
                    control_lock_mod.identifier = new_ident;

                    // Prepare to re-index.
                    var new_control_lock_installed = new InstalledModule(
                        inst,
                        control_lock_mod,
                        control_lock_entry.Files,
                        control_lock_entry.AutoInstalled
                    );

                    // Re-insert into registry.
                    installed_modules[new_control_lock_installed.identifier] = new_control_lock_installed;

                    // Re-index files.
                    ReindexInstalled();
                }
            }

            // If we spot a default repo with the old .zip URL, flip it to the new .tar.gz URL
            // Any other repo we leave *as-is*, even if it's the github meta-repo, as it's been
            // custom-added by our user.

            var oldDefaultRepo = new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip");
            if (repositories != null
                && repositories.TryGetValue(Repository.default_ckan_repo_name,
                                            out Repository? default_repo)
                && default_repo.uri == oldDefaultRepo
                && context.Context is GameInstance gameInst)
            {
                log.InfoFormat("Updating default metadata URL from {0} to {1}",
                               oldDefaultRepo, gameInst.Game.DefaultRepositoryURL);
                repositories[Repository.default_ckan_repo_name].uri = gameInst.Game.DefaultRepositoryURL;
            }

            if (repositories != null)
            {
                // Fix duplicate priorities
                var sorted = repositories.Values.OrderBy(r => r.priority)
                                                // Break ties alphanumerically
                                                .ThenBy(r => r.name)
                                                .ToArray();
                for (int i = 0; i < sorted.Length; ++i)
                {
                    sorted[i].priority = i;
                }
            }

            registry_version = LATEST_REGISTRY_VERSION;
        }

        /// <summary>
        /// Rebuilds our master index of installed_files.
        /// Called on registry format updates, but safe to be triggered at any time.
        /// </summary>
        [MemberNotNull(nameof(installed_files))]
        public void ReindexInstalled()
        {
            installed_files = installed_modules
                .Values
                .SelectMany(module => module.Files
                                            // Register each file we know about as belonging to the given module.
                                            .Select(file => new KeyValuePair<string, string>(file,
                                                                                             module.identifier)))
                // We need case insensitive path matching on Windows
                .ToDictionary(Platform.PathComparer);
        }

        /// <summary>
        /// Do we what we can to repair/preen the registry.
        /// </summary>
        public void Repair()
        {
            ReindexInstalled();
        }

        #endregion

        #region Constructors / destructor

        [JsonConstructor]
        private Registry(RepositoryDataManager? repoData)
        {
            if (repoData != null)
            {
                repoDataMgr = repoData;
                repoDataMgr.Updated += RepositoriesUpdated;
            }
            installed_modules = new Dictionary<string, InstalledModule>();
            installed_files   = new Dictionary<string, string>();
            installed_dlls    = new Dictionary<string, string>();
        }

        ~Registry()
        {
            if (repoDataMgr != null)
            {
                repoDataMgr.Updated -= RepositoriesUpdated;
            }
        }

        public Registry(RepositoryDataManager?               repoData,
                        Dictionary<string, InstalledModule>  installed_modules,
                        Dictionary<string, string>           installed_dlls,
                        IDictionary<string, string>          installed_files,
                        SortedDictionary<string, Repository> repositories)
            : this(repoData)
        {
            // Is there a better way of writing constructors than this? Srsly?
            this.installed_modules = installed_modules;
            this.installed_dlls    = installed_dlls;
            this.installed_files   = installed_files;
            this.repositories      = repositories;
            registry_version       = LATEST_REGISTRY_VERSION;
        }

        public Registry(RepositoryDataManager repoData,
                        params Repository[] repositories)
            : this(repoData,
                   new Dictionary<string, InstalledModule>(),
                   new Dictionary<string, string>(),
                   new Dictionary<string, string>(),
                   new SortedDictionary<string, Repository>(
                       repositories.ToDictionary(r => r.name ?? "",
                                                 r => r)))
        {
        }

        public Registry(RepositoryDataManager repoData,
                        IEnumerable<Repository> repositories)
            : this(repoData, repositories.ToArray())
        {
        }

        public static Registry Empty(RepositoryDataManager repoData)
            => new Registry(repoData,
                            new Dictionary<string, InstalledModule>(),
                            new Dictionary<string, string>(),
                            new Dictionary<string, string>(),
                            new SortedDictionary<string, Repository>());

        public static Registry FromJson(GameInstance          inst,
                                        RepositoryDataManager repoData,
                                        string                json)
        {
            var registry = new Registry(repoData)
            {
                // Let DeSerialisationFixes detect if registry_version is missing
                registry_version = 0,
                // Let DeSerialisationFixes detect if installed_files is missing
                installed_files  = null!,
            };
            try
            {
                JsonConvert.PopulateObject(json, registry, LoadSettings(inst));
            }
            catch (TargetInvocationException tiExc) when (tiExc is { InnerException: Exception exc })
            {
                // "The exception that is thrown by methods invoked through reflection."
                // The JSON library uses reflection for OnDeserialized.
                ExceptionDispatchInfo.Capture(exc).Throw();
            }
            return registry;
        }

        // Our registry needs to know our game instance when upgrading from older
        // registry formats. This lets us encapsulate that to make it available
        // after deserialisation.
        private static JsonSerializerSettings LoadSettings(GameInstance inst)
            => new JsonSerializerSettings
               {
                   DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                   Context = new StreamingContext(StreamingContextStates.Other, inst)
               };

        #endregion

        #region Transaction Handling

        // Which transaction we're in
        private string? enlisted_tx;

        // JSON serialization of self when enlisted with tx
        private string? transaction_backup;

        // Coordinate access of multiple threads to the tx info
        private readonly object txMutex = new object();

        // This *doesn't* get called when we get enlisted in a Tx, it gets
        // called when we're about to commit a transaction. We can *probably*
        // get away with calling .Done() here and skipping the commit phase,
        // but I'm not sure if we'd get InDoubt signalling if we did that.
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            log.Debug("Registry prepared to commit transaction");

            preparingEnlistment.Prepared();
        }

        public void InDoubt(Enlistment enlistment)
        {
            // In doubt apparently means we don't know if we've committed or not.
            // Since our TxFileMgr treats this as a rollback, so do we.
            log.Warn("Transaction involving registry in doubt.");
            Rollback(enlistment);
        }

        public void Commit(Enlistment enlistment)
        {
            // Hooray! All Tx participants have signalled they're ready.
            // So we're done, and can clear our resources.

            log.DebugFormat("Committing registry tx {0}", enlisted_tx);
            lock (txMutex) {
                enlisted_tx = null;
                transaction_backup = null;

                enlistment.Done();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            log.Info("Aborted transaction, rolling back in-memory registry changes.");

            // In theory, this should put everything back the way it was, overwriting whatever
            // we had previously.

            lock (txMutex) {
                var options = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                if (transaction_backup != null)
                {
                    JsonConvert.PopulateObject(transaction_backup, this, options);
                }

                enlisted_tx = null;
                transaction_backup = null;

                enlistment.Done();
            }
        }

        private void SaveState()
        {
            // Hey, you know what's a great way to back-up your own object?
            // JSON. ;)
            transaction_backup = JsonConvert.SerializeObject(this, Formatting.None);
            log.Debug("State saved");
        }

        /// <summary>
        /// Adds our registry to the current transaction. This should be called whenever we
        /// do anything which may dirty the registry.
        /// </summary>
        private void EnlistWithTransaction()
        {
            // This property is thread static, so other threads can't mess with our value
            if (Transaction.Current != null)
            {
                string current_tx = Transaction.Current.TransactionInformation.LocalIdentifier;

                // Multiple threads might be accessing this shared state, make sure they play nice
                lock (txMutex)
                {
                    if (enlisted_tx == null)
                    {
                        log.DebugFormat("Enlisting registry with tx {0}", current_tx);
                        // Let's save our state before we enlist and potentially allow ourselves
                        // to be reverted by outside code
                        SaveState();
                        Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                        enlisted_tx = current_tx;
                    }
                    else if (enlisted_tx != current_tx)
                    {
                        throw new TransactionalKraken(
                            $"Registry already enlisted with tx {enlisted_tx}, can't enlist with tx {current_tx}");
                    }
                    else
                    {
                        // If we're here, it's a transaction we're already participating in,
                        // so do nothing.
                        log.DebugFormat("Already enlisted with tx {0}", current_tx);
                    }
                }
            }
        }

        #endregion

        #region Stateful views of data from repo data manager based on which repos we use

        [JsonIgnore]
        private readonly RepositoryDataManager? repoDataMgr;

        [JsonIgnore]
        private CompatibilitySorter? sorter;

        [JsonIgnore]
        private Dictionary<string, ProvidesModuleVersion>? installedProvides = null;

        [JsonIgnore]
        private Dictionary<string, ModuleTag>? tags;

        [JsonIgnore]
        private HashSet<string>? untagged;

        [JsonIgnore]
        private Dictionary<string, List<CkanModule>>? downloadHashesIndex;

        [JsonIgnore]
        private Dictionary<string, List<CkanModule>>? downloadUrlHashIndex;

        // Index of which mods provide what, format:
        //   providers[provided] = { provider1, provider2, ... }
        // Built by BuildProvidesIndex, makes LatestAvailableWithProvides much faster.
        [JsonIgnore]
        private Dictionary<string, AvailableModule[]>? providers;

        [JsonIgnore]
        private IDictionary<string, UnmanagedModuleVersion>? installedDlc;

        private void InvalidateAvailableModCaches()
        {
            log.Debug("Invalidating available mod caches");
            // These member variables hold references to data from our repo data manager
            // that reflects how the available modules look to this instance.
            // Clear them when we have reason to believe the upstream available modules have changed.
            providers            = null;
            sorter               = null;
            tags                 = null;
            untagged             = null;
            downloadHashesIndex  = null;
            downloadUrlHashIndex = null;
        }

        private void InvalidateInstalledCaches()
        {
            log.Debug("Invalidating installed mod caches");
            // These member variables hold references to data that depends on installed modules.
            // Clear them when the installed modules have changed.
            sorter            = null;
            installedProvides = null;
            installedDlc      = null;
        }

        private void RepositoriesUpdated(Repository[] which)
        {
            if (Repositories.Values.Any(r => which.Contains(r)))
            {
                // One of our repos changed, old cached data is now junk
                EnlistWithTransaction();
                InvalidateAvailableModCaches();
            }
        }

        public bool HasAnyAvailable()
            => repositories != null && repoDataMgr != null
                && repoDataMgr.GetAllAvailableModules(repositories.Values).Any();

        /// <summary>
        /// Partition all CkanModules in available_modules into
        /// compatible and incompatible groups.
        /// </summary>
        /// <param name="stabilityTolerance">Stability tolerance to determine compatibility</param>
        /// <param name="versCrit">Version criteria to determine compatibility</param>
        public CompatibilitySorter SetCompatibleVersion(StabilityToleranceConfig stabilityTolerance,
                                                        GameVersionCriteria      versCrit)
        {
            if (sorter == null
                || stabilityTolerance != sorter.StabilityTolerance
                || !versCrit.Equals(sorter.CompatibleVersions))
            {
                if (providers == null)
                {
                    BuildProvidesIndex();
                }
                sorter = new CompatibilitySorter(
                    stabilityTolerance,
                    versCrit,
                    repoDataMgr?.GetAllAvailDicts(Repositories.Values.OrderBy(r => r.priority)
                                                                     // Break ties alphanumerically
                                                                     .ThenBy(r => r.name))
                               ?? Enumerable.Empty<Dictionary<string, AvailableModule>>(),
                    providers,
                    installed_modules, InstalledDlls, InstalledDlc);
            }
            return sorter;
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.CompatibleModules"/>
        /// </summary>
        public IEnumerable<CkanModule> CompatibleModules(StabilityToleranceConfig stabilityTolerance,
                                                         GameVersionCriteria?     crit)
            // Set up our compatibility partition
            => crit != null ? SetCompatibleVersion(stabilityTolerance, crit).LatestCompatible
                            : repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                                          .Select(am => am.Latest(stabilityTolerance))
                                          .OfType<CkanModule>()
                                         ?? Enumerable.Empty<CkanModule>();

        /// <summary>
        /// <see cref="IRegistryQuerier.IncompatibleModules"/>
        /// </summary>
        public IEnumerable<CkanModule> IncompatibleModules(StabilityToleranceConfig stabilityTolerance,
                                                           GameVersionCriteria      crit)
            // Set up our compatibility partition
            => SetCompatibleVersion(stabilityTolerance, crit).LatestIncompatible;

        /// <summary>
        /// Check whether any versions of this mod are installable (including dependencies) on the given game versions.
        /// Quicker than checking CompatibleModules for one identifier.
        /// </summary>
        /// <param name="identifier">Identifier of mod</param>
        /// <param name="stabilityTolerance">Stability tolerance to determine compatibility</param>
        /// <param name="crit">Game versions</param>
        /// <returns>true if any version is recursively compatible, false otherwise</returns>
        public bool IdentifierCompatible(string                   identifier,
                                         StabilityToleranceConfig stabilityTolerance,
                                         GameVersionCriteria      crit)
            // Set up our compatibility partition
            => SetCompatibleVersion(stabilityTolerance, crit).Compatible.ContainsKey(identifier);

        private AvailableModule[] getAvail(string identifier)
        {
            var availMods = (repositories == null || repoDataMgr == null
                                ? Enumerable.Empty<AvailableModule>()
                                : repoDataMgr.GetAvailableModules(repositories.Values, identifier))
                            .ToArray();
            if (availMods.Length < 1)
            {
                throw new ModuleNotFoundKraken(identifier);
            }
            return availMods;
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.LatestAvailable" />
        /// </summary>
        public CkanModule? LatestAvailable(string                           identifier,
                                           StabilityToleranceConfig         stabilityTolerance,
                                           GameVersionCriteria?             gameVersion,
                                           RelationshipDescriptor?          relationshipDescriptor = null,
                                           IReadOnlyCollection<CkanModule>? installed              = null,
                                           IReadOnlyCollection<CkanModule>? toInstall              = null)
            => getAvail(identifier).Select(am => am.Latest(stabilityTolerance, gameVersion, relationshipDescriptor,
                                                           installed, toInstall))
                                   .OfType<CkanModule>()
                                   .OrderByDescending(m => m.version)
                                   .FirstOrDefault();

        /// <summary>
        /// Find modules with a given identifier
        /// </summary>
        /// <param name="identifier">Identifier of modules to find</param>
        /// <returns>
        /// List of all modules with this identifier
        /// </returns>
        public IEnumerable<CkanModule> AvailableByIdentifier(string identifier)
            => getAvail(identifier).SelectMany(am => am.AllAvailable())
                                   .Distinct()
                                   .OrderByDescending(m => m.version);

        /// <summary>
        /// Returns the specified CkanModule with the version specified,
        /// or null if it does not exist.
        /// <see cref = "IRegistryQuerier.GetModuleByVersion" />
        /// </summary>
        public CkanModule? GetModuleByVersion(string ident, ModuleVersion version)
            => Utilities.DefaultIfThrows(() => getAvail(ident))
                        ?.Select(am => am.ByVersion(version))
                         .FirstOrDefault(m => m != null);

        /// <summary>
        /// Get full JSON metadata string for a mod's available versions
        /// </summary>
        /// <param name="identifier">Name of the mod to look up</param>
        /// <returns>
        /// JSON formatted string for all the available versions of the mod
        /// </returns>
        public string GetAvailableMetadata(string identifier)
            => repoDataMgr == null
                ? ""
                : string.Join("",
                              repoDataMgr.GetAvailableModules(Repositories.Values, identifier)
                                         .Select(am => am.FullMetadata()));

        /// <summary>
        /// Return the latest game version compatible with the given mod.
        /// </summary>
        /// <param name="realVersions">Game versions to check against</param>
        /// <param name="identifier">Name of mod to check</param>
        public GameVersion? LatestCompatibleGameVersion(List<GameVersion> realVersions,
                                                        string            identifier)
            => Utilities.DefaultIfThrows(() => getAvail(identifier))
                        ?.Select(am => am.LatestCompatibleGameVersion(realVersions))
                         .Max();

        /// <summary>
        /// Generate the providers index so we can find providing modules quicker
        /// </summary>
        [MemberNotNull(nameof(providers))]
        private Dictionary<string, AvailableModule[]> BuildProvidesIndex()
            => providers = (repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                                       ?? Enumerable.Empty<AvailableModule>())
                                        .SelectMany(am => am.AllAvailable()
                                                            .SelectMany(m => m.ProvidesList)
                                                            .Distinct()
                                                            .Select(provided => (provided, am)))
                                        .ToGroupedDictionary(tuple => tuple.provided,
                                                             tuple => tuple.am);

        [JsonIgnore]
        public Dictionary<string, ModuleTag> Tags
        {
            get
            {
                lock (tagMutex)
                {
                    if (tags == null)
                    {
                        BuildTagIndex();
                    }
                }
                return tags;
            }
        }

        [JsonIgnore]
        public HashSet<string> Untagged
        {
            get
            {
                lock (tagMutex)
                {
                    if (untagged == null)
                    {
                        BuildTagIndex();
                    }
                }
                return untagged;
            }
        }

        private readonly object tagMutex = new object();

        /// <summary>
        /// Assemble a mapping from tags to modules
        /// </summary>
        [MemberNotNull(nameof(tags), nameof(untagged))]
        private void BuildTagIndex()
        {
            tags = (repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                               ?? Enumerable.Empty<AvailableModule>())
                                .SelectMany(am => am.AllAvailable()
                                                    .SelectMany(m => m.Tags ?? Enumerable.Empty<string>())
                                                    .Select(tag => (tag, ident: am.AllAvailable().First().identifier))
                                                    .DefaultIfEmpty((tag: "", ident: am.AllAvailable().First().identifier)))
                                .GroupBy(tuple => tuple.tag,
                                         tuple => tuple.ident)
                                .ToDictionary(grp => grp.Key,
                                              grp => new ModuleTag(grp.Key, grp.ToHashSet()));
            untagged = tags.TryGetValue("", out ModuleTag? t) ? t.ModuleIdentifiers
                                                              : new HashSet<string>();
            tags.Remove("");
        }

        public IEnumerable<AvailableModule> AllAvailableByProvides(string identifier)
            => (providers ?? BuildProvidesIndex())
                    is Dictionary<string, AvailableModule[]> allProvs
                && allProvs.TryGetValue(identifier, out AvailableModule[]? provs)
                    ? provs
                    : Enumerable.Empty<AvailableModule>();

        /// <summary>
        /// <see cref="IRegistryQuerier.LatestAvailableWithProvides" />
        /// </summary>
        public List<CkanModule> LatestAvailableWithProvides(string                           identifier,
                                                            StabilityToleranceConfig         stabilityTolerance,
                                                            GameVersionCriteria?             gameVersion,
                                                            RelationshipDescriptor?          relationship = null,
                                                            IReadOnlyCollection<CkanModule>? installed    = null,
                                                            IReadOnlyCollection<CkanModule>? toInstall    = null)
            => ((providers ?? BuildProvidesIndex())
                    is Dictionary<string, AvailableModule[]> allProvs
                && allProvs.TryGetValue(identifier, out AvailableModule[]? provs)
                    // For each AvailableModule, we want the latest one matching our constraints
                    ? provs.Select(am => am.Latest(stabilityTolerance, gameVersion, relationship, installed, toInstall))
                           .OfType<CkanModule>()
                           .Where(m => m.ProvidesList.Contains(identifier))
                           .Distinct()
                           // Put the most popular one on top
                           .OrderByDescending(m => repoDataMgr?.GetDownloadCount(Repositories.Values, m.identifier)
                                                              ?? 0)
                    // Nothing provides this
                    : Enumerable.Empty<CkanModule>())
               .ToList();

        #endregion

        /// <summary>
        /// Register the supplied module as having been installed, thereby keeping
        /// track of its metadata and files.
        /// </summary>
        public InstalledModule RegisterModule(CkanModule                  mod,
                                              IReadOnlyCollection<string> absoluteFiles,
                                              GameInstance                inst,
                                              bool                        autoInstalled)
        {
            log.DebugFormat("Registering module {0}", mod);
            EnlistWithTransaction();

            sorter = null;

            // But we also want to keep track of all its files.
            // We start by checking to see if any files are owned by another mod,
            // if so, we abort with a list of errors.

            var inconsistencies = new List<string>();

            // We always work with relative files, so let's get some!
            var relativeFiles = absoluteFiles.Select(inst.ToRelativeGameDir)
                                             .ToHashSet(Platform.PathComparer);

            // For now, it's always cool if a module wants to register a directory.
            // We have to flip back to absolute paths to actually test this.
            foreach (string file in relativeFiles.Where(file => !Directory.Exists(inst.ToAbsoluteGameDir(file))))
            {
                if (installed_files.TryGetValue(file, out string? owner))
                {
                    // Woah! Registering an already owned file? Not cool!
                    // (Although if it existed, we should have thrown a kraken well before this.)
                    inconsistencies.Add(string.Format(
                        Properties.Resources.RegistryFileConflict,
                        mod.identifier, file, owner));
                }
            }

            if (inconsistencies.Count > 0)
            {
                throw new InconsistentKraken(inconsistencies);
            }

            // If everything is fine, then we copy our files across. By not doing this
            // in the loop above, we make sure we don't have a half-registered module
            // when we throw our exceptinon.

            // This *will* result in us overwriting who owns a directory, and that's cool,
            // directories aren't really owned like files are. However because each mod maintains
            // its own list of files, we'll remove directories when the last mod using them
            // is uninstalled.
            foreach (string file in relativeFiles)
            {
                installed_files[file] = mod.identifier;
            }

            // Make sure this mod and its files aren't in the manually installed DLL dict
            installed_dlls.RemoveWhere(kvp => kvp.Key == mod.identifier
                                              || relativeFiles.Contains(kvp.Value));

            // Finally register our module proper
            var instMod = new InstalledModule(inst, mod, relativeFiles, autoInstalled);
            installed_modules.Add(mod.identifier, instMod);

            // Installing and uninstalling mods can change compatibility due to conflicts,
            // so we'll need to reset the compatibility sorter
            InvalidateInstalledCaches();

            return instMod;
        }

        /// <summary>
        /// Deregister a module, which must already have its files removed, thereby
        /// forgetting abouts its metadata and files.
        ///
        /// Throws an InconsistentKraken if not all files have been removed.
        /// </summary>
        public void DeregisterModule(GameInstance inst, string identifier)
        {
            log.DebugFormat("Deregistering module {0}", identifier);
            EnlistWithTransaction();

            // Note, this checks to see if a *file* exists; it doesn't
            // trigger on directories, which we allow to still be present
            // (they may be shared by multiple mods.
            var inconsistencies = installed_modules[identifier].Files
                .Where(f => File.Exists(inst.ToAbsoluteGameDir(f)))
                .Select(relPath => string.Format(
                                       Properties.Resources.RegistryFileNotRemoved,
                                       relPath, identifier))
                .ToList();

            if (inconsistencies.Count > 0)
            {
                // Uh oh, what mess have we got ourselves into now, Inconsistency Kraken?
                throw new InconsistentKraken(inconsistencies);
            }

            // Okay, all the files are gone. Let's clear our metadata.
            foreach (string rel_file in installed_modules[identifier].Files)
            {
                installed_files.Remove(rel_file);
            }

            // Bye bye, module, it's been nice having you visit.
            installed_modules.Remove(identifier);

            // Installing and uninstalling mods can change compatibility due to conflicts,
            // so we'll need to reset the compatibility sorter
            InvalidateInstalledCaches();
        }

        /// <summary>
        /// Set the list of manually installed DLLs to the given mapping.
        /// Files registered to a mod are not allowed and will be ignored.
        /// Does nothing if we already have this data.
        /// </summary>
        /// <param name="dlls">Mapping from identifier to relative path</param>
        public bool SetDlls(IDictionary<string, string> dlls)
        {
            var instIdents = InstalledModules.Select(im => im.identifier)
                                             .ToHashSet();
            var unregistered = dlls.Where(kvp => !instIdents.Contains(kvp.Key)
                                                 && !installed_files.ContainsKey(kvp.Value))
                                   .ToDictionary();
            if (!unregistered.DictionaryEquals(installed_dlls))
            {
                EnlistWithTransaction();
                InvalidateInstalledCaches();
                installed_dlls = unregistered;
                return true;
            }
            return false;
        }

        public bool SetDlcs(IDictionary<string, UnmanagedModuleVersion> dlcs)
        {
            var installed = InstalledDlc;
            if (!dlcs.DictionaryEquals(installed))
            {
                EnlistWithTransaction();
                InvalidateInstalledCaches();

                foreach (var identifier in installed.Keys.Except(dlcs.Keys))
                {
                    installed_modules.Remove(identifier);
                }

                foreach ((string identifier, UnmanagedModuleVersion version) in dlcs)
                {
                    // Overwrite everything in case there are version differences
                    installed_modules[identifier] =
                        new InstalledModule(null,
                            GetModuleByVersion(identifier, version)
                                ?? new CkanModule(
                                    new ModuleVersion("v1.28"),
                                    identifier,
                                    identifier,
                                    Properties.Resources.RegistryDefaultDLCAbstract,
                                    null,
                                    new List<string>() { "SQUAD" },
                                    new List<License>() { new License("restricted") },
                                    version,
                                    null,
                                    ModuleKind.dlc),
                            Enumerable.Empty<string>(), false);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// <see cref = "IRegistryQuerier.Installed" />
        /// </summary>
        public Dictionary<string, ModuleVersion> Installed(bool withProvides = true, bool withDLLs = true)
        {
            var installed = new Dictionary<string, ModuleVersion>();

            if (withDLLs)
            {
                // Index our DLLs, as much as we dislike them.
                foreach (var dllinfo in installed_dlls)
                {
                    installed[dllinfo.Key] = new UnmanagedModuleVersion(null);
                }
            }

            // Index our provides list, so users can see virtual packages
            if (withProvides)
            {
                foreach (var provided in ProvidedByInstalled())
                {
                    installed[provided.Key] = provided.Value;
                }
            }

            // Index our installed modules (which may overwrite the installed DLLs and provides)
            // (Includes DLCs)
            foreach (var modinfo in installed_modules)
            {
                installed[modinfo.Key] = modinfo.Value.Module.version;
            }

            return installed;
        }

        /// <summary>
        /// <see cref = "IRegistryQuerier.InstalledModule" />
        /// </summary>
        public InstalledModule? InstalledModule(string module)
            => installed_modules.TryGetValue(module, out InstalledModule? installedModule)
                ? installedModule
                : null;

        /// <summary>
        /// Find modules provided by currently installed modules
        /// </summary>
        /// <returns>
        /// Dictionary of provided (virtual) modules and a
        /// ProvidesVersion indicating what provides them
        /// </returns>
        internal Dictionary<string, ProvidesModuleVersion> ProvidedByInstalled()
        {
            installedProvides ??= installed_modules.Values
                                                   .Select(im => im.Module)
                                                   .SelectMany(m => m.provides?.Select(p =>
                                                                        new KeyValuePair<string, ProvidesModuleVersion>(
                                                                            p, new ProvidesModuleVersion(
                                                                                   m.identifier, m.version.ToString())))
                                                                    ?? Enumerable.Empty<KeyValuePair<string, ProvidesModuleVersion>>())
                                                   .DistinctBy(kvp => kvp.Key)
                                                   .ToDictionary();
            return installedProvides;
        }

        private ProvidesModuleVersion? ProvidedByInstalled(string provided)
            => installedProvides != null
                   // The dictionary helps if we already have it cached...
                   ? installedProvides.TryGetValue(provided, out ProvidesModuleVersion? version)
                         ? version
                         : null
                   // ... but otherwise it's not worth the expense to calculate it
                   : installed_modules.Values
                                      .Select(im => im.Module)
                                      .Where(m => m.provides != null
                                                  && m.provides.Contains(provided))
                                      .Select(m => new ProvidesModuleVersion(m.identifier,
                                                                             m.version.ToString()))
                                      .FirstOrDefault();

        /// <summary>
        /// <see cref = "IRegistryQuerier.InstalledVersion" />
        /// </summary>
        public ModuleVersion? InstalledVersion(string modIdentifier, bool with_provides = true)
            // If it's genuinely installed, return the details we have.
            // (Includes DLCs)
            => installed_modules.TryGetValue(modIdentifier,
                                             out InstalledModule? installedModule)
                   ? installedModule.Module.version
                   // If it's in our autodetected registry, return that.
                   : installed_dlls.ContainsKey(modIdentifier)
                       ? new UnmanagedModuleVersion(null)
                   // Finally we have our provided checks. We'll skip these if
                   // withProvides is false.
                   : with_provides ? ProvidedByInstalled(modIdentifier)
                   : null;

        /// <summary>
        /// <see cref = "IRegistryQuerier.GetInstalledVersion" />
        /// </summary>
        public CkanModule? GetInstalledVersion(string mod_identifier)
            => InstalledModule(mod_identifier)?.Module;

        /// <summary>
        /// Returns the module which owns this file, or null if not known.
        /// Throws a PathErrorKraken if an absolute path is provided.
        /// </summary>
        public InstalledModule? FileOwner(string file)
            => installed_files.TryGetValue(CKANPathUtils.NormalizePath(file),
                                           out string? fileOwner)
                ? InstalledModule(fileOwner)
                : null;

        public IEnumerable<(string relPath, InstalledModule owner)> InstalledFileInfo()
            => installed_files.Select(kvp => (kvp.Key, InstalledModule(kvp.Value)))
                              .OfType<(string, InstalledModule)>();

        /// <summary>
        /// <see cref="IRegistryQuerier.CheckSanity"/>
        /// </summary>
        public void CheckSanity()
        {
            SanityChecker.EnforceConsistency(installed_modules.Select(pair => pair.Value.Module),
                                             installed_dlls.Keys, InstalledDlc);
        }

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// Acts recursively and lazily.
        /// </summary>
        /// <param name="modulesToRemove">Modules that are about to be removed.</param>
        /// <param name="modulesToInstall">Optional list of modules that are about to be installed.</param>
        /// <param name="origInstalled">Modules that are already installed, MUST include the FULL changeset if installing mods</param>
        /// <param name="dlls">Installed DLLs</param>
        /// <param name="dlc">Installed DLCs</param>
        /// <param name="satisfiedFilter">Optional filter to apply to the dependencies</param>
        /// <returns>List of modules whose dependencies are about to be or already removed.</returns>
        public static IEnumerable<string> FindReverseDependencies(
            IReadOnlyCollection<string>                 modulesToRemove,
            IReadOnlyCollection<CkanModule>?            modulesToInstall,
            IReadOnlyCollection<CkanModule>             origInstalled,
            IReadOnlyCollection<string>                 dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc,
            Func<RelationshipDescriptor, bool>?         satisfiedFilter = null)
        {
            log.DebugFormat("Finding reverse dependencies of: {0}", string.Join(", ", modulesToRemove));
            log.DebugFormat("From installed mods: {0}", string.Join(", ", origInstalled));
            if (modulesToInstall != null)
            {
                log.DebugFormat("Installing mods: {0}", string.Join(", ", modulesToInstall));
            }

            // The empty list has no reverse dependencies
            // (Don't remove broken modules if we're only installing)
            if (modulesToRemove.Count != 0)
            {
                // All modules in the input are included in the output
                foreach (var starter in modulesToRemove)
                {
                    yield return starter;
                }
                while (true)
                {
                    // Make our hypothetical install, and remove the listed modules from it.
                    // Clone because we alter hypothetical.
                    var hypothetical = origInstalled.ToHashSet();
                    if (modulesToInstall != null)
                    {
                        // Pretend the mods we are going to install are already installed, so that dependencies that will be
                        // satisfied by a mod that is going to be installed count as satisfied.
                        hypothetical.UnionWith(modulesToInstall);
                    }
                    hypothetical.RemoveWhere(mod => modulesToRemove.Contains(mod.identifier));

                    log.DebugFormat("Removing: {0}", string.Join(", ", modulesToRemove));
                    log.DebugFormat("Keeping: {0}", string.Join(", ", hypothetical));

                    // Find what would break with this configuration
                    var brokenDeps = SanityChecker.FindUnsatisfiedDepends(hypothetical,
                                                                          dlls, dlc)
                                                  .ToList();
                    if (satisfiedFilter != null)
                    {
                        brokenDeps.RemoveAll(tuple => satisfiedFilter(tuple.Item2));
                    }
                    var brokenIdents = brokenDeps.Select(tuple => tuple.Item1.identifier)
                                                 .ToHashSet();

                    if (modulesToInstall != null)
                    {
                        // Make sure to only report modules as broken if they are actually currently installed.
                        // This is mainly to remove the modulesToInstall again which we added
                        // earlier to the hypothetical list.
                        brokenIdents.IntersectWith(origInstalled.Select(m => m.identifier));
                    }
                    log.DebugFormat("Broken: {0}", string.Join(", ", brokenIdents));
                    // Lazily return each newly found rev dep
                    foreach (var newFound in brokenIdents.Except(modulesToRemove))
                    {
                        yield return newFound;
                    }

                    // If nothing else would break, it's just the list of modules we're removing
                    var toRemove = modulesToRemove.ToHashSet();
                    if (toRemove.IsSupersetOf(brokenIdents))
                    {
                        log.DebugFormat("{0} is a superset of {1}, work done",
                                        string.Join(", ", toRemove),
                                        string.Join(", ", brokenIdents));
                        break;
                    }

                    // Otherwise, remove our broken modules as well, and recurse
                    modulesToRemove = brokenIdents.Union(toRemove).ToList();
                }
            }
        }

        /// <summary>
        /// Return modules which are dependent on the modules passed in or modules in the return list
        /// </summary>
        public IEnumerable<string> FindReverseDependencies(
                IReadOnlyCollection<string>         modulesToRemove,
                IReadOnlyCollection<CkanModule>?    modulesToInstall = null,
                Func<RelationshipDescriptor, bool>? satisfiedFilter  = null)
            => FindReverseDependencies(modulesToRemove, modulesToInstall,
                                       installed_modules.Values.Select(im => im.Module)
                                                               .ToHashSet(),
                                       InstalledDlls, InstalledDlc,
                                       satisfiedFilter);

        /// <summary>
        /// Get a dictionary of all mod versions indexed by their downloads' SHA-256 and SHA-1 hashes.
        /// Useful for finding the mods for a group of files without repeatedly searching the entire registry.
        /// </summary>
        /// <returns>
        /// dictionary[sha256 or sha1] = {mod1, mod2, mod3};
        /// </returns>
        public IReadOnlyDictionary<string, List<CkanModule>> GetDownloadHashesIndex()
            => downloadHashesIndex ??=
                   (repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                                .SelectMany(availMod => availMod.module_version.Values)
                               ?? Enumerable.Empty<CkanModule>())
                                .SelectMany(ModWithDownloadHashes)
                                .GroupBy(tuple => tuple.Item1,
                                         tuple => tuple.Item2)
                                .ToDictionary(grp => grp.Key,
                                              grp => grp.ToList());

        private IEnumerable<Tuple<string, CkanModule>> ModWithDownloadHashes(CkanModule m)
        {
            if (m.download_hash is DownloadHashesDescriptor descr)
            {
                if (descr.sha256 != null && !string.IsNullOrEmpty(descr.sha256))
                {
                    yield return new Tuple<string, CkanModule>(descr.sha256, m);
                }
                if (descr.sha1 != null && !string.IsNullOrEmpty(descr.sha1))
                {
                    yield return new Tuple<string, CkanModule>(descr.sha1, m);
                }
            }
        }

        /// <summary>
        /// Get a dictionary of all mod versions indexed by their download URLs' hashes.
        /// Useful for finding the mods for a group of URLs without repeatedly searching the entire registry.
        /// </summary>
        /// <returns>
        /// dictionary[urlHash] = {mod1, mod2, mod3};
        /// </returns>
        public IReadOnlyDictionary<string, List<CkanModule>> GetDownloadUrlHashIndex()
            => downloadUrlHashIndex ??=
                   (repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                               ?? Enumerable.Empty<AvailableModule>())
                                .SelectMany(am => am.module_version.Values)
                                .SelectMany(m => m.download?.Select(url => new Tuple<Uri, CkanModule>(url, m))
                                                           ?? Enumerable.Empty<Tuple<Uri, CkanModule>>())
                                .GroupBy(tuple => tuple.Item1,
                                         tuple => tuple.Item2)
                                .ToDictionary(grp => NetFileCache.CreateURLHash(grp.Key),
                                              grp => grp.ToList());

        public IReadOnlyDictionary<string, Uri> GetDownloadUrlsByHash()
            => (repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                           ?? Enumerable.Empty<AvailableModule>())
                            .SelectMany(am => am.module_version.Values)
                            .SelectMany(m => m.download ?? Enumerable.Empty<Uri>())
                            .Distinct()
                            .ToDictionary(NetFileCache.CreateURLHash,
                                          url => url);

        /// <summary>
        /// Return all hosts from latest versions of all available modules,
        /// sorted by number of occurrences, most common first
        /// </summary>
        /// <returns>Host strings without duplicates</returns>
        public IEnumerable<string> GetAllHosts()
            => repoDataMgr?.GetAllAvailableModules(Repositories.Values)
                           // Pick all latest modules where download is not null
                           // Merge all the URLs into one sequence
                           .SelectMany(availMod => (availMod?.Latest(ReleaseStatus.development)?.download
                                                    ?? Enumerable.Empty<Uri>())
                                                   .Append(availMod?.Latest(ReleaseStatus.development)?.InternetArchiveDownload))
                           .OfType<Uri>()
                           // Skip relative URLs because they don't have hosts
                           .Where(dlUri => dlUri.IsAbsoluteUri)
                           // Group the URLs by host
                           .GroupBy(dlUri => dlUri.Host)
                           // Put most commonly used hosts first
                           .OrderByDescending(grp => grp.Count())
                           // Alphanumeric sort if same number of usages
                           .ThenBy(grp => grp.Key)
                           // Return the host from each group
                           .Select(grp => grp.Key)
                          ?? Enumerable.Empty<string>();

        // Older clients expect these properties and can handle them being empty ("{}") but not null
        #pragma warning disable IDE0052
        [JsonProperty("available_modules",
                      NullValueHandling = NullValueHandling.Include)]
        [JsonConverter(typeof(JsonAlwaysEmptyObjectConverter))]
        private readonly Dictionary<string, string> legacyAvailableModulesDoNotUse = new Dictionary<string, string>();

        [JsonProperty("download_counts",
                      NullValueHandling = NullValueHandling.Include)]
        [JsonConverter(typeof(JsonAlwaysEmptyObjectConverter))]
        private readonly Dictionary<string, string> legacyDownloadCountsDoNotUse = new Dictionary<string, string>();
        #pragma warning restore IDE0052

    }
}
