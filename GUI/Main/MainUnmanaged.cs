using System;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void viewUnmanagedFilesStripMenuItem_Click(object sender, EventArgs e)
        {
            UnmanagedFiles.LoadFiles(Manager.CurrentInstance, repoData, currentUser);
            tabController.ShowTab("UnmanagedFilesTabPage", 2);
            DisableMainWindow();
        }

        private void UnmanagedFiles_Done()
        {
            UpdateStatusBar();
            tabController.ShowTab("ManageModsTabPage");
            tabController.HideTab("UnmanagedFilesTabPage");
            EnableMainWindow();
        }
    }
}
