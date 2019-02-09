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
        internal SortedDictionary<ModuleVersion, CkanModule> module_version =
            new SortedDictionary<ModuleVersion, CkanModule>();

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
        public void Remove(ModuleVersion version)
        {
            module_version.Remove(version);
        }

        /// <summary>
        /// Return the most recent release of a module with a optional ksp version to target and a RelationshipDescriptor to satisfy.
        /// </summary>
        /// <param name="ksp_version">If not null only consider mods which match this ksp version.</param>
        /// <param name="relationship">If not null only consider mods which satisfy the RelationshipDescriptor.</param>
        /// <param name="installed">Modules that are already installed</param>
        /// <param name="toInstall">Modules that are planned to be installed</param>
        /// <returns></returns>
        public CkanModule Latest(
            KspVersionCriteria      ksp_version  = null,
            RelationshipDescriptor  relationship = null,
            IEnumerable<CkanModule> installed    = null,
            IEnumerable<CkanModule> toInstall    = null
        )
        {
            log.DebugFormat("Our dictionary has {0} keys", module_version.Keys.Count);
            IEnumerable<CkanModule> modules = module_version.Values;
            if (relationship != null)
            {
                modules = modules.Where(relationship.WithinBounds);
            }
            if (ksp_version != null)
            {
                modules = modules.Where(m => m.IsCompatibleKSP(ksp_version));
            }
            if (installed != null)
            {
                modules = modules.Where(m => DependsAndConflictsOK(m, installed));
            }
            if (toInstall != null)
            {
                modules = modules.Where(m => DependsAndConflictsOK(m, toInstall));
            }
            return modules.LastOrDefault();
        }

        private static bool DependsAndConflictsOK(CkanModule module, IEnumerable<CkanModule> others)
        {
            if (module.depends != null)
            {
                foreach (RelationshipDescriptor rel in module.depends)
                {
                    // If 'others' matches an identifier, it must also match the versions, else fail
                    if (rel.ContainsAny(others.Select(m => m.identifier)) && !rel.MatchesAny(others, null, null))
                    {
                        return false;
                    }
                }
            }
            if (module.conflicts != null)
            {
                foreach (RelationshipDescriptor rel in module.conflicts)
                {
                    // If any of the conflicts are present, fail
                    if (rel.MatchesAny(others, null, null))
                    {
                        return false;
                    }
                }
            }
            return true;
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
        public CkanModule ByVersion(ModuleVersion v)
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
