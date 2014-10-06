namespace CKAN {

    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Linq;
    using log4net;

    class RegistryVersionNotSupportedException : Exception {
        public int requested_version;

        public RegistryVersionNotSupportedException (int v) {
            requested_version = v;
        }
    }

    public class Registry {
        const int LATEST_REGISTRY_VERSION = 0;

        // TODO: Perhaps flip these from public to protected somehow, and
        // declare allegiance to the JSON class that serialises them.
        // Is that something you can do in C#? In Moose we'd use a role.

        public int registry_version;
        public Dictionary<string, InstalledModule> installed_modules;
        public Dictionary<string, AvailableModule> available_modules;
        public Dictionary<string, string> installed_dlls;

        private static readonly ILog log = LogManager.GetLogger(typeof(Registry));

        public Registry (
            int version, 
            Dictionary<string, InstalledModule> mods, 
            Dictionary<string, string> dlls,
            Dictionary<string, AvailableModule> available
        ) {
            /* TODO: support more than just the latest version */
            if (version != LATEST_REGISTRY_VERSION) {
                throw new RegistryVersionNotSupportedException (version);
            }

            installed_modules = mods;
            installed_dlls    = dlls;
            available_modules = available;
        }

        public static Registry Empty () {
            return new Registry (
                LATEST_REGISTRY_VERSION,
                new Dictionary<string, InstalledModule> (),
                new Dictionary<string,string> (),
                new Dictionary<string, AvailableModule> ()
            );
        }

        public void ClearAvailable() {
            available_modules = new Dictionary<string, AvailableModule> ();
        }

        public void AddAvailable(CkanModule module) {

            // If we've never seen this module before, create an entry for it.

            if (! available_modules.ContainsKey (module.identifier)) {
                log.DebugFormat ("Adding new available module {0}", module.identifier);
                available_modules [module.identifier] = new AvailableModule ();
            }

            // Now register the actual version that we have.
            // (It's okay to have multiple versions of the same mod.)

            log.DebugFormat ("Available: {0} version {1}", module.identifier, module.version);
            available_modules [module.identifier].Add (module);

        } 

        /// <summary>
        /// Returns a simple array of all available modules.
        /// </summary>

        public string[] Available() {
            return available_modules.Keys.ToArray();
        }

        /// <summary>
        /// Register the supplied module as having been installed, thereby keeping
        /// track of its metadata and files.
        /// </summary>
        /// <param name="mod">Mod.</param>

        public void RegisterModule (InstalledModule mod) {
            installed_modules.Add (mod.source_module.identifier, mod);
        }

        public void DeregisterModule(string module) {
            installed_modules.Remove (module);
        }

        public void RegisterDll (string path) {
            // Oh my, does .NET support extended regexps (like Perl?), we could use them right now.
            Match match = Regex.Match (path, @".*?(?:^|/)GameData/((?:.*/|)([^.]+).*dll)");

            string relPath = match.Groups[1].Value;
            string modName = match.Groups[2].Value;

            Console.WriteLine ("Registering {0} -> {1}", modName, relPath);

            // We're fine if we overwrite an existing key.
            installed_dlls[modName] = relPath;
        }

        public void ClearDlls() {
            installed_dlls = new Dictionary<string,string> ();
        }

        /// <summary>
        /// Returns the installed version of a given mod.
        /// If the mod was autodetected (but present), a "0" is returned.
        /// If the mod is not found, a null will be returned.
        /// </summary>
        /// <returns>The version.</returns>
        /// <param name="modName">Mod name.</param>

        public string InstalledVersion(string modName) {
            if (installed_modules.ContainsKey(modName)) {
                return installed_modules [modName].source_module.version;
            }
            else if (installed_dlls.ContainsKey(modName)) {
                return "0";    // We probably want a better way to signal auto-detected modules.
            }

            return null;
        }

        /// <summary>
        /// Check if a mod is installed (either via CKAN, or a DLL detected)
        /// </summary>
        /// <returns><c>true</c>, if installed<c>false</c> otherwise.</returns>

        public bool IsInstalled(string modName) {
            if (InstalledVersion (modName) == null) {
                return false;
            }
            return true;
        }
    }
}