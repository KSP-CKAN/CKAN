using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class ChooseProvidedMods : UserControl
    {
        public ChooseProvidedMods()
        {
            InitializeComponent();
        }

        [ForbidGUICalls]
        public void LoadProviders(string message, List<CkanModule> modules, NetModuleCache cache)
        {
            Util.Invoke(this, () =>
            {
                ChooseProvidedModsLabel.Text = message;
                ChooseProvidedModsLabel.Height =
                    Util.LabelStringHeight(CreateGraphics(), ChooseProvidedModsLabel);

                ChooseProvidedModsListView.Items.Clear();
                ChooseProvidedModsListView.Items.AddRange(modules
                    .Select((module, index) => new ListViewItem(new string[]
                    {
                        cache.DescribeAvailability(module),
                        module.@abstract
                    })
                    {
                        Tag     = module,
                        Checked = index == 0,
                    })
                    .ToArray());
                ChooseProvidedModsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                ChooseProvidedModsContinueButton.Enabled =
                    (ChooseProvidedModsListView.CheckedItems.Count > 0);
            });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ChooseProvidedModsLabel.Height = Util.LabelStringHeight(CreateGraphics(), ChooseProvidedModsLabel);
        }

        [ForbidGUICalls]
        public CkanModule Wait()
        {
            task = new TaskCompletionSource<CkanModule>();
            return task.Task.Result;
        }

        public ListView.SelectedListViewItemCollection SelectedItems => ChooseProvidedModsListView.SelectedItems;

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        private void ChooseProvidedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedItemsChanged?.Invoke(ChooseProvidedModsListView.SelectedItems);
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
