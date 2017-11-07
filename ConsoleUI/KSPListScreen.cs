using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Object representing list of available instances of KSP.
    /// </summary>
    public class KSPListScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen.
        /// </summary>
        /// <param name="mgr">KSP manager object for getting hte Instances</param>
        /// <param name="first">If true, this is the first screen after the splash, so Alt+X exits, else Esc exits</param>
        public KSPListScreen(KSPManager mgr, bool first = false)
        {
            manager = mgr;

            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => "Select a game instance:"
            ));

            kspList = new ConsoleListBox<KSP>(
                1, 4, -1, -2,
                manager.Instances.Values,
                new List<ConsoleListBoxColumn<KSP>>() {
                    new ConsoleListBoxColumn<KSP>() {
                        Header   = "Default",
                        Renderer = StatusSymbol,
                        Width    = 7
                    }, new ConsoleListBoxColumn<KSP>() {
                        Header   = "Name",
                        Renderer = k => InstallName(manager, k),
                        Width    = 20
                    }, new ConsoleListBoxColumn<KSP>() {
                        Header   = "Version",
                        Renderer = k => k.Version().ToString(),
                        Width    = 12
                    }, new ConsoleListBoxColumn<KSP>() {
                        Header   = "Path",
                        Renderer = k => k.GameDir(),
                        Width    = 70
                    }
                },
                1, 0, ListSortDirection.Descending
            );

            if (first) {
                AddTip("Alt+X", "Quit");
                AddBinding(Keys.AltX, (object sender) => false);
            } else {
                AddTip("Esc", "Quit");
                AddBinding(Keys.Escape, (object sender) => false);
            }

            AddTip("Enter", "Select");
            AddBinding(Keys.Enter, (object sender) => {

                ConsoleMessageDialog d = new ConsoleMessageDialog(
                    $"Loading instance {InstallName(manager, kspList.Selection)}...",
                    new List<string>()
                );

                if (TryGetInstance(kspList.Selection, () => { d.Run(() => {}); })) {
                    try {
                        manager.SetCurrentInstance(InstallName(manager, kspList.Selection));
                    } catch (Exception ex) {
                        // This can throw if the previous current instance had an error,
                        // since it gets destructed when it's replaced.
                        RaiseError(ex.Message);
                    }
                    return false;
                } else {
                    return true;
                }
            });

            kspList.AddTip("A", "Add");
            kspList.AddBinding(Keys.A, (object sender) => {
                LaunchSubScreen(new KSPAddScreen(manager));
                kspList.SetData(manager.Instances.Values);
                return true;
            });
            kspList.AddTip("R", "Remove");
            kspList.AddBinding(Keys.R, (object sender) => {
                manager.RemoveInstance(InstallName(manager, kspList.Selection));
                kspList.SetData(manager.Instances.Values);
                return true;
            });
            kspList.AddTip("E", "Edit");
            kspList.AddBinding(Keys.E, (object sender) => {
                LaunchSubScreen(new KSPEditScreen(manager, kspList.Selection));
                return true;
            });

            kspList.AddTip("D", "Default");
            kspList.AddBinding(Keys.D, (object sender) => {
                string name = InstallName(manager, kspList.Selection);
                if (name == manager.AutoStartInstance) {
                    manager.ClearAutoStart();
                } else {
                    manager.SetAutoStart(name);
                }
                return true;
            });

            AddObject(kspList);

            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => "KSP Instances";
        }

        /// <summary>
        /// Return the name of an instance of the game.
        /// It's not in an object because it's the key of the main dictionary holding them.
        /// </summary>
        /// <param name="manager">KSP manager object containing the instances</param>
        /// <param name="ksp">Game instance to look up</param>
        /// <returns>
        /// Name of the instance
        /// </returns>
        public static string InstallName(KSPManager manager, KSP ksp)
        {
            foreach (var kvp in manager.Instances) {
                if (kvp.Value == ksp) {
                    return kvp.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Try to load the registry of an instance
        /// </summary>
        /// <param name="ksp">Game instance</param>
        /// <param name="render">Function that shows a loading message</param>
        /// <returns>
        /// True if successfully loaded, false if it's locked or the registry was corrupted, etc.
        /// </returns>
        public static bool TryGetInstance(KSP ksp, Action render)
        {
            bool retry;
            do {
                try {

                    retry = false;
                    // Show loading message
                    render();
                    // Try to get the lock; this will throw if another instance is in there
                    RegistryManager.Instance(ksp);

                } catch (RegistryInUseKraken k) {

                    ConsoleMessageDialog md = new ConsoleMessageDialog(
                        $"Lock file with live process ID found at {k.lockfilePath}\n\n"
                        + "This means that another instance of CKAN probably is accessing this instance."
                        + " You can delete the file to continue, but data corruption is very likely.\n\n"
                        + "Do you want to delete this lock file to force access?",
                        new List<string>() {"Cancel", "Force"}
                    );
                    if (md.Run() == 1) {
                        // Delete it
                        File.Delete(k.lockfilePath);
                        retry = true;
                    } else {
                        // User cancelled, return failure
                        return false;
                    }

                } catch (Exception e) {

                    ConsoleMessageDialog errd = new ConsoleMessageDialog(
                        $"Error loading {Path.Combine(ksp.CkanDir(), "registry.json")}:\n{e.Message}",
                        new List<string>() {"OK"}
                    );
                    errd.Run();
                    return false;

                }

            } while (retry);

            // If we got the lock, then return success
            return true;
        }

        /// <summary>
        /// Return the list box's sort menu as the main menu
        /// </summary>
        protected override ConsolePopupMenu GetMainMenu()
        {
            if (mainMenu == null) {
                mainMenu = kspList.SortMenu();
            }
            return mainMenu;
        }

        private string StatusSymbol(KSP k)
        {
            return InstallName(manager, k) == manager.AutoStartInstance
                ? defaultMark
                : " ";
        }

        private KSPManager          manager;
        private ConsoleListBox<KSP> kspList;
        private ConsolePopupMenu    mainMenu;

        private static readonly string defaultMark = Symbols.checkmark;
    }

}
