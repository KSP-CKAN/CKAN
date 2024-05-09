using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

using CKAN.Versioning;
#if NETFRAMEWORK
using CKAN.Extensions;
#endif
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
        /// <param name="mgr">Game instance manager containing instances</param>
        /// <param name="reg">Registry of the current instance for finding mods</param>
        /// <param name="cp">Plan of mods to add and remove</param>
        /// <param name="rej">Mods that the user saw and did not select, in this pass or a previous pass</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public DependencyScreen(GameInstanceManager mgr, Registry reg, ChangePlan cp, HashSet<string> rej, bool dbg) : base()
        {
            debug    = dbg;
            manager  = mgr;
            plan     = cp;
            registry = reg;
            rejected = rej;

            AddObject(new ConsoleLabel(1, 2, -1,
                                       () => Properties.Resources.RecommendationsLabel));

            generateList(new ModuleInstaller(manager.CurrentInstance, manager.Cache, this),
                         plan.Install
                             .Concat(ReplacementModules(plan.Replace,
                                                        manager.CurrentInstance.VersionCriteria()))
                             .ToHashSet());

            dependencyList = new ConsoleListBox<Dependency>(
                1, 4, -1, -2,
                new List<Dependency>(dependencies.Values),
                new List<ConsoleListBoxColumn<Dependency>>() {
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = Properties.Resources.RecommendationsInstallHeader,
                        Width    = 7,
                        Renderer = (Dependency d) => StatusSymbol(d.module),
                    },
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = Properties.Resources.RecommendationsNameHeader,
                        Width    = null,
                        Renderer = (Dependency d) => d.module.ToString(),
                    },
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = Properties.Resources.RecommendationsSourcesHeader,
                        Width    = 42,
                        Renderer = (Dependency d) => string.Join(", ", d.dependents),
                    }
                },
                1, 0, ListSortDirection.Descending
            );
            dependencyList.AddTip("+", Properties.Resources.Toggle);
            dependencyList.AddBinding(Keys.Plus, (object sender, ConsoleTheme theme) => {
                var mod = dependencyList.Selection.module;
                if (accepted.Contains(mod) || TryWithoutConflicts(accepted.Append(mod))) {
                    ChangePlan.toggleContains(accepted, mod);
                }
                return true;
            });

            dependencyList.AddTip($"{Properties.Resources.Ctrl}+A", Properties.Resources.SelectAll);
            dependencyList.AddBinding(Keys.CtrlA, (object sender, ConsoleTheme theme) => {
                if (TryWithoutConflicts(dependencies.Keys)) {
                    foreach (var kvp in dependencies) {
                        if (!accepted.Contains(kvp.Key)) {
                            ChangePlan.toggleContains(accepted, kvp.Key);
                        }
                    }
                }
                return true;
            });

            dependencyList.AddTip($"{Properties.Resources.Ctrl}+D", Properties.Resources.DeselectAll, () => accepted.Count > 0);
            dependencyList.AddBinding(Keys.CtrlD, (object sender, ConsoleTheme theme) => {
                accepted.Clear();
                return true;
            });

            dependencyList.AddTip(Properties.Resources.Enter, Properties.Resources.Details);
            dependencyList.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                if (dependencyList.Selection != null) {
                    LaunchSubScreen(theme, new ModInfoScreen(manager, reg, plan,
                                                             dependencyList.Selection.module,
                                                             debug));
                }
                return true;
            });

            AddObject(dependencyList);

            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => {
                // Add everything to rejected
                rejected.UnionWith(dependencies.Keys.Select(m => m.identifier));
                return false;
            });

            AddTip("F9", Properties.Resources.Accept);
            AddBinding(Keys.F9, (object sender, ConsoleTheme theme) => {
                if (TryWithoutConflicts(accepted)) {
                    plan.Install.UnionWith(accepted);
                    // Add the rest to rejected
                    rejected.UnionWith(dependencies.Keys
                                                   .Except(accepted)
                                                   .Select(m => m.identifier));
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader() => $"{Meta.GetProductName()} {Meta.GetVersion()}";

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader() => Properties.Resources.RecommendationsTitle;

        /// <summary>
        /// Return whether there are any options to show.
        /// ModListScreen uses this to avoid showing this screen when empty.
        /// </summary>
        public bool HaveOptions() => dependencies.Count > 0;

        private void generateList(ModuleInstaller installer, HashSet<CkanModule> inst)
        {
            if (installer.FindRecommendations(
                inst, new List<CkanModule>(inst), registry as Registry,
                out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                out Dictionary<CkanModule, List<string>> suggestions,
                out Dictionary<CkanModule, HashSet<string>> supporters
            )) {
                foreach ((CkanModule mod, Tuple<bool, List<string>> checkedAndDependents) in recommendations) {
                    dependencies.Add(mod, new Dependency() {
                        module     = mod,
                        dependents = checkedAndDependents.Item2.OrderBy(d => d).ToList()
                    });
                }
                foreach ((CkanModule mod, List<string> dependents) in suggestions) {
                    dependencies.Add(mod, new Dependency() {
                        module     = mod,
                        dependents = dependents.OrderBy(d => d).ToList()
                    });
                }
                foreach ((CkanModule mod, HashSet<string> dependents) in supporters) {
                    dependencies.Add(mod, new Dependency() {
                        module     = mod,
                        dependents = dependents.OrderBy(d => d).ToList()
                    });
                }
                // Check the default checkboxes
                accepted.UnionWith(recommendations.Where(kvp => kvp.Value.Item1)
                                                  .Select(kvp => kvp.Key));
            }
        }

        private IEnumerable<CkanModule> ReplacementModules(IEnumerable<string> replaced_identifiers,
                                                           GameVersionCriteria crit)
            => replaced_identifiers.Select(replaced => registry.GetReplacement(replaced, crit))
                                   .Where(repl => repl != null)
                                   .Select(repl => repl.ReplaceWith);

        private string StatusSymbol(CkanModule mod)
            => accepted.Contains(mod) ? installing
                                      : notinstalled;

        private bool TryWithoutConflicts(IEnumerable<CkanModule> toAdd)
        {
            if (HasConflicts(toAdd, out List<string> conflictDescriptions)) {
                RaiseError("{0}", string.Join(Environment.NewLine,
                                              conflictDescriptions));
                return false;
            }
            return true;
        }

        private bool HasConflicts(IEnumerable<CkanModule> toAdd,
                                  out List<string>        descriptions)
        {
            try
            {
                var resolver = new RelationshipResolver(
                    plan.Install.Concat(toAdd).Distinct(),
                    plan.Remove.Select(ident => registry.InstalledModule(ident)?.Module),
                    RelationshipResolverOptions.ConflictsOpts(), registry,
                    manager.CurrentInstance.VersionCriteria());
                descriptions = resolver.ConflictDescriptions.ToList();
                return descriptions.Count > 0;
            }
            catch (DependencyNotSatisfiedKraken k)
            {
                descriptions = new List<string>() { k.Message };
                return true;
            }
        }

        private readonly HashSet<CkanModule> accepted = new HashSet<CkanModule>();
        private readonly HashSet<string>     rejected;

        private readonly IRegistryQuerier    registry;
        private readonly GameInstanceManager manager;
        private readonly ChangePlan          plan;
        private readonly bool                debug;

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
        /// The mod
        /// </summary>
        public CkanModule module;

        /// <summary>
        /// List of mods that recommended or suggested this mod
        /// </summary>
        public List<string> dependents = new List<string>();
    }

}
