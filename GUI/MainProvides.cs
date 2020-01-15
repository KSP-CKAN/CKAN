using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        // Ugly Hack. Possible fix is to alter the relationship provider so we can use a loop
        // over reason for to find a user requested mod. Or, you know, pass in a handler to it.
        private readonly ConcurrentStack<GUIMod> last_mod_to_have_install_toggled = new ConcurrentStack<GUIMod>();

        public async Task<CkanModule> TooManyModsProvide(TooManyModsProvideKraken kraken)
        {
            // We want LMtHIT to be the last user selection. If we alter this handling a too many provides
            // it needs to be reset so a potential second too many provides doesn't use the wrong mod.
            GUIMod mod;

            var module = await TooManyModsProvideCore(kraken);

            if (module == null
                    && last_mod_to_have_install_toggled.TryPeek(out mod))
            {
                MarkModForInstall(mod.Identifier, true);
            }
            Util.Invoke(this, () =>
            {
                tabController.SetTabLock(false);
                tabController.HideTab("ChooseProvidedModsTabPage");
                tabController.ShowTab("ManageModsTabPage");
            });

            last_mod_to_have_install_toggled.TryPop(out mod);
            return module;
        }

        private async Task<CkanModule> TooManyModsProvideCore(TooManyModsProvideKraken kraken)
        {
            TaskCompletionSource<CkanModule> task = new TaskCompletionSource<CkanModule>();
            Util.Invoke(this, () =>
            {
                UpdateProvidedModsDialog(kraken, task);
                tabController.ShowTab("ChooseProvidedModsTabPage", 3);
                tabController.SetTabLock(true);
            });
            return await task.Task;
        }

        private TaskCompletionSource<CkanModule> toomany_source;
		
        private void UpdateProvidedModsDialog(TooManyModsProvideKraken tooManyProvides, TaskCompletionSource<CkanModule> task)
        {
            toomany_source = task;
            ChooseProvidedModsLabel.Text = String.Format(
                Properties.Resources.MainInstallProvidedBy,
                tooManyProvides.requested
            );

            ChooseProvidedModsListView.Items.Clear();

            ChooseProvidedModsListView.ItemChecked += ChooseProvidedModsListView_ItemChecked;

            foreach (CkanModule module in tooManyProvides.modules)
            {
                ChooseProvidedModsListView.Items.Add(new ListViewItem(new string[]
                {
                    Manager.Cache.IsMaybeCachedZip(module)
                        ? string.Format(Properties.Resources.MainChangesetCached, module.name, module.version)
                        : string.Format(Properties.Resources.MainChangesetHostSize, module.name, module.version, module.download.Host ?? "", CkanModule.FmtSize(module.download_size)),
                    module.@abstract
                })
                {
                    Tag = module,
                    Checked = false
                });
            }
            ChooseProvidedModsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ChooseProvidedModsContinueButton.Enabled = false;
        }

        private void ChooseProvidedModsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var any_item_selected = ChooseProvidedModsListView.Items.Cast<ListViewItem>().Any(item => item.Checked);
            ChooseProvidedModsContinueButton.Enabled = any_item_selected;
            if (!e.Item.Checked)
            {
                return;
            }

            foreach (ListViewItem item in ChooseProvidedModsListView.Items.Cast<ListViewItem>()
                .Where(item => item != e.Item && item.Checked))
            {
                item.Checked = false;
            }
        }

        private void ChooseProvidedModsCancelButton_Click(object sender, EventArgs e)
        {
            toomany_source.SetResult(null);
        }

        private void ChooseProvidedModsContinueButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in ChooseProvidedModsListView.Items)
            {
                if (item.Checked)
                {
                    toomany_source.SetResult((CkanModule)item.Tag);
                }
            }
        }

	}
}
