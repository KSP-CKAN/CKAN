using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{
    /// <summary>
    /// This is the CKAN registry. All the modules that we know about or have installed
    /// are contained in here.
    /// </summary>

    // TODO: It would be *great* for the registry to have a 'dirty' bit, that records if
    // anything has changed. But that would involve catching access to a lot of the data
    // structures we pass back, and we're not doing that yet.

    public class Registry :IEnlistmentNotification
    {
        [JsonIgnore] private const int LATEST_REGISTRY_VERSION = 3;
        [JsonIgnore] private static readonly ILog log = LogManager.GetLogger(typeof (Registry));

        [JsonProperty] private int registry_version;

        [JsonProperty("sorted_repositories")]
        private SortedDictionary<string, Repository> repositories; // name => Repository

        // TODO: These may be good as custom types, especially those which process
        // paths (and flip from absolute to relative, and vice-versa).
        [JsonProperty] internal Dictionary<string, AvailableModule> available_modules;
        [JsonProperty] private Dictionary<string, string> installed_dlls; // name => path
        [JsonProperty] private Dictionary<string, InstalledModule> installed_modules;
        [JsonProperty] private Dictionary<string, string> installed_files; // filename => module

        [JsonIgnore] private string transaction_backup;

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
        [JsonIgnore] public ICollection<InstalledModule> InstalledModules
        {
            get { return installed_modules.Values; }
        }

        /// <summary>
        /// Returns the names of installed DLLs.
        /// </summary>
        [JsonIgnore] public ICollection<string> InstalledDlls
        {
            get { return installed_dlls.Keys; }
        }

        #region Registry Upgrades

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext context)
        {
            // Our context is our KSP install.
            KSP ksp = (KSP) context.Context;


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

                var normalised_installed_files = new Dictionary<string,string>();

                foreach (KeyValuePair<string,string> tuple in installed_files)
                {
                    string path = KSPPathUtils.NormalizePath(tuple.Key);

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
                    Module control_lock_mod = control_lock_entry.Module;

                    // Change to the correct ident.
                    control_lock_mod.identifier = new_ident;

                    // Prepare to re-index.
                    var new_control_lock_installed = new InstalledModule(
                        ksp,
                        control_lock_mod,
                        control_lock_entry.Files
                    );

                    // Re-insert into registry.
                    installed_modules[new_control_lock_installed.identifier] = new_control_lock_installed;

                    // Re-index files.
                    ReindexInstalled();
                }
            }

            registry_version = LATEST_REGISTRY_VERSION;
        }

        /// <summary>
        /// Rebuilds our master index of installed_files.
        /// Called on registry format updates, but safe to be triggered at any time.
        /// </summary>
        public void ReindexInstalled()
        {
            installed_files = new Dictionary<string, string>();

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
            Dictionary<string, InstalledModule> installed_modules,
            Dictionary<string, string> installed_dlls,
            Dictionary<string, AvailableModule> available_modules,
            Dictionary<string, string> installed_files,
            SortedDictionary<string, Repository> repositories
            )
        {
            // Is there a better way of writing constructors than this? Srsly?
            this.installed_modules = installed_modules;
            this.installed_dlls = installed_dlls;
            this.available_modules = available_modules;
            this.installed_files = installed_files;
            this.repositories = repositories;
            registry_version = LATEST_REGISTRY_VERSION;
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

        // We use this to record which transaction we're in.
        private string enlisted_tx;

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

            enlisted_tx = null;
            transaction_backup = null;

            enlistment.Done();
            log.Debug("Registry transaction committed");

            // TODO: Should we save to disk at the end of a Tx?
            // TODO: If so, we should abort if we find a save that's while a Tx is in progress?
            //
            // In either case, do we want the registry_manager to be Tx aware? 
        }

        public void Rollback(Enlistment enlistment)
        {
            log.Info("Aborted transaction, rolling back in-memory registry changes.");

            // In theory, this should put everything back the way it was, overwriting whatever
            // we had previously.

            var options = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};

            JsonConvert.PopulateObject(transaction_backup, this, options);

            enlisted_tx = null;
            transaction_backup = null;

            enlistment.Done();
        }

        private void SaveState()
        {
            // Hey, you know what's a great way to back-up your own object?
            // JSON. ;)
            transaction_backup = JsonConvert.SerializeObject(this, Formatting.None);
            log.Debug("State saved");
        }

        /// <summary>
        /// "Pardon me, but I couldn't help but overhear you're in a Transaction..."
        /// 
        /// Adds our registry to the current transaction. This should be called whenever we
        /// do anything which may dirty the registry.
        /// </summary>
        // 
        // http://wondermark.com/1k62/
        private void SealionTransaction()
        {
            if (Transaction.Current != null)
            {
                string current_tx = Transaction.Current.TransactionInformation.LocalIdentifier;

                if (enlisted_tx == null)
                {
                    log.Debug("Pardon me, but I couldn't help overhear you're in a transaction...");
                    Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                    SaveState();
                    enlisted_tx = current_tx;
                }
                else if (enlisted_tx != current_tx)
                {
                    log.Error("CKAN registry does not support nested transactions.");
                    throw new TransactionalKraken("CKAN registry does not support nested transactions.");
                }

                // If we're here, it's a transaction we're already participating in,
                // so do nothing.
            }
        }

        #endregion

        /// <summary>
        /// Clears all available modules from the registry.
        /// </summary>
        public void ClearAvailable()
        {
            SealionTransaction();
            available_modules = new Dictionary<string, AvailableModule>();
        }

        /// <summary>
        /// Mark a given module as available.
        /// </summary>
        public void AddAvailable(CkanModule module)
        {
            SealionTransaction();

            // If we've never seen this module before, create an entry for it.

            if (! available_modules.ContainsKey(module.identifier))
            {
                log.DebugFormat("Adding new available module {0}", module.identifier);
                available_modules[module.identifier] = new AvailableModule();
            }

            // Now register the actual version that we have.
            // (It's okay to have multiple versions of the same mod.)

            log.DebugFormat("Available: {0} version {1}", module.identifier, module.version);
            available_modules[module.identifier].Add(module);
        }

        /// <summary>
        /// Remove the given module from the registry of available modules.
        /// Does *nothing* if the module was not present to begin with.
        /// </summary>
        public void RemoveAvailable(string identifier, Version version)
        {
            AvailableModule availableModule;
            if (available_modules.TryGetValue(identifier, out availableModule))
            {
                SealionTransaction();
                availableModule.Remove(version);
            }
        }

        /// <summary>
        /// Removes the given module from the registry of available modules.
        /// Does *nothing* if the module was not present to begin with.</summary>
        public void RemoveAvailable(Module module)
        {
            RemoveAvailable(module.identifier, module.version);
        }

        /// <summary>
        /// Returns a simple array of all latest available modules for
        /// the specified version of KSP (installed version by default)
        /// </summary>
        // TODO: This name is misleading. It's more a LatestAvailable's'
        public List<CkanModule> Available(KSPVersion ksp_version)
        {            
            var candidates = new List<string>(available_modules.Keys);
            var compatible = new List<CkanModule>();

            // It's nice to see things in alphabetical order, so sort our keys first.
            candidates.Sort();

            //Cache 
            AvailableModule[] modules_for_current_version = available_modules.Values.Where(pair => pair.Latest(ksp_version) != null).ToArray();
            // Now find what we can give our user.
            foreach (string candidate in candidates)
            {
                CkanModule available = LatestAvailable(candidate, ksp_version);

                if (available != null)
                {
                    // we need to check that we can get everything we depend on
                    bool failedDepedency = false;

                    if (available.depends != null)
                    {
                        foreach (RelationshipDescriptor dependency in available.depends)
                        {
                            try
                            {
                                if (LatestAvailableWithProvides(dependency.name, ksp_version, modules_for_current_version).Count == 0)
                                {
                                    failedDepedency = true;
                                    break;
                                }
                            }
                            catch (KeyNotFoundException e)
                            {
                                log.ErrorFormat("Cannot find available version with provides for {0} in registry", dependency.name);
                                throw e;
                            }
                            catch (ModuleNotFoundKraken)
                            {
                                failedDepedency = true;
                                break;
                            }
                        }
                    }

                    if (!failedDepedency)
                    {
                        compatible.Add(available);
                    }
                }
            }

            return compatible;
        }

        /// <summary>
        ///     Returns a simple array of all incompatible modules for
        ///     the specified version of KSP (installed version by default)
        /// </summary>
        public List<CkanModule> Incompatible(KSPVersion ksp_version)
        {           
            var candidates = new List<string>(available_modules.Keys);
            var incompatible = new List<CkanModule>();

            // It's nice to see things in alphabetical order, so sort our keys first.
            candidates.Sort();

            // Now find what we can give our user.
            foreach (string candidate in candidates)
            {
                CkanModule available = LatestAvailable(candidate, ksp_version);

                if (available == null)
                {
                    incompatible.Add(LatestAvailable(candidate, null));
                }
            }

            return incompatible;
        }
        

        /// <summary>
        ///     Returns the latest available version of a module that
        ///     satisifes the specified version.
        ///     Throws a ModuleNotFoundException if asked for a non-existant module.
        ///     Returns null if there's simply no compatible version for this system.
        ///     If no ksp_version is provided, the latest module for *any* KSP is returned.
        /// </summary>
         
        // TODO: Consider making this internal, because practically everything should
        // be calling LatestAvailableWithProvides()
        public CkanModule LatestAvailable(string module, KSPVersion ksp_version)
        {
            log.DebugFormat("Finding latest available for {0}", module);

            // TODO: Check user's stability tolerance (stable, unstable, testing, etc)

            try
            {
                return available_modules[module].Latest(ksp_version);
            }
            catch (KeyNotFoundException)
            {
                throw new ModuleNotFoundKraken(module);
            }
        }

        /// <summary>
        ///     Returns the latest available version of a module that
        ///     satisifes the specified version. Takes into account module 'provides',
        ///     which may result in a list of alternatives being provided.
        ///     Returns an empty list if nothing is available for our system, which includes if no such module exists.
        ///     If no KSP version is provided, the latest module for *any* KSP version is given.
        /// </summary>
        public List<CkanModule> LatestAvailableWithProvides(string module, KSPVersion ksp_version)
        {
            // This public interface calcultes a cache of modules which
            // are compatible with the current version of KSP, and then
            // calls the private version below for heavy lifting.
            return LatestAvailableWithProvides(module, ksp_version,
                available_modules.Values.Where(pair => pair.Latest(ksp_version) != null));
        }

        /// <summary>
        /// Returns the latest version of a module that can be installed for
        /// the given KSP version. This is a *private* method that assumes
        /// the `available_for_current_version` list has been correctly
        /// calculated. Not for direct public consumption. ;)
        /// </summary>
        private List<CkanModule> LatestAvailableWithProvides(string module, KSPVersion ksp_version,
            IEnumerable<AvailableModule> available_for_current_version)
        {
            log.DebugFormat("Finding latest available with provides for {0}", module);

            // TODO: Check user's stability tolerance (stable, unstable, testing, etc)

            var modules = new List<CkanModule>();

            try
            {
                // If we can find the module requested for our KSP, use that.
                CkanModule mod = LatestAvailable(module, ksp_version);
                if (mod != null)
                {
                    modules.Add(mod);
                }
            }
            catch (ModuleNotFoundKraken)
            {
                // It's cool if we can't find it, though.
            }

            // Walk through all our available modules, and see if anything
            // provides what we need.

            foreach (AvailableModule available_module in available_for_current_version)
            {
                // Get our candidate module. We can assume this is non-null, as
                // if it *is* null then available_for_current_version is corrupted,
                // and something is terribly wrong.
                CkanModule candidate = available_module.Latest(ksp_version);

                // Find everything this module provides (for our version of KSP)
                List<string> provides = candidate.provides;

                // If the module has provides, and any of them are what we're looking
                // for, the add it to our list.
                if (provides != null && provides.Any(provided => provided == module))
                {
                    modules.Add(candidate);
                }
            }
            return modules;
        }


        public CkanModule GetModuleByVersion(string ident, string version)
        {
            return GetModuleByVersion(ident, new Version(version));
        }

        public CkanModule GetModuleByVersion(string ident, Version version)
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
        ///     Register the supplied module as having been installed, thereby keeping
        ///     track of its metadata and files.
        /// </summary>
        public void RegisterModule(Module mod, IEnumerable<string> absolute_files, KSP ksp)
        {
            SealionTransaction();

            // But we also want to keep track of all its files.
            // We start by checking to see if any files are owned by another mod,
            // if so, we abort with a list of errors.

            var inconsistencies = new List<string>();

            // We always work with relative files, so let's get some!
            IEnumerable<string> relative_files = absolute_files.Select(x => ksp.ToRelativeGameDir(x));

            // For now, it's always cool if a module wants to register a directory.
            // We have to flip back to absolute paths to actually test this.
            foreach (string file in relative_files.Where(file => !Directory.Exists(ksp.ToAbsoluteGameDir(file))))
            {
                string owner;
                if (installed_files.TryGetValue(file, out owner))
                {
                    // Woah! Registering an already owned file? Not cool!
                    // (Although if it existed, we should have thrown a kraken well before this.)                    
                    inconsistencies.Add(
                        string.Format("{0} wishes to install {1}, but this file is registered to {2}",
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
            var installed = new InstalledModule(ksp, mod, relative_files);
            installed_modules.Add(mod.identifier, installed);
        }

        /// <summary>
        /// Deregister a module, which must already have its files removed, thereby
        /// forgetting abouts its metadata and files.
        /// 
        /// Throws an InconsistentKraken if not all files have been removed.
        /// </summary>
        public void DeregisterModule(KSP ksp, string module)
        {
            SealionTransaction();

            var inconsistencies = new List<string>();

            var absolute_files = installed_modules[module].Files.Select(ksp.ToAbsoluteGameDir);
            // Note, this checks to see if a *file* exists; it doesn't
            // trigger on directories, which we allow to still be present
            // (they may be shared by multiple mods.
                
            foreach (var absolute_file in absolute_files.Where(File.Exists))
            {
                inconsistencies.Add(string.Format(
                    "{0} is registered to {1} but has not been removed!",
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
        public void RegisterDll(KSP ksp, string absolute_path)
        {
            SealionTransaction();

            string relative_path = ksp.ToRelativeGameDir(absolute_path);

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
                
            // http://xkcd.com/208/
            // This regex works great for things like GameData/Foo/Foo-1.2.dll
            Match match = Regex.Match(
                relative_path, @"
                    ^GameData/            # DLLs only live in GameData
                    (?:.*/)?              # Intermediate paths (ending with /)
                    (?<modname>[^.]+)     # Our DLL name, up until the first dot.
                    .*\.dll$              # Everything else, ending in dll
                ",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            );

            string modName = match.Groups["modname"].Value;

            if (modName.Length == 0)
            {
                log.WarnFormat("Attempted to index {0} which is not a DLL", relative_path);
                return;
            }

            log.InfoFormat("Registering {0} from {1}", modName, relative_path);

            // We're fine if we overwrite an existing key.
            installed_dlls[modName] = relative_path;
        }

        /// <summary>
        /// Clears knowledge of all DLLs from the registry.
        /// </summary>
        public void ClearDlls()
        {
            SealionTransaction();
            installed_dlls = new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns a dictionary of all modules installed, along with their
        /// versions.
        /// This includes DLLs, which will have a version type of `DllVersion`.
        /// This includes Provides, which will have a version of `ProvidesVersion`.
        /// </summary>
        public Dictionary<string, Version> Installed(bool withProvides = true)
        {
            var installed = new Dictionary<string, Version>();

            // Index our DLLs, as much as we dislike them.
            foreach (var dllinfo in installed_dlls)
            {
                installed[dllinfo.Key] = new DllVersion();
            }

            // Index our provides list, so users can see virtual packages
            if (withProvides)
            {
                foreach (var provided in Provided())
                {
                    installed[provided.Key] = provided.Value;
                }
            }

            // Index our installed modules (which may overwrite the installed DLLs and provides)
            foreach (var modinfo in installed_modules)
            {
                installed[modinfo.Key] = modinfo.Value.Module.version;
            }

            return installed;
        }

        /// <summary>
        /// Returns the InstalledModule, or null if it is not installed.
        /// Does *not* look up virtual modules.
        /// </summary>
        public InstalledModule InstalledModule(string module)
        {
            // In theory, someone could then modify the data they get back from
            // this, so we sea-lion just in case.

            SealionTransaction();

            InstalledModule installedModule;
            return installed_modules.TryGetValue(module, out installedModule) ? installedModule : null;
        }
            
        /// <summary>
        /// Returns a dictionary of provided (virtual) modules, and a
        /// ProvidesVersion indicating what provides them.
        /// </summary>

        // TODO: In the future it would be nice to cache this list, and mark it for rebuild
        // if our installed modules change.
        internal Dictionary<string, ProvidesVersion> Provided()
        {
            var installed = new Dictionary<string, ProvidesVersion>();

            foreach (var modinfo in installed_modules)
            {
                Module module = modinfo.Value.Module;

                // Skip if this module provides nothing.
                if (module.provides == null)
                {
                    continue;
                }

                foreach (string provided in module.provides)
                {
                    installed[provided] = new ProvidesVersion(module.identifier);
                }
            }

            return installed;
        }

        /// <summary>
        ///     Returns the installed version of a given mod.
        ///     If the mod was autodetected (but present), a version of type `DllVersion` is returned.
        ///     If the mod is provided by another mod (ie, virtual) a type of ProvidesVersion is returned.
        ///     If `withProvides` is to set to false, null will be returned instead of the ProvidesVersion type.
        ///     If the mod is not found, a null will be returned.
        /// </summary>
        public Version InstalledVersion(string modIdentifier, bool withProvides=true)
        {
            InstalledModule installedModule;

            // If it's genuinely installed, return the details we have.
            if (installed_modules.TryGetValue(modIdentifier, out installedModule))
            {
                return installedModule.Module.version;
            }

            // If it's in our autodetected registry, return that.
            if (installed_dlls.ContainsKey(modIdentifier))
            {
                return new DllVersion();
            }

            // Finally we have our provided checks. We'll skip these if
            // withProvides is false.
            if (!withProvides) return null;

            var provided = Provided();

            ProvidesVersion version;
            return provided.TryGetValue(modIdentifier, out version) ? version : null;
        }

        /// <summary>
        /// Gets the installed version of a mod. Returns null if provided or autodetected. 
        /// </summary>
        /// <param name="mod_identifer"></param>
        /// <returns></returns>
        public Module GetInstalledVersion(string mod_identifer)
        {
            InstalledModule installedModule;
            return installed_modules.TryGetValue(mod_identifer, out installedModule) ? installedModule.Module : null;
        }

        /// <summary>
        ///     Check if a mod is installed (either via CKAN, DLL, or virtually)
        ///     If withProvides is set to false then we skip the check for if the
        ///     mod has been provided (rather than existing as a real mod).
        /// </summary>
        /// <returns><c>true</c>, if installed<c>false</c> otherwise.</returns>
        public bool IsInstalled(string modName, bool withProvides = true)
        {
            return InstalledVersion(modName, withProvides) != null;
        }

        /// <summary>
        ///     Check if a mod is autodetected.
        /// </summary>
        /// <returns><c>true</c>, if autodetected<c>false</c> otherwise.</returns>
        public bool IsAutodetected(string identifier)
        {            
            return IsInstalled(identifier) && InstalledVersion(identifier).ToString().Equals("autodetected dll");
        }

        public bool HasUpdate(string identifier, KSPVersion version)
        {
            CkanModule newestVersion;
            try
            {
                newestVersion = LatestAvailable(identifier, version);
            }
            catch (ModuleNotFoundKraken)
            {
                return false;
            }
            if (newestVersion == null) return false;
            return IsInstalled(identifier) && newestVersion.version.IsGreaterThan(InstalledVersion(identifier));
        }

        /// <summary>
        /// Returns the module which owns this file, or null if not known.
        /// Throws a PathErrorKraken if an absolute path is provided.
        /// </summary>
        public string FileOwner(string file)
        {
            file = KSPPathUtils.NormalizePath(file);

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
        ///     Checks the sanity of the registry, to ensure that all dependencies are met,
        ///     and no mods conflict with each other. Throws an InconsistentKraken on failure.
        /// </summary>
        public void CheckSanity()
        {
            IEnumerable<Module> installed = from pair in installed_modules select pair.Value.Module;
            SanityChecker.EnforceConsistency(installed, installed_dlls.Keys);
        }

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// Acts recursively.
        /// </summary>
        public static HashSet<string> FindReverseDependencies(IEnumerable<string> modules_to_remove, IEnumerable<Module> orig_installed, IEnumerable<string> dlls)
        {
            while (true)
            {
                // Make our hypothetical install, and remove the listed modules from it.
                HashSet<Module> hypothetical = new HashSet<Module>(orig_installed); // Clone because we alter hypothetical.
                hypothetical.RemoveWhere(mod => modules_to_remove.Contains(mod.identifier));

                log.DebugFormat("Started with {0}, removing {1}, and keeping {2}; our dlls are {3}", string.Join(", ", orig_installed), string.Join(", ", modules_to_remove), string.Join(", ", hypothetical), string.Join(", ", dlls));

                // Find what would break with this configuration.
                // The Values.SelectMany() flattens our list of broken mods.
                var broken = new HashSet<string>(SanityChecker.FindUnmetDependencies(hypothetical, dlls)
                    .Values.SelectMany(x => x).Select(x => x.identifier));

                // If nothing else would break, it's just the list of modules we're removing.
                HashSet<string> to_remove = new HashSet<string>(modules_to_remove);
                
                if (to_remove.IsSupersetOf(broken))
                {
                    log.DebugFormat("{0} is a superset of {1}, work done", string.Join(", ", to_remove), string.Join(", ", broken));
                    return to_remove;
                }

                // Otherwise, remove our broken modules as well, and recurse.
                broken.UnionWith(to_remove);
                modules_to_remove = broken;
            }
        }

        public HashSet<string> FindReverseDependencies(IEnumerable<string> modules_to_remove)
        {
            var installed = new HashSet<Module>(installed_modules.Values.Select(x => x.Module));
            return FindReverseDependencies(modules_to_remove, installed, new HashSet<string>(installed_dlls.Keys));
        }

        /// <summary>
        /// Finds and returns all modules that could not exist without the given module installed
        /// </summary>
        public HashSet<string> FindReverseDependencies(string module)
        {
            var set = new HashSet<string> {module};
            return FindReverseDependencies(set);
        }
    }
}
