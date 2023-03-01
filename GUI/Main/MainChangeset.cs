using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void UpdateChangesDialog(List<ModChange> changeset, Dictionary<CkanModule, string> conflicts)
        {
            Changeset.LoadChangeset(
                changeset,
                ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.AlertOnInstall)
                    .ToList(),
                conflicts);
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
                UpdateChangesDialog(null, null);
            }
            tabController.ShowTab("ManageModsTabPage");
        }

        private void Changeset_OnConfirmChanges(List<ModChange> changeset)
        {
            DisableMainWindow();
            try
            {
                Wait.StartWaiting(InstallMods, PostInstallMods, true,
                    new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                        changeset
                            // Only pass along user requested mods, so auto-installed can be determined
                            .Where(ch => ch.Reasons.Any(r => r is SelectionReason.UserRequested)
                                // Include all removes and upgrades
                                || ch.ChangeType != GUIModChangeType.Install)
                            .ToList(),
                        RelationshipResolver.DependsOnlyOpts()));
            }
            catch (InvalidOperationException)
            {
                // Thrown if it's already busy, can happen if the user double-clicks the button. Ignore it.
                // More thread-safe than checking installWorker.IsBusy beforehand.
            }
        }
    }
}
