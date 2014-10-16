namespace CKAN {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using log4net;

    public struct RelationshipResolverOptions {
        public bool with_recommends;
        public bool with_suggests;
        public bool with_all_suggests;
    }

    public class RelationshipResolver {

        // A list of all the mods we're going to install.
        private Dictionary<string,CkanModule> modlist = new Dictionary<string, CkanModule> ();
        private static readonly ILog log = LogManager.GetLogger(typeof(RelationshipResolver));
        private Registry registry = RegistryManager.Instance().registry;

        public RelationshipResolver (List<string> modules, RelationshipResolverOptions options) {

            // Start by figuring out what versions we're installing, and then
            // adding them to the list. This *must* be pre-populated with all
            // user-specified modules, as they may be supplying things that provide
            // virtual packages.

            var user_mods = new List<CkanModule> ();

            log.DebugFormat ("Processing relationships for {0} modules", modules.Count);

            foreach (string module in modules) {
                CkanModule mod = registry.LatestAvailable (module);
                log.DebugFormat ("Preparing to resolve relationships for {0} {1}", mod.identifier, mod.version);
                user_mods.Add(mod);
                this.Add(mod);
            }

            // Now that we've already pre-populated modlist, we can resolve
            // the rest of our dependencies.

            foreach (CkanModule module in user_mods) {
                log.InfoFormat ("Resolving relationships for {0}", module.identifier);
                Resolve (module, options);
            }

        }

        // Resolve all relationships for a module.
        // May recurse to ResolveStanza.
        private void Resolve(CkanModule module, RelationshipResolverOptions options) {

            // Even though we may resolve top-level suggests for our module,
            // we don't install suggestions all the down unless with_all_suggests
            // is true.
            var sub_options = options;
            sub_options.with_suggests = false;

            // Resolve all the things!

            if (module.pre_depends != null) {
                log.FatalFormat("pre-depends not yet implemented while processing {0}", module.identifier);
                throw new NotSupportedException ("Pre-depends not implemented");
            }

            log.DebugFormat ("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, sub_options);

            if (options.with_recommends) {
                log.DebugFormat ("Resolving recommends for {0}", module.identifier);
                ResolveStanza (module.recommends, sub_options);
            }

            if (options.with_suggests || options.with_all_suggests) {
                log.DebugFormat ("Resolving suggests for {0}", module.identifier);
                ResolveStanza (module.suggests, sub_options);
            }

        }

        // Resolve a relationship stanza (a list of relationships).
        // May recurse back to Resolve.
        private void ResolveStanza(dynamic[] stanza, RelationshipResolverOptions options) {

            if (stanza == null) {
                return;
            }

            foreach (dynamic dep in stanza) {
                string dep_name = dep.name;

                log.DebugFormat ("Considering {0}", dep_name);

                // If we already have this dependency covered, skip.
                if (modlist.ContainsKey (dep_name)) {
                    continue;
                }

                // If it's already installed, skip.
                if (registry.IsInstalled (dep_name)) {
                    continue;
                }

                // Otherwise, find, add, and recurse!
                CkanModule candidate;

                try {
                    candidate = registry.LatestAvailable (dep_name);
                }
                catch (ModuleNotFoundException) {
                    log.ErrorFormat ("Dependency on {0} found, but nothing provides it.", dep_name);
                    throw new ModuleNotFoundException (dep_name);
                }

                Add(candidate);
                Resolve (candidate, options);
            }
        }

        private void Add(CkanModule module) {
            log.DebugFormat ("Adding {0} {1}", module.identifier, module.version);

            if (modlist.ContainsKey (module.identifier)) {
                // We should never be adding something twice!
                log.ErrorFormat ("Assertion failed: Adding {0} twice in relationship resolution", module.identifier);
                throw new Exception (); // TODO: Something more meaningful!
            }
            modlist.Add(module.identifier, module);
            log.DebugFormat("Added {0}", module.identifier);

            // Stop here if it doesn't have any provide aliases.
            if (module.provides == null) {
                return;
            }

            // Handle provides/aliases if it does.
            foreach (string alias in module.provides) {

                // It's okay if there's already a key for one of our aliases
                // in the resolution list. In which case, we don't do anything.
                if (! modlist.ContainsKey(alias)) {
                    log.DebugFormat ("Adding {0} providing {1}", module.identifier, alias);
                    modlist.Add(alias, module);
                }
            }
        }

        public List<CkanModule> ModList() {
            var modules = new HashSet<CkanModule> (modlist.Values);
            return modules.ToList ();
        }
    }
}

