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

            Dictionary<string, List<string>> providers = ModulesToProvides(modules, dlls);

            foreach (KeyValuePair<string,List<CkanModule>> entry in FindUnmetDependencies(modules, dlls))
            {
                foreach (CkanModule unhappy_mod in entry.Value)
                {
                    errors.Add(string.Format("{0} depends on {1} but it is not listed in the index, or not available for your version of KSP.", unhappy_mod.identifier, entry.Key));
                }
            }

            // Conflicts are more difficult. Mods are allowed to conflict with themselves.
            // So we walk all our mod conflicts, find what (if anything) provide those
            // conflicts, and return false if it's not the module we're examining.

            // TODO: This doesn't examine versions. We should!
            // TODO: It would be great to factor this into its own function, too.

            foreach (CkanModule mod in modules)
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
        /// Maps a list of modules and dlls to a dictionary of what's provided, and a list of
        /// identifiers that supply each.
        /// </summary>
        // 
        // Eg: {
        //          LifeSupport => [ "TACLS", "Snacks" ]
        //          DogeCoinFlag => [ "DogeCoinFlag" ]
        // }
        public static Dictionary<string, List<string>> ModulesToProvides(IEnumerable<CkanModule> modules, IEnumerable<string> dlls = null)
        {
            var providers = new Dictionary<string, List<string>>();

            if (dlls == null)
            {
                dlls = new List<string>();
            }

            foreach (CkanModule mod in modules)
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

            return providers;
        }

        /// <summary>
        /// Given a list of modules and optional dlls, returns a dictionary of dependencies which are still not met.
        /// The dictionary keys are the un-met depdendencies, the values are the modules requesting them.
        /// </summary>
        public static Dictionary<string,List<CkanModule>> FindUnmetDependencies(IEnumerable<CkanModule> modules, IEnumerable<string> dlls = null)
        {
            return FindUnmetDependencies(modules, ModulesToProvides(modules, dlls));
        }

        /// <summary>
        /// Given a list of modules, and a dictionary of providers, returns a dictionary of depdendencies which have not been met.
        /// </summary>
        internal static Dictionary<string,List<CkanModule>> FindUnmetDependencies(IEnumerable<CkanModule> modules, Dictionary<string,List<string>> provided)
        {
            return FindUnmetDependencies(modules, new HashSet<string> (provided.Keys));
        }

        /// <summary>
        /// Given a list of modules, and a set of providers, returns a dictionary of dependencies which have not been met.
        /// </summary>
        internal static Dictionary<string,List<CkanModule>> FindUnmetDependencies(IEnumerable<CkanModule> modules, HashSet<string> provided)
        {
            var unmet = new Dictionary<string,List<CkanModule>> ();

            // TODO: This doesn't examine versions, it should!

            foreach (CkanModule mod in modules)
            {
                // If this module has no dependencies, we're done.
                if (mod.depends == null)
                {
                    continue;
                }

                // If it does have dependencies, but we can't find anything that provides them,
                // add them to our unmet list.
                foreach (RelationshipDescriptor dep in mod.depends.Where(dep => ! provided.Contains(dep.name)))
                {
                    if (!unmet.ContainsKey(dep.name))
                    {
                        unmet[dep.name] = new List<CkanModule>();
                    }
                    unmet[dep.name].Add(mod); // mod needs dep.name, but doesn't have it.
                }
            }

            return unmet;
        }
    }
}