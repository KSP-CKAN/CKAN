using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CKAN
{
    public struct RelationshipResolverOptions
    {
        public bool with_all_suggests;
        public bool with_recommends;
        public bool with_suggests;
    }

    // Alas, it appears that structs cannot have defaults. Try
    // DefaultOpts() to get friendly defaults.

    public class RelationshipResolver
    {
        // A list of all the mods we're going to install.
        private static readonly ILog log = LogManager.GetLogger(typeof (RelationshipResolver));
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();
        private Registry registry;

        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
        //
        // TODO: This should be able to handle un-installs as well.
        public RelationshipResolver(List<string> modules, RelationshipResolverOptions options, Registry registry)
        {

            this.registry = registry;

            // Start by figuring out what versions we're installing, and then
            // adding them to the list. This *must* be pre-populated with all
            // user-specified modules, as they may be supplying things that provide
            // virtual packages.

            var user_requested_mods = new List<CkanModule>();

            log.DebugFormat("Processing relationships for {0} modules", modules.Count);

            foreach (string module in modules)
            {
                CkanModule mod = registry.LatestAvailable(module);
                if (mod == null)
                {
                    throw new ModuleNotFoundKraken(module);
                }
                 
                log.DebugFormat("Preparing to resolve relationships for {0} {1}", mod.identifier, mod.version);
                user_requested_mods.Add(mod);
                this.Add(mod);
            }
             
            // Now that we've already pre-populated modlist, we can resolve
            // the rest of our dependencies.

            foreach (CkanModule module in user_requested_mods)
            {
                log.InfoFormat("Resolving relationships for {0}", module.identifier);
                Resolve(module, options);
            }
        }

        /// <summary>
        ///     Returns the default options for relationship resolution.
        /// </summary>
        public static RelationshipResolverOptions DefaultOpts()
        {
            var opts = new RelationshipResolverOptions();
            opts.with_recommends = true;
            opts.with_suggests = false;
            opts.with_all_suggests = false;

            return opts;
        }

        /// <summary>
        /// Resolves all relationships for a module.
        /// May recurse to ResolveStanza, which may add additional modules to be installed.
        /// </summary>
        private void Resolve(CkanModule module, RelationshipResolverOptions options)
        {
            // Even though we may resolve top-level suggests for our module,
            // we don't install suggestions all the down unless with_all_suggests
            // is true.
            RelationshipResolverOptions sub_options = options;
            sub_options.with_suggests = false;

            // Resolve all the things!

            if (module.pre_depends != null)
            {
                log.FatalFormat("pre-depends not yet implemented while processing {0}", module.identifier);
                throw new NotSupportedException("Pre-depends not implemented");
            }

            log.DebugFormat("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, sub_options);

            if (options.with_recommends)
            {
                log.DebugFormat("Resolving recommends for {0}", module.identifier);
                ResolveStanza(module.recommends, sub_options);
            }

            if (options.with_suggests || options.with_all_suggests)
            {
                log.DebugFormat("Resolving suggests for {0}", module.identifier);
                ResolveStanza(module.suggests, sub_options);
            }
        }

        /// <summary>
        /// Resolve a relationship stanza (a list of relationships).
        /// This will add modules to be installed, if required.
        /// May recurse back to Resolve for those new modules.
        /// 
        /// Throws a TooManyModsProvideKraken if we have too many choices.
        /// </summary>
        private void ResolveStanza(RelationshipDescriptor[] stanza, RelationshipResolverOptions options)
        {
            if (stanza == null)
            {
                return;
            }

            foreach (RelationshipDescriptor dep in stanza)
            {
                string dep_name = dep.name;

                log.DebugFormat("Considering {0}", dep_name);

                // If we already have this dependency covered, skip.
                if (modlist.ContainsKey(dep_name))
                {
                    continue;
                }

                // If it's already installed, skip.
                if (registry.IsInstalled(dep_name))
                {
                    continue;
                }

                List<CkanModule> candidates = registry.LatestAvailableWithProvides(dep_name);

                if (candidates.Count == 0)
                {
                    log.ErrorFormat("Dependency on {0} found, but nothing provides it.", dep_name);
                    throw new ModuleNotFoundKraken(dep_name);
                }
                else if (candidates.Count > 1)
                {
                    // Oh no, too many to pick from!
                    throw new TooManyModsProvideKraken(dep_name, candidates);
                }

                Add(candidates[0]);
                Resolve(candidates[0], options);
            }
        }

        /// <summary>
        /// Adds the specified module to the list of modules we're installing.
        /// This also adds its provides list to what we have available.
        /// </summary>
        private void Add(CkanModule module)
        {
            log.DebugFormat("Adding {0} {1}", module.identifier, module.version);

            if (modlist.ContainsKey(module.identifier))
            {
                // We should never be adding something twice!
                log.ErrorFormat("Assertion failed: Adding {0} twice in relationship resolution", module.identifier);
                throw new Exception(); // TODO: Something more meaningful!
            }
            modlist.Add(module.identifier, module);
            log.DebugFormat("Added {0}", module.identifier);

            // Stop here if it doesn't have any provide aliases.
            if (module.provides == null)
            {
                return;
            }

            // Handle provides/aliases if it does.
            foreach (string alias in module.provides)
            {
                // It's okay if there's already a key for one of our aliases
                // in the resolution list. In which case, we don't do anything.
                if (! modlist.ContainsKey(alias))
                {
                    log.DebugFormat("Adding {0} providing {1}", module.identifier, alias);
                    modlist.Add(alias, module);
                }
            }
        }

        /// <summary>
        /// Returns a list of all modules to install to satisify the changes required.
        /// </summary>
        public List<CkanModule> ModList()
        {
            var modules = new HashSet<CkanModule>(modlist.Values);
            return modules.ToList();
        }
    }
}