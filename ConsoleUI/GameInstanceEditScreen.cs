using System;
using System.Collections.Generic;
using System.ComponentModel;

using CKAN.Versioning;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for editing an existing game instance
    /// </summary>
    public class GameInstanceEditScreen : GameInstanceScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">Game instance manager containing the instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="k">Instance to edit</param>
        public GameInstanceEditScreen(GameInstanceManager mgr, RepositoryDataManager repoData, GameInstance k)
            : base(mgr, k.Name, k.GameDir())
        {
            ksp = k;
            try {
                // If we can't parse the registry, just leave the repo list blank
                regMgr   = RegistryManager.Instance(ksp, repoData);
                registry = regMgr.registry;
            } catch { }

            // Show the repositories if we can
            if (registry != null) {

                // Need to edit a copy of the list so it doesn't save on cancel
                repoEditList = new SortedDictionary<string, Repository>();
                foreach (var kvp in registry.Repositories) {
                    repoEditList.Add(kvp.Key, new Repository(
                        kvp.Value.name,
                        kvp.Value.uri.ToString(),
                        kvp.Value.priority
                    ));
                }

                // Also edit copy of the compatible versions
                compatEditList = new List<GameVersion>(ksp.GetCompatibleVersions());

                // I'm not a huge fan of this layout, but I think it's better than just a label
                AddObject(new ConsoleDoubleFrame(
                    1, repoFrameTop, -1, compatFrameBottom, compatFrameTop,
                    () => Properties.Resources.InstanceEditRepoFrameTitle,
                    () => Properties.Resources.InstanceEditCompatFrameTitle,
                    th => th.LabelFg
                ));

                repoList = new ConsoleListBox<Repository>(
                    3, repoListTop, -3, repoListBottom,
                    new List<Repository>(repoEditList.Values),
                    new List<ConsoleListBoxColumn<Repository>>() {
                        new ConsoleListBoxColumn<Repository>() {
                            Header   = Properties.Resources.InstanceEditRepoIndexHeader,
                            Renderer = r => r.priority.ToString(),
                            Width    = 7
                        }, new ConsoleListBoxColumn<Repository>() {
                            Header   = Properties.Resources.InstanceEditRepoNameHeader,
                            Renderer = r => r.name,
                            Width    = 16
                        }, new ConsoleListBoxColumn<Repository>() {
                            Header   = Properties.Resources.InstanceEditRepoURLHeader,
                            Renderer = r => r.uri.ToString(),
                            Width    = null
                        }
                    },
                    1, 0, ListSortDirection.Ascending
                );
                AddObject(repoList);
                repoList.AddTip("A", Properties.Resources.Add);
                repoList.AddBinding(Keys.A, (object sender, ConsoleTheme theme) => {
                    LaunchSubScreen(theme, new RepoAddScreen(ksp.game, repoEditList));
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });
                repoList.AddTip("R", Properties.Resources.Remove);
                repoList.AddBinding(Keys.R, (object sender, ConsoleTheme theme) => {
                    int oldPrio = repoList.Selection.priority;
                    repoEditList.Remove(repoList.Selection.name);
                    // Reshuffle the priorities to fill
                    foreach (Repository r in repoEditList.Values) {
                        if (r.priority > oldPrio) {
                            --r.priority;
                        }
                    }
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });
                repoList.AddTip("E", Properties.Resources.Edit);
                repoList.AddBinding(Keys.E, (object sender, ConsoleTheme theme) => {
                    LaunchSubScreen(theme, new RepoEditScreen(ksp.game, repoEditList, repoList.Selection));
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });
                repoList.AddTip("-", Properties.Resources.Up);
                repoList.AddBinding(Keys.Minus, (object sender, ConsoleTheme theme) => {
                    if (repoList.Selection.priority > 0) {
                        Repository prev = SortedDictFind(repoEditList,
                            r => r.priority == repoList.Selection.priority - 1);
                        if (prev != null) {
                            ++prev.priority;
                        }
                        --repoList.Selection.priority;
                        repoList.SetData(new List<Repository>(repoEditList.Values));
                    }
                    return true;
                });
                repoList.AddTip("+", Properties.Resources.Down);
                repoList.AddBinding(Keys.Plus, (object sender, ConsoleTheme theme) => {
                    Repository next = SortedDictFind(repoEditList,
                        r => r.priority == repoList.Selection.priority + 1);
                    if (next != null) {
                        --next.priority;
                    }
                    ++repoList.Selection.priority;
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });

                compatList = new ConsoleListBox<GameVersion>(
                    3, compatListTop, -3, compatListBottom,
                    compatEditList,
                    new List<ConsoleListBoxColumn<GameVersion>>() {
                        new ConsoleListBoxColumn<GameVersion>() {
                            Header   = Properties.Resources.InstanceEditCompatVersionHeader,
                            Width    = 10,
                            Renderer = v => v.ToString(),
                            Comparer = (a, b) => a.CompareTo(b)
                        }
                    },
                    0, 0, ListSortDirection.Descending
                );
                AddObject(compatList);

                compatList.AddTip("A", Properties.Resources.Add);
                compatList.AddBinding(Keys.A, (object sender, ConsoleTheme theme) => {
                    CompatibleVersionDialog vd = new CompatibleVersionDialog(ksp.game);
                    GameVersion newVersion = vd.Run(theme);
                    DrawBackground(theme);
                    if (newVersion != null && !compatEditList.Contains(newVersion)) {
                        compatEditList.Add(newVersion);
                        compatList.SetData(compatEditList);
                    }
                    return true;
                });
                compatList.AddTip("R", Properties.Resources.Remove, () => compatList.Selection != null);
                compatList.AddBinding(Keys.R, (object sender, ConsoleTheme theme) => {
                    compatEditList.Remove(compatList.Selection);
                    compatList.SetData(compatEditList);
                    return true;
                });

            } else {

                // Notify the user that the registry doesn't parse
                AddObject(new ConsoleLabel(
                    1, repoFrameTop, -1,
                    () => string.Format(Properties.Resources.InstanceEditRegistryParseError, ksp.Name)
                ));

            }
        }

        private static V SortedDictFind<K, V>(SortedDictionary<K, V> dict, Func<V, bool> pred)
        {
            foreach (var kvp in dict) {
                if (pred(kvp.Value)) {
                    return kvp.Value;
                }
            }
            return default;
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
            => Properties.Resources.InstanceEditTitle;

        /// <summary>
        /// Return whether the fields are valid.
        /// Similar to adding, except leaving the fields unchanged is allowed.
        /// </summary>
        protected override bool Valid()
            => (name.Value == ksp.Name || nameValid())
                && (path.Value == ksp.GameDir() || pathValid());

        /// <summary>
        /// Save the changes.
        /// Similar to adding, except we have to remove the old instance,
        /// and there's an API specifically for renaming.
        /// </summary>
        protected override void Save()
        {
            if (repoEditList != null) {
                // Copy the temp list of repositories to the registry
                registry.RepositoriesSet(repoEditList);
                regMgr.Save();
            }
            if (compatEditList != null) {
                ksp.SetCompatibleVersions(compatEditList);
            }

            string oldName = ksp.Name;
            if (path.Value != ksp.GameDir()) {
                // If the path is changed, then we have to remove the old instance
                // and replace it with a new one, whether or not the name is changed.
                manager.RemoveInstance(oldName);
                manager.AddInstance(path.Value, name.Value, new NullUser());
            } else if (name.Value != oldName) {
                // If only the name changed, there's an API for that.
                manager.RenameInstance(ksp.Name, name.Value);
            }
        }

        private readonly GameInstance    ksp;
        private readonly RegistryManager regMgr;
        private readonly Registry        registry;

        private readonly SortedDictionary<string, Repository> repoEditList;
        private readonly ConsoleListBox<Repository>           repoList;
        private readonly List<GameVersion>                    compatEditList;
        private readonly ConsoleListBox<GameVersion>          compatList;

        private const int repoFrameTop      = pathRow           + 2;
        private const int repoListTop       = repoFrameTop      + 2;
        private const int repoFrameHeight   = 9;
        private const int repoFrameBottom   = repoFrameTop      + repoFrameHeight - 1;
        private const int repoListBottom    = repoFrameBottom   - 2;
        private const int compatFrameTop    = repoFrameBottom;
        private const int compatFrameBottom = -1;
        private const int compatListTop     = compatFrameTop    + 2;
        private const int compatListBottom  = compatFrameBottom - 2;
    }

}
