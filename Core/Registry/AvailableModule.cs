using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using CKAN.Versioning;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{
    /// <summary>
    /// Utility class to track version -> module mappings
    /// </summary>
    /// <remarks>
    /// Json must not contain AvailableModules which are empty
    /// </remarks>
    public class AvailableModule
    {
        [JsonIgnore]
        private string identifier;

        [JsonConstructor]
        private AvailableModule()
        {
        }

        [OnDeserialized]
        internal void SetIdentifier(StreamingContext context)
        {
            var mod = module_version.Values.LastOrDefault();
            identifier = mod.identifier;
            Debug.Assert(module_version.Values.All(m=>identifier.Equals(m.identifier)));
        }

        /// <param name="identifier">The module to keep track of</param>
        public AvailableModule(string identifier)
        {
            this.identifier = identifier;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof (AvailableModule));

        // The map of versions -> modules, that's what we're about!
        // First element is the oldest version, last is the newest.
        [JsonProperty]
        internal SortedDictionary<Version, CkanModule> module_version =
            new SortedDictionary<Version, CkanModule>();

        /// <summary>
        /// Record the given module version as being available.
        /// </summary>
        public void Add(CkanModule module)
        {
            if (!module.identifier.Equals(identifier))
                throw new ArgumentException(
                    string.Format("This AvailableModule is for tracking {0} not {1}", identifier, module.identifier));

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
        /// Return the most recent release of a module with a optional ksp version to target and a RelationshipDescriptor to satisfy.
        /// </summary>
        /// <param name="ksp_version">If not null only consider mods which match this ksp version.</param>
        /// <param name="relationship">If not null only consider mods which satisfy the RelationshipDescriptor.</param>
        /// <returns></returns>
        public CkanModule Latest(KspVersionCriteria ksp_version = null, RelationshipDescriptor relationship=null)
        {
            var available_versions = new List<Version>(module_version.Keys);
            CkanModule module;
            log.DebugFormat("Our dictionary has {0} keys", module_version.Keys.Count);
            log.DebugFormat("Choosing between {0} available versions", available_versions.Count);

            // Uh oh, nothing available. Maybe this existed once, but not any longer.
            if (available_versions.Count == 0)
            {
                return null;
            }

            // No restrictions? Great, we can just pick the latest one!
            if (ksp_version == null && relationship == null)
            {
                module = module_version[available_versions.Last()];

                log.DebugFormat("No KSP version restriction, {0} is most recent", module);
                return module;
            }

            // If there's no relationship to satisfy, we can just pick the latest that is
            // compatible with our version of KSP.
            if (relationship == null)
            {
                // Time to check if there's anything that we can satisfy.
                var version =
                    available_versions.LastOrDefault(v => module_version[v].IsCompatibleKSP(ksp_version));
                if (version != null)
                    return module_version[version];

                log.DebugFormat("No version of {0} is compatible with KSP {1}",
                    module_version[available_versions[0]].identifier, ksp_version.ToString());

                return null;
            }

            // If we're here, then we have a relationship to satisfy, so things get more complex.
            if (ksp_version == null)
            {
                var version = available_versions.LastOrDefault(relationship.version_within_bounds);
                return version == null ? null : module_version[version];
            }
            else
            {
                var version = available_versions.LastOrDefault(v =>
                    relationship.version_within_bounds(v) &&
                    module_version[v].IsCompatibleKSP(ksp_version));
                return version == null ? null : module_version[version];
            }
        }

        /// <summary>
        /// Returns the latest game version that is compatible with this mod.
        /// Checks all versions of the mod.
        /// </summary>
        public KspVersion LatestCompatibleKSP()
        {
            KspVersion best = null;
            foreach (var pair in module_version)
            {
                KspVersion v = pair.Value.LatestCompatibleKSP();
                if (v.IsAny)
                    // Can't get later than Any, so stop
                    return v;
                else if (best == null || best < v)
                    best = v;
            }
            return best;
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

        public List<CkanModule> AllAvailable()
        {
            // Some code may expect this to be sorted in descending order
            return new List<CkanModule>(module_version.Values.Reverse());
        }

        /// <summary>
        /// Return the entire section of registry.json for this mod
        /// </summary>
        /// <returns>
        /// Nicely formatted JSON string containing metadata for all of this mod's available versions
        /// </returns>
        public string FullMetadata()
        {
            StringWriter sw = new StringWriter(new StringBuilder());
            using (JsonTextWriter writer = new JsonTextWriter(sw) {
                    Formatting  = Formatting.Indented,
                    Indentation = 4,
                    IndentChar  = ' '
                })
            {
                new JsonSerializer().Serialize(writer, this);
            }
            return sw.ToString();
        }

    }

}
