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
                        Renderer = (Dependency d) => StatusSymbol(d.identifier),
                    },
                    new ConsoleListBoxColumn<Dependency>() {
                        Header   = "Name",
                        Width    = 24,
                        Renderer = (Dependency d) => d.identifier,
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
                ChangePlan.toggleContains(accepted, dependencyList.Selection.identifier);
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
                        registry.LatestAvailable(dependencyList.Selection.identifier, manager.CurrentInstance.VersionCriteria()),
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
                    rejected.Add(kvp.Key);
                }
                return false;
            });

            AddTip("F9", "Accept");
            AddBinding(Keys.F9, (object sender) => {
                foreach (string name in accepted) {
                    plan.Install.Add(name);
                }
                // Add the rest to rejected
                foreach (var kvp in dependencies) {
                    if (!accepted.Contains(kvp.Key)) {
                        rejected.Add(kvp.Key);
                    }
                }
                return false;
            });

            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => "Recommendations & Suggestions";
        }

        /// <summary>
        /// Return whether there are any options to show.
        /// ModListScreen uses this to avoid showing this screen when empty.
        /// </summary>
        public bool HaveOptions()
        {
            return dependencies.Count > 0;
        }

        private void generateList(HashSet<string> inst)
        {
            foreach (string mod in inst) {
                CkanModule m = registry.LatestAvailable(mod, manager.CurrentInstance.VersionCriteria());
                if (m != null) {
                    AddDependencies(inst, mod, m.recommends, true);
                    AddDependencies(inst, mod, m.suggests,   false);
                }
            }
        }

        private void AddDependencies(HashSet<string> alreadyInstalling, string identifier, List<RelationshipDescriptor> source, bool installByDefault)
        {
            if (source != null) {
                foreach (RelationshipDescriptor dependency in source) {
                    if (!rejected.Contains(dependency.name)) {
                        try {
                            if (registry.LatestAvailable(
                                    dependency.name,
                                    manager.CurrentInstance.VersionCriteria(),
                                    dependency
                                ) != null
                                    && !registry.IsInstalled(dependency.name)
                                    && !alreadyInstalling.Contains(dependency.name)) {

                                AddDep(dependency.name, installByDefault, identifier);
                            }
                        } catch (ModuleNotFoundKraken) {
                            // LatestAvailable throws if you recommend a "provides" name,
                            // so ask the registry again for provides-based choices
                            List<CkanModule> opts = registry.LatestAvailableWithProvides(
                                dependency.name,
                                manager.CurrentInstance.VersionCriteria(),
                                dependency
                            );
                            foreach (CkanModule provider in opts) {
                                if (!registry.IsInstalled(provider.identifier)
                                        && !alreadyInstalling.Contains(provider.identifier)) {

                                    // Default to not installing because these probably conflict with each other
                                    AddDep(provider.identifier, false, identifier);
                                }
                            }
                        } catch (Kraken) {
                            // GUI/MainInstall.cs::AddMod just ignores all exceptions,
                            // so that's baked into the infrastructure
                        }
                    }
                }
            }
        }

        private void AddDep(string identifier, bool defaultInstall, string dependent)
        {
            if (dependencies.ContainsKey(identifier)) {
                dependencies[identifier].defaultInstall |= defaultInstall;
                dependencies[identifier].dependents.Add(dependent);
            } else {
                dependencies.Add(identifier, new Dependency() {
                    identifier     = identifier,
                    defaultInstall = defaultInstall,
                    dependents     = new List<string>() {dependent}
                });
            }
            if (defaultInstall) {
                accepted.Add(identifier);
            }
        }

        private string StatusSymbol(string identifier)
        {
            if (accepted.Contains(identifier)) {
                return installing;
            } else {
                return notinstalled;
            }
        }

        private HashSet<string> accepted = new HashSet<string>();
        private HashSet<string> rejected;

        private IRegistryQuerier registry;
        private KSPManager       manager;
        private ChangePlan       plan;
        private bool             debug;

        private Dictionary<string, Dependency> dependencies = new Dictionary<string, Dependency>();
        private ConsoleListBox<Dependency>     dependencyList;

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
        public string       identifier;

        /// <summary>
        /// True if we default to installing, false otherwise
        /// </summary>
        public bool         defaultInstall;

        /// <summary>
        /// List of mods that recommended or suggested this mod
        /// </summary>
        public List<string> dependents = new List<string>();
    }

}
