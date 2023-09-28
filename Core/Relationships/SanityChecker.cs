using System.Collections.Generic;
using System.Linq;

using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
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
            List<KeyValuePair<CkanModule, RelationshipDescriptor>> unmetDepends;
            List<KeyValuePair<CkanModule, RelationshipDescriptor>> conflicts;
            if (!CheckConsistency(modules, dlls, dlc, out unmetDepends, out conflicts))
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

        private static bool CheckConsistency(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls,
            IDictionary<string, ModuleVersion> dlc,
            out List<KeyValuePair<CkanModule, RelationshipDescriptor>> UnmetDepends,
            out List<KeyValuePair<CkanModule, RelationshipDescriptor>> Conflicts)
        {
            modules = modules?.Memoize();
            var dllSet = dlls?.ToHashSet();
            UnmetDepends = FindUnsatisfiedDepends(modules?.ToList(), dllSet, dlc);
            Conflicts    = FindConflicting(       modules,           dllSet, dlc);
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
        public static List<KeyValuePair<CkanModule, RelationshipDescriptor>> FindUnsatisfiedDepends(
            ICollection<CkanModule>            modules,
            HashSet<string>                    dlls,
            IDictionary<string, ModuleVersion> dlc)
            => (modules?.Where(m => m.depends != null)
                        .SelectMany(m => m.depends.Select(dep =>
                            new KeyValuePair<CkanModule, RelationshipDescriptor>(m, dep)))
                        .Where(kvp => !kvp.Value.MatchesAny(modules, dlls, dlc))
                       ?? Enumerable.Empty<KeyValuePair<CkanModule, RelationshipDescriptor>>())
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
        public static List<KeyValuePair<CkanModule, RelationshipDescriptor>> FindConflicting(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, ModuleVersion> dlc)
        {
            var confl = new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
            if (modules != null)
            {
                modules = modules.Memoize();
                foreach (CkanModule m in modules.Where(m => m.conflicts != null))
                {
                    // Remove self from the list, so we're only comparing to OTHER modules.
                    // Also remove other versions of self, to avoid conflicts during upgrades.
                    var others = modules.Where(other => other.identifier != m.identifier).Memoize();
                    foreach (RelationshipDescriptor dep in m.conflicts)
                    {
                        if (dep.MatchesAny(others, dlls, dlc))
                        {
                            confl.Add(new KeyValuePair<CkanModule, RelationshipDescriptor>(m, dep));
                        }
                    }
                }
            }
            return confl;
        }


    }
}
