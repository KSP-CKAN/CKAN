using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ChooseProvidedMods : UserControl
    {
        public ChooseProvidedMods()
        {
            InitializeComponent();
        }

        public void LoadProviders(string requested, List<CkanModule> modules, NetModuleCache cache)
        {
            Util.Invoke(this, () =>
            {
                ChooseProvidedModsLabel.Text = String.Format(
                    Properties.Resources.MainInstallProvidedBy,
                    requested
                );
    
                ChooseProvidedModsListView.Items.Clear();
                ChooseProvidedModsListView.Items.AddRange(modules
                    .Select(module => new ListViewItem(new string[]
                    {
                        cache.IsMaybeCachedZip(module)
                            ? string.Format(Properties.Resources.MainChangesetCached, module.name, module.version)
                            : string.Format(Properties.Resources.MainChangesetHostSize, module.name, module.version, module.download.Host ?? "", CkanModule.FmtSize(module.download_size)),
                        module.@abstract
                    })
                    {
                        Tag = module,
                        Checked = false
                    })
                    .ToArray());
                ChooseProvidedModsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                ChooseProvidedModsContinueButton.Enabled = false;
            });
        }

        public CkanModule Wait()
        {
            task = new TaskCompletionSource<CkanModule>();
            return task.Task.Result;
        }

        public ListView.SelectedListViewItemCollection SelectedItems
        {
            get
            {
                return ChooseProvidedModsListView.SelectedItems;
            }
        }

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        private void ChooseProvidedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnSelectedItemsChanged != null)
            {
                OnSelectedItemsChanged(ChooseProvidedModsListView.SelectedItems);
            }
        }

        private void ChooseProvidedModsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ChooseProvidedModsContinueButton.Enabled =
                (ChooseProvidedModsListView.CheckedItems.Count > 0);

            if (e.Item.Checked)
            {
                foreach (ListViewItem item in ChooseProvidedModsListView.CheckedItems.Cast<ListViewItem>()
                    .Where(item => item != e.Item))
                {
                    item.Checked = false;
                }
            }
        }

        private void ChooseProvidedModsCancelButton_Click(object sender, EventArgs e)
        {
            task.SetResult(null);
        }

        private void ChooseProvidedModsContinueButton_Click(object sender, EventArgs e)
        {
            task.SetResult(ChooseProvidedModsListView.CheckedItems.Cast<ListViewItem>()
                .Select(item => item?.Tag as CkanModule)
                .FirstOrDefault());
        }

        private TaskCompletionSource<CkanModule> task;
    }
}
