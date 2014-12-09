using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CKAN
{
    // Alas, it appears that structs cannot have defaults. Try
    // DefaultOpts() to get friendly defaults.
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

    public class RelationshipResolver
    {
        // A list of all the mods we're going to install.
        private static readonly ILog log = LogManager.GetLogger(typeof(RelationshipResolver));
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();
        private Registry registry;
        private KSPVersion kspversion;

        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
        public RelationshipResolver(ICollection<string> modules, RelationshipResolverOptions options, Registry registry, KSPVersion kspversion)
        {

            this.registry = registry;
            this.kspversion = kspversion;

            // Start by figuring out what versions we're installing, and then
            // adding them to the list. This *must* be pre-populated with all
            // user-specified modules, as they may be supplying things that provide
            // virtual packages.

            var user_requested_mods = new List<CkanModule>();

            log.DebugFormat("Processing relationships for {0} modules", modules.Count);

            foreach (string module in modules)
            {

                CkanModule mod = registry.LatestAvailable(module, kspversion);
                if (mod == null)
                {
                    throw new ModuleNotFoundKraken(module);
                }

                log.DebugFormat("Preparing to resolve relationships for {0} {1}", mod.identifier, mod.version);

                foreach (CkanModule listed_mod in modlist.Values.Where(listed_mod => listed_mod.ConflictsWith(mod)))
                {
                    throw new InconsistentKraken(string.Format("{0} conflicts with {1}, can't install both.", mod, listed_mod));
                }

                user_requested_mods.Add(mod);
                this.Add(mod);
            }

            // Now that we've already pre-populated modlist, we can resolve
            // the rest of our dependencies.

            foreach (CkanModule module in user_requested_mods)
            {
                log.InfoFormat("Resolving relationships for {0}", module.identifier);
                Resolve(module, options, true);
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


        private readonly ResolveQueue queue = new ResolveQueue();

        /// <summary>
        /// Resolves all relationships for a module.
        /// May recurse to ResolveStanza, which may add additional modules to be installed.
        /// </summary>
        private void Resolve(CkanModule module, RelationshipResolverOptions options, bool head_of_recursion = false)
        {
            // Even though we may resolve top-level suggests for our module,
            // we don't install suggestions all the down unless with_all_suggests
            // is true.
            var sub_options = (RelationshipResolverOptions)options.Clone();
            sub_options.with_suggests = false;

            queue.Add(ResolvePhase.Depends, module);

            if (options.with_recommends)
            {
                queue.Add(ResolvePhase.Recommends, module);
            }

            if (options.with_suggests || options.with_all_suggests)
            {
                queue.Add(ResolvePhase.Suggests, module);
            }

            if (!head_of_recursion) return;

            while (queue.HasNext())
            {
                var pair = queue.Dequeue();
                List<RelationshipDescriptor> relationships;
                switch (pair.Key)
                {
                    case ResolvePhase.Depends:
                        relationships = pair.Value.depends;
                        break;
                    case ResolvePhase.Recommends:
                        relationships = pair.Value.recommends;
                        break;
                    case ResolvePhase.Suggests:
                        relationships = pair.Value.suggests;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                log.DebugFormat("Relsolving {0} for {1}", pair.Key, module.identifier);
                ResolveStanza(relationships, sub_options, pair.Key != ResolvePhase.Depends);
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

            foreach (string dep_name in stanza.Select(dep => dep.name))
            {
                log.DebugFormat("Considering {0}", dep_name);

                // If we already have this dependency covered, skip.
                // If it's already installed, skip.
                if (modlist.ContainsKey(dep_name) || registry.IsInstalled(dep_name))
                {
                    continue;
                }

                List<CkanModule> candidates = registry.LatestAvailableWithProvides(dep_name, kspversion);

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
                    new HashSet<Module>(this.modlist.Values);

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
                        else
                        {
                            throw new InconsistentKraken(new List<string>
                            {
                                string.Format(
                                    "{0} and {1} conflict with each other, yet we require them both!",
                                    candidate, mod)
                            });
                        }
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

            // It's okay if there's already a key for one of our aliases
            // in the resolution list. In which case, we don't do anything.
            var aliases = module.provides.Where(alias => !modlist.ContainsKey(alias));
            foreach (string alias in aliases)
            {
                log.DebugFormat("Adding {0} providing {1}", module.identifier, alias);
                modlist.Add(alias, module);
            }
        }

        private enum ResolvePhase
        {
            Depends, Recommends, Suggests
        }

        private class ResolveQueue
        {
            private readonly Dictionary<ResolvePhase, Queue<Module>> to_be_resolved
                = new Dictionary<ResolvePhase, Queue<Module>>
            {
                {ResolvePhase.Depends, new Queue<Module>()},
                {ResolvePhase.Recommends, new Queue<Module>()},
                {ResolvePhase.Suggests, new Queue<Module>()}
            };

            public void Add(ResolvePhase phase, Module module)
            {
                to_be_resolved[phase].Enqueue(module);
            }

            public bool HasNext()
            {
                return to_be_resolved[ResolvePhase.Depends].Count > 0
                       || to_be_resolved[ResolvePhase.Recommends].Count > 0
                       || to_be_resolved[ResolvePhase.Suggests].Count > 0;
            }
            //Returns in depends,recommends, suggests order.
            public KeyValuePair<ResolvePhase, Module> Dequeue()
            {
                //Contract.Requires(HasNext());
                var phases = Enum.GetValues(typeof(ResolvePhase)).Cast<ResolvePhase>();
                foreach (var phase in phases.Where(phase => to_be_resolved[phase].Count > 0))
                {
                    return new KeyValuePair<ResolvePhase, Module>(phase, to_be_resolved[phase].Dequeue());
                }
                return default(KeyValuePair<ResolvePhase, Module>);
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