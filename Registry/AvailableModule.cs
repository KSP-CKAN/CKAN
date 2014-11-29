using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{
    /// <summary>
    ///     Utility class to track version -> module mappings
    /// </summary>
    public class AvailableModule
    {

        // TODO: It would be great for this have a field tracking which module we're
        // working with, so we don't allow mixed modules in our list.

        private static readonly ILog log = LogManager.GetLogger(typeof (AvailableModule));

        // The map of versions -> modules, that's what we're about!
        [JsonProperty]
        internal SortedDictionary<Version, CkanModule> module_version = new SortedDictionary<Version, CkanModule>();

        /// <summary>
        /// Record the given module version as being available.
        /// </summary>
        public void Add(CkanModule module)
        {
            log.DebugFormat("Adding {0}", module);
            module_version[module.version] = module;
        }

        /// <summary>
        /// Remove the given version from our list of available.
        /// </summary>
        public void Remove(Version version)
        {
            module_version.Remove(version);
        }

        /// <summary>
        ///     Return the most recent release of a module.
        ///     Optionally takes a KSP version number to target.
        ///     If no KSP version is supplied, we return the latest release of that module for any KSP.
        ///     Returns null if there are no compatible versions.
        /// </summary>
        public CkanModule Latest(KSPVersion ksp_version = null)
        {            
            var available_versions = new List<Version>(module_version.Keys);

            log.DebugFormat("Our dictionary has {0} keys", module_version.Keys.Count);
            log.DebugFormat("Choosing between {0} available versions", available_versions.Count);            
            // Uh oh, nothing available. Maybe this existed once, but not any longer.
            if (available_versions.Count == 0)
            {
                return null;
            }

            // Sort most recent versions first.            
            available_versions.Reverse();

            if (ksp_version == null)
            {
                CkanModule module = module_version[available_versions.First()];

                log.DebugFormat("No KSP version restriction, {0} is most recent", module);
                return module;
            }

            // Time to check if there's anything that we can satisfy.

            foreach (Version v in available_versions)
            {
                if (module_version[v].IsCompatibleKSP(ksp_version))
                {
                    return module_version[v];
                }
            }

            log.DebugFormat("No version of {0} is compatible with KSP {1}",
                module_version[available_versions[0]].identifier, ksp_version);

            // Oh noes! Nothing available!
            return null;
        }

        /// <summary>
        /// Returns the module with the specified version, or null if that does not exist.
        /// </summary>
        public CkanModule ByVersion(Version v)
        {            
            CkanModule module;
            module_version.TryGetValue(v, out module);
            return module;
        }
    }
}