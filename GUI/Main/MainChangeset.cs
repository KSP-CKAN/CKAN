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
                ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.AlertOnInstall)
                    .ToList());
        }

        private void Changeset_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }

        private void Changeset_OnCancelChanges()
        {
            ManageMods.ClearChangeSet();
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
                    ManageMods.mainModList.ComputeUserChangeSet(RegistryManager.Instance(Main.Instance.CurrentInstance).registry).ToList(),
                    RelationshipResolver.DependsOnlyOpts()
                )
            );
        }

    }
}
