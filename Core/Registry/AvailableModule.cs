using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using CKAN.Extensions;
using CKAN.Versioning;

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
        internal void DeserialisationFixes(StreamingContext context)
        {
            identifier = module_version.Values.LastOrDefault()?.identifier;
            Debug.Assert(module_version.Values.All(m => identifier.Equals(m.identifier)));
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
        [JsonConverter(typeof(JsonLeakySortedDictionaryConverter<ModuleVersion, CkanModule>))]
        internal SortedDictionary<ModuleVersion, CkanModule> module_version =
            new SortedDictionary<ModuleVersion, CkanModule>();

        [OnError]
        private void OnError(StreamingContext context, ErrorContext errorContext)
        {
            log.WarnFormat("Discarding CkanModule, failed to parse {0}: {1}",
                errorContext.Path, errorContext.Error.GetBaseException().Message);
            errorContext.Handled = true;
        }

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
            GameVersionCriteria     ksp_version  = null,
            RelationshipDescriptor  relationship = null,
            IEnumerable<CkanModule> installed    = null,
            IEnumerable<CkanModule> toInstall    = null
        )
        {
            IEnumerable<CkanModule> modules = module_version.Values.Reverse();
            if (relationship != null)
            {
                modules = modules.Where(relationship.WithinBounds);
            }
            if (ksp_version != null)
            {
                modules = modules.Where(m => m.IsCompatible(ksp_version));
            }
            if (installed != null)
            {
                modules = modules.Where(m => DependsAndConflictsOK(m, installed));
            }
            if (toInstall != null)
            {
                modules = modules.Where(m => DependsAndConflictsOK(m, toInstall));
            }
            return modules.FirstOrDefault();
        }

        private static bool DependsAndConflictsOK(CkanModule module, IEnumerable<CkanModule> others)
        {
            others = others.Memoize();
            if (module.depends != null)
            {
                foreach (RelationshipDescriptor rel in module.depends)
                {
                    // If 'others' matches an identifier, it must also match the versions, else fail
                    if (rel.ContainsAny(others.Select(m => m.identifier)) && !rel.MatchesAny(others, null, null))
                    {
                        log.DebugFormat("Unsatisfied dependency {0}, rejecting", rel);
                        return false;
                    }
                }
            }
            var othersMinusSelf = others.Where(m => m.identifier != module.identifier).Memoize();
            if (module.conflicts != null)
            {
                // Skip self-conflicts (but catch other modules providing self)
                foreach (RelationshipDescriptor rel in module.conflicts)
                {
                    // If any of the conflicts are present, fail
                    if (rel.MatchesAny(othersMinusSelf, null, null, out CkanModule matched))
                    {
                        log.DebugFormat("Found conflict with {0}, rejecting", matched);
                        return false;
                    }
                }
            }
            // Check reverse conflicts so user isn't prompted to choose modules that will error out immediately
            var selfArray = new CkanModule[] { module };
            foreach (CkanModule other in othersMinusSelf)
            {
                if (other.conflicts != null)
                {
                    foreach (RelationshipDescriptor rel in other.conflicts)
                    {
                        if (rel.MatchesAny(selfArray, null, null))
                        {
                            log.DebugFormat("Found reverse conflict with {0}, rejecting", other);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the latest game version that is compatible with this mod.
        /// Checks all versions of the mod.
        /// </summary>
        public GameVersion LatestCompatibleKSP()
        {
            GameVersion best = null;
            foreach (var pair in module_version)
            {
                GameVersion v = pair.Value.LatestCompatibleKSP();
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

        public IEnumerable<CkanModule> AllAvailable()
        {
            // Some code may expect this to be sorted in descending order
            return module_version.Values.Reverse();
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
