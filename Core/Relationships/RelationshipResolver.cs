using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using CKAN.Versioning;
using CKAN.Extensions;

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
        public bool proceed_with_inconsistencies = false;

        /// <summary>
        /// If true, then if a module has no versions that are compatible with
        /// the current game version, then we will consider incompatible versions
        /// of that module.
        /// This replaces the former behavior of ignoring compatibility for
        /// `install identifier=version` commands.
        /// </summary>
        public bool allow_incompatible = false;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    // TODO: RR currently conducts a depth-first resolution of requirements. While we do the
    // right thing in processing all dependencies first, then recommends, and then suggests,
    // we could find that a recommendation many layers deep prevents a recommendation in the
    // original mod's recommends list.
    //
    // If we resolved in things breadth-first order, we're less likely to encounter surprises
    // where a nth-deep recommend blocks a top-level recommend.

    // TODO: Add mechanism so that clients can add mods with relationshup other than UserAdded.
    // Currently only made to support the with_{} options.


    /// <summary>
    /// A class used to resolve relationships between mods. Primarily used to satisfy missing dependencies and to check for conflicts on proposed installs.
    /// </summary>
    /// <remarks>
    /// All constructors start with currently installed modules, to remove <see cref="RelationshipResolver.RemoveModsFromInstalledList" />
    /// </remarks>
    public class RelationshipResolver
    {
        // A list of all the mods we're going to install.
        private static readonly ILog log = LogManager.GetLogger(typeof (RelationshipResolver));
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();
        private readonly List<CkanModule> user_requested_mods = new List<CkanModule>();

        //TODO As the conflict detection gets more advanced there is a greater need to have messages in here
        // as recreating them from reasons is no longer possible.
        private readonly List<KeyValuePair<CkanModule, CkanModule>> conflicts =
            new List<KeyValuePair<CkanModule, CkanModule>>();
        private readonly Dictionary<CkanModule, SelectionReason> reasons =
            new Dictionary<CkanModule, SelectionReason>(new NameComparer());

        private readonly IRegistryQuerier registry;
        private readonly GameVersionCriteria GameVersion;
        private readonly RelationshipResolverOptions options;
        private readonly HashSet<CkanModule> installed_modules;

        /// <summary>
        /// Creates a new Relationship resolver.
        /// </summary>
        /// <param name="options">Options for the RelationshipResolver</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="GameVersion">The current KSP version criteria to consider</param>
        public RelationshipResolver(RelationshipResolverOptions options, IRegistryQuerier registry, GameVersionCriteria GameVersion)
        {
            this.registry = registry;
            this.GameVersion = GameVersion;
            this.options = options;

            installed_modules = new HashSet<CkanModule>(registry.InstalledModules.Select(i_module => i_module.Module));
            var installed_relationship = new SelectionReason.Installed();
            foreach (var module in installed_modules)
            {
                reasons.Add(module, installed_relationship);
            }
        }

        /// <summary>
        /// Attempts to convert the identifiers to CkanModules and then calls RelationshipResolver.ctor(IEnumerable{CkanModule}, IEnumerable{CkanModule}, Registry, GameVersion)"/>
        /// </summary>
        /// <param name="modulesToInstall">Identifiers of modules to install, will be converted to CkanModules using CkanModule.FromIDandVersion</param>
        /// <param name="modulesToRemove">Identifiers of modules to remove, will be converted to CkanModules using Registry.InstalledModule</param>
        /// <param name="options">Options for the RelationshipResolver</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="GameVersion">The current KSP version criteria to consider</param>
        public RelationshipResolver(IEnumerable<string> modulesToInstall, IEnumerable<string> modulesToRemove, RelationshipResolverOptions options, IRegistryQuerier registry,
            GameVersionCriteria GameVersion) :
                this(
                    modulesToInstall?.Select(mod => TranslateModule(mod, options, registry, GameVersion)),
                    modulesToRemove?
                        .Select(mod =>
                        {
                            var match = CkanModule.idAndVersionMatcher.Match(mod);
                            return match.Success ? match.Groups["mod"].Value : mod;
                        })
                        .Where(identifier => registry.InstalledModule(identifier) != null)
                        .Select(identifier => registry.InstalledModule(identifier).Module),
                    options, registry, GameVersion)
        {
            // Does nothing, just calls the other overloaded constructor
        }

        /// <summary>
        /// Translate mods from identifiers in its default or identifier=version format into CkanModules,
        /// optionally falling back to incompatible modules if no compatibles could be found.
        /// </summary>
        /// <param name="name">The identifier or identifier=version of the module</param>
        /// <param name="options">If options.allow_incompatible is set, fall back to searching incompatible modules if no compatible has been found</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="GameVersion">The current KSP version criteria to consider</param>
        /// <returns>A CkanModule</returns>
        private static CkanModule TranslateModule(string name, RelationshipResolverOptions options, IRegistryQuerier registry, GameVersionCriteria GameVersion)
        {
            if (options.allow_incompatible)
            {
                try
                {
                    return CkanModule.FromIDandVersion(registry, name, GameVersion);
                }
                catch (ModuleNotFoundKraken)
                {
                    // No versions found matching our game version, so
                    // look for incompatible versions.
                    return CkanModule.FromIDandVersion(registry, name, null);
                }
            }
            else
            {
                return CkanModule.FromIDandVersion(registry, name, GameVersion);
            }
        }

        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
        /// <param name="modulesToInstall">Modules to install</param>
        /// <param name="modulesToRemove">Modules to remove</param>
        /// <param name="options">Options for the RelationshipResolver</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="GameVersion">The current KSP version criteria to consider</param>
        public RelationshipResolver(IEnumerable<CkanModule> modulesToInstall, IEnumerable<CkanModule> modulesToRemove, RelationshipResolverOptions options, IRegistryQuerier registry,
            GameVersionCriteria GameVersion)
            : this(options, registry, GameVersion)
        {
            if (modulesToRemove != null)
            {
                RemoveModsFromInstalledList(modulesToRemove);
            }
            if (modulesToInstall != null)
            {
                AddModulesToInstall(modulesToInstall);
            }
        }

        /// <summary>
        ///     Returns the default options for relationship resolution.
        /// </summary>

        // TODO: This should just be able to return a new RelationshipResolverOptions
        // and the defaults in the class definition should do the right thing.
        public static RelationshipResolverOptions DefaultOpts()
        {
            return new RelationshipResolverOptions
            {
                with_recommends   = true,
                with_suggests     = false,
                with_all_suggests = false
            };
        }

        /// <summary>
        /// Options to install without recommendations.
        /// </summary>
        public static RelationshipResolverOptions DependsOnlyOpts()
        {
            return new RelationshipResolverOptions
            {
                with_recommends   = false,
                with_suggests     = false,
                with_all_suggests = false
            };
        }

        /// <summary>
        /// Add modules to consideration of the relationship resolver.
        /// </summary>
        /// <param name="modules">Modules to attempt to install</param>
        private void AddModulesToInstall(IEnumerable<CkanModule> modules)
        {
            //Count may need to do a full enumeration. Might as well convert to array
            var ckan_modules = modules as CkanModule[] ?? modules.ToArray();
            log.DebugFormat("Processing relationships for {0} modules", ckan_modules.Count());

            // Start by figuring out what versions we're installing, and then
            // adding them to the list. This *must* be pre-populated with all
            // user-specified modules, as they may be supplying things that provide
            // virtual packages.
            foreach (CkanModule module in ckan_modules)
            {
                log.DebugFormat("Preparing to resolve relationships for {0} {1}", module.identifier, module.version);

                //Need to check against installed mods and those to install.
                var mods = modlist.Values.Concat(installed_modules).Where(listed_mod => listed_mod.ConflictsWith(module));
                foreach (CkanModule listed_mod in mods)
                {
                    if (options.proceed_with_inconsistencies)
                    {
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(listed_mod, module));
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(module, listed_mod));
                    }
                    else
                    {
                        throw new InconsistentKraken(
                            $"{module} conflicts with {listed_mod}");
                    }
                }

                user_requested_mods.Add(module);
                Add(module, new SelectionReason.UserRequested());
            }

            // Now that we've already pre-populated the modlist, we can resolve
            // the rest of our dependencies.

            foreach (var module in user_requested_mods)
            {
                log.DebugFormat("Resolving relationships for {0}", module.identifier);
                Resolve(module, options);
            }

            try
            {
                // Finally, let's do a sanity check that our solution is actually sane.
                SanityChecker.EnforceConsistency(
                    modlist.Values.Concat(installed_modules),
                    registry.InstalledDlls,
                    registry.InstalledDlc
                );
            }
            catch (BadRelationshipsKraken k)
            {
                // Add to this.conflicts (catches conflicting DLLs and DLCs that the above loops miss)
                foreach (var kvp in k.Conflicts)
                {
                    conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(
                        kvp.Key, null
                    ));
                }
                if (!options.without_enforce_consistency)
                {
                    // Only re-throw if caller asked for consistency enforcement
                    throw;
                }
            }
        }

        /// <summary>
        /// Removes mods from the list of installed modules. Intended to be used for cases
        /// in which the mod is to be un-installed.
        /// </summary>
        /// <param name="mods">The mods to remove.</param>
        private void RemoveModsFromInstalledList(IEnumerable<CkanModule> mods)
        {
            foreach (var module in mods)
            {
                installed_modules.Remove(module);
                conflicts.RemoveAll(kvp => kvp.Key.Equals(module) || kvp.Value.Equals(module));
            }
        }

        /// <summary>
        /// Resolves all relationships for a module.
        /// May recurse to ResolveStanza, which may add additional modules to be installed.
        /// </summary>
        private void Resolve(CkanModule module, RelationshipResolverOptions options, IEnumerable<RelationshipDescriptor> old_stanza = null)
        {
            // Even though we may resolve top-level suggests for our module,
            // we don't install suggestions all the down unless with_all_suggests
            // is true.
            var sub_options = (RelationshipResolverOptions) options.Clone();
            sub_options.with_suggests = false;

            old_stanza = old_stanza?.Memoize();

            log.DebugFormat("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, new SelectionReason.Depends(module), sub_options, false, old_stanza);

            if (options.with_recommends)
            {
                log.DebugFormat("Resolving recommends for {0}", module.identifier);
                ResolveStanza(module.recommends, new SelectionReason.Recommended(module), sub_options, true, old_stanza);
            }

            if (options.with_suggests || options.with_all_suggests)
            {
                log.DebugFormat("Resolving suggests for {0}", module.identifier);
                ResolveStanza(module.suggests, new SelectionReason.Suggested(module), sub_options, true, old_stanza);
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
        /// </summary>
        private void ResolveStanza(IEnumerable<RelationshipDescriptor> stanza, SelectionReason reason,
            RelationshipResolverOptions options, bool soft_resolve = false, IEnumerable<RelationshipDescriptor> old_stanza = null)
        {
            if (stanza == null)
            {
                return;
            }
            stanza = stanza.Memoize();

            foreach (RelationshipDescriptor descriptor in stanza)
            {
                log.DebugFormat("Considering {0}", descriptor.ToString());

                // If we already have this dependency covered, skip.
                if (descriptor.MatchesAny(modlist.Values, null, null))
                {
                    continue;
                }
                else if (descriptor.ContainsAny(modlist.Keys))
                {
                    CkanModule module = modlist.Values
                        .FirstOrDefault(m => descriptor.ContainsAny(new string[] { m.identifier }));
                    if (options.proceed_with_inconsistencies)
                    {
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(module, reason.Parent));
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(reason.Parent, module));
                        continue;
                    }
                    else
                    {
                        throw new InconsistentKraken(
                            $"{descriptor} required, but an incompatible version is in the resolver"
                        );
                    }
                }

                // If it's already installed, skip.
                if (descriptor.MatchesAny(
                    installed_modules,
                    registry.InstalledDlls.ToHashSet(),
                    registry.InstalledDlc))
                {
                    continue;
                }
                else if (descriptor.ContainsAny(installed_modules.Select(im => im.identifier)))
                {
                    CkanModule module = installed_modules
                        .FirstOrDefault(m => descriptor.ContainsAny(new string[] { m.identifier }));
                    if (options.proceed_with_inconsistencies)
                    {
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(module, reason.Parent));
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(reason.Parent, module));
                        continue;
                    }
                    else
                    {
                        throw new InconsistentKraken(
                            $"{descriptor} required, but an incompatible version is installed"
                        );
                    }
                }

                // Pass mod list in case an older version of a module is conflict-free while later versions have conflicts
                var descriptor1 = descriptor;
                List<CkanModule> candidates = descriptor
                    .LatestAvailableWithProvides(registry, GameVersion, modlist.Values)
                    .Where(mod => !modlist.ContainsKey(mod.identifier)
                        && descriptor1.WithinBounds(mod)
                        && MightBeInstallable(mod))
                    .ToList();
                if (candidates.Count == 0)
                {
                    // Nothing found, try again without mod list
                    // (conflicts will still be caught below)
                    candidates = descriptor
                        .LatestAvailableWithProvides(registry, GameVersion)
                        .Where(mod => !modlist.ContainsKey(mod.identifier)
                            && descriptor1.WithinBounds(mod)
                            && MightBeInstallable(mod))
                        .ToList();
                }

                if (candidates.Count == 0)
                {
                    if (!soft_resolve)
                    {
                        log.InfoFormat("Dependency on {0} found but it is not listed in the index, or not available for your version of KSP.", descriptor.ToString());
                        throw new DependencyNotSatisfiedKraken(reason.Parent, descriptor.ToString());
                    }
                    log.InfoFormat("{0} is recommended/suggested but it is not listed in the index, or not available for your version of KSP.", descriptor.ToString());
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

                    // If we've got a parent stanza that has a relationship on a mod that provides what
                    // we need, then select that.
                    if (old_stanza != null)
                    {
                        List<CkanModule> provide = candidates
                            .Where(cand => old_stanza.Any(rel => rel.WithinBounds(cand)))
                            .ToList();
                        if (!provide.Any() || provide.Count() > 1)
                        {
                            //We still have either nothing, or too many to pick from
                            //Just throw the TMP now
                            throw new TooManyModsProvideKraken(descriptor.ToString(), candidates);
                        }
                        candidates[0] = provide.First();
                    }
                    else
                    {
                        throw new TooManyModsProvideKraken(descriptor.ToString(), candidates);
                    }
                }

                CkanModule candidate = candidates[0];

                // Finally, check our candidate against everything which might object
                // to it being installed; that's all the mods which are fixed in our
                // list thus far, as well as everything on the system.

                var fixed_mods = new HashSet<CkanModule>(modlist.Values);
                fixed_mods.UnionWith(installed_modules);

                CkanModule conflicting_mod = fixed_mods.FirstOrDefault(mod => mod.ConflictsWith(candidate));
                if (conflicting_mod == null)
                {
                    // Okay, looks like we want this one. Adding.
                    Add(candidate, reason);
                    Resolve(candidate, options, stanza);
                }
                else if (soft_resolve)
                {
                    log.InfoFormat("{0} would cause conflicts, excluding it from consideration", candidate);
                }
                else
                {
                    if (options.proceed_with_inconsistencies)
                    {
                        Add(candidate, reason);
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(conflicting_mod, candidate));
                        conflicts.Add(new KeyValuePair<CkanModule, CkanModule>(candidate, conflicting_mod));
                    }
                    else
                    {
                        throw new InconsistentKraken(
                            $"{conflicting_mod} conflicts with {candidate}");
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified module to the list of modules we're installing.
        /// This also adds its provides list to what we have available.
        /// </summary>
        private void Add(CkanModule module, SelectionReason reason)
        {
            if (module.IsMetapackage)
                return;
            if (module.IsDLC)
            {
                throw new ModuleIsDLCKraken(module);
            }

            log.DebugFormat("Adding {0} {1}", module.identifier, module.version);

            if (modlist.ContainsKey(module.identifier))
            {
                // We should never be adding something twice!
                log.ErrorFormat("Assertion failed: Adding {0} twice in relationship resolution", module.identifier);
                throw new ArgumentException("Already contains module: " + module.identifier);
            }
            modlist.Add(module.identifier, module);
            if (!reasons.ContainsKey(module))
                reasons.Add(module, reason);
            // Override Installed for upgrades
            else if (reasons[module] is SelectionReason.Installed)
                reasons[module] = reason;

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
            if (module.IsDLC)
                return false;
            if (module.depends == null)
                return true;
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

            // Get list of lists of dependency choices
            var needed = module.depends
                // Skip dependencies satisfied by installed modules
                .Where(depend => !depend.MatchesAny(installed_modules, null, null))
                .Select(depend => depend.LatestAvailableWithProvides(registry, GameVersion));

            log.DebugFormat("Trying to satisfy: {0}",
                string.Join("; ", needed.Select(need =>
                    string.Join(", ", need.Select(mod => mod.identifier)))));

            //We need every dependency to have at least one possible module
            var installable = needed.All(need => need.Any(mod => MightBeInstallable(mod, compatible)));
            compatible.Remove(module.identifier);
            return installable;
        }


        /// <summary>
        /// Returns a list of all modules to install to satisfy the changes required.
        /// </summary>
        public IEnumerable<CkanModule> ModList()
        {
            return new HashSet<CkanModule>(modlist.Values);
        }

        /// <summary>
        /// Returns a dictionary consisting of keyValuePairs containing conflicting mods.
        /// </summary>
        public Dictionary<CkanModule, String> ConflictList
        {
            get
            {
                return conflicts
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(kvp => kvp.Value)
                                      .Where(v => v != null)
                                      .Distinct())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => $"{kvp.Key} conflicts with " + (
                            kvp.Value.Count() == 0
                                ? "an unmanaged DLL or DLC"
                                : string.Join(", ", kvp.Value)));
            }
        }

        public bool IsConsistent
        {
            get { return !conflicts.Any(); }
        }

        /// <summary>
        /// Displays a user readable string explaining why the mod was chosen.
        /// </summary>
        /// <param name="mod">A Mod in the resolvers modlist. Must not be null</param>
        /// <returns></returns>
        public string ReasonStringFor(CkanModule mod)
        {
            if (mod == null)
            {
                // If we don't have a CkanModule, it must be a DLL or DLC
                return "Unmanaged";
            }
            var reason = ReasonFor(mod);
            var is_root_type = reason.GetType() == typeof(SelectionReason.UserRequested)
                || reason.GetType() == typeof(SelectionReason.Installed);
            return is_root_type
                ? reason.Reason
                : reason.Reason + ReasonStringFor(reason.Parent);
        }

        public SelectionReason ReasonFor(CkanModule mod)
        {
            if (mod == null) throw new ArgumentNullException();
            if (!reasons.ContainsKey(mod) && !ModList().Contains(mod))
            {
                throw new ArgumentException("Mod " + mod.identifier + " is not in the list");
            }

            return reasons[mod];
        }

        /// <summary>
        /// Indicates whether a module should be considered auto-installed in this change set.
        /// A mod is auto-installed if it is in the list because it's a dependency
        /// and if its depending mod is not a metpaackage.
        /// </summary>
        /// <param name="mod">Module to check</param>
        /// <returns>
        /// true if auto-installed, false otherwise
        /// </returns>
        public bool IsAutoInstalled(CkanModule mod)
        {
            var reason = ReasonFor(mod);
            return reason is SelectionReason.Depends && !reason.Parent.IsMetapackage;
        }
    }

    /// <summary>
    /// Used to keep track of the relationships between modules in the resolver.
    /// Intended to be used for displaying messages to the user.
    /// </summary>
    public abstract class SelectionReason
    {
        //Currently assumed to exist for any relationship other than useradded or installed
        public virtual CkanModule Parent { get; protected set; }
        //Should contain a newline at the end of the string.
        public abstract String Reason { get; }


        public class Installed : SelectionReason
        {
            public override CkanModule Parent
            {
                get
                {
                    Debug.Assert(false);
                    throw new Exception("Should never be called on Installed");
                }
            }

            public override string Reason
            {
                get { return "  Currently installed.\r\n"; }
            }
        }

        public class UserRequested : SelectionReason
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
                get { return "  Requested by user.\r\n"; }
            }
        }

        public class NoLongerUsed: SelectionReason
        {
            public override string Reason
            {
                get { return "  Auto-installed, depending modules removed.\r\n"; }
            }
        }

        public class Replacement : SelectionReason
        {
            public Replacement(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  Replacing " + Parent.name + ".\r\n"; }
            }
        }

        public sealed class Suggested : SelectionReason
        {
            public Suggested(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  Suggested by " + Parent.name + ".\r\n"; }
            }
        }

        public sealed class Depends : SelectionReason
        {
            public Depends(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  To satisfy dependency from " + Parent.name + ".\r\n"; }
            }
        }

        public sealed class Recommended : SelectionReason
        {
            public Recommended(CkanModule module)
            {
                if (module == null) throw new ArgumentNullException();
                Parent = module;
            }

            public override string Reason
            {
                get { return "  Recommended by " + Parent.name + ".\r\n"; }
            }
        }
    }
}
