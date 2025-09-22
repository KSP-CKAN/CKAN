using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CKAN.IO;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for letting the user choose additional packages to install
    /// based on others they've selected
    /// </summary>
    public class DependencyScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="mgr">Game instance manager containing instances</param>
        /// <param name="instance">The game instance</param>
        /// <param name="reg">Registry of the current instance for finding mods</param>
        /// <param name="userAgent">HTTP useragent string to use</param>
        /// <param name="cp">Plan of mods to add and remove</param>
        /// <param name="rej">Mods that the user saw and did not select, in this pass or a previous pass</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public DependencyScreen(ConsoleTheme        theme,
                                GameInstanceManager mgr,
                                GameInstance        instance,
                                Registry            reg,
                                string?             userAgent,
                                ChangePlan          cp,
                                HashSet<CkanModule> rej,
                                bool                dbg)
            : base(theme)
        {
            plan     = cp;
            rejected = rej;

            AddObject(new ConsoleLabel(1, 2, -1,
                                       () => Properties.Resources.RecommendationsLabel));

            generateList(plan.Install.Concat(ReplacementModules(plan.Replace, instance, reg))
                                     .Distinct(),
                         instance, reg);

            dependencyList = new ConsoleListBox<Dependency>(
                1, 4, -1, -2,
                new List<Dependency>(dependencies.Values),
                new List<ConsoleListBoxColumn<Dependency>>() {
                    new ConsoleListBoxColumn<Dependency>(
                        Properties.Resources.RecommendationsInstallHeader,
                        d => StatusSymbol(d.module),
                        null,
                        7),
                    new ConsoleListBoxColumn<Dependency>(
                        Properties.Resources.RecommendationsNameHeader,
                        d => d.module.ToString(),
                        null,
                        null),
                    new ConsoleListBoxColumn<Dependency>(
                        Properties.Resources.RecommendationsSourcesHeader,
                        d => string.Join(", ", d.dependents),
                        null,
                        42)
                },
                1, 0, ListSortDirection.Descending
            );
            dependencyList.AddTip("+", Properties.Resources.Toggle);
            dependencyList.AddBinding(Keys.Plus, sender =>
            {
                if (dependencyList.Selection?.module is CkanModule mod
                    && (accepted.Contains(mod)
                        || TryWithoutConflicts(accepted.Append(mod), instance, reg))) {
                    ChangePlan.toggleContains(accepted, mod);
                }
                return true;
            });

            dependencyList.AddTip($"{Properties.Resources.Ctrl}+A", Properties.Resources.SelectAll);
            dependencyList.AddBinding(Keys.CtrlA, sender =>
            {
                if (TryWithoutConflicts(dependencies.Keys, instance, reg)) {
                    foreach (var kvp in dependencies) {
                        if (!accepted.Contains(kvp.Key)) {
                            ChangePlan.toggleContains(accepted, kvp.Key);
                        }
                    }
                }
                return true;
            });

            dependencyList.AddTip($"{Properties.Resources.Ctrl}+D", Properties.Resources.DeselectAll, () => accepted.Count > 0);
            dependencyList.AddBinding(Keys.CtrlD, sender =>
            {
                accepted.Clear();
                return true;
            });

            dependencyList.AddTip(Properties.Resources.Enter, Properties.Resources.Details);
            dependencyList.AddBinding(Keys.Enter, sender =>
            {
                if (dependencyList.Selection != null) {
                    LaunchSubScreen(new ModInfoScreen(theme, mgr, instance, reg, userAgent, plan,
                                                             dependencyList.Selection.module,
                                                             null, dbg));
                }
                return true;
            });

            AddObject(dependencyList);

            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, sender =>
            {
                // Add everything to rejected
                rejected.UnionWith(dependencies.Keys);
                return false;
            });

            AddTip("F9", Properties.Resources.Accept);
            AddBinding(Keys.F9, sender =>
            {
                if (TryWithoutConflicts(accepted, instance, reg)) {
                    plan.Install.UnionWith(accepted);
                    // Add the rest to rejected
                    rejected.UnionWith(dependencies.Keys
                                                   .Except(accepted));
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader() => $"{Meta.ProductName} {Meta.GetVersion()}";

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader() => Properties.Resources.RecommendationsTitle;

        /// <summary>
        /// Return whether there are any options to show.
        /// ModListScreen uses this to avoid showing this screen when empty.
        /// </summary>
        public bool HaveOptions() => dependencies.Count > 0;

        private void generateList(IEnumerable<CkanModule> userInstalling,
                                  GameInstance            instance,
                                  Registry                registry)
        {
            var opts = new RelationshipResolverOptions(instance.StabilityToleranceConfig)
            {
                without_toomanyprovides_kraken = true,
                proceed_with_inconsistencies   = true,
                without_enforce_consistency    = true,
                with_recommends                = false,
            };
            var resolver = new RelationshipResolver(userInstalling, null, opts,
                                                    registry, instance.Game, instance.VersionCriteria());
            var inst = resolver.ModList().ToArray();
            // Don't show dependencies that we must install as recommendations
            rejected.UnionWith(inst);

            if (ModuleInstaller.FindRecommendations(
                    instance, inst, inst, rejected, registry,
                    out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                    out Dictionary<CkanModule, List<string>> suggestions,
                    out Dictionary<CkanModule, HashSet<string>> supporters
            )) {
                foreach ((CkanModule mod, Tuple<bool, List<string>> checkedAndDependents) in recommendations) {
                    dependencies.Add(mod, new Dependency(mod, checkedAndDependents.Item2.Order()));
                }
                foreach ((CkanModule mod, List<string> dependents) in suggestions) {
                    dependencies.Add(mod, new Dependency(mod, dependents.Order()));
                }
                foreach ((CkanModule mod, HashSet<string> dependents) in supporters) {
                    dependencies.Add(mod, new Dependency(mod, dependents.Order()));
                }
                // Check the default checkboxes
                accepted.UnionWith(recommendations.Where(kvp => kvp.Value.Item1)
                                                  .Select(kvp => kvp.Key));
            }
        }

        private IEnumerable<CkanModule> ReplacementModules(IEnumerable<string> replaced_identifiers,
                                                           GameInstance        instance,
                                                           Registry            registry)
            => replaced_identifiers.Select(replaced => registry.GetReplacement(
                                                           replaced,
                                                           instance.StabilityToleranceConfig,
                                                           instance.VersionCriteria()))
                                   .OfType<ModuleReplacement>()
                                   .Select(repl => repl.ReplaceWith);

        private string StatusSymbol(CkanModule mod)
            => accepted.Contains(mod) ? installing
                                      : notinstalled;

        private bool TryWithoutConflicts(IEnumerable<CkanModule> toAdd,
                                         GameInstance            instance,
                                         Registry                registry)
        {
            if (HasConflicts(toAdd, instance, registry, out List<string>? conflictDescriptions)) {
                RaiseError("{0}", string.Join(Environment.NewLine,
                                              conflictDescriptions));
                return false;
            }
            return true;
        }

        private bool HasConflicts(IEnumerable<CkanModule>               toAdd,
                                  GameInstance                          instance,
                                  Registry                              registry,
                                  [NotNullWhen(true)] out List<string>? descriptions)
        {
            try
            {
                var resolver = new RelationshipResolver(
                    plan.Install.Concat(toAdd).Distinct(),
                    plan.Remove.Select(ident => registry.InstalledModule(ident)?.Module)
                               .OfType<CkanModule>(),
                    RelationshipResolverOptions.ConflictsOpts(instance.StabilityToleranceConfig),
                    registry, instance.Game, instance.VersionCriteria());
                descriptions = resolver.ConflictDescriptions.ToList();
                return descriptions.Count > 0;
            }
            catch (DependenciesNotSatisfiedKraken k)
            {
                descriptions = new List<string>() { k.Message };
                return true;
            }
        }

        private readonly HashSet<CkanModule> accepted = new HashSet<CkanModule>();
        private readonly HashSet<CkanModule> rejected;

        private readonly ChangePlan plan;

        private readonly Dictionary<CkanModule, Dependency> dependencies = new Dictionary<CkanModule, Dependency>();
        private readonly ConsoleListBox<Dependency>         dependencyList;

        private const string notinstalled = " ";
        private const string installing   = "+";
    }

    /// <summary>
    /// Object representing a mod we could install
    /// </summary>
    public class Dependency {

        /// <summary>
        /// Initialize a dependency
        /// </summary>
        /// <param name="m">The mod</param>
        /// <param name="d">Mods that recommend or suggest m</param>
        public Dependency(CkanModule m, IEnumerable<string> d)
        {
            module     = m;
            dependents = d.ToArray();
        }

        /// <summary>
        /// The mod
        /// </summary>
        public readonly CkanModule module;

        /// <summary>
        /// Mods that recommend or suggest this mod
        /// </summary>
        public readonly string[] dependents;
    }

}
