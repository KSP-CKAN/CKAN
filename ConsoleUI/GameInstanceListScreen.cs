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
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="mgr">Game instance manager object for getting hte Instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="userAgent">HTTP useragent to use</param>
        /// <param name="first">If true, this is the first screen after the splash, so Ctrl+Q exits, else Esc exits</param>
        public GameInstanceListScreen(ConsoleTheme          theme,
                                      GameInstanceManager   mgr,
                                      RepositoryDataManager repoData,
                                      string?               userAgent,
                                      bool                  first = false)
            : base(theme)
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
                    new ConsoleListBoxColumn<GameInstance>(
                        Properties.Resources.InstanceListDefaultHeader,
                        StatusSymbol,
                        null,
                        7),
                    new ConsoleListBoxColumn<GameInstance>(
                        Properties.Resources.InstanceListNameHeader,
                        k => k.Name,
                        null,
                        20),
                    new ConsoleListBoxColumn<GameInstance>(
                        Properties.Resources.InstanceListGameHeader,
                        k => k.Game.ShortName,
                        null,
                        5),
                    new ConsoleListBoxColumn<GameInstance>(
                        Properties.Resources.InstanceListVersionHeader,
                        k => k.Version()?.ToString() ?? Properties.Resources.InstanceListNoVersion,
                        (a, b) => a.Version()?.CompareTo(b.Version() ?? GameVersion.Any) ?? 1,
                        12),
                    new ConsoleListBoxColumn<GameInstance>(
                        Properties.Resources.InstanceListPathHeader,
                        k => k.GameDir,
                        null,
                        null)
                },
                1, 0, ListSortDirection.Descending
            );

            if (first) {
                AddTip($"{Properties.Resources.Ctrl}+Q", Properties.Resources.Quit);
                AddBinding(Keys.AltX,  sender => false);
                AddBinding(Keys.CtrlQ, sender => false);
            } else {
                AddTip(Properties.Resources.Esc, Properties.Resources.Quit);
                AddBinding(Keys.Escape, sender => false);
            }

            AddTip(Properties.Resources.Enter, Properties.Resources.Select);
            AddBinding(Keys.Enter, sender =>
            {
                if (instanceList.Selection is GameInstance inst)
                {
                    var d = new ConsoleMessageDialog(theme, string.Format(Properties.Resources.InstanceListLoadingInstance,
                                                                   inst.Name),
                                                     new List<string>());

                    if (TryGetInstance(theme, inst, repoData,
                                       th => { d.Run(() => {}); },
                                       null)) {
                        try {
                            manager.SetCurrentInstance(inst.Name);
                        } catch (Exception ex) {
                            // This can throw if the previous current instance had an error,
                            // since it gets destructed when it's replaced.
                            RaiseError("{0}", ex.Message);
                        }
                        return false;
                    } else {
                        return true;
                    }
                }
                return true;
            });

            instanceList.AddTip("A", Properties.Resources.Add);
            instanceList.AddBinding(Keys.A, sender =>
            {
                LaunchSubScreen(new GameInstanceAddScreen(theme, manager));
                instanceList.SetData(manager.Instances.Values);
                return true;
            });
            instanceList.AddTip("R", Properties.Resources.Remove);
            instanceList.AddBinding(Keys.R, sender =>
            {
                if (instanceList.Selection is GameInstance inst)
                {
                    manager.RemoveInstance(inst.Name);
                    instanceList.SetData(manager.Instances.Values);
                }
                return true;
            });
            instanceList.AddTip("E", Properties.Resources.Edit);
            instanceList.AddBinding(Keys.E, sender =>
            {
                if (instanceList.Selection is GameInstance inst)
                {
                    var d = new ConsoleMessageDialog(
                        theme,
                        string.Format(Properties.Resources.InstanceListLoadingInstance, inst.Name),
                        new List<string>());
                    TryGetInstance(theme, inst, repoData,
                                   th => { d.Run(() => {}); },
                                   null);
                    // Still launch the screen even if the load fails,
                    // because you need to be able to fix the name/path.
                    LaunchSubScreen(new GameInstanceEditScreen(theme, manager, repoData, instanceList.Selection, userAgent));
                }
                return true;
            });

            instanceList.AddTip("D", Properties.Resources.InstanceListDefaultToggle);
            instanceList.AddBinding(Keys.D, sender =>
            {
                if (instanceList.Selection is GameInstance inst)
                {
                    string name = inst.Name;
                    if (name == manager.Configuration.AutoStartInstance) {
                        manager.ClearAutoStart();
                    } else {
                        try {
                            manager.SetAutoStart(name);
                        } catch (NotGameDirKraken k) {
                            var errd = new ConsoleMessageDialog(
                                theme,
                                string.Format(Properties.Resources.InstanceListLoadingError, k.path, k.Message),
                                new List<string>() { Properties.Resources.OK }
                            );
                            errd.Run();
                        }
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
            => $"{Meta.ProductName} {Meta.GetVersion()}";

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
            => Properties.Resources.InstanceListTitle;

        /// <summary>
        /// Label the menu as Sort
        /// </summary>
        protected override string MenuTip()
            => Properties.Resources.Sort;

        /// <summary>
        /// Try to load the registry of an instance
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="inst">Game instance</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="render">Function that shows a loading message</param>
        /// <param name="progress">Function to call with progress updates 0-100</param>
        /// <returns>
        /// True if successfully loaded, false if it's locked or the registry was corrupted, etc.
        /// </returns>
        public static bool TryGetInstance(ConsoleTheme          theme,
                                          GameInstance          inst,
                                          RepositoryDataManager repoData,
                                          Action<ConsoleTheme>  render,
                                          IProgress<int>?       progress)
        {
            bool retry;
            do {
                try {

                    retry = false;
                    // Show loading message
                    render(theme);
                    // Try to get the lock; this will throw if another instance is in there
                    var regMgr = RegistryManager.Instance(inst, repoData);
                    repoData.Prepopulate(regMgr.registry.Repositories.Values.ToList(),
                                         progress);
                    var compat = regMgr.registry.CompatibleModules(inst.StabilityToleranceConfig, inst.VersionCriteria());

                } catch (RegistryInUseKraken k) {

                    ConsoleMessageDialog md = new ConsoleMessageDialog(
                        theme,
                        k.Message,
                        new List<string>() {
                            Properties.Resources.Cancel,
                            Properties.Resources.Force
                        }
                    );
                    if (md.Run() == 1) {
                        // Delete it
                        File.Delete(k.lockfilePath);
                        retry = true;
                    } else {
                        // User cancelled, return failure
                        return false;
                    }

                } catch (NotGameDirKraken k) {

                    ConsoleMessageDialog errd = new ConsoleMessageDialog(
                        theme,
                        string.Format(Properties.Resources.InstanceListLoadingError, inst.GameDir, k.Message),
                        new List<string>() { Properties.Resources.OK }
                    );
                    errd.Run();
                    return false;

                } catch (Exception e) {

                    ConsoleMessageDialog errd = new ConsoleMessageDialog(
                        theme,
                        string.Format(Properties.Resources.InstanceListLoadingError,
                                      Platform.FormatPath(Path.Combine(inst.CkanDir, "registry.json")),
                                      e.ToString()),
                        new List<string>() { Properties.Resources.OK }
                    );
                    errd.Run();
                    return false;

                }

            } while (retry);

            // If we got the lock, then return success
            return true;
        }

        private string StatusSymbol(GameInstance k)
        {
            return k.Name == manager.Configuration.AutoStartInstance
                ? defaultMark
                : " ";
        }

        private readonly GameInstanceManager          manager;
        private readonly ConsoleListBox<GameInstance> instanceList;

        private static readonly string defaultMark = Symbols.checkmark;
    }

}
