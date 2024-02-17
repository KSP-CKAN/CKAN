using System;
using System.Collections.Generic;
using System.Linq;

using CKAN.Versioning;
#if NETSTANDARD2_0
using CKAN.Extensions;
#endif

namespace CKAN
{
    using modRelPair = Tuple<CkanModule, RelationshipDescriptor, CkanModule>;
    using modRelList = List<Tuple<CkanModule, RelationshipDescriptor, CkanModule>>;

    /// <summary>
    /// Sanity checks on what mods we have installed, or may install.
    /// </summary>
    public static class SanityChecker
    {
        /// <summary>
        /// Ensures all modules in the list provided can co-exist.
        /// Throws a BadRelationshipsKraken describing the problems otherwise.
        /// Does nothing if the modules can happily co-exist.
        /// </summary>
        public static void EnforceConsistency(IEnumerable<CkanModule>            modules,
                                              IEnumerable<string>                dlls = null,
                                              IDictionary<string, ModuleVersion> dlc  = null)
        {
            if (!CheckConsistency(modules, dlls, dlc,
                                  out modRelList unmetDepends, out modRelList conflicts))
            {
                throw new BadRelationshipsKraken(unmetDepends, conflicts);
            }
        }

        /// <summary>
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// This is only used by tests!
        /// </summary>
        public static bool IsConsistent(IEnumerable<CkanModule>            modules,
                                        IEnumerable<string>                dlls = null,
                                        IDictionary<string, ModuleVersion> dlc  = null)
            => CheckConsistency(modules, dlls, dlc,
                                out var _, out var _);

        private static bool CheckConsistency(IEnumerable<CkanModule>            modules,
                                             IEnumerable<string>                dlls,
                                             IDictionary<string, ModuleVersion> dlc,
                                             out modRelList                     UnmetDepends,
                                             out modRelList                     Conflicts)
        {
            var modList = modules?.ToList();
            var dllSet = dlls?.ToHashSet();
            UnmetDepends = FindUnsatisfiedDepends(modList, dllSet, dlc);
            Conflicts = FindConflicting(modList, dllSet, dlc);
            return !UnmetDepends.Any() && !Conflicts.Any();
        }

        /// <summary>
        /// Find unsatisfied dependencies among the given modules and DLLs.
        /// </summary>
        /// <param name="modules">List of modules to check</param>
        /// <param name="dlls">List of DLLs that can also count toward relationships</param>
        /// <param name="dlc">List of DLC that can also count toward relationships</param>
        /// <returns>
        /// List of dependencies that aren't satisfied represented as pairs.
        /// Each Key is the depending module, and each Value is the relationship.
        /// </returns>
        public static modRelList FindUnsatisfiedDepends(ICollection<CkanModule>            modules,
                                                        HashSet<string>                    dlls,
                                                        IDictionary<string, ModuleVersion> dlc)
            => (modules?.Where(m => m.depends != null)
                        .SelectMany(m => m.depends.Select(dep => new modRelPair(m, dep, null)))
                        .Where(kvp => !kvp.Item2.MatchesAny(modules, dlls, dlc))
                       ?? Enumerable.Empty<modRelPair>())
                       .ToList();

        /// <summary>
        /// Find conflicts among the given modules and DLLs.
        /// </summary>
        /// <param name="modules">List of modules to check</param>
        /// <param name="dlls">List of DLLs that can also count toward relationships</param>
        /// <param name="dlc">List of DLC that can also count toward relationships</param>
        /// <returns>
        /// List of conflicts represented as pairs.
        /// Each Key is the depending module, and each Value is the relationship.
        /// </returns>
        private static modRelList FindConflicting(List<CkanModule>                   modules,
                                                  HashSet<string>                    dlls,
                                                  IDictionary<string, ModuleVersion> dlc)
            => (modules?.Where(m => m.conflicts != null)
                        .SelectMany(m => FindConflictingWith(
                                             m,
                                             modules.Where(other => other.identifier != m.identifier)
                                                    .ToList(),
                                             dlls, dlc))
                       ?? Enumerable.Empty<modRelPair>())
                       .ToList();

        private static IEnumerable<modRelPair> FindConflictingWith(CkanModule                         module,
                                                                   List<CkanModule>                   otherMods,
                                                                   HashSet<string>                    dlls,
                                                                   IDictionary<string, ModuleVersion> dlc)
            => module.conflicts.Select(rel => rel.MatchesAny(otherMods, dlls, dlc, out CkanModule other)
                                                  ? new modRelPair(module, rel, other)
                                                  : null)
                               .Where(pair => pair != null);
    }
}
