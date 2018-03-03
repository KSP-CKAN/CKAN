using System.Collections.Generic;
using System.Linq;
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
        public static ICollection<string> ConsistencyErrors(IEnumerable<CkanModule> modules, IEnumerable<string> dlls)
        {
            var errors = new HashSet<string>();

            // If we have no modules, then everything is fine. DLLs can't depend or conflict on things.
            if (modules == null)
            {
                return errors;
            }

            foreach (var kvp in FindUnsatisfiedDepends(modules.ToList(), dlls?.ToHashSet()))
            {
                errors.Add($"{kvp.Key} has an unsatisfied dependency: {kvp.Value} is not installed");
            }

            // Conflicts are more difficult. Mods are allowed to conflict with themselves.
            // So we walk all our mod conflicts, find what (if anything) provide those
            // conflicts, and return false if it's not the module we're examining.
            foreach (var kvp in FindConflicting(modules.ToList(), dlls?.ToHashSet()))
            {
                errors.Add($"{kvp.Key} conflicts with {kvp.Value}");
            }

            // Return whatever we've found, which could be empty.
            return errors;
        }

        /// <summary>
        /// Ensures all modules in the list provided can co-exist.
        /// Throws a InconsistentKraken containing a list of inconsistences if they do not.
        /// Does nothing if the modules can happily co-exist.
        /// </summary>
        public static void EnforceConsistency(IEnumerable<CkanModule> modules, IEnumerable<string> dlls = null)
        {
            ICollection<string> errors = ConsistencyErrors(modules, dlls);

            if (errors.Count != 0)
            {
                throw new InconsistentKraken(errors);
            }
        }

        /// <summary>
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// </summary>
        public static bool IsConsistent(IEnumerable<CkanModule> modules, IEnumerable<string> dlls = null)
        {
            return ConsistencyErrors(modules, dlls).Count == 0;
        }

        /// <summary>
        /// Find unsatisfied dependencies among the given modules and DLLs.
        /// </summary>
        /// <param name="modules">List of modules to check</param>
        /// <param name="dlls">List of DLLs that can also count toward relationships</param>
        /// <returns>
        /// List of dependencies that aren't satisfied represented as pairs.
        /// Each Key is the depending module, and each Value is the relationship.
        /// </returns>
        public static List<KeyValuePair<CkanModule, RelationshipDescriptor>>
            FindUnsatisfiedDepends(IEnumerable<CkanModule> modules, HashSet<string> dlls)
        {
            var unsat = new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
            if (modules != null)
            {
                foreach (CkanModule m in modules.Where(m => m.depends != null))
                {
                    foreach (RelationshipDescriptor dep in m.depends)
                    {
                        if (!dep.MatchesAny(modules, dlls))
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
        /// <returns>
        /// List of conflicts represented as pairs.
        /// Each Key is the depending module, and each Value is the relationship.
        /// </returns>
        public static List<KeyValuePair<CkanModule, RelationshipDescriptor>>
            FindConflicting(IEnumerable<CkanModule> modules, HashSet<string> dlls)
        {
            var confl = new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
            if (modules != null)
            {
                foreach (CkanModule m in modules.Where(m => m.conflicts != null))
                {
                    // Remove self from the list, so we're only comparing to OTHER modules
                    var others = modules.Where(other => other != m);
                    foreach (RelationshipDescriptor dep in m.conflicts)
                    {
                        if (dep.MatchesAny(others, dlls))
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
