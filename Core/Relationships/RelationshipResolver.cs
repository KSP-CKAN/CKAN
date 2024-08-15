using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN
{
    using ModPair = KeyValuePair<CkanModule, CkanModule>;

    /// <summary>
    /// Resolves relationships between mods. Primarily used to satisfy missing dependencies and to check for conflicts on proposed installs.
    /// </summary>
    /// <remarks>
    /// All constructors start with currently installed modules, to remove <see cref="RemoveModsFromInstalledList" />
    /// </remarks>
    public class RelationshipResolver
    {
        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
        /// <param name="modulesToInstall">Modules to install</param>
        /// <param name="modulesToRemove">Modules to remove</param>
        /// <param name="options">Options for the RelationshipResolver</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="versionCrit">The current KSP version criteria to consider</param>
        public RelationshipResolver(IEnumerable<CkanModule>     modulesToInstall,
                                    IEnumerable<CkanModule>     modulesToRemove,
                                    RelationshipResolverOptions options,
                                    IRegistryQuerier            registry,
                                    GameVersionCriteria         versionCrit)
            : this(options, registry, versionCrit)
        {
            if (modulesToRemove != null)
            {
                RemoveModsFromInstalledList(modulesToRemove);
            }
            if (modulesToInstall != null)
            {
                AddModulesToInstall(modulesToInstall.ToArray());
            }
        }

        /// <summary>
        /// Creates a new Relationship resolver.
        /// </summary>
        /// <param name="options">Options for the RelationshipResolver</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="versionCrit">The current KSP version criteria to consider</param>
        private RelationshipResolver(RelationshipResolverOptions options,
                                     IRegistryQuerier            registry,
                                     GameVersionCriteria         versionCrit)
        {
            this.options     = options;
            this.registry    = registry;
            this.versionCrit = versionCrit;

            installed_modules = registry.InstalledModules
                                        .Select(i_module => i_module.Module)
                                        .ToHashSet();
            var installed_relationship = new SelectionReason.Installed();
            foreach (var module in installed_modules)
            {
                AddReason(module, installed_relationship);
            }
        }

        /// <summary>
        /// Add modules to consideration of the relationship resolver.
        /// </summary>
        /// <param name="modules">Modules to attempt to install</param>
        private void AddModulesToInstall(CkanModule[] modules)
        {
            log.DebugFormat("Processing relationships for {0} modules", modules.Length);

            // Start by figuring out what versions we're installing, and then
            // adding them to the list. This *must* be pre-populated with all
            // user-specified modules, as they may be supplying things that provide
            // virtual packages.

            // This part can't be parallel, to preserve order of processing
            foreach (CkanModule module in modules)
            {
                log.DebugFormat("Preparing to resolve relationships for {0} {1}", module.identifier, module.version);

                // Need to check against installed mods and those to install.
                var conflictingModules = modlist.Values
                                                .Concat(installed_modules)
                                                .Where(listed_mod => listed_mod.ConflictsWith(module));
                foreach (CkanModule listed_mod in conflictingModules)
                {
                    if (options.proceed_with_inconsistencies)
                    {
                        conflicts.Add(new ModPair(listed_mod, module));
                        conflicts.Add(new ModPair(module, listed_mod));
                    }
                    else
                    {
                        throw new InconsistentKraken(string.Format(
                            Properties.Resources.RelationshipResolverConflictsWith,
                            module, listed_mod));
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
                // Check that our solution is actually sane
                SanityChecker.EnforceConsistency(modlist.Values.Concat(installed_modules),
                                                 registry.InstalledDlls,
                                                 registry.InstalledDlc);
            }
            catch (BadRelationshipsKraken k) when (options.without_enforce_consistency)
            {
                conflicts.AddRange(k.Conflicts.Select(kvp => new ModPair(kvp.Item1, kvp.Item3))
                                              .Where(kvp => !conflicts.Contains(kvp))
                                              .ToArray());
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
        private void Resolve(CkanModule                          module,
                             RelationshipResolverOptions         options,
                             IEnumerable<RelationshipDescriptor> old_stanza = null)
        {
            if (alreadyResolved.Contains(module))
            {
                return;
            }
            else
            {
                // Mark this module as resolved so we don't recurse here again
                alreadyResolved.Add(module);
            }

            old_stanza = old_stanza?.Memoize();

            log.DebugFormat("Resolving dependencies for {0}", module.identifier);
            ResolveStanza(module.depends, new SelectionReason.Depends(module), options, false, old_stanza);

            // TODO: RR currently conducts a depth-first resolution of requirements. While we do the
            // right thing in processing all dependencies first, then recommends, and then suggests,
            // we could find that a recommendation many layers deep prevents a recommendation in the
            // original mod's recommends list.
            //
            // If we resolved in things breadth-first order, we're less likely to encounter surprises
            // where a nth-deep recommend blocks a top-level recommend.

            if (options.with_recommends)
            {
                log.DebugFormat("Resolving recommends for {0}", module.identifier);
                ResolveStanza(module.recommends, new SelectionReason.Recommended(module, 0),
                              options.get_recommenders ? options.WithoutRecommendations()
                                                       : options.WithoutSuggestions(),
                              true, old_stanza);
            }

            if (options.with_suggests || options.with_all_suggests)
            {
                log.DebugFormat("Resolving suggests for {0}", module.identifier);
                ResolveStanza(module.suggests, new SelectionReason.Suggested(module),
                              options.get_recommenders ? options.WithoutRecommendations()
                                                       : options.WithoutSuggestions(),
                              true, old_stanza);
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
        private void ResolveStanza(List<RelationshipDescriptor>        stanza,
                                   SelectionReason                     reason,
                                   RelationshipResolverOptions         options,
                                   bool                                soft_resolve = false,
                                   IEnumerable<RelationshipDescriptor> old_stanza   = null)
        {
            if (stanza == null)
            {
                return;
            }

            var orig_options = options;
            foreach (RelationshipDescriptor descriptor in stanza)
            {
                log.DebugFormat("Considering {0}", descriptor.ToString());

                if (options.get_recommenders && descriptor.suppress_recommendations)
                {
                    log.DebugFormat("Skipping {0} because get_recommenders option is set", descriptor.ToString());
                    suppressedRecommenders.Add(descriptor);
                    continue;
                }
                options = orig_options.OptionsFor(descriptor);

                // If we already have this dependency covered,
                // resolve its relationships if we haven't already.
                if (descriptor.MatchesAny(modlist.Values, null, null, out CkanModule installingCandidate))
                {
                    if (installingCandidate != null)
                    {
                        log.DebugFormat("Match already in changeset: {0}, adding reason {1}", installingCandidate, reason);
                        // Resolve the relationships of the matching module here
                        // because that's when it would happen if non-virtual
                        AddReason(installingCandidate, reason);
                        Resolve(installingCandidate, options, stanza);
                    }
                    // If null, it's a DLL or DLC, which we can't resolve
                    continue;
                }
                else if (descriptor.ContainsAny(modlist.Keys))
                {
                    // Two installing mods depend on different versions of this dependency
                    CkanModule module = modlist.Values.FirstOrDefault(m => descriptor.ContainsAny(new string[] { m.identifier }));
                    if (options.proceed_with_inconsistencies)
                    {
                        conflicts.Add(new ModPair(module, reason.Parent));
                        conflicts.Add(new ModPair(reason.Parent, module));
                        continue;
                    }
                    else
                    {
                        throw new InconsistentKraken(string.Format(
                            Properties.Resources.RelationshipResolverRequiredButResolver,
                            descriptor));
                    }
                }

                // If it's already installed, skip.
                if (descriptor.MatchesAny(installed_modules,
                                          registry.InstalledDlls.ToHashSet(),
                                          registry.InstalledDlc))
                {
                    continue;
                }
                else if (descriptor.ContainsAny(installed_modules.Select(im => im.identifier)))
                {
                    // We need a different version of the mod than is already installed
                    CkanModule module = installed_modules.FirstOrDefault(m => descriptor.ContainsAny(new string[] { m.identifier }));
                    if (options.proceed_with_inconsistencies)
                    {
                        conflicts.Add(new ModPair(module, reason.Parent));
                        conflicts.Add(new ModPair(reason.Parent, module));
                        continue;
                    }
                    else
                    {
                        throw new InconsistentKraken(string.Format(
                            Properties.Resources.RelationshipResolverRequiredButInstalled,
                            descriptor));
                    }
                }

                // Pass mod list in case an older version of a module is conflict-free while later versions have conflicts
                var descriptor1 = descriptor;
                List<CkanModule> candidates = descriptor
                    .LatestAvailableWithProvides(registry, versionCrit, installed_modules, modlist.Values)
                    .Where(mod => !modlist.ContainsKey(mod.identifier)
                                  && descriptor1.WithinBounds(mod)
                                  && MightBeInstallable(mod, reason.Parent, installed_modules))
                    .ToList();
                if (!candidates.Any())
                {
                    // Nothing found, try again while simulating an empty mod list
                    // Necessary for e.g. proceed_with_inconsistencies, conflicts will still be caught below
                    candidates = descriptor
                        .LatestAvailableWithProvides(registry, versionCrit, Array.Empty<CkanModule>())
                        .Where(mod => !modlist.ContainsKey(mod.identifier)
                                      && descriptor1.WithinBounds(mod)
                                      && MightBeInstallable(mod))
                        .ToList();
                }

                if (!candidates.Any())
                {
                    if (!soft_resolve)
                    {
                        log.InfoFormat("Dependency on {0} found but it is not listed in the index, or not available for your game version.", descriptor.ToString());
                        throw new DependencyNotSatisfiedKraken(reason.Parent, descriptor.ToString());
                    }
                    log.InfoFormat("{0} is recommended/suggested but it is not listed in the index, or not available for your game version.", descriptor.ToString());
                    continue;
                }
                if (candidates.Count > 1)
                {
                    // Oh no, too many to pick from!
                    if (options.without_toomanyprovides_kraken)
                    {
                        if (options.get_recommenders && !(reason is SelectionReason.Depends))
                        {
                            for (int i = 0; i < candidates.Count; ++i)
                            {
                                Add(candidates[i], reason is SelectionReason.Recommended rec
                                                       ? rec.WithIndex(i)
                                                       : reason);
                            }
                        }
                        continue;
                    }

                    // If we've got a parent stanza that has a relationship on a mod that provides what
                    // we need, then select that.
                    if (old_stanza != null)
                    {
                        List<CkanModule> provide = candidates
                            .Where(cand => old_stanza.Any(rel => rel.WithinBounds(cand)))
                            .ToList();
                        if (provide.Count != 1)
                        {
                            // We still have either nothing, or too many to pick from
                            // Just throw the TMP now
                            throw new TooManyModsProvideKraken(reason.Parent, descriptor.ToString(),
                                                               candidates, descriptor.choice_help_text);
                        }
                        candidates[0] = provide.First();
                    }
                    else
                    {
                        throw new TooManyModsProvideKraken(reason.Parent, descriptor.ToString(),
                                                           candidates, descriptor.choice_help_text);
                    }
                }

                CkanModule candidate = candidates[0];

                // Finally, check our candidate against everything which might object
                // to it being installed; that's all the mods which are fixed in our
                // list thus far, as well as everything on the system.
                var fixed_mods = modlist.Values.Concat(installed_modules).ToHashSet();

                var conflicting_mod = fixed_mods.FirstOrDefault(mod => mod.ConflictsWith(candidate));
                if (conflicting_mod == null)
                {
                    // Okay, looks like we want this one. Adding.
                    Add(candidate, reason);
                    Resolve(candidate, options, stanza);
                }
                else if (options.proceed_with_inconsistencies)
                {
                    Add(candidate, reason);
                    conflicts.Add(new ModPair(conflicting_mod, candidate));
                    conflicts.Add(new ModPair(candidate, conflicting_mod));
                }
                else if (soft_resolve)
                {
                    log.InfoFormat("{0} would cause conflicts, excluding it from consideration", candidate);
                }
                else
                {
                    throw new InconsistentKraken(string.Format(
                        Properties.Resources.RelationshipResolverConflictsWith,
                        conflictingModDescription(conflicting_mod, null),
                        conflictingModDescription(candidate, reason.Parent)));
                }
            }
        }

        private string conflictingModDescription(CkanModule mod, CkanModule parent)
            => mod == null
                ? Properties.Resources.RelationshipResolverAnUnmanaged
                : parent == null && ReasonsFor(mod).Any(r => r is SelectionReason.UserRequested
                                                             || r is SelectionReason.Installed)
                    // No parenthetical needed if it's user requested
                    ? mod.ToString()
                    // Explain why we're looking at this mod
                    : string.Format(Properties.Resources.RelationshipResolverConflictingModDescription,
                                    mod.ToString(),
                                    string.Join(", ", UserReasonsFor(parent ?? mod).Select(m => m.ToString())));

        /// <summary>
        /// Adds the specified module to the list of modules we're installing.
        /// This also adds its provides list to what we have available.
        /// </summary>
        private void Add(CkanModule module, SelectionReason reason)
        {
            if (module.IsMetapackage)
            {
                AddReason(module, reason);
                return;
            }
            if (module.IsDLC)
            {
                throw new ModuleIsDLCKraken(module);
            }

            log.DebugFormat("Adding {0} {1}", module.identifier, module.version);

            if (modlist.TryGetValue(module.identifier, out CkanModule possibleDup))
            {
                if (possibleDup.identifier == module.identifier)
                {
                    // We should never add the same module twice!
                    log.ErrorFormat("Assertion failed: Adding {0} twice in relationship resolution", module.identifier);
                    throw new ArgumentException("Already contains module: " + module.identifier);
                }
                else
                {
                    // Duplicates via "provides" are OK though, we'll just replace it
                    modlist.Remove(module.identifier);
                }
            }
            modlist.Add(module.identifier, module);
            AddReason(module, reason);

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

        private bool MightBeInstallable(CkanModule              module,
                                        CkanModule              stanzaSource = null,
                                        ICollection<CkanModule> installed    = null)
            => MightBeInstallable(module, stanzaSource,
                                  installed ?? new List<CkanModule>(),
                                  new List<string>());

        /// <summary>
        /// Tests that a module might be able to be installed via checking if dependencies
        /// exist for current version.
        /// </summary>
        /// <param name="module">The module to consider</param>
        /// <param name="stanzaSource">The source of the relationship stanza we're investigating the candidate for</param>
        /// <param name="installed">The list of installed modules in the current resolver state</param>
        /// <param name="compatible">For internal use</param>
        /// <returns>Whether its dependencies are compatible with the current game version</returns>
        private bool MightBeInstallable(CkanModule              module,
                                        CkanModule              stanzaSource,
                                        ICollection<CkanModule> installed,
                                        List<string>            parentCompat)
        {
            if (module.IsDLC)
            {
                return false;
            }
            if (module.depends == null)
            {
                return true;
            }

            // When checking the dependencies we assume that this module is installable
            // in case a dependent depends on it
            var compatible = parentCompat.Append(module.identifier).ToList();
            var dlls       = registry.InstalledDlls.ToHashSet();
            var dlcs       = registry.InstalledDlc;

            var toInstall = stanzaSource != null
                                ? new List<CkanModule> { stanzaSource }
                                : null;

            // Note, .AsParallel() breaks this, too many threads for recursion
            return (parentCompat.Count > 0 ? (IEnumerable<RelationshipDescriptor>)module.depends
                                                  : module.depends.AsParallel())
                // Skip dependencies satisfied by installed modules
                .Where(rel => !rel.MatchesAny(installed, dlls, dlcs))
                // ... or by modules that are about to be installed
                .Select(rel => rel.LatestAvailableWithProvides(registry, versionCrit,
                                                               installed, toInstall))
                // We need every dependency to have at least one possible module
                .All(need => need.Any(mod => compatible.Contains(mod.identifier)
                                             || MightBeInstallable(mod, stanzaSource,
                                                                   installed, compatible)));
        }

        /// <summary>
        /// Returns a list of all modules to install to satisfy the changes required.
        /// Each mod is after its dependencies and before its reverse dependencies.
        /// </summary>
        public IEnumerable<CkanModule> ModList(bool parallel = true)
            => modlist.Values
                      .Distinct()
                      .AsParallelIf(parallel)
                      // Put user choices at the bottom; .OrderBy(bool) -> false first
                      .OrderBy(m => ReasonsFor(m).Any(r => r is SelectionReason.UserRequested))
                      // Put dependencies before dependers
                      .ThenByDescending(totalDependers)
                      // Resolve ties in name order
                      .ThenBy(m => m.name);

        // The more nodes of the reverse-dependency graph we can paint, the higher up in the list it goes
        private int totalDependers(CkanModule module)
            => allDependers(module).Count();

        private static bool AnyRelationship(SelectionReason r)
            => r is SelectionReason.Depends
                || r is SelectionReason.Recommended
                || r is SelectionReason.Suggested;

        private static IEnumerable<T> BreadthFirstSearch<T>(IEnumerable<T>                      startingGroup,
                                                            Func<T, HashSet<T>, IEnumerable<T>> getNextGroup)
        {
            var found    = startingGroup.ToHashSet();
            var toSearch = new Queue<T>(found);

            while (toSearch.Count > 0)
            {
                var searching = toSearch.Dequeue();
                yield return searching;
                foreach (var nextNode in getNextGroup(searching, found))
                {
                    found.Add(nextNode);
                    toSearch.Enqueue(nextNode);
                }
            }
        }

        private IEnumerable<CkanModule> allDependers(CkanModule                  module,
                                                     Func<SelectionReason, bool> followReason = null)
            => BreadthFirstSearch(Enumerable.Repeat(module, 1),
                                  (searching, found) =>
                                      ReasonsFor(searching).Where(followReason ?? AnyRelationship)
                                                           .Select(r => r.Parent)
                                                           .Except(found));

        public IEnumerable<CkanModule> Dependencies()
            => BreadthFirstSearch(user_requested_mods,
                                  (searching, found) =>
                                      modlist.Values
                                             .Except(found)
                                             .Where(m => ReasonsFor(m).Any(r => r is SelectionReason.Depends
                                                                                && r.Parent == searching)));

        public IEnumerable<CkanModule> Recommendations(HashSet<CkanModule> dependencies)
            => modlist.Values.Except(dependencies)
                             .Where(m => ValidRecSugReasons(dependencies,
                                                            ReasonsFor(m).Where(r => r is SelectionReason.Recommended)
                                                                         .ToList()))
                             .OrderByDescending(totalDependers);

        public IEnumerable<CkanModule> Suggestions(HashSet<CkanModule> dependencies,
                                                   List<CkanModule>    recommendations)
            => modlist.Values.Except(dependencies)
                             .Except(recommendations)
                             .Where(m => ValidRecSugReasons(dependencies,
                                                            ReasonsFor(m).Where(r => r is SelectionReason.Suggested)
                                                                         .ToList()))
                             .OrderByDescending(totalDependers);

        private bool ValidRecSugReasons(HashSet<CkanModule>   dependencies,
                                        List<SelectionReason> recSugReasons)
            => recSugReasons.Any(r => dependencies.Contains(r.Parent))
               && !suppressedRecommenders.Any(rel => recSugReasons.Any(r => rel.WithinBounds(r.Parent)));

        public ParallelQuery<KeyValuePair<CkanModule, HashSet<string>>> Supporters(
            HashSet<CkanModule>     supported,
            IEnumerable<CkanModule> toExclude)
            => registry.CompatibleModules(versionCrit)
                       .Except(toExclude)
                       .AsParallel()
                       // Find installable modules with "supports" relationships
                       .Where(mod => !registry.IsInstalled(mod.identifier)
                                     && mod.supports != null)
                       // Find each module that "supports" something we're installing
                       .Select(mod => new KeyValuePair<CkanModule, HashSet<string>>(
                                          mod,
                                          mod.supports
                                             .Where(rel => rel.MatchesAny(supported, null, null))
                                             .Select(rel => (rel as ModuleRelationshipDescriptor)?.name)
                                             .Where(name => !string.IsNullOrEmpty(name))
                                             .ToHashSet()))
                       .Where(kvp => kvp.Value.Count > 0);

        /// <summary>
        /// Returns a dictionary consisting of KeyValuePairs containing conflicting mods.
        /// The keys are the mods that the user chose that led to the conflict being in the list!
        /// Use this for coloring/labelling rows, use ConflictDescriptions for reporting the conflicts to the user.
        /// </summary>
        public Dictionary<CkanModule, string> ConflictList
            => conflicts
                .SelectMany(kvp => UserReasonsFor(kvp.Key).Select(k => new KeyValuePair<CkanModule, ModPair>(k, kvp)))
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(group => group.Key,
                              group => string.Join(", ", group.Select(kvp => kvp.Value)
                                                              .Distinct()
                                                              .Select(origKvp => string.Format(
                        Properties.Resources.RelationshipResolverConflictsWith,
                        conflictingModDescription(origKvp.Key, null),
                        conflictingModDescription(origKvp.Value, null)))));

        /// <summary>
        /// Return descriptions of all the current conflicts.
        /// Avoids duplicates and explains why dependencies are in the list.
        /// Use for reporting the conflicts to the user, use ConflictsList for coloring rows.
        /// </summary>
        public IEnumerable<string> ConflictDescriptions
            => conflicts.Where(kvp => kvp.Value == null
                                      // Pick the pair with the least distantly selected one first
                                      || totalDependers(kvp.Key) <= totalDependers(kvp.Value))
                        .Select(kvp => string.Format(
                            Properties.Resources.RelationshipResolverConflictsWith,
                            conflictingModDescription(kvp.Key,   null),
                            conflictingModDescription(kvp.Value, null)));

        public bool IsConsistent => !conflicts.Any();

        public List<SelectionReason> ReasonsFor(CkanModule mod)
            => reasons.TryGetValue(mod, out List<SelectionReason> r)
                ? r
                : new List<SelectionReason>();

        public IEnumerable<CkanModule> UserReasonsFor(CkanModule mod)
            => allDependers(mod).Intersect(user_requested_mods);

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
            => ReasonsFor(mod).All(reason => reason is SelectionReason.Depends
                                             && !reason.Parent.IsMetapackage);

        private void AddReason(CkanModule module, SelectionReason reason)
        {
            if (reasons.TryGetValue(module, out List<SelectionReason> modReasons))
            {
                modReasons.Add(reason);
            }
            else
            {
                reasons.Add(module, new List<SelectionReason>() { reason });
            }
        }

        /// <summary>
        /// The list of all additional mods that need to be installed to satisfy all relationships.
        /// </summary>
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();
        private readonly List<CkanModule> user_requested_mods = new List<CkanModule>();

        private readonly List<ModPair> conflicts = new List<ModPair>();
        private readonly Dictionary<CkanModule, List<SelectionReason>> reasons =
            new Dictionary<CkanModule, List<SelectionReason>>();

        /// <summary>
        /// Depends relationships with suppress_recommendations=true,
        /// to be applied to all recommendations and suggestions
        /// </summary>
        private readonly HashSet<RelationshipDescriptor> suppressedRecommenders = new HashSet<RelationshipDescriptor>();

        private readonly IRegistryQuerier            registry;
        private readonly GameVersionCriteria         versionCrit;
        private readonly RelationshipResolverOptions options;

        /// <summary>
        /// The list of already installed modules.
        /// </summary>
        private readonly HashSet<CkanModule> installed_modules;

        private readonly HashSet<CkanModule> alreadyResolved = new HashSet<CkanModule>();

        private static readonly ILog log = LogManager.GetLogger(typeof(RelationshipResolver));
    }

    // TODO: It would be lovely to get rid of the `without` fields,
    // and replace them with `with` fields. Humans suck at inverting
    // cases in their heads.
    public class RelationshipResolverOptions
    {
        /// <summary>
        /// Default options for relationship resolution.
        /// </summary>
        public static RelationshipResolverOptions DefaultOpts()
            => new RelationshipResolverOptions();

        /// <summary>
        /// Options to install without recommendations.
        /// </summary>
        public static RelationshipResolverOptions DependsOnlyOpts()
            => new RelationshipResolverOptions()
            {
                with_recommends   = false,
                with_suggests     = false,
                with_all_suggests = false,
            };

        /// <summary>
        /// Options to find all dependencies, recommendations, and suggestions
        /// of anything in the changeset (except when suppress_recommendations==true),
        /// without throwing exceptions, so the calling code can decide what to do about conflicts
        /// </summary>
        public static RelationshipResolverOptions KitchenSinkOpts()
            => new RelationshipResolverOptions()
            {
                with_recommends                = true,
                with_suggests                  = true,
                with_all_suggests              = true,
                without_toomanyprovides_kraken = true,
                without_enforce_consistency    = true,
                proceed_with_inconsistencies   = true,
                get_recommenders               = true,
            };

        public static RelationshipResolverOptions ConflictsOpts()
            => new RelationshipResolverOptions()
            {
                without_toomanyprovides_kraken = true,
                proceed_with_inconsistencies   = true,
                without_enforce_consistency    = true,
                with_recommends                = false,
            };

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

        /// <summary>
        /// If true, get the list of mods that should be checked for
        /// recommendations and suggestions.
        /// Differs from normal resolution in that it stops when
        /// ModuleRelationshipDescriptor.suppress_recommendations==true
        /// </summary>
        public bool get_recommenders = false;

        public RelationshipResolverOptions OptionsFor(RelationshipDescriptor descr)
            => descr.suppress_recommendations ? WithoutRecommendations() : this;

        public RelationshipResolverOptions WithoutRecommendations()
        {
            if (with_recommends || with_all_suggests || with_suggests)
            {
                var newOptions = (RelationshipResolverOptions)MemberwiseClone();
                newOptions.with_recommends   = false;
                newOptions.with_all_suggests = false;
                newOptions.with_suggests     = false;
                return newOptions;
            }
            return this;
        }

        public RelationshipResolverOptions WithoutSuggestions()
        {
            if (with_suggests)
            {
                var newOptions = (RelationshipResolverOptions)MemberwiseClone();
                newOptions.with_suggests = false;
                return newOptions;
            }
            return this;
        }

    }

    /// <summary>
    /// Used to keep track of the relationships between modules in the resolver.
    /// Intended to be used for displaying messages to the user.
    /// </summary>
    public abstract class SelectionReason : IEquatable<SelectionReason>
    {
        // Currently assumed to exist for any relationship other than UserRequested or Installed
        public virtual CkanModule Parent { get; protected set; }
        public virtual string DescribeWith(IEnumerable<SelectionReason> others) => ToString();

        public override bool Equals(object obj)
            => Equals(obj as SelectionReason);

        public bool Equals(SelectionReason rsn)
        {
            // Parent throws in some derived classes
            try
            {
                return GetType() == rsn?.GetType()
                       && Parent == rsn?.Parent;
            }
            catch
            {
                // If thrown, then the type check passed and Parent threw
                return true;
            }
        }

        public override int GetHashCode()
        {
            var type = GetType();
            // Parent throws in some derived classes
            try
            {
                #if NET5_0_OR_GREATER
                return HashCode.Combine(type, Parent);
                #else
                unchecked
                {
                    return (type, Parent).GetHashCode();
                }
                #endif
            }
            catch
            {
                // If thrown, then we're type-only
                return type.GetHashCode();
            }
        }

        public class Installed : SelectionReason
        {
            public override CkanModule Parent
                => throw new Exception("Should never be called on Installed");

            public override string ToString()
                => Properties.Resources.RelationshipResolverInstalledReason;
        }

        public class UserRequested : SelectionReason
        {
            public override CkanModule Parent
                => throw new Exception("Should never be called on UserRequested");

            public override string ToString()
                => Properties.Resources.RelationshipResolverUserReason;
        }

        public class DependencyRemoved : SelectionReason
        {
            public override string ToString()
                => Properties.Resources.RelationshipResolverDependencyRemoved;
        }

        public class NoLongerUsed : SelectionReason
        {
            public override string ToString()
                => Properties.Resources.RelationshipResolverNoLongerUsedReason;
        }

        public class Replacement : SelectionReason
        {
            public Replacement(CkanModule module)
            {
                if (module == null)
                {
                    #pragma warning disable IDE0016
                    throw new ArgumentNullException();
                    #pragma warning restore IDE0016
                }
                Parent = module;
            }

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverReplacementReason, Parent.name);

            public override string DescribeWith(IEnumerable<SelectionReason> others)
                => string.Format(Properties.Resources.RelationshipResolverReplacementReason,
                    string.Join(", ", Enumerable.Repeat(this, 1).Concat(others).Select(r => r.Parent.name)));
        }

        public sealed class Suggested : SelectionReason
        {
            public Suggested(CkanModule module)
            {
                if (module == null)
                {
                    #pragma warning disable IDE0016
                    throw new ArgumentNullException();
                    #pragma warning restore IDE0016
                }
                Parent = module;
            }

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverSuggestedReason, Parent.name);
        }

        public sealed class Depends : SelectionReason
        {
            public Depends(CkanModule module)
            {
                if (module == null)
                {
                    #pragma warning disable IDE0016
                    throw new ArgumentNullException();
                    #pragma warning restore IDE0016
                }
                Parent = module;
            }

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverDependsReason, Parent.name);

            public override string DescribeWith(IEnumerable<SelectionReason> others)
                => string.Format(Properties.Resources.RelationshipResolverDependsReason,
                    string.Join(", ", Enumerable.Repeat(this, 1).Concat(others).Select(r => r.Parent.name)));
        }

        public sealed class Recommended : SelectionReason
        {
            public Recommended(CkanModule module, int providesIndex)
            {
                if (module == null)
                {
                    #pragma warning disable IDE0016
                    throw new ArgumentNullException();
                    #pragma warning restore IDE0016
                }
                Parent        = module;
                ProvidesIndex = providesIndex;
            }

            public readonly int ProvidesIndex;

            public Recommended WithIndex(int providesIndex)
                => new Recommended(Parent, providesIndex);

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverRecommendedReason, Parent.name);
        }
    }
}
