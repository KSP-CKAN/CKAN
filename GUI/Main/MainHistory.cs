using System;
using System.Linq;

using Autofac;

using CKAN.Configuration;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void InstallationHistoryToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null && configuration != null)
            {
                InstallationHistory.LoadHistory(CurrentInstance, configuration, repoData);
                tabController.ShowTab(InstallationHistoryTabPage.Name, 2);
                DisableMainWindow();
            }
        }

        private void InstallationHistory_Install(CkanModule[] modules)
        {
            if (CurrentInstance != null && ManageMods.mainModList != null)
            {
                InstallationHistory_Done();
                var tuple = ManageMods.mainModList.ComputeFullChangeSetFromUserChangeSet(
                    RegistryManager.Instance(CurrentInstance, repoData).registry,
                    modules.Select(mod => new ModChange(mod, GUIModChangeType.Install,
                                                        ServiceLocator.Container.Resolve<IConfiguration>()))
                           .ToHashSet(),
                    CurrentInstance.game,
                    CurrentInstance.StabilityToleranceConfig,
                    CurrentInstance.VersionCriteria());
                UpdateChangesDialog(tuple.Item1.ToList(), tuple.Item2);
                tabController.ShowTab(ChangesetTabPage.Name, 1);
            }
        }

        private void InstallationHistory_Done()
        {
            EnableMainWindow();
            UpdateStatusBar();
            tabController.ShowTab(ManageModsTabPage.Name);
            tabController.HideTab(InstallationHistoryTabPage.Name);
        }

        private void InstallationHistory_OnSelectedModuleChanged(CkanModule m)
        {
            if (CurrentInstance != null)
            {
                ActiveModInfo = m == null
                    ? null
                    : new GUIMod(m, repoData,
                                 RegistryManager.Instance(CurrentInstance, repoData).registry,
                                 CurrentInstance.StabilityToleranceConfig,
                                 CurrentInstance.VersionCriteria(),
                                 null,
                                 configuration?.HideEpochs ?? false,
                                 configuration?.HideV      ?? false);
            }
        }

    }
}
