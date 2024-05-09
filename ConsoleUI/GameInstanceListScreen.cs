using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using CKAN.Versioning;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Object representing list of available game instances.
    /// </summary>
    public class GameInstanceListScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen.
        /// </summary>
        /// <param name="mgr">Game instance manager object for getting hte Instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="first">If true, this is the first screen after the splash, so Ctrl+Q exits, else Esc exits</param>
        public GameInstanceListScreen(GameInstanceManager mgr, RepositoryDataManager repoData, bool first = false)
        {
            manager = mgr;

            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => Properties.Resources.InstanceListLabel
            ));

            instanceList = new ConsoleListBox<GameInstance>(
                1, 4, -1, -2,
                manager.Instances.Values,
                new List<ConsoleListBoxColumn<GameInstance>>() {
                    new ConsoleListBoxColumn<GameInstance>() {
                        Header   = Properties.Resources.InstanceListDefaultHeader,
                        Width    = 7,
                        Renderer = StatusSymbol
                    }, new ConsoleListBoxColumn<GameInstance>() {
                        Header   = Properties.Resources.InstanceListNameHeader,
                        Width    = 20,
                        Renderer = k => k.Name
                    }, new ConsoleListBoxColumn<GameInstance>() {
                        Header   = Properties.Resources.InstanceListGameHeader,
                        Width    = 5,
                        Renderer = k => k.game.ShortName
                    }, new ConsoleListBoxColumn<GameInstance>() {
                        Header   = Properties.Resources.InstanceListVersionHeader,
                        Width    = 12,
                        Renderer = k => k.Version()?.ToString() ?? Properties.Resources.InstanceListNoVersion,
                        Comparer = (a, b) => a.Version()?.CompareTo(b.Version() ?? GameVersion.Any) ?? 1
                    }, new ConsoleListBoxColumn<GameInstance>() {
                        Header   = Properties.Resources.InstanceListPathHeader,
                        Width    = null,
                        Renderer = k => k.GameDir()
                    }
                },
                1, 0, ListSortDirection.Descending
            );

            if (first) {
                AddTip($"{Properties.Resources.Ctrl}+Q", Properties.Resources.Quit);
                AddBinding(Keys.AltX,  (object sender, ConsoleTheme theme) => false);
                AddBinding(Keys.CtrlQ, (object sender, ConsoleTheme theme) => false);
            } else {
                AddTip(Properties.Resources.Esc, Properties.Resources.Quit);
                AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => false);
            }

            AddTip(Properties.Resources.Enter, Properties.Resources.Select);
            AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {

                ConsoleMessageDialog d = new ConsoleMessageDialog(
                    string.Format(Properties.Resources.InstanceListLoadingInstance, instanceList.Selection.Name),
                    new List<string>()
                );

                if (TryGetInstance(theme, instanceList.Selection, repoData,
                                   (ConsoleTheme th) => { d.Run(th, (ConsoleTheme thm) => {}); },
                                   null)) {
                    try {
                        manager.SetCurrentInstance(instanceList.Selection.Name);
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

            instanceList.AddTip("A", Properties.Resources.Add);
            instanceList.AddBinding(Keys.A, (object sender, ConsoleTheme theme) => {
                LaunchSubScreen(theme, new GameInstanceAddScreen(manager));
                instanceList.SetData(manager.Instances.Values);
                return true;
            });
            instanceList.AddTip("R", Properties.Resources.Remove);
            instanceList.AddBinding(Keys.R, (object sender, ConsoleTheme theme) => {
                manager.RemoveInstance(instanceList.Selection.Name);
                instanceList.SetData(manager.Instances.Values);
                return true;
            });
            instanceList.AddTip("E", Properties.Resources.Edit);
            instanceList.AddBinding(Keys.E, (object sender, ConsoleTheme theme) => {

                ConsoleMessageDialog d = new ConsoleMessageDialog(
                    string.Format(Properties.Resources.InstanceListLoadingInstance, instanceList.Selection.Name),
                    new List<string>()
                );
                TryGetInstance(theme, instanceList.Selection, repoData,
                               (ConsoleTheme th) => { d.Run(theme, (ConsoleTheme thm) => {}); },
                               null);
                // Still launch the screen even if the load fails,
                // because you need to be able to fix the name/path.
                LaunchSubScreen(theme, new GameInstanceEditScreen(manager, repoData, instanceList.Selection));

                return true;
            });

            instanceList.AddTip("D", Properties.Resources.InstanceListDefaultToggle);
            instanceList.AddBinding(Keys.D, (object sender, ConsoleTheme theme) => {
                string name = instanceList.Selection.Name;
                if (name == manager.AutoStartInstance) {
                    manager.ClearAutoStart();
                } else {
                    try {
                        manager.SetAutoStart(name);
                    } catch (NotKSPDirKraken k) {
                        ConsoleMessageDialog errd = new ConsoleMessageDialog(
                            string.Format(Properties.Resources.InstanceListLoadingError, k.path, k.Message),
                            new List<string>() { Properties.Resources.OK }
                        );
                        errd.Run(theme);
                    }
                }
                return true;
            });

            AddObject(instanceList);
            mainMenu = instanceList.SortMenu();
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"{Meta.GetProductName()} {Meta.GetVersion()}";
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return Properties.Resources.InstanceListTitle;
        }

        /// <summary>
        /// Label the menu as Sort
        /// </summary>
        protected override string MenuTip()
        {
            return Properties.Resources.Sort;
        }

        /// <summary>
        /// Try to load the registry of an instance
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="ksp">Game instance</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="render">Function that shows a loading message</param>
        /// <param name="progress">Function to call with progress updates 0-100</param>
        /// <returns>
        /// True if successfully loaded, false if it's locked or the registry was corrupted, etc.
        /// </returns>
        public static bool TryGetInstance(ConsoleTheme          theme,
                                          GameInstance          ksp,
                                          RepositoryDataManager repoData,
                                          Action<ConsoleTheme>  render,
                                          IProgress<int>        progress)
        {
            bool retry;
            do {
                try {

                    retry = false;
                    // Show loading message
                    render(theme);
                    // Try to get the lock; this will throw if another instance is in there
                    var regMgr = RegistryManager.Instance(ksp, repoData);
                    repoData.Prepopulate(regMgr.registry.Repositories.Values.ToList(),
                                         progress);
                    var compat = regMgr.registry.CompatibleModules(ksp.VersionCriteria());

                } catch (RegistryInUseKraken k) {

                    ConsoleMessageDialog md = new ConsoleMessageDialog(
                        k.ToString(),
                        new List<string>() {
                            Properties.Resources.Cancel,
                            Properties.Resources.Force
                        }
                    );
                    if (md.Run(theme) == 1) {
                        // Delete it
                        File.Delete(k.lockfilePath);
                        retry = true;
                    } else {
                        // User cancelled, return failure
                        return false;
                    }

                } catch (NotKSPDirKraken k) {

                    ConsoleMessageDialog errd = new ConsoleMessageDialog(
                        string.Format(Properties.Resources.InstanceListLoadingError, ksp.GameDir(), k.Message),
                        new List<string>() { Properties.Resources.OK }
                    );
                    errd.Run(theme);
                    return false;

                } catch (Exception e) {

                    ConsoleMessageDialog errd = new ConsoleMessageDialog(
                        string.Format(Properties.Resources.InstanceListLoadingError,
                                      Platform.FormatPath(Path.Combine(ksp.CkanDir(), "registry.json")),
                                      e.ToString()),
                        new List<string>() { Properties.Resources.OK }
                    );
                    errd.Run(theme);
                    return false;

                }

            } while (retry);

            // If we got the lock, then return success
            return true;
        }

        private string StatusSymbol(GameInstance k)
        {
            return k.Name == manager.AutoStartInstance
                ? defaultMark
                : " ";
        }

        private readonly GameInstanceManager          manager;
        private readonly ConsoleListBox<GameInstance> instanceList;

        private static readonly string defaultMark = Symbols.checkmark;
    }

}
