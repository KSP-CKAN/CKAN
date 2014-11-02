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
        public static List<string> ConsistencyErrors(IEnumerable<Module> modules, IEnumerable<string> dlls)
        {
            var errors = new List<string>();

            // If we have no modules, then everything is fine. DLLs can't depend or conflict on things.
            if (modules == null)
            {
                return errors;
            }

            if (dlls == null)
            {
                dlls = new List<string>();
            }

            // Build a data structure of which modules provide what
            var providers = new Dictionary<string, List<string>>();

            foreach (Module mod in modules)
            {
                foreach (string provides in mod.ProvidesList)
                {
                    log.DebugFormat("{0} provides {1}", mod, provides);
                    providers[provides] = providers.ContainsKey(provides) ? providers[provides] : new List<string>();
                    providers[provides].Add(mod.identifier);
                }
            }

            // Add in our DLLs as things we know exist.
            foreach (string dll in dlls)
            {
                if (! providers.ContainsKey(dll))
                {
                    providers[dll] = new List<string>();
                }
                providers[dll].Add(dll);
            }

            // Walk everything we depend upon, and make sure it's there.

            // TODO: This doesn't examine versions, it should!
            foreach (Module mod in modules)
            {
                if (mod.depends == null)
                {
                    continue;
                }

                foreach (RelationshipDescriptor dep in mod.depends)
                {
                    if (! providers.ContainsKey(dep.name))
                    {
                        errors.Add(string.Format("{1} requires {0}, but nothing provides it.", mod.identifier, dep.name));
                    }
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
                    foreach (string provider in providers[conflict.name])
                    {
                        if (provider != mod.identifier)
                        {
                            errors.Add(string.Format("{0} conflicts with {1}.", mod.identifier, provider));
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
        public static void EnforceConsistency(IEnumerable<Module> modules, IEnumerable<string> dlls = null)
        {
            List<string> errors = ConsistencyErrors(modules, dlls);

            if (errors.Count != 0)
            {
                throw new InconsistentKraken(errors);
            }
        }
    
        /// <summary>
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// </summary>
        public static bool IsConsistent(IEnumerable<Module> modules, IEnumerable<string> dlls = null)
        {
            return ConsistencyErrors(modules, dlls).Count == 0;
        }
    }
}