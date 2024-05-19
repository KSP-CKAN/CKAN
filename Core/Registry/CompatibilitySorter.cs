using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using log4net;

using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Class to track which mods are compatible with a given set of versions.
    /// Handles all levels of dependencies.
    /// </summary>
    public class CompatibilitySorter
    {
        /// <summary>
        /// Initialize the sorter and partition the mods.
        /// </summary>
        /// <param name="crit">Versions to be considered compatible</param>
        /// <param name="available">Collection of mods from registry</param>
        /// <param name="providers">Dictionary mapping every identifier to the modules providing it</param>
        /// <param name="dlls">Collection of found dlls</param>
        /// <param name="dlc">Collection of installed DLCs</param>
        public CompatibilitySorter(GameVersionCriteria                              crit,
                                   IEnumerable<Dictionary<string, AvailableModule>> available,
                                   Dictionary<string, HashSet<AvailableModule>>     providers,
                                   Dictionary<string, InstalledModule>              installed,
                                   HashSet<string>                                  dlls,
                                   IDictionary<string, ModuleVersion>               dlc)
        {
            CompatibleVersions = crit;
            this.installed = installed;
            this.dlls = dlls;
            this.dlc  = dlc;

            // Running these independent prep steps in parallel isn't faster (they're each already parallel)

            // Mapping from identifiers to trivially compatible mods providing those identifiers
            log.Debug("Calculating compatible provider mapping");
            var compatProv = CompatibleProviders(crit, providers);

            // Split mods into compatible, incompatible, and indeterminate
            log.Debug("Partitioning modules by compatibility");
            var groups = getCompatGroups(available).ToArray();

            Compatible = groups.FirstOrDefault(tuple => tuple.Item1 == true)
                               ?.Item2
                               ?? new ConcurrentDictionary<string, AvailableModule>();

            Incompatible = groups.FirstOrDefault(tuple => tuple.Item1 == false)
                                 ?.Item2
                                 ?? new ConcurrentDictionary<string, AvailableModule>();

            // Mods that might be compatible or incompatible based on their dependencies
            var indeterminate = groups.FirstOrDefault(tuple => tuple.Item1 == null)
                                  ?.Item2
                                  ?? new ConcurrentDictionary<string, AvailableModule>();

            // Sort out the complexly [in]compatible mods
            indeterminate.AsParallel()
                         .ForAll(kvp => CheckDepends(kvp.Key, kvp.Value,
                                                     compatProv, new Stack<string>()));
            log.Debug("Done partitioning modules by compatibility");
        }

        /// <summary>
        /// Version criteria that this partition represents
        /// </summary>
        public readonly GameVersionCriteria CompatibleVersions;

        /// <summary>
        /// Mods that are compatible with our versions
        /// </summary>
        public readonly ConcurrentDictionary<string, AvailableModule> Compatible;

        public ICollection<CkanModule> LatestCompatible
        {
            get
            {
                if (latestCompatible == null)
                {
                    latestCompatible = Compatible.Values.Select(avail => avail.Latest(CompatibleVersions)).ToList();
                }
                return latestCompatible;
            }
        }

        /// <summary>
        /// Mods that are incompatible with our versions
        /// </summary>
        public readonly ConcurrentDictionary<string, AvailableModule> Incompatible;

        public ICollection<CkanModule> LatestIncompatible
        {
            get
            {
                if (latestIncompatible == null)
                {
                    latestIncompatible = Incompatible.Values.Select(avail => avail.Latest(null)).ToList();
                }
                return latestIncompatible;
            }
        }

        private readonly Dictionary<string, InstalledModule> installed;
        private readonly HashSet<string> dlls;
        private readonly IDictionary<string, ModuleVersion> dlc;

        private List<CkanModule> latestCompatible;
        private List<CkanModule> latestIncompatible;

        /// <summary>
        /// Filter the provides mapping by compatibility
        /// </summary>
        /// <param name="crit">Versions to be considered compatible</param>
        /// <param name="providers">Mapping from identifiers to mods providing those identifiers</param>
        /// <returns>
        /// Mapping from identifiers to compatible mods providing those identifiers
        /// </returns>
        private Dictionary<string, HashSet<AvailableModule>> CompatibleProviders(
            GameVersionCriteria                          crit,
            Dictionary<string, HashSet<AvailableModule>> providers)
            => providers
                .AsParallel()
                .Select(kvp => new KeyValuePair<string, HashSet<AvailableModule>>(
                    kvp.Key,
                    kvp.Value.Where(availMod => availMod.AllAvailable()
                                                        .Any(ckm => !ckm.IsDLC
                                                                    && ckm.ProvidesList.Contains(kvp.Key)
                                                                    && ckm.IsCompatible(crit)))
                             .ToHashSet()))
                .Where(kvp => kvp.Value.Count > 0)
                .ToDictionary();

        private IEnumerable<Tuple<bool?, ConcurrentDictionary<string, AvailableModule>>> getCompatGroups(
            IEnumerable<Dictionary<string, AvailableModule>> available)
            // Merge AvailableModules with duplicate identifiers
            => available.SelectMany(dict => dict)
                        .GroupBy(kvp => kvp.Key,
                                 kvp => kvp.Value)
                        .ToDictionary(grp => grp.Key,
                                      grp => AvailableModule.Merge(grp))
                        // Group into trivially [in]compatible (false/true) and indeterminate (null)
                        .AsParallel()
                        .GroupBy(kvp => kvp.Value.AllAvailable()
                                                 .All(m => !m.IsCompatible(CompatibleVersions))
                                            // No versions compatible == incompatible
                                            ? false
                                            : kvp.Value.AllAvailable()
                                                       .All(m => m.depends == null)
                                                 // No dependencies == compatible
                                                 ? true
                                                 // Need to investigate this one more later
                                                 : (bool?)null)
                        .Select(grp => new Tuple<bool?, ConcurrentDictionary<string, AvailableModule>>(
                                           grp.Key, grp.ToConcurrentDictionary()));

        /// <summary>
        /// Move an indeterminate module to Compatible or Incompatible
        /// based on its dependencies.
        /// </summary>
        /// <param name="identifier">Identifier of the module to check</param>
        /// <param name="am">The module to check</param>
        /// <param name="providers">Mapping from identifiers to mods providing those identifiers</param>
        /// <param name="investigating">Mods for which we have an active call to CheckDepends right now in the call stack, used to avoid infinite recursion on circular deps</param>
        private void CheckDepends(string                                       identifier,
                                  AvailableModule                              am,
                                  Dictionary<string, HashSet<AvailableModule>> providers,
                                  Stack<string>                                investigating)
        {
            if (Compatible.ContainsKey(identifier) || Incompatible.ContainsKey(identifier))
            {
                // Already checked this one on another branch, don't repeat
                return;
            }
            investigating.Push(identifier);
            foreach (CkanModule m in am.AllAvailable().Where(m => m.IsCompatible(CompatibleVersions)))
            {
                log.DebugFormat("What about {0}?", m.version);
                bool installable = true;
                if (m.depends != null)
                {
                    foreach (RelationshipDescriptor rel in m.depends)
                    {
                        bool foundCompat = false;
                        if (rel.MatchesAny(installed.Select(kvp => kvp.Value.Module).ToList(), dlls, dlc))
                        {
                            // Matches a DLL or DLC, cool
                            foundCompat = true;
                        }
                        else
                        {
                            // Get the list of identifiers that would satisfy this dependency
                            // (mostly only one, except for any_of relationships).
                            // For each of those identifiers, if it is provided by at least one module, get all the modules
                            // that provide it (and make sure each is only once in the list)
                            var candidates = RelationshipIdentifiers(rel)
                                .Where(ident => providers.ContainsKey(ident))
                                .SelectMany(ident => providers[ident])
                                .Distinct();

                            foreach (AvailableModule provider in candidates)
                            {
                                string ident = provider.AllAvailable().First().identifier;
                                log.DebugFormat("Checking depends: {0}", ident);
                                if (investigating.Contains(ident))
                                {
                                    // Circular dependency, pretend it's fine for now
                                    foundCompat = true;
                                    break;
                                }
                                if (!Compatible.ContainsKey(ident) && !Incompatible.ContainsKey(ident))
                                {
                                    CheckDepends(ident, provider, providers, investigating);
                                }
                                if (Compatible.ContainsKey(ident))
                                {
                                    // This one's OK, go to next relationship
                                    foundCompat = true;
                                    break;
                                }
                            }
                        }
                        if (!foundCompat)
                        {
                            // Not satisfiable!! Next CkanModule
                            installable = false;
                            break;
                        }
                    }
                }
                if (installable)
                {
                    // Apparently everything is OK, so we are compatible
                    log.DebugFormat("Complexly compatible: {0}", identifier);
                    Compatible.TryAdd(identifier, am);
                    investigating.Pop();
                    return;
                }
            }
            // None of the CkanModules can be installed!
            log.DebugFormat("Complexly incompatible: {0}", identifier);
            Incompatible.TryAdd(identifier, am);
            investigating.Pop();
        }

        /// <summary>
        /// Find the identifiers that could satisfy this relationship.
        /// Handles the different types of relationships.
        /// </summary>
        /// <param name="rel">Relationship to satisfy</param>
        /// <returns>
        /// The identifier for a ModuleRelationshipDescriptor,
        /// multiple for AnyOfRelationshipDescriptor,
        /// nothing otherwise.
        /// </returns>
        private IEnumerable<string> RelationshipIdentifiers(RelationshipDescriptor rel)
            => rel is ModuleRelationshipDescriptor modRel
                ? Enumerable.Repeat(modRel.name, 1)
                : rel is AnyOfRelationshipDescriptor anyRel
                    ? anyRel.any_of.SelectMany(RelationshipIdentifiers)
                    : Enumerable.Empty<string>();

        private static readonly ILog log = LogManager.GetLogger(typeof(CompatibilitySorter));
    }
}
