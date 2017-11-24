using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            : base(mgr, KSPListScreen.InstallName(mgr, k), k.GameDir())
        {
            ksp = k;
            try {
                // If we can't parse the registry, just leave the repo list blank
                registry = RegistryManager.Instance(ksp).registry;
            } catch { }

            // Show the repositories if we can
            if (registry != null) {

                // Need to edit a copy of the list so it doesn't save on cancel
                editList = new SortedDictionary<string, Repository>();
                foreach (var kvp in registry.Repositories) {
                    editList.Add(kvp.Key, new Repository(
                        kvp.Value.name,
                        kvp.Value.uri.ToString(),
                        kvp.Value.priority
                    ));
                }

                // I'm not a huge fan of this layout, but I think it's better than just a label
                AddObject(new ConsoleFrame(
                    1, repoLabelRow, -1, -1,
                    () => $"Mod List Sources",
                    () => ConsoleTheme.Current.LabelFg
                ));

                repoList = new ConsoleListBox<Repository>(
                    3, repoListTop, -3, -3,
                    new List<Repository>(editList.Values),
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
                    LaunchSubScreen(new RepoAddScreen(editList));
                    repoList.SetData(new List<Repository>(editList.Values));
                    return true;
                });
                repoList.AddTip("R", "Remove");
                repoList.AddBinding(Keys.R, (object sender) => {
                    int oldPrio = repoList.Selection.priority;
                    editList.Remove(repoList.Selection.name);
                    // Reshuffle the priorities to fill
                    foreach (Repository r in editList.Values) {
                        if (r.priority > oldPrio) {
                            --r.priority;
                        }
                    }
                    repoList.SetData(new List<Repository>(editList.Values));
                    return true;
                });
                repoList.AddTip("E", "Edit");
                repoList.AddBinding(Keys.E, (object sender) => {
                    LaunchSubScreen(new RepoEditScreen(editList, repoList.Selection));
                    repoList.SetData(new List<Repository>(editList.Values));
                    return true;
                });
                repoList.AddTip("-", "Up");
                repoList.AddBinding(Keys.Minus, (object sender) => {
                    if (repoList.Selection.priority > 0) {
                        Repository prev = SortedDictFind(editList,
                            r => r.priority == repoList.Selection.priority - 1);
                        if (prev != null) {
                            ++prev.priority;
                        }
                        --repoList.Selection.priority;
                        repoList.SetData(new List<Repository>(editList.Values));
                    }
                    return true;
                });
                repoList.AddTip("+", "Down");
                repoList.AddBinding(Keys.Plus, (object sender) => {
                    Repository next = SortedDictFind(editList,
                        r => r.priority == repoList.Selection.priority + 1);
                    if (next != null) {
                        --next.priority;
                    }
                    ++repoList.Selection.priority;
                    repoList.SetData(new List<Repository>(editList.Values));
                    return true;
                });

            } else {

                // Notify the user that the registry doesn't parse
                AddObject(new ConsoleLabel(
                    1, repoLabelRow, -1,
                    () => $"Failed to extract mod list sources from {KSPListScreen.InstallName(manager, ksp)}."
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
            if (name.Value != KSPListScreen.InstallName(manager, ksp)
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
            if (editList != null) {
                // Copy the temp list of repositories to the registry
                registry.Repositories = editList;
                RegistryManager.Instance(ksp).Save();
            }

            string oldName = KSPListScreen.InstallName(manager, ksp);
            if (path.Value != ksp.GameDir()) {
                // If the path is changed, then we have to remove the old instance
                // and replace it with a new one, whether or not the name is changed.
                manager.RemoveInstance(oldName);
                manager.AddInstance(name.Value, new KSP(path.Value, new NullUser()));
            } else if (name.Value != oldName) {
                // If only the name changed, there's an API for that.
                manager.RenameInstance(KSPListScreen.InstallName(manager, ksp), name.Value);
            }
        }

        private KSP        ksp;
        private Registry   registry;

        private SortedDictionary<string, Repository> editList;
        private ConsoleListBox<Repository>           repoList;

        private const int repoLabelRow = pathRow      + 2;
        private const int repoListTop  = repoLabelRow + 2;
    }

}
