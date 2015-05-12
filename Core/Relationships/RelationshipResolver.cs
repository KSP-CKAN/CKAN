using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;

namespace CKAN
{

    // TODO: It would be lovely to get rid of the `without` fields,
    // and replace them with `with` fields. Humans suck at inverting
    // cases in their heads.
    public class RelationshipResolverOptions : ICloneable
    {
        /// <summary>
        /// If true, add recommended mods, and their recommendations.
        /// </summary>
        public bool with_recommends = true;

        /// <summary>
        /// If true, add suggests, but not suggested suggests. :)
        /// </summary>
        public bool with_suggests = false;

        /// <summary>
        /// If true, add suggested modules, and *their* suggested modules, too!
        /// </summary>
        public bool with_all_suggests = false;

        /// <summary>
        /// If true, surpresses the TooManyProvides kraken when resolving
        /// relationships. Otherwise, we just pick the first.
        /// </summary>
        public bool without_toomanyprovides_kraken = false;

        /// <summary>
        /// If true, we skip our sanity check at the end of our relationship
        /// resolution. Note that non-sane resolutions can't actually be
        /// installed, so this is mostly useful for giving the user feedback
        /// on failed resolutions.
        /// </summary>
        public bool without_enforce_consistency = false;

        /// <summary>
        /// If true, we'll populate the `conflicts` field, rather than immediately
        /// throwing a kraken when inconsistencies are detected. Again, these
        /// solutions are non-installable, so mostly of use to provide user
        /// feedback when things go wrong.
        /// </summary>
        public bool procede_with_inconsistencies = false;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    // TODO: RR currently conducts a depth-first resolution of requirements. While we do the
    // right thing in processing all depdenencies first, then recommends, and then suggests,
    // we could find that a recommendation many layers deep prevents a recommendation in the
    // original mod's recommends list.
    //
    // If we resolved in things breadth-first order, we're less likely to encounter surprises
    // where a nth-deep recommend blocks a top-level recommend.

    // TODO: Add mechanism so that clients can add mods with relationshup other than UserAdded.
    // Currently only made to support the with_{} options.
    public class RelationshipResolver
    {
        // A list of all the mods we're going to install.
        private static readonly ILog log = LogManager.GetLogger(typeof (RelationshipResolver));
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();

        private readonly List<KeyValuePair<Module, Module>> conflicts =
            new List<KeyValuePair<Module, Module>>();

        private readonly Dictionary<Module, Relationship> reasons = new Dictionary<Module, Relationship>();
        private readonly Registry registry;
        private readonly KSPVersion kspversion;


        public RelationshipResolver(ICollection<string> moduleNames, RelationshipResolverOptions options, Registry registry,
            KSPVersion kspversion) :
                this(moduleNames.Select(name => CkanModule.FromIDandVersion(registry, name, kspversion)).ToList(),
                    options,
                    registry,
                    kspversion)
        {
            // Does nothing, just calles the other overloaded constructor
        }

        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
        public RelationshipResolver(ICollection<CkanModule> modules, RelationshipResolverOptions options, Registry registry,
            KSPVersion kspversion)
        {
            this.registry = registry;
            this.kspversion = kspversion;

            // Start by figuring out what versions we're installing, and then
            // adding them to the list. This *must* be pre-populated with all
            // user-specified modules, as they may be supplying things that provide
            // virtual packages.

            var user_requested_mods = new List<CkanModule>();

            log.DebugFormat("Processing relationships for {0} modules", modules.Count);

            foreach (CkanModule module in modules)
            {
                log.DebugFormat("Preparing to resolve relationships for {0} {1}", module.identifier, module.version);

                var module1 = module; //Silence a warning re. closures over foreach var.
                foreach (CkanModule listed_mod in modlist.Values.Where(listed_mod => listed_mod.ConflictsWith(module1)))
                {
                    if (options.procede_with_inconsistencies)
                    {
                        conflicts.Add(new KeyValuePair<Module, Module>(listed_mod, module));
                        conflicts.Add(new KeyValuePair<Module, Module>(module, listed_mod));
                    }
                    else
                    {
                        throw new InconsistentKraken(string.Format("{0} conflicts with {1}, can't install both.", module,
                            listed_mod));
                    }
                }

                user_requested_mods.Add(module);
                Add(module, new Relationship.UserRequested());
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

        // TODO: This should just be able to return a new RelationshipResolverOptions
        // and the defaults in the class definition should do the right thing.
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
            var sub_options = (RelationshipResolverOptions) options.Clone();
            sub_options.with_suggests = false;

            log.DebugFormat("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, new Relationship.Depends(module), sub_options);

            if (options.with_recommends)
            {
                log.DebugFormat("Resolving recommends for {0}", module.identifier);
                ResolveStanza(module.recommends, new Relationship.Recommended(module), sub_options, true);
            }

            if (options.with_suggests || options.with_all_suggests)
            {
                log.DebugFormat("Resolving suggests for {0}", module.identifier);
                ResolveStanza(module.suggests, new Relationship.Suggested(module), sub_options, true);
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
        /// Throws a TooManyModsProvideKraken if we have too many choices and
        /// options.without_toomanyprovides_kraken is not set.
        ///
        /// See RelationshipResolverOptions for further adjustments that can be made.
        ///
        /// </summary>

        private void ResolveStanza(IEnumerable<RelationshipDescriptor> stanza, Relationship reason,
            RelationshipResolverOptions options, bool soft_resolve = false)
        {
            if (stanza == null)
            {
                return;
            }

            foreach (var descriptor in stanza)
            {
                string dep_name = descriptor.name;
                log.DebugFormat("Considering {0}", dep_name);

                // If we already have this dependency covered, skip.
                // If it's already installed, skip.

                if (modlist.ContainsKey(dep_name))
                {
                    if (descriptor.version_within_bounds(modlist[dep_name].version))
                        continue;
                    //TODO Ideally we could check here if it can be replaced by the version we want.
                    throw new InconsistentKraken(string.Format("A certain version of {0} is needed. " +
                                                               "However a incompatible version is in the resolver", dep_name));
                }

                if (registry.IsInstalled(dep_name))
                {
                    if(descriptor.version_within_bounds(registry.InstalledVersion(dep_name)))
                    continue;
                    //TODO Ideally we could check here if it can be replaced by the version we want.
                    throw new InconsistentKraken(string.Format("A certain version of {0} is needed. " +
                                                           "However a incompatible version is already installed", dep_name));

                }

                List<CkanModule> candidates = registry.LatestAvailableWithProvides(dep_name, kspversion, descriptor)
                    .Where(mod=>MightBeInstallable(mod)).ToList();

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

                var fixed_mods = new HashSet<Module>(modlist.Values);
                fixed_mods.UnionWith(registry.InstalledModules.Select(x => x.Module));

                var conflicting_mod = fixed_mods.FirstOrDefault(mod => mod.ConflictsWith(candidate));
                if (conflicting_mod == null)
                {
                    // Okay, looks like we want this one. Adding.
                    Add(candidate, reason);
                    Resolve(candidate, options);
                }
                else if (soft_resolve)
                {
                    log.InfoFormat("{0} would cause conflicts, excluding it from consideration", candidate);
                }
                else
                {
                    if (options.procede_with_inconsistencies)
                    {
                        Add(candidate, reason);
                        conflicts.Add(new KeyValuePair<Module, Module>(conflicting_mod, candidate));
                        conflicts.Add(new KeyValuePair<Module, Module>(candidate, conflicting_mod));
                    }
                    else
                    {
                        throw new InconsistentKraken(string.Format("{0} conflicts with {1}, can't install both.", conflicting_mod,
                            candidate));
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified module to the list of modules we're installing.
        /// This also adds its provides list to what we have available.
        /// </summary>
        private void Add(CkanModule module, Relationship reason)
        {
            if (module.IsMetapackage)
                return;

            log.DebugFormat("Adding {0} {1}", module.identifier, module.version);

            if (modlist.ContainsKey(module.identifier))
            {
                // We should never be adding something twice!
                log.ErrorFormat("Assertion failed: Adding {0} twice in relationship resolution", module.identifier);
                throw new ArgumentException("Already contains module:" + module.identifier);
            }
            modlist.Add(module.identifier, module);
            reasons.Add(module, reason);

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

        /// <summary>
        /// Tests that a module might be able to be installed via checking if dependencies
        /// exist for current version.
        /// </summary>
        /// <param name="module">The module to consider</param>
        /// <param name="compatible">For internal use</param>
        /// <returns>If it has dependencies compatible for the current version</returns>
        private bool MightBeInstallable(CkanModule module, List<string> compatible = null)
        {
            if (module.depends == null) return true;
            if (compatible == null)
            {
                compatible = new List<string>();
            }
            else if (compatible.Contains(module.identifier))
            {
                return true;
            }
            //When checking the dependencies we assume that this module is installable
            // in case a dependent depends on it
            compatible.Add(module.identifier);

            var needed = module.depends.Select(depend => registry.LatestAvailableWithProvides(depend.name, kspversion));
            //We need every dependency to have at least one possible module
            var installable = needed.All(need => need.Any(mod => MightBeInstallable(mod, compatible)));
            compatible.Remove(module.identifier);
            return installable;
        }


        /// <summary>
        /// Returns a list of all modules to install to satisfy the changes required.
        /// </summary>
        public List<CkanModule> ModList()
        {
            var modules = new HashSet<CkanModule>(modlist.Values);
            return modules.ToList();
        }

        /// <summary>
        ///  Returns a IList consisting of keyValuePairs containing conflicting mods.
        /// Note: (a,b) in the list should imply that (b,a) is in the list.
        /// </summary>
        public Dictionary<Module, String> ConflictList
        {
            get
            {
                var dict = new Dictionary<Module, String>();
                foreach (var conflict in conflicts)
                {
                    var module = conflict.Key;
                    dict[module] = string.Format("{0} conflicts with {1}\n\n{0}:\n{2}\n{1}:\n{3}",
                        module.identifier, conflict.Value.identifier,
                        ReasonStringFor(module), ReasonStringFor(conflict.Value));
                    ;
                }
                return dict;
            }
        }

        public bool IsConsistant
        {
            get { return conflicts.Any(); }
        }

        internal Relationship ReasonFor(CkanModule mod)
        {
            if (!ModList().Contains(mod))
            {
                throw new ArgumentException("Mod " + mod.StandardName() + " is not in the list");
            }
            return reasons[mod];
        }

        /// <summary>
        /// Displays a user readable string explaining why the mod was chosen.
        /// </summary>
        /// <param name="mod">A Mod in the resolvers modlist. Must not be null</param>
        /// <returns></returns>
        public string ReasonStringFor(Module mod)
        {
            if (mod == null) throw new ArgumentNullException();
            if (!ModList().Contains(mod))
            {
                throw new ArgumentException("Mod " + mod.identifier + " is not in the list");
            }

            var reason = reasons[mod];
            return reason.GetType() == typeof (Relationship.UserRequested)
                ? reason.Reason
                : reason.Reason + ReasonStringFor(reason.Parent);
        }
    }

    /// <summary>
    /// Used to keep track of the relationships between modules in the resolver.
    /// Intended to be used for displaying messages to the user.
    /// </summary>
    internal abstract class Relationship
    {
        //Currently assumed to exist for any relationship other than useradded
        public virtual CkanModule Parent { get; protected set; }
        //Should contain a newline at the end of the string.
        public abstract String Reason { get; }


        public class UserRequested : Relationship
        {
            public override CkanModule Parent
            {
                get
                {
                    Debug.Assert(false);
                    throw new Exception("Should never be called on UserRequested");
                }
            }

            public override string Reason
            {
                get { return "  Was installed or requested by user.\n"; }
            }
        }

        public sealed class Suggested : Relationship
        {
            public Suggested(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  Suggested by " + Parent.identifier + ".\n"; }
            }
        }

        public sealed class Depends : Relationship
        {
            public Depends(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  To satisfy dependancy from " + Parent.identifier + ".\n"; }
            }
        }

        public sealed class Recommended : Relationship
        {
            public Recommended(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  Recommended by " + Parent.identifier + ".\n"; }
            }
        }
    }
}
