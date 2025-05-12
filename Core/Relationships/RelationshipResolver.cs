using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

using CKAN.Configuration;
using CKAN.Games;
using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN
{
    using ModPair = KeyValuePair<CkanModule, CkanModule?>;

    /// <summary>
    /// Resolves relationships between mods. Primarily used to satisfy missing dependencies and to check for conflicts on proposed installs.
    /// </summary>
    public class RelationshipResolver
    {
        /// <summary>
        /// Creates a new resolver that will find a way to install all the modules specified.
        /// </summary>
        /// <param name="modulesToInstall">Modules to install</param>
        /// <param name="modulesToRemove">Modules to remove</param>
        /// <param name="options">Options for the RelationshipResolver</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="game">The game to mention in error messages</param>
        /// <param name="versionCrit">The current game version criteria to consider</param>
        public RelationshipResolver(IEnumerable<CkanModule>     modulesToInstall,
                                    IEnumerable<CkanModule>?    modulesToRemove,
                                    RelationshipResolverOptions options,
                                    IRegistryQuerier            registry,
                                    IGame                       game,
                                    GameVersionCriteria         versionCrit)
        {
            this.options     = options;
            this.registry    = registry;
            this.game        = game;
            this.versionCrit = versionCrit;

            installed_modules = registry.InstalledModules
                                        .Select(i_module => i_module.Module)
                                        .Except(modulesToRemove ?? Enumerable.Empty<CkanModule>())
                                        .ToHashSet();
            var installed_relationship = new SelectionReason.Installed();
            foreach (var module in installed_modules)
            {
                AddReason(module, installed_relationship);
            }

            var toInst = modulesToInstall.ToArray();
            // DLLs that we are upgrading to full modules should be excluded
            dlls = registry.InstalledDlls
                           .Except(modulesToInstall.Select(m => m.identifier))
                           .ToHashSet();
            resolved = new ResolvedRelationshipsTree(toInst, registry, dlls,
                                                     installed_modules,
                                                     options.stability_tolerance ?? new StabilityToleranceConfig(""),
                                                     versionCrit,
                                                     options.OptionalHandling());
            if (!options.proceed_with_inconsistencies)
            {
                var unsatisfied = resolved.Unsatisfied().ToArray();
                if (unsatisfied.Length > 0)
                {
                    log.DebugFormat("Dependencies failed!{0}{0}{1}{0}{0}{2}",
                                    Environment.NewLine,
                                    Environment.StackTrace,
                                    resolved);
                    throw new DependenciesNotSatisfiedKraken(unsatisfied, registry, game, resolved);
                }
            }

            AddModulesToInstall(toInst);
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
                                                 dlls,
                                                 registry.InstalledDlc);
            }
            catch (BadRelationshipsKraken k) when (options.without_enforce_consistency)
            {
                conflicts.AddRange(k.Conflicts.Select(tuple => new ModPair(tuple.Item1, tuple.Item3))
                                              .Where(pair => !conflicts.Contains(pair))
                                              .ToArray());
            }
        }

        /// <summary>
        /// Resolves all relationships for a module.
        /// May recurse to ResolveStanza, which may add additional modules to be installed.
        /// </summary>
        private void Resolve(CkanModule                           module,
                             RelationshipResolverOptions          options,
                             IEnumerable<RelationshipDescriptor>? old_stanza = null)
        {
            if (alreadyResolved.Contains(module))
            {
                return;
            }
            // Mark this module as resolved so we don't recurse here again
            alreadyResolved.Add(module);

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
        private void ResolveStanza(List<RelationshipDescriptor>?        stanza,
                                   SelectionReason                      reason,
                                   RelationshipResolverOptions          options,
                                   bool                                 soft_resolve,
                                   IEnumerable<RelationshipDescriptor>? old_stanza)
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
                    log.DebugFormat("Skipping {0} because get_recommenders option is set",
                                    descriptor.ToString());
                    suppressedRecommenders.Add(descriptor);
                    continue;
                }
                options = orig_options.OptionsFor(descriptor);

                // If we already have this dependency covered,
                // resolve its relationships if we haven't already
                if (descriptor.MatchesAny(modlist.Values, null, null,
                                          out CkanModule? installingCandidate)
                    && installingCandidate != null)
                {
                    log.DebugFormat("Match already in changeset: {0}, adding reason {1}",
                                    installingCandidate, reason);
                    AddReason(installingCandidate, reason);
                    // Resolve the relationships of the matching module here
                    // because that's when it would happen if non-virtual
                    Resolve(installingCandidate, options, stanza);
                    continue;
                }
                if (descriptor.ContainsAny(modlist.Keys))
                {
                    // Two installing mods depend on different versions of this dependency
                    var module = modlist.Values.FirstOrDefault(m => descriptor.ContainsAny(new string[] { m.identifier }));
                    log.DebugFormat("Changeset contains {0}, which doesn't match {1}",
                                    module, descriptor.ToString());
                    if (options.proceed_with_inconsistencies)
                    {
                        if (module != null && reason is SelectionReason.RelationshipReason rel)
                        {
                            conflicts.Add(new ModPair(module, rel.Parent));
                            conflicts.Add(new ModPair(rel.Parent, module));
                        }
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
                                          dlls,
                                          registry.InstalledDlc,
                                          out CkanModule? installedCandidate))
                {
                    if (installedCandidate != null)
                    {
                        log.DebugFormat("Match already installed: {0}, adding reason {1}",
                                        installedCandidate, reason);
                        AddReason(installedCandidate, reason);
                    }
                    else
                    {
                        log.DebugFormat("Matches installed DLL or DLC");
                    }
                    continue;
                }
                if (descriptor.ContainsAny(installed_modules.Select(im => im.identifier)))
                {
                    // We need a different version of the mod than is already installed
                    var module = installed_modules.FirstOrDefault(m => descriptor.ContainsAny(new string[] { m.identifier }));
                    log.DebugFormat("Found installed mod {0}, which doesn't match {1}",
                                    module, descriptor.ToString());
                    if (options.proceed_with_inconsistencies)
                    {
                        if (module != null && reason is SelectionReason.RelationshipReason rel)
                        {
                            conflicts.Add(new ModPair(module, rel.Parent));
                            conflicts.Add(new ModPair(rel.Parent, module));
                        }
                        continue;
                    }
                    else
                    {
                        throw new InconsistentKraken(string.Format(
                            Properties.Resources.RelationshipResolverRequiredButInstalled,
                            descriptor));
                    }
                }

                var candidates = resolved.Candidates(descriptor,
                                                     modlist.Values.Except(user_requested_mods)
                                                                   .ToArray())
                                         .ToArray();
                log.DebugFormat("Got {0} candidates for {1}",
                                candidates.Length, descriptor.ToString());
                if (candidates.Length == 0)
                {
                    if (!soft_resolve
                        && !options.proceed_with_inconsistencies
                        && reason is SelectionReason.RelationshipReason rel)
                    {
                        log.InfoFormat("Dependency on {0} found but it is not listed in the index, or not available for your game version.", descriptor.ToString());

                        throw new DependenciesNotSatisfiedKraken(
                            new ResolvedByNew(rel.Parent, descriptor, reason),
                            registry, game, resolved);
                    }
                    log.InfoFormat("{0} is recommended/suggested but it is not listed in the index, or not available for your game version.", descriptor.ToString());
                    continue;
                }
                if (candidates.Length > 1)
                {
                    // Oh no, too many to pick from!
                    if (options.without_toomanyprovides_kraken)
                    {
                        if (options.get_recommenders && reason is not SelectionReason.Depends)
                        {
                            for (int i = 0; i < candidates.Length; ++i)
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
                        var provide = candidates.Where(c => old_stanza.Any(rel => rel.WithinBounds(c)))
                                                .ToArray();
                        if (provide.Length != 1 && reason is SelectionReason.RelationshipReason rel)
                        {
                            // We still have either nothing, or too many to pick from
                            // Just throw the TMP now
                            throw new TooManyModsProvideKraken(rel.Parent, descriptor.ToString() ?? "",
                                                               candidates.ToList(),
                                                               descriptor.choice_help_text);
                        }
                        candidates[0] = provide.First();
                    }
                    else if (reason is SelectionReason.RelationshipReason rel)
                    {
                        throw new TooManyModsProvideKraken(rel.Parent, descriptor.ToString() ?? "",
                                                           candidates.ToList(),
                                                           descriptor.choice_help_text);
                    }
                }

                CkanModule candidate = candidates.First();

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
                        conflictingModDescription(candidate, (reason as SelectionReason.RelationshipReason)?.Parent)));
                }
            }
        }

        private string conflictingModDescription(CkanModule? mod, CkanModule? parent)
            => mod == null
                ? Properties.Resources.RelationshipResolverAnUnmanaged
                : parent == null && ReasonsFor(mod).Any(r => r is SelectionReason.UserRequested
                                                               or SelectionReason.Installed)
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
            log.DebugFormat("Adding {0} {1}", module.identifier, module.version);

            if (modlist.TryGetValue(module.identifier, out CkanModule? possibleDup))
            {
                if (possibleDup.identifier == module.identifier)
                {
                    if (possibleDup.version == module.version
                        && !possibleDup.MetadataEquals(module))
                    {
                        // If the version is the same and the metadata changed,
                        // queue this up as a reinstall (remove the old version)
                        reasons.Remove(possibleDup);
                        modlist.Remove(possibleDup.identifier);
                    }
                    else
                    {
                        // We should never add the same module twice!
                        log.ErrorFormat("Assertion failed: Adding {0} twice in relationship resolution", module.identifier);
                        throw new ArgumentException("Already contains module: " + module.identifier);
                    }
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

        /// <summary>
        /// Returns a list of all modules to install to satisfy the changes required.
        /// Each mod is after its dependencies and before its reverse dependencies.
        /// </summary>
        public IEnumerable<CkanModule> ModList(bool parallel = true)
            => modlist.Values.Distinct()
                             .AsParallelIf(parallel)
                             // Put user choices at the bottom; .OrderBy(bool) -> false first
                             .OrderBy(m => ReasonsFor(m).Any(r => r is SelectionReason.UserRequested))
                             // Put dependencies before dependers
                             .ThenByDescending(totalDependers)
                             // Resolve ties in name order
                             .ThenBy(m => m.name);

        public bool ReadyToInstall(CkanModule mod, ICollection<CkanModule> installed)
            => !modlist.Values.Distinct()
                              .Where(m => m != mod)
                              .Except(installed)
                              // Ignore circular dependencies
                              .Except(allDependers(mod))
                              .SelectMany(allDependers)
                              .Contains(mod);

        // The more nodes of the reverse-dependency graph we can paint, the higher up in the list it goes
        private int totalDependers(CkanModule module)
            => allDependers(module).Count();

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

        private IEnumerable<CkanModule> allDependers(CkanModule module)
            => BreadthFirstSearch(Enumerable.Repeat(module, 1),
                                  (searching, found) =>
                                      ReasonsFor(searching).OfType<SelectionReason.RelationshipReason>()
                                                           .Select(r => r.Parent)
                                                           .OfType<CkanModule>()
                                                           .Except(found));

        public IEnumerable<CkanModule> Dependencies()
            => BreadthFirstSearch(user_requested_mods.Where(m => !suppressedRecommenders.Any(rel => rel.WithinBounds(m))),
                                  (searching, found) =>
                                      modlist.Values
                                             .Except(found)
                                             .Where(m => ReasonsFor(m).Any(r => r is SelectionReason.Depends dep
                                                                                && dep.Parent == searching)));

        public IEnumerable<CkanModule> Recommendations(HashSet<CkanModule> dependencies)
            => modlist.Values.Except(dependencies)
                             .Where(m => ValidRecSugReasons(dependencies,
                                                            ReasonsFor(m).OfType<SelectionReason.Recommended>()
                                                                         .ToArray()))
                             .OrderByDescending(totalDependers);

        public IEnumerable<CkanModule> Suggestions(HashSet<CkanModule> dependencies,
                                                   List<CkanModule>    recommendations)
            => modlist.Values.Except(dependencies)
                             .Except(recommendations)
                             .Where(m => ValidRecSugReasons(dependencies,
                                                            ReasonsFor(m).OfType<SelectionReason.Suggested>()
                                                                         .ToArray()))
                             .OrderByDescending(totalDependers);

        private bool ValidRecSugReasons(HashSet<CkanModule>                  dependencies,
                                        SelectionReason.RelationshipReason[] recSugReasons)
            => recSugReasons.Any(r => dependencies.Contains(r.Parent))
               && !suppressedRecommenders.Any(rel => recSugReasons.Any(r => r.Parent != null
                                                                            && rel.WithinBounds(r.Parent)));

        public ParallelQuery<KeyValuePair<CkanModule, HashSet<string>>> Supporters(
            HashSet<CkanModule>     supported,
            IEnumerable<CkanModule> toExclude)
            => registry.CompatibleModules(options.stability_tolerance ?? new StabilityToleranceConfig(""),
                                          versionCrit)
                       .Except(toExclude)
                       .AsParallel()
                       // Find installable modules with "supports" relationships
                       .Where(mod => !registry.IsInstalled(mod.identifier)
                                     && mod.supports != null)
                       // Find each module that "supports" something we're installing
                       .Select(mod => new KeyValuePair<CkanModule, HashSet<string>>(
                                          mod,
                                          (mod.supports ?? Enumerable.Empty<RelationshipDescriptor>())
                                             .Where(rel => rel.MatchesAny(supported, null, null))
                                             .Select(rel => (rel as ModuleRelationshipDescriptor)?.name)
                                             .OfType<string>()
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

        public bool IsConsistent => conflicts.Count == 0;

        public List<SelectionReason> ReasonsFor(CkanModule mod)
            => reasons.TryGetValue(mod, out List<SelectionReason>? r)
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
            => ReasonsFor(mod).All(reason => reason is SelectionReason.Depends dep
                                             && dep.Parent != null
                                             && !dep.Parent.IsMetapackage);

        private void AddReason(CkanModule module, SelectionReason reason)
        {
            if (reasons.TryGetValue(module, out List<SelectionReason>? modReasons))
            {
                modReasons.Add(reason);
            }
            else
            {
                reasons.Add(module, new List<SelectionReason>() { reason });
            }
        }

        private readonly ResolvedRelationshipsTree resolved;

        /// <summary>
        /// The list of all additional mods that need to be installed to satisfy all relationships.
        /// </summary>
        private readonly Dictionary<string, CkanModule> modlist = new Dictionary<string, CkanModule>();
        private readonly List<CkanModule> user_requested_mods = new List<CkanModule>();

        private readonly List<ModPair> conflicts = new List<ModPair>();
        private readonly Dictionary<CkanModule, List<SelectionReason>> reasons =
            new Dictionary<CkanModule, List<SelectionReason>>();

        private readonly HashSet<string> dlls;

        /// <summary>
        /// Depends relationships with suppress_recommendations=true,
        /// to be applied to all recommendations and suggestions
        /// </summary>
        private readonly HashSet<RelationshipDescriptor> suppressedRecommenders = new HashSet<RelationshipDescriptor>();

        private readonly IRegistryQuerier            registry;
        private readonly IGame                       game;
        private readonly GameVersionCriteria         versionCrit;
        private readonly RelationshipResolverOptions options;

        /// <summary>
        /// The list of already installed modules.
        /// </summary>
        private readonly HashSet<CkanModule> installed_modules;

        private readonly HashSet<CkanModule> alreadyResolved = new HashSet<CkanModule>();

        private static readonly ILog log = LogManager.GetLogger(typeof(RelationshipResolver));
    }

}
