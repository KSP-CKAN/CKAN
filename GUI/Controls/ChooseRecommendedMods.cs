using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
    public partial class ChooseRecommendedMods : UserControl
    {
        public ChooseRecommendedMods()
        {
            InitializeComponent();
        }

        public void LoadRecommendations(
            IRegistryQuerier registry, GameVersionCriteria GameVersion, NetModuleCache cache,
            Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
            Dictionary<CkanModule, List<string>> suggestions,
            Dictionary<CkanModule, HashSet<string>> supporters
        )
        {
            this.registry   = registry;
            this.GameVersion = GameVersion;
            Util.Invoke(this, () =>
            {
                RecommendedModsToggleCheckbox.Checked = true;
                RecommendedModsListView.Items.Clear();
                RecommendedModsListView.Items.AddRange(
                    getRecSugRows(cache, recommendations, suggestions, supporters).ToArray());
                MarkConflicts();
            });
        }

        public HashSet<CkanModule> Wait()
        {
            if (Platform.IsMono)
            {
                // Workaround: make sure the ListView headers are drawn
                Util.Invoke(this, () => RecommendedModsListView.EndUpdate());
            }
            task = new TaskCompletionSource<HashSet<CkanModule>>();
            return task.Task.Result;
        }

        public ListView.SelectedListViewItemCollection SelectedItems
        {
            get
            {
                return RecommendedModsListView.SelectedItems;
            }
        }

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        public event Action<string> OnConflictFound;

        private void RecommendedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnSelectedItemsChanged != null)
            {
                OnSelectedItemsChanged(RecommendedModsListView.SelectedItems);
            }
        }

        private void RecommendedModsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var module = e.Item.Tag as CkanModule;
            if (module?.IsDLC ?? false)
            {
                if (e.Item.Checked)
                {
                    e.Item.Checked = false;
                }
            }
            else
            {
                MarkConflicts();
            }
        }
        
        private void MarkConflicts()
        {
            var conflicts = FindConflicts();
            foreach (var item in RecommendedModsListView.Items.Cast<ListViewItem>()
                // Apparently ListView handes AddRange by:
                //   1. Expanding the Items list to the new size by filling it with nulls
                //   2. One by one, replace each null with a real item and call _ItemChecked
                // ... so the Items list can contain null!!
                .Where(it => it != null))
            {
                item.BackColor = conflicts.ContainsKey(item.Tag as CkanModule)
                    ? Color.LightCoral
                    : Color.Empty;
            }
            RecommendedModsContinueButton.Enabled = !conflicts.Any();
            if (OnConflictFound != null)
            {
                OnConflictFound(conflicts.Any() ? conflicts.First().Value : "");
            }
        }

        private static readonly RelationshipResolverOptions conflictOptions = new RelationshipResolverOptions()
        {
            without_toomanyprovides_kraken = true,
            proceed_with_inconsistencies   = true,
            without_enforce_consistency    = true,
            with_recommends                = false
        };

        private Dictionary<CkanModule, String> FindConflicts()
        {
            return new RelationshipResolver(
                RecommendedModsListView.CheckedItems.Cast<ListViewItem>()
                    .Select(item => item.Tag as CkanModule)
                    .Distinct(),
                new CkanModule[] { },
                conflictOptions, registry, GameVersion
            ).ConflictList;
        }

        private IEnumerable<ListViewItem> getRecSugRows(
            NetModuleCache cache,
            Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
            Dictionary<CkanModule, List<string>> suggestions,
            Dictionary<CkanModule, HashSet<string>> supporters
        )
        {
            foreach (var kvp in recommendations)
            {
                yield return getRecSugItem(cache, kvp.Key, string.Join(", ", kvp.Value.Item2),
                    RecommendationsGroup, kvp.Value.Item1);
            }

            foreach (var kvp in suggestions)
            {
                yield return getRecSugItem(cache, kvp.Key, string.Join(", ", kvp.Value),
                    SuggestionsGroup, false);
            }

            foreach (var kvp in supporters
                .ToDictionary(kvp => kvp.Key, kvp => string.Join(", ", kvp.Value.OrderBy(s => s)))
                .OrderBy(kvp => kvp.Value)
            )
            {
                yield return getRecSugItem(cache, kvp.Key, string.Join(", ", kvp.Value),
                    SupportedByGroup, false);
            }
        }

        private ListViewItem getRecSugItem(NetModuleCache cache, CkanModule module, string descrip, ListViewGroup group, bool check)
        {
            return new ListViewItem(new string[]
            {
                module.IsDLC ? module.name
                    : cache.IsMaybeCachedZip(module)
                        ? string.Format(Properties.Resources.MainChangesetCached, module.name, module.version)
                        : string.Format(Properties.Resources.MainChangesetHostSize, module.name, module.version, module.download?.Host ?? "", CkanModule.FmtSize(module.download_size)),
                descrip,
                module.@abstract
            })
            {
                Tag     = module,
                Checked = check,
                Group   = group
            };
        }

        private void RecommendedModsToggleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var state = ((CheckBox)sender).Checked;
            RecommendedModsListView.BeginUpdate();
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked != state)
                    item.Checked = state;
            }
            RecommendedModsListView.EndUpdate();
        }

        private void RecommendedModsCancelButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(null);
        }

        private void RecommendedModsContinueButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(
                RecommendedModsListView.CheckedItems.Cast<ListViewItem>()
                    .Select(item => item.Tag as CkanModule)
                    .ToHashSet()
            );
        }

        private IRegistryQuerier   registry;
        private GameVersionCriteria GameVersion;

        private TaskCompletionSource<HashSet<CkanModule>> task;
    }
}
