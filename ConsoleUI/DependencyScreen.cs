using System.ComponentModel;
using System.Collections.Generic;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for letting the user choose additional pacakges to install
    /// based on others they've selected
    /// </summary>
    public class DependencyScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">KSP manager containing instances</param>
        /// <param name="cp">Plan of mods to add and remove</param>
        /// <param name="rej">Mods that the user saw and did not select, in this pass or a previous pass</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public DependencyScreen(KSPManager mgr, ChangePlan cp, HashSet<string> rej, bool dbg) : base()
        {
            debug    = dbg;
            manager  = mgr;
            plan     = cp;
            registry = RegistryManager.Instance(manager.CurrentInstance).registry;
            rejected = rej;

            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => "Additional mods are recommended or suggested:"
            ));

            generateList(plan.Install);

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
            foreach (CkanModule mod in inst) {
                AddDependencies(inst, mod, mod.recommends, true);
                AddDependencies(inst, mod, mod.suggests,   false);
            }
        }

        private void AddDependencies(HashSet<CkanModule> alreadyInstalling, CkanModule dependent, List<RelationshipDescriptor> source, bool installByDefault)
        {
            if (source != null) {
                foreach (RelationshipDescriptor dependency in source) {
                    if (dependency.ContainsAny(rejected)) {
                        List<CkanModule> opts = dependency.LatestAvailableWithProvides(
                            registry,
                            manager.CurrentInstance.VersionCriteria()
                        );
                        foreach (CkanModule provider in opts) {
                            if (!registry.IsInstalled(provider.identifier)
                                    && !alreadyInstalling.Contains(provider)) {

                                // Only default to installing if there's only one
                                AddDep(provider, installByDefault && opts.Count == 1, dependent);
                            }
                        }
                    }
                }
            }
        }

        private void AddDep(CkanModule mod, bool defaultInstall, CkanModule dependent)
        {
            if (dependencies.ContainsKey(mod)) {
                dependencies[mod].defaultInstall |= defaultInstall;
                dependencies[mod].dependents.Add(dependent);
            } else {
                dependencies.Add(mod, new Dependency() {
                    module         = mod,
                    defaultInstall = defaultInstall,
                    dependents     = new List<CkanModule>() { dependent }
                });
            }
            if (defaultInstall) {
                accepted.Add(mod);
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
        private KSPManager       manager;
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
        public List<CkanModule> dependents = new List<CkanModule>();
    }

}
