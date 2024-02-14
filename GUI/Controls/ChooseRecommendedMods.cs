using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Versioning;
using CKAN.Games;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
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
                                        IGame               game,
                                        List<ModuleLabel>   labels,
                                        GUIConfiguration    config,
                                        Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                                        Dictionary<CkanModule, List<string>>              suggestions,
                                        Dictionary<CkanModule, HashSet<string>>           supporters)
        {
            this.registry    = registry;
            this.toInstall   = toInstall;
            this.toUninstall = toUninstall;
            this.versionCrit = versionCrit;
            this.config      = config;
            Util.Invoke(this, () =>
            {
                AlwaysUncheckAllButton.Checked = config.SuppressRecommendations;
                RecommendedModsListView.BeginUpdate();
                RecommendedModsListView.ItemChecked -= RecommendedModsListView_ItemChecked;
                RecommendedModsListView.Items.AddRange(
                    getRecSugRows(cache, game,
                                  labels.Where(mlbl => mlbl.HoldVersion || mlbl.Hide)
                                        .ToArray(),
                                  recommendations, suggestions, supporters).ToArray());
                MarkConflicts();
                EnableDisableButtons();
                RecommendedModsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                RecommendedModsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                RecommendedModsListView_ColumnWidthChanged(null, null);
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecommendedModsListView_ColumnWidthChanged(null, null);
        }

        private void RecommendedModsListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs args)
        {
            if (args?.ColumnIndex != DescriptionHeader.Index)
            {
                try
                {
                    DescriptionHeader.Width =
                        RecommendedModsListView.Width
                        - ModNameHeader.Width
                        - SourceModulesHeader.Width
                        - SystemInformation.VerticalScrollBarWidth
                        - (2 * SystemInformation.BorderSize.Width);
                }
                catch
                {
                    // Don't freak out if we get a negative width
                }
            }
        }

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
                EnableDisableButtons();
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
                    RelationshipResolverOptions.ConflictsOpts(), registry, versionCrit);
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

        private IEnumerable<ListViewItem> getRecSugRows(
            NetModuleCache                                    cache,
            IGame                                             game,
            ModuleLabel[]                                     uncheckLabels,
            Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
            Dictionary<CkanModule, List<string>>              suggestions,
            Dictionary<CkanModule, HashSet<string>>           supporters)
            => recommendations.Select(kvp => getRecSugItem(cache,
                                                           kvp.Key,
                                                           string.Join(", ", kvp.Value.Item2),
                                                           RecommendationsGroup,
                                                           !config.SuppressRecommendations
                                                               && kvp.Value.Item1
                                                               && !uncheckLabels.Any(mlbl =>
                                                                   mlbl.ContainsModule(game, kvp.Key.identifier))))
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

        private void UncheckAllButton_Click(object sender, EventArgs e)
        {
            CheckUncheckRows(RecommendedModsListView.Items, false);
        }

        private void AlwaysUncheckAllButton_CheckedChanged(object sender, EventArgs e)
        {
            if (config.SuppressRecommendations != AlwaysUncheckAllButton.Checked)
            {
                config.SuppressRecommendations = AlwaysUncheckAllButton.Checked;
                config.Save();
                if (config.SuppressRecommendations)
                {
                    UncheckAllButton_Click(null, null);
                }
            }
        }

        private void CheckAllButton_Click(object sender, EventArgs e)
        {
            CheckUncheckRows(RecommendedModsListView.Items, true);
        }

        private void CheckRecommendationsButton_Click(object sender, EventArgs e)
        {
            CheckUncheckRows(RecommendationsGroup.Items, true);
        }

        private void CheckUncheckRows(ListView.ListViewItemCollection items,
                                      bool check)
        {
            RecommendedModsListView.BeginUpdate();
            RecommendedModsListView.ItemChecked -= RecommendedModsListView_ItemChecked;
            foreach (ListViewItem item in items)
            {
                if (item.Checked != check)
                {
                    item.Checked = check;
                }
            }
            MarkConflicts();
            EnableDisableButtons();
            RecommendedModsListView.EndUpdate();
            RecommendedModsListView.ItemChecked += RecommendedModsListView_ItemChecked;
        }

        private void EnableDisableButtons()
        {
            CheckAllButton.Enabled = RecommendedModsListView.Items
                                                            .OfType<ListViewItem>()
                                                            .Any(lvi => !lvi.Checked);
            CheckRecommendationsButton.Enabled = RecommendationsGroup.Items
                                                                     .OfType<ListViewItem>()
                                                                     .Any(lvi => !lvi.Checked);
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
        private GUIConfiguration    config;

        private TaskCompletionSource<HashSet<CkanModule>> task;
    }
}
