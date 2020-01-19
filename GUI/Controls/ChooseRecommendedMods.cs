using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CKAN.Extensions;

namespace CKAN
{
    public partial class ChooseRecommendedMods : UserControl
    {
        public ChooseRecommendedMods()
        {
            InitializeComponent();
        }

        public void LoadRecommendations(
            NetModuleCache cache,
            Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
            Dictionary<CkanModule, List<string>> suggestions,
            Dictionary<CkanModule, HashSet<string>> supporters
        )
        {
            Util.Invoke(this, () =>
            {
                RecommendedModsToggleCheckbox.Checked = true;
                RecommendedModsListView.Items.Clear();
                RecommendedModsListView.Items.AddRange(
                    getRecSugRows(cache, recommendations, suggestions, supporters).ToArray());
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

        private void RecommendedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnSelectedItemsChanged != null)
            {
                OnSelectedItemsChanged(RecommendedModsListView.SelectedItems);
            }
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
                cache.IsMaybeCachedZip(module)
                    ? string.Format(Properties.Resources.MainChangesetCached, module.name, module.version)
                    : string.Format(Properties.Resources.MainChangesetHostSize, module.name, module.version, module.download.Host ?? "", CkanModule.FmtSize(module.download_size)),
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

        private TaskCompletionSource<HashSet<CkanModule>> task;
    }
}
