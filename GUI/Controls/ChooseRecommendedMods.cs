using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using CKAN.Versioning;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public partial class ChooseRecommendedMods : UserControl
    {
        public ChooseRecommendedMods()
        {
            InitializeComponent();
        }

        [ForbidGUICalls]
        public void LoadRecommendations(IRegistryQuerier    registry,
                                        List<CkanModule>    toInstall,
                                        HashSet<CkanModule> toUninstall,
                                        GameVersionCriteria versionCrit,
                                        NetModuleCache      cache,
                                        Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                                        Dictionary<CkanModule, List<string>>              suggestions,
                                        Dictionary<CkanModule, HashSet<string>>           supporters)
        {
            this.registry    = registry;
            this.toInstall   = toInstall;
            this.toUninstall = toUninstall;
            this.versionCrit = versionCrit;
            Util.Invoke(this, () =>
            {
                RecommendedModsToggleCheckbox.Checked = true;
                RecommendedModsListView.BeginUpdate();
                RecommendedModsListView.ItemChecked -= RecommendedModsListView_ItemChecked;
                RecommendedModsListView.Items.AddRange(
                    getRecSugRows(cache, recommendations, suggestions, supporters).ToArray());
                MarkConflicts();
                RecommendedModsListView.EndUpdate();
                // Don't set this before AddRange, it will fire for every row we add!
                RecommendedModsListView.ItemChecked += RecommendedModsListView_ItemChecked;
            });
        }

        [ForbidGUICalls]
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
            => RecommendedModsListView.SelectedItems;

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        public event Action<string> OnConflictFound;

        private void RecommendedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedItemsChanged?.Invoke(RecommendedModsListView.SelectedItems);
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
            try
            {
                var resolver = new RelationshipResolver(
                    RecommendedModsListView.CheckedItems
                                           .Cast<ListViewItem>()
                                           .Select(item => item.Tag as CkanModule)
                                           .Concat(toInstall)
                                           .Distinct(),
                    toUninstall,
                    conflictOptions, registry, versionCrit);
                var conflicts = resolver.ConflictList;
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
                OnConflictFound?.Invoke(string.Join("; ", resolver.ConflictDescriptions));
            }
            catch (DependencyNotSatisfiedKraken k)
            {
                var row = RecommendedModsListView.Items
                                                 .Cast<ListViewItem>()
                                                 .FirstOrDefault(it => (it?.Tag as CkanModule) == k.parent);
                if (row != null)
                {
                    row.BackColor = Color.LightCoral;
                }
                RecommendedModsContinueButton.Enabled = false;
                OnConflictFound?.Invoke(k.Message);
            }
        }

        private static readonly RelationshipResolverOptions conflictOptions = new RelationshipResolverOptions()
        {
            without_toomanyprovides_kraken = true,
            proceed_with_inconsistencies   = true,
            without_enforce_consistency    = true,
            with_recommends                = false
        };

        private IEnumerable<ListViewItem> getRecSugRows(
            NetModuleCache                                    cache,
            Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
            Dictionary<CkanModule, List<string>>              suggestions,
            Dictionary<CkanModule, HashSet<string>>           supporters)
            => recommendations.Select(kvp => getRecSugItem(cache,
                                                           kvp.Key,
                                                           string.Join(", ", kvp.Value.Item2),
                                                           RecommendationsGroup,
                                                           kvp.Value.Item1))
                              .OrderBy(item => item.SubItems[1].Text)
                              .Concat(suggestions.Select(kvp => getRecSugItem(cache,
                                                                              kvp.Key,
                                                                              string.Join(", ", kvp.Value),
                                                                              SuggestionsGroup,
                                                                              false))
                                                 .OrderBy(item => item.SubItems[1].Text))
                              .Concat(supporters.Select(kvp => getRecSugItem(cache,
                                                                             kvp.Key,
                                                                             string.Join(", ", kvp.Value.OrderBy(s => s)),
                                                                             SupportedByGroup,
                                                                             false))
                                                .OrderBy(item => item.SubItems[1].Text));

        private ListViewItem getRecSugItem(NetModuleCache cache,
                                           CkanModule     module,
                                           string         descrip,
                                           ListViewGroup  group,
                                           bool           check)
        => new ListViewItem(new string[]
            {
                module.IsDLC ? module.name : cache.DescribeAvailability(module),
                descrip,
                module.@abstract
            })
            {
                Tag     = module,
                Checked = check,
                Group   = group
            };

        private void RecommendedModsToggleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var state = ((CheckBox)sender).Checked;
            RecommendedModsListView.BeginUpdate();
            RecommendedModsListView.ItemChecked -= RecommendedModsListView_ItemChecked;
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked != state)
                {
                    item.Checked = state;
                }
            }
            MarkConflicts();
            RecommendedModsListView.EndUpdate();
            RecommendedModsListView.ItemChecked += RecommendedModsListView_ItemChecked;
        }

        private void RecommendedModsCancelButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(null);
            RecommendedModsListView.Items.Clear();
            RecommendedModsListView.ItemChecked -= RecommendedModsListView_ItemChecked;
        }

        private void RecommendedModsContinueButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(RecommendedModsListView.CheckedItems
                                                   .Cast<ListViewItem>()
                                                   .Select(item => item.Tag as CkanModule)
                                                   .ToHashSet());
            RecommendedModsListView.Items.Clear();
            RecommendedModsListView.ItemChecked -= RecommendedModsListView_ItemChecked;
        }

        private IRegistryQuerier    registry;
        private List<CkanModule>    toInstall;
        private HashSet<CkanModule> toUninstall;
        private GameVersionCriteria versionCrit;

        private TaskCompletionSource<HashSet<CkanModule>> task;
    }
}
