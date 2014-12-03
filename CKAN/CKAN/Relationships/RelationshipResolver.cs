using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CKAN
{
    public class RelationshipResolverOptions
    {
        public bool with_all_suggests = false;
        public bool with_recommends = true;
        public bool with_suggests = false;
        public bool without_toomanyprovides_kraken = false;
        public bool without_enforce_consistency = false;
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
        private static readonly ILog log = LogManager.GetLogger(typeof (RelationshipResolver));
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

                Module conflicting_mod = GetAllConflictsWith(mod).FirstOrDefault();
                if (conflicting_mod != null)
                {
                    throw new InconsistentKraken(string.Format("{0} conflicts with {1}, can't install both.", mod, conflicting_mod));
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

            IEnumerable<Module> final_modules = modlist.Values.Concat(registry.InstalledModules.Select(x => x.Module));
            if(!options.without_enforce_consistency)
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
            RelationshipResolverOptions sub_options = options;
            sub_options.with_suggests = false;

            log.DebugFormat("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, sub_options);

            if (options.with_recommends)
            {
                log.DebugFormat("Resolving recommends for {0}", module.identifier);
                ResolveStanza(module.recommends, sub_options, true);
            }
            else if (options.with_suggests || options.with_all_suggests)
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
                    else
                    {
                        log.InfoFormat("{0} is recommended/suggested, but nothing provides it.", dep_name);
                        continue;
                    }
                }
                else if (candidates.Count > 1)
                {
                    // Oh no, too many to pick from!
                    // TODO: It would be great if instead we picked the one with the
                    // most recommendations.
                    if (options.without_toomanyprovides_kraken)
                    {
                        continue;
                    }
                    else
                    {
                        throw new TooManyModsProvideKraken(dep_name, candidates);
                    }                                                            
                }

                CkanModule candidate = candidates[0];

                // Finally, check our candidate against everything which might object
                // to it being installed; that's all the mods which are fixed in our
                // list thus far, as well as everything on the system.

                var fixed_mods = new HashSet<Module>(modlist.Values);

                fixed_mods.UnionWith(registry.InstalledModules.Select(x => x.Module));
                //TODO Write tests that test this. Had Where(mod => mod.ConflictsWith(mod)) without issue.                 
                Module conflicted_mod = fixed_mods.FirstOrDefault(mod => mod.ConflictsWith(candidate));
                if (conflicted_mod != null)
                {
                    if (!soft_resolve)
                    {
                        throw new InconsistentKraken(string.Format(
                            "{0} and {1} conflict with each other, yet we require them both!",
                            candidate, conflicted_mod));
                    }
                    else
                    {
                        log.InfoFormat("{0} would cause conflicts, excluding it from consideration", candidate);                        
                    }
                }
                else
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

            // It's okay if there's already a key for one of our aliases
            // in the resolution list. In which case, we don't do anything.
            foreach (var alias in module.provides.Where(alias => !modlist.ContainsKey(alias)))
            {                
                log.DebugFormat("Adding {0} providing {1}", module.identifier, alias);
                modlist.Add(alias, module);                
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

        /// <summary>
        /// Calculate all conflicting mods between the supplied mod and the list of mods
        /// currently in the relationship provider. 
        /// </summary>
        /// <returns>IEnumerable<Module> of conflicting mods</returns>
        public IEnumerable<Module> GetAllConflictsWith(Module mod)
        {
            log.DebugFormat("Preparing to resolve relationships for {0} {1}", mod.identifier, mod.version);
            return modlist.Values.Where(listedMod => listedMod.ConflictsWith(mod));
        }
    }
}