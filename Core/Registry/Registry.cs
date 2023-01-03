using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;

using log4net;
using Newtonsoft.Json;

using CKAN.Extensions;
using CKAN.Versioning;
using CKAN.Games;

namespace CKAN
{
    /// <summary>
    /// This is the CKAN registry. All the modules that we know about or have installed
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
        private SortedDictionary<string, Repository> repositories;

        // TODO: These may be good as custom types, especially those which process
        // paths (and flip from absolute to relative, and vice-versa).
        [JsonProperty] internal Dictionary<string, AvailableModule> available_modules;
        // name => path
        [JsonProperty] private  Dictionary<string, string>          installed_dlls;
        [JsonProperty] private  Dictionary<string, InstalledModule> installed_modules;
        // filename (case insensitive on Windows) => module
        [JsonProperty] private  Dictionary<string, string>          installed_files;

        [JsonProperty] public readonly SortedDictionary<string, int> download_counts = new SortedDictionary<string, int>();

        public int? DownloadCount(string identifier)
        {
            int count;
            if (download_counts.TryGetValue(identifier, out count))
            {
                return count;
            }
            return null;
        }

        public void SetDownloadCounts(SortedDictionary<string, int> counts)
        {
            if (counts != null)
            {
                foreach (var kvp in counts)
                {
                    download_counts[kvp.Key] = kvp.Value;
                }
            }
        }

        // Index of which mods provide what, format:
        //   providers[provided] = { provider1, provider2, ... }
        // Built by BuildProvidesIndex, makes LatestAvailableWithProvides much faster.
        [JsonIgnore]
        private Dictionary<string, HashSet<AvailableModule>> providers
            = new Dictionary<string, HashSet<AvailableModule>>();

        /// <summary>
        /// Returns all the activated registries, sorted by priority and name
        /// </summary>
        [JsonIgnore] public SortedDictionary<string, Repository> Repositories
        {
            get { return this.repositories; }

            // TODO writable only so it can be initialized, better ideas welcome
            set { this.repositories = value; }
        }

        /// <summary>
        /// Returns all the installed modules
        /// </summary>
        [JsonIgnore] public IEnumerable<InstalledModule> InstalledModules
        {
            get { return installed_modules.Values; }
        }

        /// <summary>
        /// Returns the names of installed DLLs.
        /// </summary>
        [JsonIgnore] public IEnumerable<string> InstalledDlls
        {
            get { return installed_dlls.Keys; }
        }

        /// <summary>
        /// Returns the file path of a DLL.
        /// null if not found.
        /// </summary>
        public string DllPath(string identifier)
        {
            return installed_dlls.TryGetValue(identifier, out string path) ? path : null;
        }

        /// <summary>
        /// A map between module identifiers and versions for official DLC that are installed.
        /// </summary>
        [JsonIgnore] public IDictionary<string, ModuleVersion> InstalledDlc
        {
            get {
                return installed_modules.Values
                    .Where(im => im.Module.IsDLC)
                    .ToDictionary(im => im.Module.identifier, im => im.Module.version);
            }
        }

        /// <summary>
        /// Find installed modules that are not compatible with the given versions
        /// </summary>
        /// <param name="crit">Version criteria against which to check modules</param>
        /// <returns>
        /// Installed modules that are incompatible, if any
        /// </returns>
        public IEnumerable<InstalledModule> IncompatibleInstalled(GameVersionCriteria crit)
        {
            return installed_modules.Values
                .Where(im => !im.Module.IsCompatibleKSP(crit)
                    && !(GetModuleByVersion(im.identifier, im.Module.version)?.IsCompatibleKSP(crit)
                        ?? false));
        }

        #region Registry Upgrades

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext context)
        {
            // Our context is our game instance.
            GameInstance ksp = (GameInstance)context.Context;

            // Older registries didn't have the installed_files list, so we create one
            // if absent.

            if (installed_files == null)
            {
                log.Warn("Older registry format detected, adding installed files manifest...");
                ReindexInstalled();
            }

            // If we have no registry version at all, then we're from the pre-release period.
            // We would check for a null here, but ints *can't* be null.
            if (registry_version == 0)
            {
                log.Warn("Older registry format detected, normalising paths...");

                // We need case insensitive path matching on Windows
                var normalised_installed_files = Platform.IsWindows
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>();

                foreach (KeyValuePair<string,string> tuple in installed_files)
                {
                    string path = CKANPathUtils.NormalizePath(tuple.Key);

                    if (Path.IsPathRooted(path))
                    {
                        path = ksp.ToRelativeGameDir(path);
                        normalised_installed_files[path] = tuple.Value;
                    }
                    else
                    {
                        // Already relative.
                        normalised_installed_files[path] = tuple.Value;
                    }
                }

                installed_files = normalised_installed_files;

                // Now update all our module file manifests.

                foreach (InstalledModule module in installed_modules.Values)
                {
                    module.Renormalise(ksp);
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
                installed_files = new Dictionary<string, string>(installed_files, StringComparer.OrdinalIgnoreCase);
            }

            // Fix control lock, which previously was indexed with an invalid identifier.
            if (registry_version < 2)
            {
                InstalledModule control_lock_entry;
                const string old_ident = "001ControlLock";
                const string new_ident = "ControlLock";

                if (installed_modules.TryGetValue("001ControlLock", out control_lock_entry))
                {
                    if (ksp == null)
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
                        ksp,
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

            Repository default_repo;
            var oldDefaultRepo = new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip");
            if (repositories != null && repositories.TryGetValue(Repository.default_ckan_repo_name, out default_repo) && default_repo.uri == oldDefaultRepo)
            {
                log.InfoFormat("Updating default metadata URL from {0} to {1}", oldDefaultRepo, ksp.game.DefaultRepositoryURL);
                repositories["default"].uri = ksp.game.DefaultRepositoryURL;
            }

            registry_version = LATEST_REGISTRY_VERSION;
            BuildProvidesIndex();
        }

        /// <summary>
        /// Rebuilds our master index of installed_files.
        /// Called on registry format updates, but safe to be triggered at any time.
        /// </summary>
        public void ReindexInstalled()
        {
            // We need case insensitive path matching on Windows
            installed_files = Platform.IsWindows
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>();

            foreach (InstalledModule module in installed_modules.Values)
            {
                foreach (string file in module.Files)
                {
                    // Register each file we know about as belonging to the given module.
                    installed_files[file] = module.identifier;
                }
            }
        }

        /// <summary>
        /// Do we what we can to repair/preen the registry.
        /// </summary>
        public void Repair()
        {
            ReindexInstalled();
        }

        #endregion

        #region Constructors

        public Registry(
            Dictionary<string, InstalledModule>  installed_modules,
            Dictionary<string, string>           installed_dlls,
            Dictionary<string, AvailableModule>  available_modules,
            Dictionary<string, string>           installed_files,
            SortedDictionary<string, Repository> repositories)
        {
            // Is there a better way of writing constructors than this? Srsly?
            this.installed_modules = installed_modules;
            this.installed_dlls    = installed_dlls;
            this.available_modules = available_modules;
            this.installed_files   = installed_files;
            this.repositories      = repositories;
            registry_version       = LATEST_REGISTRY_VERSION;
            BuildProvidesIndex();
        }

        // If deserialsing, we don't want everything put back directly,
        // thus making sure our version number is preserved, letting us
        // detect registry version upgrades.
        [JsonConstructor]
        private Registry()
        {
        }

        public static Registry Empty()
        {
            return new Registry(
                new Dictionary<string, InstalledModule>(),
                new Dictionary<string, string>(),
                new Dictionary<string, AvailableModule>(),
                new Dictionary<string, string>(),
                new SortedDictionary<string, Repository>()
            );
        }

        #endregion

        #region Transaction Handling

        // Which transaction we're in
        private string enlisted_tx;

        // JSON serialization of self when enlisted with tx
        private string transaction_backup;

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
                var options = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};

                JsonConvert.PopulateObject(transaction_backup, this, options);

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

        public void SetAllAvailable(IEnumerable<CkanModule> newAvail)
        {
            log.DebugFormat(
                "Setting all available modules, count {0}", newAvail);
            EnlistWithTransaction();
            // Clear current modules
            available_modules = new Dictionary<string, AvailableModule>();
            providers.Clear();
            // Add the new modules
            foreach (CkanModule module in newAvail)
            {
                AddAvailable(module);
            }
        }

        /// <summary>
        /// Check whether the available_modules list is empty
        /// </summary>
        /// <returns>
        /// True if we have at least one available mod, false otherwise.
        /// </returns>
        public bool HasAnyAvailable()
        {
            return available_modules.Count > 0;
        }

        /// <summary>
        /// Mark a given module as available.
        /// </summary>
        public void AddAvailable(CkanModule module)
        {
            log.DebugFormat("Adding available module {0}", module);
            EnlistWithTransaction();

            var identifier = module.identifier;
            // If we've never seen this module before, create an entry for it.
            if (!available_modules.ContainsKey(identifier))
            {
                log.DebugFormat("Adding new available module {0}", identifier);
                available_modules[identifier] = new AvailableModule(identifier);
            }

            // Now register the actual version that we have.
            // (It's okay to have multiple versions of the same mod.)

            log.DebugFormat("Available: {0} version {1}", identifier, module.version);
            available_modules[identifier].Add(module);
            BuildProvidesIndexFor(available_modules[identifier]);
            sorter = null;
        }

        /// <summary>
        /// Remove the given module from the registry of available modules.
        /// Does *nothing* if the module was not present to begin with.
        /// </summary>
        public void RemoveAvailable(string identifier, ModuleVersion version)
        {
            AvailableModule availableModule;
            if (available_modules.TryGetValue(identifier, out availableModule))
            {
                log.DebugFormat("Removing available module {0} {1}",
                    identifier, version);
                EnlistWithTransaction();
                availableModule.Remove(version);
            }
        }

        /// <summary>
        /// Removes the given module from the registry of available modules.
        /// Does *nothing* if the module was not present to begin with.</summary>
        public void RemoveAvailable(CkanModule module)
        {
            RemoveAvailable(module.identifier, module.version);
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.CompatibleModules"/>
        /// </summary>
        public IEnumerable<CkanModule> CompatibleModules(GameVersionCriteria ksp_version)
        {
            // Set up our compatibility partition
            SetCompatibleVersion(ksp_version);
            return sorter.Compatible.Values.Select(avail => avail.Latest(ksp_version)).ToList();
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.IncompatibleModules"/>
        /// </summary>
        public IEnumerable<CkanModule> IncompatibleModules(GameVersionCriteria ksp_version)
        {
            // Set up our compatibility partition
            SetCompatibleVersion(ksp_version);
            return sorter.Incompatible.Values.Select(avail => avail.Latest(null)).ToList();
        }

        /// <summary>
        /// Check whether any versions of this mod are installable (including dependencies) on the given game versions.
        /// Quicker than checking CompatibleModules for one identifier.
        /// </summary>
        /// <param name="identifier">Identifier of mod</param>
        /// <param name="crit">Game versions</param>
        /// <returns>true if any version is recursively compatible, false otherwise</returns>
        public bool IdentifierCompatible(string identifier, GameVersionCriteria crit)
        {
            // Set up our compatibility partition
            SetCompatibleVersion(crit);
            return sorter.Compatible.ContainsKey(identifier);
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.LatestAvailable" />
        /// </summary>
        public CkanModule LatestAvailable(
            string module,
            GameVersionCriteria ksp_version,
            RelationshipDescriptor relationship_descriptor = null)
        {
            // TODO: Consider making this internal, because practically everything should
            // be calling LatestAvailableWithProvides()
            log.DebugFormat("Finding latest available for {0}", module);

            // TODO: Check user's stability tolerance (stable, unstable, testing, etc)

            try
            {
                return available_modules[module].Latest(ksp_version, relationship_descriptor);
            }
            catch (KeyNotFoundException)
            {
                throw new ModuleNotFoundKraken(module);
            }
        }

        /// <summary>
        /// Find modules with a given identifier
        /// </summary>
        /// <param name="identifier">Identifier of modules to find</param>
        /// <returns>
        /// List of all modules with this identifier
        /// </returns>
        public IEnumerable<CkanModule> AvailableByIdentifier(string identifier)
        {
            log.DebugFormat("Finding all available versions for {0}", identifier);
            try
            {
                return available_modules[identifier].AllAvailable();
            }
            catch (KeyNotFoundException)
            {
                throw new ModuleNotFoundKraken(identifier);
            }
        }

        /// <summary>
        /// Get full JSON metadata string for a mod's available versions
        /// </summary>
        /// <param name="identifier">Name of the mod to look up</param>
        /// <returns>
        /// JSON formatted string for all the available versions of the mod
        /// </returns>
        public string GetAvailableMetadata(string identifier)
        {
            try
            {
                return available_modules[identifier].FullMetadata();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Return the latest game version compatible with the given mod.
        /// </summary>
        /// <param name="identifier">Name of mod to check</param>
        public GameVersion LatestCompatibleKSP(string identifier)
        {
            return available_modules.ContainsKey(identifier)
                ? available_modules[identifier].LatestCompatibleKSP()
                : null;
        }

        /// <summary>
        /// Find the minimum and maximum mod versions and compatible game versions
        /// for a list of modules (presumably different versions of the same mod).
        /// </summary>
        /// <param name="modVersions">The modules to inspect</param>
        /// <param name="minMod">Return parameter for the lowest  mod  version</param>
        /// <param name="maxMod">Return parameter for the highest mod  version</param>
        /// <param name="minKsp">Return parameter for the lowest  game version</param>
        /// <param name="maxKsp">Return parameter for the highest game version</param>
        public static void GetMinMaxVersions(IEnumerable<CkanModule> modVersions,
                out ModuleVersion minMod, out ModuleVersion maxMod,
                out GameVersion    minKsp, out GameVersion    maxKsp)
        {
            minMod = maxMod = null;
            minKsp = maxKsp = null;
            foreach (CkanModule rel in modVersions.Where(v => v != null))
            {
                if (minMod == null || minMod > rel.version)
                {
                    minMod = rel.version;
                }
                if (maxMod == null || maxMod < rel.version)
                {
                    maxMod = rel.version;
                }
                GameVersion relMin = rel.EarliestCompatibleKSP();
                GameVersion relMax = rel.LatestCompatibleKSP();
                if (minKsp == null || !minKsp.IsAny && (minKsp > relMin || relMin.IsAny))
                {
                    minKsp = relMin;
                }
                if (maxKsp == null || !maxKsp.IsAny && (maxKsp < relMax || relMax.IsAny))
                {
                    maxKsp = relMax;
                }
            }
        }

        /// <summary>
        /// Generate the providers index so we can find providing modules quicker
        /// </summary>
        private void BuildProvidesIndex()
        {
            providers.Clear();
            foreach (AvailableModule am in available_modules.Values)
            {
                BuildProvidesIndexFor(am);
            }
        }

        /// <summary>
        /// Ensure one AvailableModule is present in the right spots in the providers index
        /// </summary>
        private void BuildProvidesIndexFor(AvailableModule am)
        {
            foreach (CkanModule m in am.AllAvailable())
            {
                foreach (string provided in m.ProvidesList)
                {
                    if (providers.TryGetValue(provided, out HashSet<AvailableModule> provs))
                        provs.Add(am);
                    else
                        providers.Add(provided, new HashSet<AvailableModule>() { am });
                }
            }
        }

        public void BuildTagIndex(ModuleTagList tags)
        {
            tags.Tags.Clear();
            tags.Untagged.Clear();
            foreach (AvailableModule am in available_modules.Values)
            {
                tags.BuildTagIndexFor(am);
            }
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.LatestAvailableWithProvides" />
        /// </summary>
        public List<CkanModule> LatestAvailableWithProvides(
            string identifier,
            GameVersionCriteria ksp_version,
            RelationshipDescriptor relationship_descriptor = null,
            IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null
        )
        {
            if (providers.TryGetValue(identifier, out HashSet<AvailableModule> provs))
            {
                // For each AvailableModule, we want the latest one matching our constraints
                return provs
                    .Select(am => am.Latest(
                        ksp_version,
                        relationship_descriptor,
                        installed ?? InstalledModules.Select(im => im.Module),
                        toInstall
                    ))
                    .Where(m => m?.ProvidesList?.Contains(identifier) ?? false)
                    .ToList();
            }
            else
            {
                // Nothing provides this, return empty list
                return new List<CkanModule>();
            }
        }

        /// <summary>
        /// Returns the specified CkanModule with the version specified,
        /// or null if it does not exist.
        /// <see cref = "IRegistryQuerier.GetModuleByVersion" />
        /// </summary>
        public CkanModule GetModuleByVersion(string ident, ModuleVersion version)
        {
            log.DebugFormat("Trying to find {0} version {1}", ident, version);

            if (!available_modules.ContainsKey(ident))
            {
                return null;
            }

            AvailableModule available = available_modules[ident];
            return available.ByVersion(version);
        }

        /// <summary>
        /// Register the supplied module as having been installed, thereby keeping
        /// track of its metadata and files.
        /// </summary>
        public void RegisterModule(CkanModule mod, IEnumerable<string> absolute_files, GameInstance ksp, bool autoInstalled)
        {
            log.DebugFormat("Registering module {0}", mod);
            EnlistWithTransaction();

            sorter = null;

            // But we also want to keep track of all its files.
            // We start by checking to see if any files are owned by another mod,
            // if so, we abort with a list of errors.

            var inconsistencies = new List<string>();

            // We always work with relative files, so let's get some!
            IEnumerable<string> relative_files = absolute_files
                .Select(x => ksp.ToRelativeGameDir(x))
                .Memoize();

            // For now, it's always cool if a module wants to register a directory.
            // We have to flip back to absolute paths to actually test this.
            foreach (string file in relative_files.Where(file => !Directory.Exists(ksp.ToAbsoluteGameDir(file))))
            {
                string owner;
                if (installed_files.TryGetValue(file, out owner))
                {
                    // Woah! Registering an already owned file? Not cool!
                    // (Although if it existed, we should have thrown a kraken well before this.)
                    inconsistencies.Add(string.Format(
                        Properties.Resources.RegistryFileConflict,
                        mod.identifier, file, owner
                    ));
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
            foreach (string file in relative_files)
            {
                installed_files[file] = mod.identifier;
            }

            // Finally, register our module proper.
            var installed = new InstalledModule(ksp, mod, relative_files, autoInstalled);
            installed_modules.Add(mod.identifier, installed);
        }

        /// <summary>
        /// Deregister a module, which must already have its files removed, thereby
        /// forgetting abouts its metadata and files.
        ///
        /// Throws an InconsistentKraken if not all files have been removed.
        /// </summary>
        public void DeregisterModule(GameInstance ksp, string module)
        {
            log.DebugFormat("Deregistering module {0}", module);
            EnlistWithTransaction();

            sorter = null;

            var inconsistencies = new List<string>();

            var absolute_files = installed_modules[module].Files.Select(ksp.ToAbsoluteGameDir);
            // Note, this checks to see if a *file* exists; it doesn't
            // trigger on directories, which we allow to still be present
            // (they may be shared by multiple mods.

            foreach (var absolute_file in absolute_files.Where(File.Exists))
            {
                inconsistencies.Add(string.Format(
                    Properties.Resources.RegistryFileNotRemoved,
                    absolute_file, module));
            }

            if (inconsistencies.Count > 0)
            {
                // Uh oh, what mess have we got ourselves into now, Inconsistency Kraken?
                throw new InconsistentKraken(inconsistencies);
            }

            // Okay, all the files are gone. Let's clear our metadata.
            foreach (string rel_file in installed_modules[module].Files)
            {
                installed_files.Remove(rel_file);
            }

            // Bye bye, module, it's been nice having you visit.
            installed_modules.Remove(module);
        }

        /// <summary>
        /// Registers the given DLL as having been installed. This provides some support
        /// for pre-CKAN modules.
        ///
        /// Does nothing if the DLL is already part of an installed module.
        /// </summary>
        public void RegisterDll(GameInstance ksp, string absolute_path)
        {
            log.DebugFormat("Registering DLL {0}", absolute_path);
            string relative_path = ksp.ToRelativeGameDir(absolute_path);

            string dllIdentifier = ksp.DllPathToIdentifier(relative_path);
            if (dllIdentifier == null)
            {
                log.WarnFormat("Attempted to index {0} which is not a DLL", relative_path);
                return;
            }

            string owner;
            if (installed_files.TryGetValue(relative_path, out owner))
            {
                log.InfoFormat(
                    "Not registering {0}, it belongs to {1}",
                    relative_path,
                    owner
                );
                return;
            }

            EnlistWithTransaction();

            log.InfoFormat("Registering {0} from {1}", dllIdentifier, relative_path);

            // We're fine if we overwrite an existing key.
            installed_dlls[dllIdentifier] = relative_path;
        }

        /// <summary>
        /// Clears knowledge of all DLLs from the registry.
        /// </summary>
        public void ClearDlls()
        {
            log.Debug("Clearing DLLs");
            EnlistWithTransaction();
            installed_dlls = new Dictionary<string, string>();
        }

        public void RegisterDlc(string identifier, UnmanagedModuleVersion version)
        {
            CkanModule dlcModule = null;
            if (available_modules.TryGetValue(identifier, out AvailableModule avail))
            {
                dlcModule = avail.ByVersion(version);
            }
            if (dlcModule == null)
            {
                // Don't have the real thing, make a fake one
                dlcModule = new CkanModule(
                    new ModuleVersion("v1.28"),
                    identifier,
                    identifier,
                    "An official expansion pack for KSP",
                    null,
                    new List<string>() { "SQUAD" },
                    new List<License>() { new License("restricted") },
                    version,
                    null,
                    "dlc"
                );
            }
            installed_modules.Add(
                identifier,
                new InstalledModule(null, dlcModule, new string[] { }, false)
            );
        }

        public void ClearDlc()
        {
            var installedDlcs = installed_modules.Values
                .Where(instMod => instMod.Module.IsDLC)
                .ToList();
            foreach (var instMod in installedDlcs)
            {
                installed_modules.Remove(instMod.identifier);
            }
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
        public InstalledModule InstalledModule(string module)
        {
            return installed_modules.TryGetValue(module, out InstalledModule installedModule)
                ? installedModule
                : null;
        }

        /// <summary>
        /// Find modules provided by currently installed modules
        /// </summary>
        /// <returns>
        /// Dictionary of provided (virtual) modules and a
        /// ProvidesVersion indicating what provides them
        /// </returns>
        internal Dictionary<string, ProvidesModuleVersion> ProvidedByInstalled()
        {
            // TODO: In the future it would be nice to cache this list, and mark it for rebuild
            // if our installed modules change.
            var installed = new Dictionary<string, ProvidesModuleVersion>();

            foreach (var modinfo in installed_modules)
            {
                CkanModule module = modinfo.Value.Module;

                // Skip if this module provides nothing.
                if (module.provides == null)
                {
                    continue;
                }

                foreach (string provided in module.provides)
                {
                    installed[provided] = new ProvidesModuleVersion(module.identifier, module.version.ToString());
                }
            }

            return installed;
        }

        /// <summary>
        /// <see cref = "IRegistryQuerier.InstalledVersion" />
        /// </summary>
        public ModuleVersion InstalledVersion(string modIdentifier, bool with_provides=true)
        {
            InstalledModule installedModule;

            // If it's genuinely installed, return the details we have.
            // (Includes DLCs)
            if (installed_modules.TryGetValue(modIdentifier, out installedModule))
            {
                return installedModule.Module.version;
            }

            // If it's in our autodetected registry, return that.
            if (installed_dlls.ContainsKey(modIdentifier))
            {
                return new UnmanagedModuleVersion(null);
            }

            // Finally we have our provided checks. We'll skip these if
            // withProvides is false.
            if (!with_provides) return null;

            var provided = ProvidedByInstalled();

            ProvidesModuleVersion version;
            return provided.TryGetValue(modIdentifier, out version) ? version : null;
        }

        /// <summary>
        /// <see cref = "IRegistryQuerier.GetInstalledVersion" />
        /// </summary>
        public CkanModule GetInstalledVersion(string mod_identifier)
        {
            InstalledModule installedModule;
            return installed_modules.TryGetValue(mod_identifier, out installedModule)
                ? installedModule.Module
                : null;
        }

        /// <summary>
        /// Returns the module which owns this file, or null if not known.
        /// Throws a PathErrorKraken if an absolute path is provided.
        /// </summary>
        public string FileOwner(string file)
        {
            file = CKANPathUtils.NormalizePath(file);

            if (Path.IsPathRooted(file))
            {
                throw new PathErrorKraken(
                    file,
                    "KSPUtils.FileOwner can only work with relative paths."
                );
            }

            string fileOwner;
            return installed_files.TryGetValue(file, out fileOwner) ? fileOwner : null;
        }

        /// <summary>
        /// <see cref="IRegistryQuerier.CheckSanity"/>
        /// </summary>
        public void CheckSanity()
        {
            IEnumerable<CkanModule> installed = from pair in installed_modules select pair.Value.Module;
            SanityChecker.EnforceConsistency(installed, installed_dlls.Keys, InstalledDlc);
        }

        public List<string> GetSanityErrors()
        {
            var installed = from pair in installed_modules select pair.Value.Module;
            return SanityChecker.ConsistencyErrors(installed, installed_dlls.Keys, InstalledDlc).ToList();
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
        /// <returns>List of modules whose dependencies are about to be or already removed.</returns>
        internal static IEnumerable<string> FindReverseDependencies(
            List<string>                       modulesToRemove,
            List<CkanModule>                   modulesToInstall,
            HashSet<CkanModule>                origInstalled,
            HashSet<string>                    dlls,
            IDictionary<string, ModuleVersion> dlc,
            Func<RelationshipDescriptor, bool> satisfiedFilter = null)
        {
            log.DebugFormat("Finding reverse dependencies of: {0}", string.Join(", ", modulesToRemove));
            log.DebugFormat("From installed mods: {0}", string.Join(", ", origInstalled));
            log.DebugFormat("Installing mods: {0}", string.Join(", ", modulesToInstall ?? new List<CkanModule>()));

            // The empty list has no reverse dependencies
            // (Don't remove broken modules if we're only installing)
            if (modulesToRemove.Any())
            {
                // All modules in the input are included in the output
                foreach (string starter in modulesToRemove)
                {
                    yield return starter;
                }
                while (true)
                {
                    // Make our hypothetical install, and remove the listed modules from it.
                    // Clone because we alter hypothetical.
                    var hypothetical = new HashSet<CkanModule>(origInstalled);
                    if (modulesToInstall != null)
                    {
                        // Pretend the mods we are going to install are already installed, so that dependencies that will be
                        // satisfied by a mod that is going to be installed count as satisfied.
                        hypothetical = hypothetical.Concat(modulesToInstall).ToHashSet();
                    }
                    hypothetical.RemoveWhere(mod => modulesToRemove.Contains(mod.identifier));

                    log.DebugFormat("Removing: {0}", string.Join(", ", modulesToRemove));
                    log.DebugFormat("Keeping: {0}", string.Join(", ", hypothetical));

                    // Find what would break with this configuration
                    var brokenDeps = SanityChecker.FindUnsatisfiedDepends(hypothetical, dlls, dlc);
                    if (satisfiedFilter != null)
                    {
                        brokenDeps.RemoveAll(kvp => satisfiedFilter(kvp.Value));
                    }
                    var brokenIdents = brokenDeps.Select(x => x.Key.identifier).ToHashSet();

                    if (modulesToInstall != null)
                    {
                        // Make sure to only report modules as broken if they are actually currently installed.
                        // This is mainly to remove the modulesToInstall again which we added
                        // earlier to the hypothetical list.
                        brokenIdents.IntersectWith(origInstalled.Select(m => m.identifier));
                    }
                    log.DebugFormat("Broken: {0}", string.Join(", ", brokenIdents));
                    // Lazily return each newly found rev dep
                    foreach (string newFound in brokenIdents.Except(modulesToRemove))
                    {
                        yield return newFound;
                    }

                    // If nothing else would break, it's just the list of modules we're removing
                    var to_remove = new HashSet<string>(modulesToRemove);

                    if (to_remove.IsSupersetOf(brokenIdents))
                    {
                        log.DebugFormat("{0} is a superset of {1}, work done", string.Join(", ", to_remove), string.Join(", ", brokenIdents));
                        break;
                    }

                    // Otherwise, remove our broken modules as well, and recurse
                    modulesToRemove = brokenIdents.Union(to_remove).ToList();
                }
            }
        }

        /// <summary>
        /// Return modules which are dependent on the modules passed in or modules in the return list
        /// </summary>
        public IEnumerable<string> FindReverseDependencies(
            List<string>                       modulesToRemove,
            List<CkanModule>                   modulesToInstall = null,
            Func<RelationshipDescriptor, bool> satisfiedFilter = null
        )
        {
            var installed = new HashSet<CkanModule>(installed_modules.Values.Select(x => x.Module));
            return FindReverseDependencies(modulesToRemove, modulesToInstall, installed, new HashSet<string>(installed_dlls.Keys), InstalledDlc, satisfiedFilter);
        }

        /// <summary>
        /// Get a dictionary of all mod versions indexed by their downloads' SHA-1 hash.
        /// Useful for finding the mods for a group of files without repeatedly searching the entire registry.
        /// </summary>
        /// <returns>
        /// dictionary[sha1] = {mod1, mod2, mod3};
        /// </returns>
        public Dictionary<string, List<CkanModule>> GetSha1Index()
        {
            var index = new Dictionary<string, List<CkanModule>>();
            foreach (var kvp in available_modules)
            {
                AvailableModule am = kvp.Value;
                foreach (var kvp2 in am.module_version)
                {
                    CkanModule mod = kvp2.Value;
                    if (mod.download_hash != null)
                    {
                        if (index.ContainsKey(mod.download_hash.sha1))
                        {
                            index[mod.download_hash.sha1].Add(mod);
                        }
                        else
                        {
                            index.Add(mod.download_hash.sha1, new List<CkanModule>() {mod});
                        }
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Get a dictionary of all mod versions indexed by their download URLs' hash.
        /// Useful for finding the mods for a group of URLs without repeatedly searching the entire registry.
        /// </summary>
        /// <returns>
        /// dictionary[urlHash] = {mod1, mod2, mod3};
        /// </returns>
        public Dictionary<string, List<CkanModule>> GetDownloadHashIndex()
        {
            var index = new Dictionary<string, List<CkanModule>>();
            foreach (var kvp in available_modules)
            {
                AvailableModule am = kvp.Value;
                foreach (var kvp2 in am.module_version)
                {
                    CkanModule mod = kvp2.Value;
                    if (mod.download != null)
                    {
                        string hash = NetFileCache.CreateURLHash(mod.download);
                        if (index.ContainsKey(hash))
                        {
                            index[hash].Add(mod);
                        }
                        else
                        {
                            index.Add(hash, new List<CkanModule>() {mod});
                        }
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Partition all CkanModules in available_modules into
        /// compatible and incompatible groups.
        /// </summary>
        /// <param name="versCrit">Version criteria to determine compatibility</param>
        public void SetCompatibleVersion(GameVersionCriteria versCrit)
        {
            if (!versCrit.Equals(sorter?.CompatibleVersions))
            {
                sorter = new CompatibilitySorter(
                    versCrit, available_modules, providers,
                    installed_modules,
                    InstalledDlls.ToHashSet(), InstalledDlc
                );
            }
        }

        [JsonIgnore] private CompatibilitySorter sorter;
    }
}
