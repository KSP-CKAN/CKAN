using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using System.Transactions;

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
        private const int LATEST_REGISTRY_VERSION = 0;
        private static readonly ILog log = LogManager.GetLogger(typeof (Registry));

        [JsonProperty] internal Dictionary<string, AvailableModule> available_modules;
        [JsonProperty] internal Dictionary<string, string> installed_dlls; // name => path
        [JsonProperty] internal Dictionary<string, InstalledModule> installed_modules;
        [JsonProperty] internal Dictionary<string, string> installed_files; // filename => module
        [JsonProperty] internal int registry_version;

        [JsonIgnore] internal string transaction_backup;

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Older registries didn't have the installed_files list, so we create one
            // if absent.

            if (installed_files == null)
            {
                log.Warn("Older registry format detected, upgrading...");

                installed_files = new Dictionary<string, string>();

                foreach (var module in installed_modules.Values)
                {
                    foreach (string file in module.installed_files.Keys)
                    {
                        // Register each file we know about as belonging to the given module.
                        installed_files[file] = module.source_module.identifier;
                    }
                }
            }
        }

        public Registry(
            int version,
            Dictionary<string, InstalledModule> installed_modules,
            Dictionary<string, string> installed_dlls,
            Dictionary<string, AvailableModule> available_modules,
            Dictionary<string, string> installed_files
            )
        {
            /* TODO: support more than just the latest version */
            if (version != LATEST_REGISTRY_VERSION)
            {
                throw new RegistryVersionNotSupportedKraken(version);
            }

            // Is there a better way of writing constructors than this? Srsly?
            this.installed_modules = installed_modules;
            this.installed_dlls = installed_dlls;
            this.available_modules = available_modules;
            this.installed_files = installed_files;
        }

        public static Registry Empty()
        {
            return new Registry(
                LATEST_REGISTRY_VERSION,
                new Dictionary<string, InstalledModule>(),
                new Dictionary<string, string>(),
                new Dictionary<string, AvailableModule>(),
                new Dictionary<string, string>()
                );
        }

        #region Transaction Handling

        // Keep track of our current transaction. Right now we only support a single
        // (ie, not nested) transaction.
        private string transaction = null;

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {

            if (transaction != null)
            {
                throw new TransactionalKraken("Registry cannot participated in nested transactions.");
            }

            log.Debug("Registry enlisting in transaction");

            // Hey, you know what's a great way to back-up your own object?
            // JSON. ;)
            this.transaction_backup = JsonConvert.SerializeObject(this, Formatting.None);

            // Record our transaction and give the thumbs up.
            transaction = Transaction.Current.TransactionInformation.LocalIdentifier;
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
            // Commit is essentially a no-op.
            transaction = null;
            enlistment.Done();
            log.Debug("Transaction registry was enlisted in has been committed");
            // TODO: Should we save at the end of a Tx?
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

            JsonConvert.PopulateObject(this.transaction_backup, this, options);

            transaction = null;
            enlistment.Done();
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
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
            }
        }

        #endregion

        public void ClearAvailable()
        {
            available_modules = new Dictionary<string, AvailableModule>();
        }

        public void AddAvailable(CkanModule module)
        {
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
        /// Does *nothing* if the module is not present to begin with.
        /// </summary>
        public void RemoveAvailable(string identifier, Version version)
        {
            if (available_modules.ContainsKey(identifier))
            {
                available_modules[identifier].Remove(version);
            }
        }

        public void RemoveAvailable(Module module)
        {
            RemoveAvailable(module.identifier, module.version);
        }

        /// <summary>
        ///     Returns a simple array of all available modules for
        ///     the specified version of KSP (installed version by default)
        /// </summary>
        public List<CkanModule> Available(KSPVersion ksp_version = null)
        {
            // Default to the user's current KSP install for version.
            if (ksp_version == null)
            {
                ksp_version = KSPManager.CurrentInstance.Version();
            }

            var candidates = new List<string>(available_modules.Keys);
            var compatible = new List<CkanModule>();

            // It's nice to see things in alphabetical order, so sort our keys first.
            candidates.Sort();

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
                                if (LatestAvailableWithProvides(dependency.name, ksp_version).Count == 0)
                                {
                                    failedDepedency = true;
                                    break;
                                }
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
        public List<CkanModule> Incompatible(KSPVersion ksp_version = null)
        {
            // Default to the user's current KSP install for version.
            if (ksp_version == null)
            {
                ksp_version = KSPManager.CurrentInstance.Version();
            }

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
        public CkanModule LatestAvailable(string module, KSPVersion ksp_version = null)
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
        /// </summary>
        public List<CkanModule> LatestAvailableWithProvides(string module, KSPVersion ksp_version = null)
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
            foreach (var pair in available_modules)
            {
                // Skip this module if not available for our system.
                if (pair.Value.Latest(ksp_version) == null)
                {
                    continue;
                }

                List<string> provides = pair.Value.Latest(ksp_version).provides;
                if (provides != null)
                {
                    foreach (string provided in provides)
                    {
                        if (provided == module)
                        {
                            modules.Add(pair.Value.Latest(ksp_version));
                        }
                    }
                }
            }

            return modules;
        }

        /// <summary>
        ///     Register the supplied module as having been installed, thereby keeping
        ///     track of its metadata and files.
        /// </summary>

        // TODO: It might be better to provide split functionality, one method
        // to register a mod (which we do at the start of an install, and which
        // can check consistency), and another to register each file with that mod
        // (which can check file consistency at the same time).
         
        public void RegisterModule(InstalledModule mod)
        {
            // But we also want to keep track of all its files.
            // We start by checking to see if any files are owned by another mod,
            // if so, we abort with a list of errors.

            var inconsistencies = new List<string>();

            foreach (string filename in mod.installed_files.Keys)
            {
                if (this.installed_files.ContainsKey(filename))
                {
                    // For now, it's cool if a module wants to register a directory.
                    if (Directory.Exists(filename))
                    {
                        continue;
                    }

                    string owner = this.installed_files[filename];
                    inconsistencies.Add(
                        string.Format("{0} wishes to install {1}, but this file is registered to {2}",
                                      mod.source_module.identifier, filename, owner
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
            foreach (string filename in mod.installed_files.Keys)
            {
                this.installed_files[filename] = mod.source_module.identifier;
            }

            // Finally, register our module proper.
            installed_modules.Add(mod.source_module.identifier, mod);
        }

        /// <summary>
        /// Register the supplied module as having been uninstalled, thereby
        /// forgetting abouts its metadata and files.
        /// </summary>
        public void DeregisterModule(string module)
        {
            var inconsistencies = new List<string>();

            foreach (string filename in this.installed_modules[module].installed_files.Keys)
            {
                if (File.Exists(filename))
                {
                    inconsistencies.Add(string.Format(
                        "{0} is registered to {1} but has not been removed!",
                        filename, module
                    ));
                }

                // We should probably be marking removal of an unregistred file
                // as inconsistent, but older registries didn't have a file list,
                // so we're cool for now.
                this.installed_files.Remove(filename);
            }

            // De-register our module, even if there were inconsistencies.
            // If nothing catches our exception, our transaction layer will roll us back.
            this.installed_modules.Remove(module);

            if (inconsistencies.Count > 0)
            {
                // Uh oh, what mess have we got ourselves into now, Inconsistency Kraken?
                throw new InconsistentKraken(inconsistencies);
            }
        }

        /// <summary>
        /// Registers the given DLL as having been installed. This provides some support
        /// for pre-CKAN modules.
        /// 
        /// Does nothing if the DLL is already part of an installed module.
        /// </summary>
        public void RegisterDll(string path)
        {
            // TODO: This is awful, as it's O(N^2), but it means we never index things which are
            // part of another mod.

            foreach (var mod in installed_modules.Values)
            {
                if (mod.installed_files.ContainsKey(path))
                {
                    log.DebugFormat("Not registering {0}, it's part of {1}", path, mod.source_module);
                    return;
                }
            }

            // Oh my, does .NET support extended regexps (like Perl?), we could use them right now.
            Match match = Regex.Match(path, @".*?(?:^|/)GameData/((?:.*/|)([^.]+).*dll)");

            string relPath = match.Groups[1].Value;
            string modName = match.Groups[2].Value;

            if (modName.Length == 0 || relPath.Length == 0)
            {
                log.WarnFormat("Attempted to index {0} which is not a DLL", path);
                return;
            }

            log.InfoFormat("Registering {0} -> {1}", modName, path);

            // We're fine if we overwrite an existing key.
            installed_dlls[modName] = relPath;
        }

        /// <summary>
        /// Clears knowledge of all DLLs from the registry.
        /// </summary>
        public void ClearDlls()
        {
            installed_dlls = new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns a dictionary of all modules installed, along with their
        /// versions.
        /// This includes DLLs, which will have a version type of `DllVersion`.
        /// This includes Provides, which will have a version of `ProvidesVersion`.
        /// </summary>
        public Dictionary<string, Version> Installed()
        {
            var installed = new Dictionary<string, Version>();

            // Index our DLLs, as much as we dislike them.
            foreach (var dllinfo in installed_dlls)
            {
                installed[dllinfo.Key] = new DllVersion();
            }

            // Index our provides list, so users can see virtual packages
            foreach (var provided in Provided())
            {
                installed[provided.Key] = provided.Value;
            }

            // Index our installed modules (which may overwrite the installed DLLs and provides)
            foreach (var modinfo in installed_modules)
            {
                installed[modinfo.Key] = modinfo.Value.source_module.version;
            }

            return installed;
        }

        /// <summary>
        /// Returns the InstalledModule, or null if it is not installed.
        /// Does *not* look up virtual modules.
        /// </summary>
        public InstalledModule InstalledModule(string module)
        {
            if (this.installed_modules.ContainsKey(module))
            {
                return this.installed_modules[module];
            }

            return null;
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
                Module module = modinfo.Value.source_module;

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
        ///     If the mod is not found, a null will be returned.
        /// </summary>
        public Version InstalledVersion(string modName)
        {
            if (installed_modules.ContainsKey(modName))
            {
                return installed_modules[modName].source_module.version;
            }
            else if (installed_dlls.ContainsKey(modName))
            {
                return new DllVersion();
            }

            var provided = Provided();

            if (provided.ContainsKey(modName))
            {
                return provided[modName];
            }

            return null;
        }

        /// <summary>
        ///     Check if a mod is installed (either via CKAN, DLL, or virtually)
        /// </summary>
        /// <returns><c>true</c>, if installed<c>false</c> otherwise.</returns>
        public bool IsInstalled(string modName)
        {
            if (InstalledVersion(modName) == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the module which owns this file, or null if not known.
        /// </summary>
        public string FileOwner(string file)
        {
            if (this.installed_files.ContainsKey(file))
            {
                return this.installed_files[file];
            }
            return null;
        }

        /// <summary>
        ///     Checks the sanity of the registry, to ensure that all dependencies are met,
        ///     and no mods conflict with each other. Throws an InconsistentKraken on failure.
        /// </summary>
        public void CheckSanity()
        {
            IEnumerable<Module> installed = from pair in installed_modules select pair.Value.source_module;
            SanityChecker.EnforceConsistency(installed, installed_dlls.Keys);
        }

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// Acts recursively.
        /// </summary>

        public static HashSet<string> FindReverseDependencies(IEnumerable<string> modules_to_remove, IEnumerable<Module> orig_installed, IEnumerable<string> dlls)
        {
            // Make our hypothetical install, and remove the listed modules from it.
            HashSet<Module> hypothetical = new HashSet<Module> (orig_installed); // Clone because we alter hypothetical.
            hypothetical.RemoveWhere(mod => modules_to_remove.Contains(mod.identifier));

            log.DebugFormat( "Started with {0}, removing {1}, and keeping {2}; our dlls are {3}",
                              string.Join(", ", orig_installed),
                              string.Join(", ", modules_to_remove),
                              string.Join(", ", hypothetical),
                              string.Join(", ", dlls)
                              );

            // Find what would break with this configuration.
            // The Values.SelectMany() flattens our list of broken mods.
            var broken = new HashSet<string> (
                SanityChecker
                .FindUnmetDependencies(hypothetical, dlls)
                .Values
                .SelectMany(x => x)
                .Select(x => x.identifier)
                );

            // If nothing else would break, it's just the list of modules we're removing.
            HashSet<string> to_remove = new HashSet<string>(modules_to_remove);
            if (to_remove.IsSupersetOf(broken))
            {
                log.DebugFormat("{0} is a superset of {1}, work done", string.Join(", ", to_remove), string.Join(", ", broken));
                return to_remove;
            }

            // Otherwise, remove our broken modules as well, and recurse.
            broken.UnionWith(to_remove);
            return FindReverseDependencies(broken, orig_installed, dlls);
        }

        public HashSet<string> FindReverseDependencies(IEnumerable<string> modules_to_remove)
        {
            var installed = new HashSet<Module>(installed_modules.Values.Select(x => x.source_module));
            return FindReverseDependencies(modules_to_remove, installed, new HashSet<string>(installed_dlls.Keys));
        }

        /// <summary>
        /// Finds and returns all modules that could not exist without the given module installed
        /// </summary>
        public HashSet<string> FindReverseDependencies(string module)
        {
            var set = new HashSet<string>();
            set.Add(module);
            return FindReverseDependencies(set);
        }

    }
}