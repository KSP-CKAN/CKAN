using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void viewPlayTimeStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayTime.loadAllPlayTime(manager);
            menuStrip1.Enabled = false;
            tabController.ShowTab("PlayTimeTabPage", 2);
            tabController.SetTabLock(true);
        }

        private void PlayTime_Done()
        {
            UpdateStatusBar();
            tabController.ShowTab("ManageModsTabPage");
            tabController.HideTab("PlayTimeTabPage");
            tabController.SetTabLock(false);
            menuStrip1.Enabled = true;
        }
    }
}
