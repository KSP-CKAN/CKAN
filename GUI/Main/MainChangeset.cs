using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void UpdateChangesDialog(List<ModChange> changeset)
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

        private void Changeset_OnCancelChanges(bool reset)
        {
            if (reset)
            {
                ManageMods.ClearChangeSet();
                UpdateChangesDialog(null);
            }
            tabController.ShowTab("ManageModsTabPage");
        }

        private void Changeset_OnConfirmChanges()
        {
            DisableMainWindow();

            // Using the changeset passed in can cause issues with versions.
            // An example is Mechjeb for FAR at 25/06/2015 with a 1.0.2 install.
            // TODO Work out why this is.
            try
            {
                Wait.StartWaiting(InstallMods, PostInstallMods, true,
                    new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                        ManageMods.mainModList
                            .ComputeUserChangeSet(RegistryManager.Instance(CurrentInstance).registry, CurrentInstance.VersionCriteria())
                            .ToList(),
                        RelationshipResolver.DependsOnlyOpts()
                    )
                );
            }
            catch (InvalidOperationException)
            {
                // Thrown if it's already busy, can happen if the user double-clicks the button. Ignore it.
                // More thread-safe than checking installWorker.IsBusy beforehand.
            }
        }
    }
}
