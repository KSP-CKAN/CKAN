using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.Extensions;
using CKAN.Versioning;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Sanity checks on what mods we have installed, or may install.
    /// </summary>
    public static class SanityChecker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SanityChecker));

        /// <summary>
        ///     Checks the list of modules for consistency errors, returning a list of
        ///     errors found. The list will be empty if everything is fine.
        /// </summary>
        public static ICollection<string> ConsistencyErrors(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc
        )
        {
            List<KeyValuePair<CkanModule, RelationshipDescriptor>> unmetDepends;
            List<KeyValuePair<CkanModule, RelationshipDescriptor>> conflicts;
            var errors = new HashSet<string>();
            if (!CheckConsistency(modules, dlls, dlc, out unmetDepends, out conflicts))
            {
                foreach (var kvp in unmetDepends)
                {
                    errors.Add($"{kvp.Key} has an unsatisfied dependency: {kvp.Value} is not installed");
                }
                foreach (var kvp in conflicts)
                {
                    errors.Add($"{kvp.Key} conflicts with {kvp.Value}");
                }
            }
            return errors;
        }

        /// <summary>
        /// Ensures all modules in the list provided can co-exist.
        /// Throws a BadRelationshipsKraken describing the problems otherwise.
        /// Does nothing if the modules can happily co-exist.
        /// </summary>
        public static void EnforceConsistency(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls = null,
            IDictionary<string, UnmanagedModuleVersion> dlc = null
        )
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
        /// </summary>
        public static bool IsConsistent(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls = null,
            IDictionary<string, UnmanagedModuleVersion> dlc = null
        )
        {
            List<KeyValuePair<CkanModule, RelationshipDescriptor>> unmetDepends;
            List<KeyValuePair<CkanModule, RelationshipDescriptor>> conflicts;
            return CheckConsistency(modules, dlls, dlc, out unmetDepends, out conflicts);
        }

        private static bool CheckConsistency(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc,
            out List<KeyValuePair<CkanModule, RelationshipDescriptor>> UnmetDepends,
            out List<KeyValuePair<CkanModule, RelationshipDescriptor>> Conflicts
        )
        {
            UnmetDepends = FindUnsatisfiedDepends(modules?.ToList(), dlls?.ToHashSet(), dlc);
            Conflicts    = FindConflicting(       modules,           dlls?.ToHashSet(), dlc);
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
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc
        )
        {
            var unsat = new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
            if (modules != null)
            {
                foreach (CkanModule m in modules.Where(m => m.depends != null))
                {
                    foreach (RelationshipDescriptor dep in m.depends)
                    {
                        if (!dep.MatchesAny(modules, dlls, dlc))
                        {
                            unsat.Add(new KeyValuePair<CkanModule, RelationshipDescriptor>(m, dep));
                        }
                    }
                }
            }
            return unsat;
        }

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
            IDictionary<string, UnmanagedModuleVersion> dlc
        )
        {
            var confl = new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
            if (modules != null)
            {
                foreach (CkanModule m in modules.Where(m => m.conflicts != null))
                {
                    // Remove self from the list, so we're only comparing to OTHER modules.
                    // Also remove other versions of self, to avoid conflicts during upgrades.
                    var others = modules.Where(other => other.identifier != m.identifier);
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

        private sealed class ProvidesInfo
        {
            public string ProviderIdentifier     { get; }
            public ModuleVersion ProviderVersion { get; }
            public string ProvideeIdentifier     { get; }
            public ModuleVersion ProvideeVersion { get; }

            public ProvidesInfo(
                string providerIdentifier,
                ModuleVersion providerVersion,
                string provideeIdentifier,
                ModuleVersion provideeVersion
            )
            {
                ProviderIdentifier = providerIdentifier;
                ProviderVersion    = providerVersion;
                ProvideeIdentifier = provideeIdentifier;
                ProvideeVersion    = provideeVersion;
            }
        }
    }
}
