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
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="mgr">Game instance manager containing the instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="inst">Instance to edit</param>
        /// <param name="userAgent">HTTP useragent string to use</param>
        public GameInstanceEditScreen(ConsoleTheme          theme,
                                      GameInstanceManager   mgr,
                                      RepositoryDataManager repoData,
                                      GameInstance          inst,
                                      string?               userAgent)
            : base(theme, mgr, inst.Name, Platform.FormatPath(inst.GameDir))
        {
            instance = inst;
            try {
                // If we can't parse the registry, just leave the repo list blank
                regMgr   = RegistryManager.Instance(instance, repoData);
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
                compatEditList = new List<GameVersion>(instance.CompatibleVersions);

                stabilityToleranceButtons = new ReleaseStatusComboButtons(
                    1, stabilityToleranceTop,
                    Properties.Resources.InstanceEditStabilityToleranceHeader,
                    null,
                    instance.StabilityToleranceConfig.OverallStabilityTolerance);
                AddObject(stabilityToleranceButtons);

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
                        new ConsoleListBoxColumn<Repository>(
                            Properties.Resources.InstanceEditRepoIndexHeader,
                            r => r.priority.ToString(),
                            null,
                            7),
                        new ConsoleListBoxColumn<Repository>(
                            Properties.Resources.InstanceEditRepoNameHeader,
                            r => r.name,
                            null,
                            16),
                        new ConsoleListBoxColumn<Repository>(
                            Properties.Resources.InstanceEditRepoURLHeader,
                            r => r.uri.ToString(),
                            null,
                            null)
                    },
                    1, 0, ListSortDirection.Ascending
                );
                AddObject(repoList);
                repoList.AddTip("A", Properties.Resources.Add);
                repoList.AddBinding(Keys.A, sender =>
                {
                    LaunchSubScreen(new RepoAddScreen(theme, instance.Game, repoEditList, userAgent));
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });
                repoList.AddTip("R", Properties.Resources.Remove);
                repoList.AddBinding(Keys.R, sender =>
                {
                    if (repoList.Selection is Repository repo)
                    {
                        int oldPrio = repo.priority;
                        repoEditList.Remove(repo.name);
                        // Reshuffle the priorities to fill
                        foreach (Repository r in repoEditList.Values) {
                            if (r.priority > oldPrio) {
                                --r.priority;
                            }
                        }
                        repoList.SetData(new List<Repository>(repoEditList.Values));
                    }
                    return true;
                });
                repoList.AddTip("E", Properties.Resources.Edit);
                repoList.AddBinding(Keys.E, sender =>
                {
                    if (repoList.Selection is Repository repo)
                    {
                        LaunchSubScreen(new RepoEditScreen(theme, instance.Game, repoEditList, repo, userAgent));
                        repoList.SetData(new List<Repository>(repoEditList.Values));
                    }
                    return true;
                });
                repoList.AddTip("-", Properties.Resources.Up);
                repoList.AddBinding(Keys.Minus, sender =>
                {
                    if (repoList.Selection is Repository repo)
                    {
                        if (repo.priority > 0) {
                            var prev = SortedDictFind(repoEditList,
                                r => r.priority == repo.priority - 1);
                            if (prev != null) {
                                ++prev.priority;
                            }
                            --repo.priority;
                            repoList.SetData(new List<Repository>(repoEditList.Values));
                        }
                    }
                    return true;
                });
                repoList.AddTip("+", Properties.Resources.Down);
                repoList.AddBinding(Keys.Plus, sender =>
                {
                    if (repoList.Selection is Repository repo)
                    {
                        var next = SortedDictFind(repoEditList,
                            r => r.priority == repo.priority + 1);
                        if (next != null) {
                            --next.priority;
                        }
                        ++repo.priority;
                        repoList.SetData(new List<Repository>(repoEditList.Values));
                    }
                    return true;
                });

                compatList = new ConsoleListBox<GameVersion>(
                    3, compatListTop, -3, compatListBottom,
                    compatEditList,
                    new List<ConsoleListBoxColumn<GameVersion>>() {
                        new ConsoleListBoxColumn<GameVersion>(
                            Properties.Resources.InstanceEditCompatVersionHeader,
                            v => v.ToString() ?? "",
                            (a, b) => a.CompareTo(b),
                            10)
                    },
                    0, 0, ListSortDirection.Descending
                );
                AddObject(compatList);

                compatList.AddTip("A", Properties.Resources.Add);
                compatList.AddBinding(Keys.A, sender =>
                {
                    CompatibleVersionDialog vd = new CompatibleVersionDialog(theme, instance.Game);
                    var newVersion = vd.Run();
                    DrawBackground();
                    if (newVersion != null && !compatEditList.Contains(newVersion)) {
                        compatEditList.Add(newVersion);
                        compatList.SetData(compatEditList);
                    }
                    return true;
                });
                compatList.AddTip("R", Properties.Resources.Remove, () => compatList.Selection != null);
                compatList.AddBinding(Keys.R, sender =>
                {
                    if (compatList.Selection is GameVersion gv)
                    {
                        compatEditList.Remove(gv);
                        compatList.SetData(compatEditList);
                    }
                    return true;
                });

            } else {

                // Notify the user that the registry doesn't parse
                AddObject(new ConsoleLabel(
                    1, repoFrameTop, -1,
                    () => string.Format(Properties.Resources.InstanceEditRegistryParseError, instance.Name)
                ));

            }
        }

        private static V? SortedDictFind<K, V>(SortedDictionary<K, V> dict, Func<V, bool> pred) where K: class
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
            => (name.Value == instance.Name || nameValid())
                && (path.Value == instance.GameDir || pathValid());

        /// <summary>
        /// Save the changes.
        /// Similar to adding, except we have to remove the old instance,
        /// and there's an API specifically for renaming.
        /// </summary>
        protected override void Save()
        {
            if (stabilityToleranceButtons?.Selection != null)
            {
                instance.StabilityToleranceConfig.OverallStabilityTolerance =
                    stabilityToleranceButtons.Selection.Value;
            }
            if (repoEditList != null) {
                // Copy the temp list of repositories to the registry
                registry?.RepositoriesSet(repoEditList);
                regMgr?.Save();
            }
            if (compatEditList != null) {
                instance.SetCompatibleVersions(compatEditList);
            }

            string oldName = instance.Name;
            if (path.Value != instance.GameDir) {
                // If the path is changed, then we have to remove the old instance
                // and replace it with a new one, whether or not the name is changed.
                manager.RemoveInstance(oldName);
                manager.AddInstance(path.Value, name.Value, new NullUser());
            } else if (name.Value != oldName) {
                // If only the name changed, there's an API for that.
                manager.RenameInstance(instance.Name, name.Value);
            }
        }

        private readonly GameInstance     instance;
        private readonly RegistryManager? regMgr;
        private readonly Registry?        registry;

        private readonly SortedDictionary<string, Repository>? repoEditList;
        private readonly ConsoleListBox<Repository>?           repoList;
        private readonly List<GameVersion>?                    compatEditList;
        private readonly ConsoleListBox<GameVersion>?          compatList;
        private readonly ReleaseStatusComboButtons?            stabilityToleranceButtons;

        private const int stabilityToleranceTop    = pathRow + 2;
        private const int stabilityToleranceHeight = 4;
        private const int repoFrameTop      = stabilityToleranceTop + stabilityToleranceHeight + 1;
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
