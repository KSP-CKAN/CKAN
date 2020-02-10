using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        public void UpdateChangesDialog(List<ModChange> changeset)
        {
            Changeset.LoadChangeset(
                changeset,
                mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.AlertOnInstall)
                    .ToList());
        }

        private void Changeset_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }

        private void Changeset_OnCancelChanges()
        {
            ClearChangeSet();
            UpdateChangesDialog(null);
            tabController.ShowTab("ManageModsTabPage");
        }

        private void Changeset_OnConfirmChanges()
        {
            menuStrip1.Enabled = false;

            // Using the changeset passed in can cause issues with versions.
            // An example is Mechjeb for FAR at 25/06/2015 with a 1.0.2 install.
            // TODO Work out why this is.
            installWorker.RunWorkerAsync(
                new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                    mainModList.ComputeUserChangeSet(RegistryManager.Instance(Main.Instance.CurrentInstance).registry).ToList(),
                    RelationshipResolver.DependsOnlyOpts()
                )
            );
        }

        private void ClearChangeSet()
        {
            foreach (DataGridViewRow row in mainModList.full_list_of_mod_rows.Values)
            {
                GUIMod mod = row.Tag as GUIMod;
                if (mod.IsInstallChecked != mod.IsInstalled)
                {
                    mod.SetInstallChecked(row, Installed, mod.IsInstalled);
                }
                mod.SetUpgradeChecked(row, UpdateCol, false);
                mod.SetReplaceChecked(row, ReplaceCol, false);
            }
        }
    }
}
