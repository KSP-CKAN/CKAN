using System;
using System.Linq;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void installationHistoryStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null && configuration != null)
            {
                InstallationHistory.LoadHistory(CurrentInstance, configuration, repoData);
                tabController.ShowTab("InstallationHistoryTabPage", 2);
            }
        }

        private void InstallationHistory_Install(CkanModule[] modules)
        {
            if (CurrentInstance != null)
            {
                InstallationHistory_Done();
                var tuple = ManageMods.mainModList.ComputeFullChangeSetFromUserChangeSet(
                    RegistryManager.Instance(CurrentInstance, repoData).registry,
                    modules.Select(mod => new ModChange(mod, GUIModChangeType.Install))
                           .ToHashSet(),
                    CurrentInstance.game,
                    CurrentInstance.VersionCriteria());
                UpdateChangesDialog(tuple.Item1.ToList(), tuple.Item2);
                tabController.ShowTab("ChangesetTabPage", 1);
            }
        }

        private void InstallationHistory_Done()
        {
            UpdateStatusBar();
            tabController.ShowTab("ManageModsTabPage");
            tabController.HideTab("InstallationHistoryTabPage");
        }

        private void InstallationHistory_OnSelectedModuleChanged(CkanModule m)
        {
            if (CurrentInstance != null)
            {
                ActiveModInfo = m == null
                    ? null
                    : new GUIMod(m, repoData,
                                 RegistryManager.Instance(CurrentInstance, repoData).registry,
                                 CurrentInstance.VersionCriteria(),
                                 null,
                                 configuration?.HideEpochs ?? false,
                                 configuration?.HideV      ?? false);
            }
        }

    }
}
