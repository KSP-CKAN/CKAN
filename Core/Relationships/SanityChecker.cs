using System;
using System.Collections.Generic;
using System.Linq;

using CKAN.Versioning;

namespace CKAN
{
    using modRelPair = Tuple<CkanModule, RelationshipDescriptor, CkanModule?>;
    using modRelList = List<Tuple<CkanModule, RelationshipDescriptor, CkanModule?>>;

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
        public static void EnforceConsistency(IEnumerable<CkanModule>                     modules,
                                              IReadOnlyCollection<string>                 dlls,
                                              IDictionary<string, UnmanagedModuleVersion> dlc)
        {
            if (!CheckConsistency(modules, dlls, dlc,
                                  out List<Tuple<CkanModule, RelationshipDescriptor>> unmetDepends,
                                  out modRelList conflicts))
            {
                throw new BadRelationshipsKraken(unmetDepends, conflicts);
            }
        }

        /// <summary>
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// This is only used by tests!
        /// </summary>
        public static bool IsConsistent(IEnumerable<CkanModule>                     modules,
                                        IReadOnlyCollection<string>                 dlls,
                                        IDictionary<string, UnmanagedModuleVersion> dlc)
            => CheckConsistency(modules, dlls, dlc,
                                out var _, out var _);

        private static bool CheckConsistency(IEnumerable<CkanModule>                             modules,
                                             IReadOnlyCollection<string>                         dlls,
                                             IDictionary<string, UnmanagedModuleVersion>         dlc,
                                             out List<Tuple<CkanModule, RelationshipDescriptor>> UnmetDepends,
                                             out modRelList                                      Conflicts)
        {
            var modList = modules.ToList();
            UnmetDepends = FindUnsatisfiedDepends(modList, dlls, dlc).ToList();
            Conflicts = FindConflicting(modList, dlls, dlc);
            return UnmetDepends.Count == 0 && Conflicts.Count == 0;
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
        public static IEnumerable<Tuple<CkanModule, RelationshipDescriptor>> FindUnsatisfiedDepends(
                IReadOnlyCollection<CkanModule>             modules,
                IReadOnlyCollection<string>?                dlls,
                IDictionary<string, UnmanagedModuleVersion> dlc)
            => modules.SelectMany(m => (m.depends ?? Enumerable.Empty<RelationshipDescriptor>())
                                         .Where(dep => !dep.MatchesAny(modules, dlls, dlc))
                                         .Select(dep => Tuple.Create(m, dep)));

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
        private static modRelList FindConflicting(List<CkanModule>                            modules,
                                                  IReadOnlyCollection<string>                 dlls,
                                                  IDictionary<string, UnmanagedModuleVersion> dlc)
            => modules.Where(m => m.conflicts != null)
                      .SelectMany(m => FindConflictingWith(
                                           m,
                                           modules.Where(other => other.identifier != m.identifier)
                                                  .ToList(),
                                           dlls, dlc))
                      .ToList();

        private static IEnumerable<modRelPair> FindConflictingWith(CkanModule                                  module,
                                                                   List<CkanModule>                            otherMods,
                                                                   IReadOnlyCollection<string>                 dlls,
                                                                   IDictionary<string, UnmanagedModuleVersion> dlc)
            => module.conflicts?.Select(rel => rel.MatchesAny(otherMods, dlls, dlc, out CkanModule?            other)
                                                   ? new modRelPair(module, rel, other)
                                                   : null)
                                .OfType<modRelPair>()
                               ?? Enumerable.Empty<modRelPair>();
    }
}
