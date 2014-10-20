using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{

    internal class RegistryVersionNotSupportedException : Exception
    {
        public int requested_version;

        public RegistryVersionNotSupportedException(int v)
        {
            requested_version = v;
        }
    }

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
                throw new RegistryVersionNotSupportedException(version);
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
        ///     Returns a simple array of all available modules for
        ///     the specified version of KSP (installed version by default)
        /// </summary>
        public List<CkanModule> Available(KSPVersion ksp_version = null)
        {
            // Default to the user's current KSP install for version.
            if (ksp_version == null)
            {
                ksp_version = KSP.Version();
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
                            catch (ModuleNotFoundException)
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
                ksp_version = KSP.Version();
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
                throw new ModuleNotFoundException(module);
            }
        }

        /// <summary>
        ///     Returns the latest available version of a module that
        ///     satisifes the specified version. Takes into account module 'provides'
        ///     Throws a ModuleNotFoundException if asked for a non-existant module.
        ///     Returns null if there's simply no compatible version for this system.
        /// </summary>
        public List<CkanModule> LatestAvailableWithProvides(string module, KSPVersion ksp_version = null)
        {
            log.DebugFormat("Finding latest available for {0}", module);

            // TODO: Check user's stability tolerance (stable, unstable, testing, etc)

            var modules = new List<CkanModule>();

            try
            {
                CkanModule mod = LatestAvailable(module, ksp_version);
                if (mod != null)
                {
                    modules.Add(mod);
                }
            }
            catch (ModuleNotFoundException)
            {
                foreach (var pair in available_modules)
                {
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

        public void DeregisterModule(string module)
        {
            installed_modules.Remove(module);
        }

        public void RegisterDll(string path)
        {
            // Oh my, does .NET support extended regexps (like Perl?), we could use them right now.
            Match match = Regex.Match(path, @".*?(?:^|/)GameData/((?:.*/|)([^.]+).*dll)");

            string relPath = match.Groups[1].Value;
            string modName = match.Groups[2].Value;

            if (modName.Length == 0 || relPath.Length == 0)
            {
                return;
            }

            User.WriteLine("Registering {0} -> {1}", modName, relPath);

            // We're fine if we overwrite an existing key.
            installed_dlls[modName] = relPath;
        }

        public void ClearDlls()
        {
            installed_dlls = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Returns the installed version of a given mod.
        ///     If the mod was autodetected (but present), a "0" is returned.
        ///     If the mod is not found, a null will be returned.
        /// </summary>
        /// <returns>The version.</returns>
        /// <param name="modName">Mod name.</param>
        public Version InstalledVersion(string modName)
        {
            if (installed_modules.ContainsKey(modName))
            {
                return installed_modules[modName].source_module.version;
            }
            if (installed_dlls.ContainsKey(modName))
            {
                return new DllVersion();
            }

            return null;
        }

        /// <summary>
        ///     Check if a mod is installed (either via CKAN, or a DLL detected)
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