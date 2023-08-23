using System;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void viewPlayTimeStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayTime.loadAllPlayTime(Manager);
            tabController.ShowTab("PlayTimeTabPage", 2);
            DisableMainWindow();
        }

        private void PlayTime_Done()
        {
            UpdateStatusBar();
            tabController.ShowTab("ManageModsTabPage");
            tabController.HideTab("PlayTimeTabPage");
            EnableMainWindow();
        }
    }
}
