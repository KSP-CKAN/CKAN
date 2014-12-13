using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CKAN
{
    public class RelationshipResolverOptions : ICloneable
    {
        public bool with_all_suggests;
        public bool with_recommends = true;
        public bool with_suggests;
        public bool without_toomanyprovides_kraken = false;
        public bool without_enforce_consistency = false;
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    // Alas, it appears that structs cannot have defaults. Try
    // DefaultOpts() to get friendly defaults.

    // TODO: RR currently conducts a depth-first resolution of requirements. While we do the
    // right thing in processing all depdenencies first, then recommends, and then suggests,
    // we could find that a recommendation many layers deep prevents a recommendation in the
    // original mod's recommends list.
    //
    // If we resolved in things breadth-first order, we're less likely to encounter surprises
    // where a nth-deep recommend blocks a top-level recommend.

    public class RelationshipResolver
    {
        // A list of all the mods we're going to install.
        private static readonly ILog log = LogManager.GetLogger(typeof(RelationshipResolver));
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();
        private Registry registry;

        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
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

                foreach (CkanModule listed_mod in modlist.Values)
                {
                    if (listed_mod.ConflictsWith(mod))
                    {
                        throw new InconsistentKraken(string.Format("{0} conflicts with {1}, can't install both.", mod, listed_mod));
                    }
                }

                user_requested_mods.Add(mod);
                Add(mod);
            }

            // Now that we've already pre-populated modlist, we can resolve
            // the rest of our dependencies.

            foreach (CkanModule module in user_requested_mods)
            {
                log.InfoFormat("Resolving relationships for {0}", module.identifier);
                Resolve(module, options);
            }

            var final_modules = new List<Module>(modlist.Values);
            final_modules.AddRange(registry.InstalledModules.Select(x => x.Module));

            if (!options.without_enforce_consistency)
            {
                // Finally, let's do a sanity check that our solution is actually sane.
                SanityChecker.EnforceConsistency(
                    final_modules,
                    registry.InstalledDlls
                );
            }
        }

        /// <summary>
        ///     Returns the default options for relationship resolution.
        /// </summary>
        public static RelationshipResolverOptions DefaultOpts()
        {
            var opts = new RelationshipResolverOptions
            {
                with_recommends = true,
                with_suggests = false,
                with_all_suggests = false
            };

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
            var sub_options = (RelationshipResolverOptions)options.Clone();
            sub_options.with_suggests = false;

            log.DebugFormat("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, sub_options);

            if (options.with_recommends)
            {
                log.DebugFormat("Resolving recommends for {0}", module.identifier);
                ResolveStanza(module.recommends, sub_options, true);
            }

            if (options.with_suggests || options.with_all_suggests)
            {
                log.DebugFormat("Resolving suggests for {0}", module.identifier);
                ResolveStanza(module.suggests, sub_options, true);
            }
        }

        /// <summary>
        /// Resolve a relationship stanza (a list of relationships).
        /// This will add modules to be installed, if required.
        /// May recurse back to Resolve for those new modules.
        /// 
        /// If `soft_resolve` is true, we warn rather than throw exceptions on mods we cannot find.
        /// If `soft_resolve` is false (default), we throw a ModuleNotFoundKraken if we can't find a dependency.
        /// 
        /// Throws a TooManyModsProvideKraken if we have too many choices.
        /// </summary>
        private void ResolveStanza(IEnumerable<RelationshipDescriptor> stanza, RelationshipResolverOptions options, bool soft_resolve = false)
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
                    if (!soft_resolve)
                    {
                        log.ErrorFormat("Dependency on {0} found, but nothing provides it.", dep_name);
                        throw new ModuleNotFoundKraken(dep_name);
                    }
                    log.InfoFormat("{0} is recommended/suggested, but nothing provides it.", dep_name);
                    continue;
                }
                if (candidates.Count > 1)
                {
                    // Oh no, too many to pick from!
                    // TODO: It would be great if instead we picked the one with the
                    // most recommendations.
                    if (options.without_toomanyprovides_kraken)
                    {
                        continue;
                    }

                    throw new TooManyModsProvideKraken(dep_name, candidates);
                }

                CkanModule candidate = candidates[0];

                // Finally, check our candidate against everything which might object
                // to it being installed; that's all the mods which are fixed in our
                // list thus far, as well as everything on the system.

                var fixed_mods =
                    new HashSet<Module>(modlist.Values);

                fixed_mods.UnionWith(registry.InstalledModules.Select(x => x.Module));

                foreach (Module mod in fixed_mods)
                {
                    if (mod.ConflictsWith(candidate))
                    {
                        if (soft_resolve)
                        {
                            log.InfoFormat("{0} would cause conflicts, excluding it from consideration", candidate);

                            // I want labeled loops please, so I don't have to set this to null,
                            // break, and then look at it at the end. o_O
                            candidate = null;
                            break;
                        }
                        var this_is_why_we_cant_have_nice_things = new List<string> {
                            string.Format(
                                "{0} and {1} conflict with each other, yet we require them both!",
                                candidate, mod)
                        };

                        throw new InconsistentKraken(this_is_why_we_cant_have_nice_things);
                    }
                }

                // Our candidate may have been set to null if it was vetoed by our
                // sanity check above.
                if (candidate != null)
                {
                    // Okay, looks like we want this one. Adding.
                    Add(candidate);
                    Resolve(candidate, options);
                }
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
                if (!modlist.ContainsKey(alias))
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