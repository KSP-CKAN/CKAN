using System;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.Versioning;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for editing an existing game instance
    /// </summary>
    public class KSPEditScreen : KSPScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="mgr">KSP manager containing the instances</param>
        /// <param name="k">Instance to edit</param>
        public KSPEditScreen(KSPManager mgr, KSP k)
            : base(mgr, k.Name, k.GameDir())
        {
            ksp = k;
            try {
                // If we can't parse the registry, just leave the repo list blank
                registry = RegistryManager.Instance(ksp).registry;
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
                compatEditList = new List<KspVersion>(ksp.GetCompatibleVersions());

                // I'm not a huge fan of this layout, but I think it's better than just a label
                AddObject(new ConsoleDoubleFrame(
                    1, repoFrameTop, -1, compatFrameBottom, compatFrameTop,
                    () => $"Mod List Sources",
                    () => $"Additional Compatible Versions",
                    () => ConsoleTheme.Current.LabelFg
                ));

                repoList = new ConsoleListBox<Repository>(
                    3, repoListTop, -3, repoListBottom,
                    new List<Repository>(repoEditList.Values),
                    new List<ConsoleListBoxColumn<Repository>>() {
                        new ConsoleListBoxColumn<Repository>() {
                            Header   = "Index",
                            Renderer = r => r.priority.ToString(),
                            Width    = 7
                        }, new ConsoleListBoxColumn<Repository>() {
                            Header   = "Name",
                            Renderer = r => r.name,
                            Width    = 16
                        }, new ConsoleListBoxColumn<Repository>() {
                            Header   = "URL",
                            Renderer = r => r.uri.ToString(),
                            Width    = 50
                        }
                    },
                    1, 0, ListSortDirection.Ascending
                );
                AddObject(repoList);
                repoList.AddTip("A", "Add");
                repoList.AddBinding(Keys.A, (object sender) => {
                    LaunchSubScreen(new RepoAddScreen(repoEditList));
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });
                repoList.AddTip("R", "Remove");
                repoList.AddBinding(Keys.R, (object sender) => {
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
                repoList.AddTip("E", "Edit");
                repoList.AddBinding(Keys.E, (object sender) => {
                    LaunchSubScreen(new RepoEditScreen(repoEditList, repoList.Selection));
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });
                repoList.AddTip("-", "Up");
                repoList.AddBinding(Keys.Minus, (object sender) => {
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
                repoList.AddTip("+", "Down");
                repoList.AddBinding(Keys.Plus, (object sender) => {
                    Repository next = SortedDictFind(repoEditList,
                        r => r.priority == repoList.Selection.priority + 1);
                    if (next != null) {
                        --next.priority;
                    }
                    ++repoList.Selection.priority;
                    repoList.SetData(new List<Repository>(repoEditList.Values));
                    return true;
                });

                compatList = new ConsoleListBox<KspVersion>(
                    3, compatListTop, -3, compatListBottom,
                    compatEditList,
                    new List<ConsoleListBoxColumn<KspVersion>>() {
                        new ConsoleListBoxColumn<KspVersion>() {
                            Header   = "Version",
                            Width    = 10,
                            Renderer = v => v.ToString(),
                            Comparer = (a, b) => a.CompareTo(b)
                        }
                    },
                    0, 0, ListSortDirection.Descending
                );
                AddObject(compatList);

                compatList.AddTip("A", "Add");
                compatList.AddBinding(Keys.A, (object sender) => {
                    CompatibleVersionDialog vd = new CompatibleVersionDialog();
                    KspVersion newVersion = vd.Run();
                    DrawBackground();
                    if (newVersion != null && !compatEditList.Contains(newVersion)) {
                        compatEditList.Add(newVersion);
                        compatList.SetData(compatEditList);
                    }
                    return true;
                });
                compatList.AddTip("R", "Remove", () => compatList.Selection != null);
                compatList.AddBinding(Keys.R, (object sender) => {
                    compatEditList.Remove(compatList.Selection);
                    compatList.SetData(compatEditList);
                    return true;
                });

            } else {

                // Notify the user that the registry doesn't parse
                AddObject(new ConsoleLabel(
                    1, repoFrameTop, -1,
                    () => $"Failed to extract mod list sources from {ksp.Name}."
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
            return default(V);
        }

        /// <summary>
        /// Return whether the fields are valid.
        /// Similar to adding, except leaving the fields unchanged is allowed.
        /// </summary>
        protected override bool Valid()
        {
            if (name.Value != ksp.Name
                    && !nameValid()) {
                return false;
            }
            if (path.Value != ksp.GameDir()
                    && !pathValid()) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the changes.
        /// Similar to adding, except we have to remove the old instance,
        /// and there's an API specifically for renaming.
        /// </summary>
        protected override void Save()
        {
            if (repoEditList != null) {
                // Copy the temp list of repositories to the registry
                registry.Repositories = repoEditList;
                RegistryManager.Instance(ksp).Save();
            }
            if (compatEditList != null) {
                ksp.SetCompatibleVersions(compatEditList);
            }

            string oldName = ksp.Name;
            if (path.Value != ksp.GameDir()) {
                // If the path is changed, then we have to remove the old instance
                // and replace it with a new one, whether or not the name is changed.
                manager.RemoveInstance(oldName);
                manager.AddInstance(new KSP(path.Value, name.Value, new NullUser()));
            } else if (name.Value != oldName) {
                // If only the name changed, there's an API for that.
                manager.RenameInstance(ksp.Name, name.Value);
            }
        }

        private KSP        ksp;
        private Registry   registry;

        private SortedDictionary<string, Repository> repoEditList;
        private ConsoleListBox<Repository>           repoList;
        private List<KspVersion>                     compatEditList;
        private ConsoleListBox<KspVersion>           compatList;

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
