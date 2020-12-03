using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
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
        /// <param name="cp">Plan of mods to add and remove</param>
        /// <param name="rej">Mods that the user saw and did not select, in this pass or a previous pass</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public DependencyScreen(GameInstanceManager mgr, ChangePlan cp, HashSet<string> rej, bool dbg) : base()
        {
            debug     = dbg;
            manager   = mgr;
            plan      = cp;
            registry  = RegistryManager.Instance(manager.CurrentInstance).registry;
            installer = ModuleInstaller.GetInstance(manager.CurrentInstance, manager.Cache, this);
            rejected  = rej;

            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => "Additional mods are recommended or suggested:"
            ));

            HashSet<CkanModule> sourceModules = new HashSet<CkanModule>();
            sourceModules.UnionWith(plan.Install);
            sourceModules.UnionWith(new HashSet<CkanModule>(
                ReplacementIdentifiers(plan.Replace)
                    .Select(id => registry.InstalledModule(id).Module)
            ));
            generateList(sourceModules);

            dependencyList = new ConsoleListBox<Dependency>(
                1, 4, -1, -2,
                new List<Dependency>(dependencies.Values),
                new List<ConsoleListBoxColumn<Dependency>>() {
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = "Install",
                        Width    = 7,
                        Renderer = (Dependency d) => StatusSymbol(d.module)
                    },
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = "Name",
                        Width    = 36,
                        Renderer = (Dependency d) => d.module.ToString()
                    },
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = "Sources",
                        Width    = 42,
                        Renderer = (Dependency d) => string.Join(", ", d.dependents)
                    }
                },
                1, 0, ListSortDirection.Descending
            );
            dependencyList.AddTip("+", "Toggle");
            dependencyList.AddBinding(Keys.Plus, (object sender) => {
                ChangePlan.toggleContains(accepted, dependencyList.Selection.module);
                return true;
            });

            dependencyList.AddTip("Ctrl+A", "Select all");
            dependencyList.AddBinding(Keys.CtrlA, (object sender) => {
                foreach (var kvp in dependencies) {
                    if (!accepted.Contains(kvp.Key)) {
                        ChangePlan.toggleContains(accepted, kvp.Key);
                    }
                }
                return true;
            });

            dependencyList.AddTip("Ctrl+D", "Deselect all", () => accepted.Count > 0);
            dependencyList.AddBinding(Keys.CtrlD, (object sender) => {
                accepted.Clear();
                return true;
            });

            dependencyList.AddTip("Enter", "Details");
            dependencyList.AddBinding(Keys.Enter, (object sender) => {
                if (dependencyList.Selection != null) {
                    LaunchSubScreen(new ModInfoScreen(
                        manager, plan,
                        dependencyList.Selection.module,
                        debug
                    ));
                }
                return true;
            });

            AddObject(dependencyList);

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender) => {
                // Add everything to rejected
                foreach (var kvp in dependencies) {
                    rejected.Add(kvp.Key.identifier);
                }
                return false;
            });

            AddTip("F9", "Accept");
            AddBinding(Keys.F9, (object sender) => {
                foreach (CkanModule mod in accepted) {
                    plan.Install.Add(mod);
                }
                // Add the rest to rejected
                foreach (var kvp in dependencies) {
                    if (!accepted.Contains(kvp.Key)) {
                        rejected.Add(kvp.Key.identifier);
                    }
                }
                return false;
            });
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"CKAN {Meta.GetVersion()}";
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return "Recommendations & Suggestions";
        }

        /// <summary>
        /// Return whether there are any options to show.
        /// ModListScreen uses this to avoid showing this screen when empty.
        /// </summary>
        public bool HaveOptions()
        {
            return dependencies.Count > 0;
        }

        private void generateList(HashSet<CkanModule> inst)
        {
            if (installer.FindRecommendations(
                inst, inst, registry as Registry,
                out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                out Dictionary<CkanModule, List<string>> suggestions,
                out Dictionary<CkanModule, HashSet<string>> supporters
            )) {
                foreach (var kvp in recommendations) {
                    dependencies.Add(kvp.Key, new Dependency() {
                        module         = kvp.Key,
                        defaultInstall = kvp.Value.Item1,
                        dependents     = kvp.Value.Item2.OrderBy(d => d).ToList()
                    });
                    if (kvp.Value.Item1) {
                        accepted.Add(kvp.Key);
                    }
                }
                foreach (var kvp in suggestions) {
                    dependencies.Add(kvp.Key, new Dependency() {
                        module         = kvp.Key,
                        defaultInstall = false,
                        dependents     = kvp.Value.OrderBy(d => d).ToList()
                    });
                }
                foreach (var kvp in supporters) {
                    dependencies.Add(kvp.Key, new Dependency() {
                        module         = kvp.Key,
                        defaultInstall = false,
                        dependents     = kvp.Value.OrderBy(d => d).ToList()
                    });
                }
            }
        }

        private IEnumerable<string> ReplacementIdentifiers(IEnumerable<string> replaced_identifiers)
        {
            foreach (string replaced in replaced_identifiers) {
                ModuleReplacement repl = registry.GetReplacement(
                    replaced, manager.CurrentInstance.VersionCriteria()
                );
                if (repl != null) {
                    yield return repl.ReplaceWith.identifier;
                }
            }
        }

        private string StatusSymbol(CkanModule mod)
        {
            if (accepted.Contains(mod)) {
                return installing;
            } else {
                return notinstalled;
            }
        }

        private HashSet<CkanModule> accepted = new HashSet<CkanModule>();
        private HashSet<string> rejected;

        private IRegistryQuerier registry;
        private GameInstanceManager       manager;
        private ModuleInstaller  installer;
        private ChangePlan       plan;
        private bool             debug;

        private Dictionary<CkanModule, Dependency> dependencies = new Dictionary<CkanModule, Dependency>();
        private ConsoleListBox<Dependency>         dependencyList;

        private static readonly string notinstalled = " ";
        private static readonly string installing   = "+";
    }

    /// <summary>
    /// Object representing a mod we could install
    /// </summary>
    public class Dependency {

        /// <summary>
        /// Identifier of mod
        /// </summary>
        public CkanModule       module;

        /// <summary>
        /// True if we default to installing, false otherwise
        /// </summary>
        public bool             defaultInstall;

        /// <summary>
        /// List of mods that recommended or suggested this mod
        /// </summary>
        public List<string>     dependents = new List<string>();
    }

}
