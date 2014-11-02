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
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// </summary>
        public static bool IsConsistent(IEnumerable<Module> modules)
        {
            if (modules == null)
            {
                return true;
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
                    log.DebugFormat ("Cannot find required dependency {0}", dep);
                    return false;
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
                            return false;
                        }
                    }
                }
            }

            // Looks like everything's good!
            return true;
        }
    }
}

