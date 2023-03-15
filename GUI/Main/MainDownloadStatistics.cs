using System;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void DownloadStatisticsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (Manager.Cache != null)
            {
                DownloadStatistics.SetData(Manager.Cache,
                                           RegistryManager.Instance(CurrentInstance!,
                                                                    repoData)
                                                          .registry);
                tabController.ShowTab(DownloadStatisticsTabPage.Name, 2);
                DisableMainWindow();
            }
        }

        private void DownloadStatisticsOKButton_Click(object? sender, EventArgs? e)
        {
            EnableMainWindow();
            tabController.ShowTab(ManageModsTabPage.Name);
            tabController.HideTab(DownloadStatisticsTabPage.Name);
        }

    }
}
