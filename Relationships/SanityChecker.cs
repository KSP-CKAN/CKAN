using System.Linq;
using System.Collections.Generic;
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
        public static List<string> ConsistencyErrors(IEnumerable<Module> modules)
        {
            var errors = new List<string>();

            if (modules == null)
            {
                return errors; // The empty list of errors, of course.
            }

            // Build a data structure of which modules provide what
            var providers = new Dictionary<string, List<Module>>();

            foreach (Module mod in modules)
            {
                foreach (string provides in mod.ProvidesList)
                {
                    log.DebugFormat("{0} provides {1}", mod, provides);
                    providers[provides] = providers.ContainsKey(provides) ? providers[provides] : new List<Module>();
                    providers[provides].Add(mod);
                }
            }

            // These three bits of code walk all our modules, and extract flattened sets of
            // dependencies, conflicts, and provides. It would be nice to have a way to combine
            // them.

            var depends = new HashSet<string> (
                modules
                .Select(mod => mod.depends) // Get all our depends lists
                .Where(x => x != null)      // Filter out nulls
                .SelectMany(x => x)         // Flatten to one big list
                .Select(dependency => dependency.name)
            );

            // Walk everything we depend upon, and make sure it's there.

            // TODO: This doesn't examine versions, it should!
            foreach (string dep in depends)
            {
                if (! providers.ContainsKey(dep))
                {
                    errors.Add(string.Format("Cannot find required dependency {0}", dep));
                }
            }

            // Conflicts are more difficult. Mods are allowed to conflict with themselves.
            // So we walk all our mod conflicts, find what (if anything) provide those
            // conflicts, and return false if it's not the module we're examining.

            // TODO: This doesn't examine versions. We should!

            foreach (Module mod in modules)
            {
                // If our mod doesn't conflict with anything, skip it.
                if (mod.conflicts == null)
                {
                    continue;
                }

                foreach (var conflict in mod.conflicts)
                {
                    // If nothing conflicts with us, skip.
                    if (! providers.ContainsKey(conflict.name))
                    {
                        continue;
                    }

                    // If something does conflict with us, and it's not ourselves, that's a fail.
                    foreach (Module provider in providers[conflict.name])
                    {
                        if (provider != mod)
                        {
                            errors.Add(string.Format("{0} conflicts with {1}", mod.identifier, provider.identifier));
                        }
                    }
                }
            }

            // Return whatever we've found, which could be empty.
            return errors;
        }

        /// <summary>
        /// Ensures all modules in the list provided can co-exist.
        /// Throws a InconsistentKraken containing a list of inconsistences if they do not.
        /// Does nothing if the modules can happily co-exist.
        /// </summary>
        public static void EnforceConsistency(IEnumerable<Module> modules)
        {
            List<string> errors = ConsistencyErrors(modules);

            if (errors.Count != 0)
            {
                throw new InconsistentKraken(errors);
            }
        }
    
        /// <summary>
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// </summary>
        public static bool IsConsistent(IEnumerable<Module> modules)
        {
            return ConsistencyErrors(modules).Count == 0;
        }
    }
}