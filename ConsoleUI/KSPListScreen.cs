using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.Versioning;
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
        /// <param name="first">If true, this is the first screen after the splash, so Ctrl+Q exits, else Esc exits</param>
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
                        Width    = 7,
                        Renderer = StatusSymbol
                    }, new ConsoleListBoxColumn<KSP>() {
                        Header   = "Name",
                        Width    = 20,
                        Renderer = k => k.Name
                    }, new ConsoleListBoxColumn<KSP>() {
                        Header   = "Version",
                        Width    = 12,
                        Renderer = k => k.Version()?.ToString() ?? noVersion,
                        Comparer = (a, b) => a.Version()?.CompareTo(b.Version() ?? KspVersion.Any) ?? 1
                    }, new ConsoleListBoxColumn<KSP>() {
                        Header   = "Path",
                        Width    = 70,
                        Renderer = k => k.GameDir()
                    }
                },
                1, 0, ListSortDirection.Descending
            );

            if (first) {
                AddTip("Ctrl+Q", "Quit");
                AddBinding(Keys.AltX,  (object sender) => false);
                AddBinding(Keys.CtrlQ, (object sender) => false);
            } else {
                AddTip("Esc", "Quit");
                AddBinding(Keys.Escape, (object sender) => false);
            }

            AddTip("Enter", "Select");
            AddBinding(Keys.Enter, (object sender) => {

                ConsoleMessageDialog d = new ConsoleMessageDialog(
                    $"Loading instance {kspList.Selection.Name}...",
                    new List<string>()
                );

                if (TryGetInstance(kspList.Selection, () => { d.Run(() => {}); })) {
                    try {
                        manager.SetCurrentInstance(kspList.Selection.Name);
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
                manager.RemoveInstance(kspList.Selection.Name);
                kspList.SetData(manager.Instances.Values);
                return true;
            });
            kspList.AddTip("E", "Edit");
            kspList.AddBinding(Keys.E, (object sender) => {

                ConsoleMessageDialog d = new ConsoleMessageDialog(
                    $"Loading instance {kspList.Selection.Name}...",
                    new List<string>()
                );
                TryGetInstance(kspList.Selection, () => { d.Run(() => {}); });
                // Still launch the screen even if the load fails,
                // because you need to be able to fix the name/path.
                LaunchSubScreen(new KSPEditScreen(manager, kspList.Selection));

                return true;
            });

            kspList.AddTip("D", "Default");
            kspList.AddBinding(Keys.D, (object sender) => {
                string name = kspList.Selection.Name;
                if (name == manager.AutoStartInstance) {
                    manager.ClearAutoStart();
                } else {
                    try {
                        manager.SetAutoStart(name);
                    } catch (NotKSPDirKraken k) {
                        ConsoleMessageDialog errd = new ConsoleMessageDialog(
                            $"Error loading {k.path}:\n{k.Message}",
                            new List<string>() {"OK"}
                        );
                        errd.Run();
                    }
                }
                return true;
            });

            AddObject(kspList);

            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => "KSP Instances";
            mainMenu     = kspList.SortMenu();
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

                } catch (NotKSPDirKraken k) {

                    ConsoleMessageDialog errd = new ConsoleMessageDialog(
                        $"Error loading {ksp.GameDir()}:\n{k.Message}",
                        new List<string>() {"OK"}
                    );
                    errd.Run();
                    return false;

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

        private string StatusSymbol(KSP k)
        {
            return k.Name == manager.AutoStartInstance
                ? defaultMark
                : " ";
        }

        private KSPManager          manager;
        private ConsoleListBox<KSP> kspList;

        private const string noVersion = "<NONE>";
        private static readonly string defaultMark = Symbols.checkmark;
    }

}
