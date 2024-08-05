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

using CKAN.Versioning;
using CKAN.Extensions;

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

        /// <param name="identifier">The module to keep track of</param>
        public AvailableModule(string identifier)
        {
            this.identifier = identifier;
        }

        public AvailableModule(string identifier, IEnumerable<CkanModule> modules)
            : this(identifier)
        {
            foreach (var module in modules)
            {
                Add(module);
            }
        }

        [OnDeserialized]
        internal void DeserialisationFixes(StreamingContext context)
        {
            identifier = module_version.Values.LastOrDefault()?.identifier;
            Debug.Assert(module_version.Values.All(m => identifier.Equals(m.identifier)));
        }

        /// <summary>
        /// Generate a new AvailableModule given its CkanModules
        /// </summary>
        /// <param name="availMods">Sequence of mods to be contained, expected to be IGrouping&lt;&gt;, so it should support O(1) Count(), even though IEnumerable&lt;&gt; in general does not</param>
        /// <returns></returns>
        public static AvailableModule Merge(IEnumerable<AvailableModule> availMods)
            => availMods.Count() == 1 ? availMods.First()
                                      : new AvailableModule(availMods.First().identifier,
                                                            availMods.Reverse().SelectMany(am => am.AllAvailable()));

        // The map of versions -> modules, that's what we're about!
        // First element is the oldest version, last is the newest.
        [JsonProperty]
        [JsonConverter(typeof(JsonLeakySortedDictionaryConverter<ModuleVersion, CkanModule>))]
        internal SortedDictionary<ModuleVersion, CkanModule> module_version =
            new SortedDictionary<ModuleVersion, CkanModule>();

        [OnError]
        #pragma warning disable IDE0051, IDE0060
        private void OnError(StreamingContext context, ErrorContext errorContext)
        #pragma warning restore IDE0051, IDE0060
        {
            log.WarnFormat("Discarding CkanModule, failed to parse {0}: {1}",
                errorContext.Path, errorContext.Error.GetBaseException().Message);
            errorContext.Handled = true;
        }

        /// <summary>
        /// Record the given module version as being available.
        /// </summary>
        private void Add(CkanModule module)
        {
            if (!module.identifier.Equals(identifier))
            {
                throw new ArgumentException(
                    string.Format("This AvailableModule is for tracking {0} not {1}", identifier, module.identifier));
            }

            log.DebugFormat("Adding to available module: {0}", module);
            module_version[module.version] = module;
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
            ICollection<CkanModule> installed    = null,
            ICollection<CkanModule> toInstall    = null)
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

        private static bool DependsAndConflictsOK(CkanModule module, ICollection<CkanModule> others)
        {
            if (module.depends != null)
            {
                foreach (RelationshipDescriptor rel in module.depends)
                {
                    // If 'others' matches an identifier, it must also match the versions, else fail
                    if (rel.ContainsAny(others.Select(m => m.identifier)) && !rel.MatchesAny(others, null, null))
                    {
                        log.DebugFormat("Unsatisfied dependency {0}, rejecting {1}", rel, module);
                        return false;
                    }
                }
            }
            var othersMinusSelf = others.Where(m => m.identifier != module.identifier).ToList();
            if (module.conflicts != null)
            {
                // Skip self-conflicts (but catch other modules providing self)
                foreach (RelationshipDescriptor rel in module.conflicts)
                {
                    // If any of the conflicts are present, fail
                    if (rel.MatchesAny(othersMinusSelf, null, null, out CkanModule matched))
                    {
                        log.DebugFormat("Found conflict with {0}, rejecting {1}", matched, module);
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
                            log.DebugFormat("Found reverse conflict with {0}, rejecting {1}", other, module);
                            return false;
                        }
                    }
                }
                // And check reverse depends for version limits
                if (other.depends != null)
                {
                    foreach (RelationshipDescriptor rel in other.depends)
                    {
                        // If 'others' matches an identifier, it must also match the versions, else fail
                        if (rel.ContainsAny(Enumerable.Repeat(module.identifier, 1))
                            && !rel.MatchesAny(selfArray, null, null))
                        {
                            log.DebugFormat("Unmatched reverse dependency from {0}, rejecting", other);
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
        public GameVersion LatestCompatibleGameVersion(List<GameVersion> realVersions)
        {
            var ranges = module_version.Values
                                       .Select(m => new GameVersionRange(m.EarliestCompatibleGameVersion(),
                                                                         m.LatestCompatibleGameVersion()))
                                       .Memoize();
            if (ranges.Any(r => r.Upper.Value.IsAny))
            {
                // Can't get later than Any, so no need for more complex logic
                return realVersions?.LastOrDefault()
                                   ?? module_version.Values.Select(m => m.LatestCompatibleGameVersion())
                                                           .Max();
            }
            // Find the range with the highest upper bound
            var bestRange = ranges.Distinct()
                                  .Aggregate((best, r) => r.Upper == GameVersionBound.Highest(best.Upper, r.Upper)
                                                              ? r
                                                              : best);
            return realVersions?.LastOrDefault(v => bestRange.Contains(v))
                               ?? module_version.Values.Select(m => m.LatestCompatibleGameVersion())
                                                       .Max();
        }

        /// <summary>
        /// Returns the module with the specified version, or null if that does not exist.
        /// </summary>
        public CkanModule ByVersion(ModuleVersion v)
            => module_version.TryGetValue(v, out CkanModule module) ? module : null;

        /// <summary>
        /// Some code may expect this to be sorted in descending order
        /// </summary>
        public IEnumerable<CkanModule> AllAvailable()
            => module_version.Values.Reverse();

        /// <summary>
        /// Return the entire section of registry.json for this mod
        /// </summary>
        /// <returns>
        /// Nicely formatted JSON string containing metadata for all of this mod's available versions
        /// </returns>
        public string FullMetadata()
        {
            StringWriter sw = new StringWriter(new StringBuilder());
            using (JsonTextWriter writer = new JsonTextWriter(sw)
            {
                Formatting  = Formatting.Indented,
                Indentation = 4,
                IndentChar  = ' '
            })
            {
                new JsonSerializer().Serialize(writer, this);
            }
            return sw.ToString();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(AvailableModule));
    }
}
