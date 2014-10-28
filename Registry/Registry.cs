using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{
    /// <summary>
    /// This is the CKAN registry. All the modules that we know about or have installed
    /// are contained in here.
    /// 
    /// Please try to avoid accessing the attributes directly. Right now they're public
    /// so our JSON layer can access them, but in the future they will become private.
    /// </summary>
    public class Registry
    {
        private const int LATEST_REGISTRY_VERSION = 0;
        private static readonly ILog log = LogManager.GetLogger(typeof (Registry));

        // TODO: Perhaps flip these from public to protected somehow, and
        // declare allegiance to the JSON class that serialises them.
        // Is that something you can do in C#? In Moose we'd use a role.

        public Dictionary<string, AvailableModule> available_modules;
        public Dictionary<string, string> installed_dlls;
        public Dictionary<string, InstalledModule> installed_modules;
        public int registry_version;

        public Registry(
            int version,
            Dictionary<string, InstalledModule> mods,
            Dictionary<string, string> dlls,
            Dictionary<string, AvailableModule> available
            )
        {
            /* TODO: support more than just the latest version */
            if (version != LATEST_REGISTRY_VERSION)
            {
                throw new RegistryVersionNotSupportedKraken(version);
            }

            installed_modules = mods;
            installed_dlls = dlls;
            available_modules = available;
        }

        public static Registry Empty()
        {
            return new Registry(
                LATEST_REGISTRY_VERSION,
                new Dictionary<string, InstalledModule>(),
                new Dictionary<string, string>(),
                new Dictionary<string, AvailableModule>()
                );
        }

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

                string[] provides = pair.Value.Latest(ksp_version).provides;
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
        /// <param name="mod">Mod.</param>
        public void RegisterModule(InstalledModule mod)
        {
            installed_modules.Add(mod.source_module.identifier, mod);
        }

        /// <summary>
        /// Register the supplied module as having been uninstalled, thereby
        /// forgetting abouts its metadata and files.
        /// </summary>
        public void DeregisterModule(string module)
        {
            installed_modules.Remove(module);
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
    }
}