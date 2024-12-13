using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Main
    {
        [ForbidGUICalls]
        public void ShowWaitDialog()
        {
            Util.Invoke(this, () =>
            {
                tabController.ShowTab(WaitTabPage.Name, 2);
                StatusProgress.Value = 0;
                StatusProgress.Style = ProgressBarStyle.Marquee;
                StatusProgress.Visible = true;
            });
        }

        public void HideWaitDialog()
        {
            Util.Invoke(this, () =>
            {
                Wait.Finish();
                RecreateDialogs();

                tabController.HideTab(WaitTabPage.Name);
                tabController.SetActiveTab(ManageModsTabPage.Name);

                StatusProgress.Value = 0;
                StatusProgress.Style = ProgressBarStyle.Continuous;
                StatusProgress.Visible = false;
            });
        }

        /// <summary>
        /// Stay on log page and reset StatusProgress
        /// </summary>
        /// <param name="statusMsg">Message for the lower status bar</param>
        /// <param name="logMsg">Message to display on WaitDialog-Log (not the real log!)</param>
        /// <param name="description">Message displayed above the DialogProgress bar</param>
        public void FailWaitDialog(string statusMsg, string logMsg, string description)
        {
            Util.Invoke(this, () =>
            {
                StatusProgress.Visible = false;
                currentUser.RaiseMessage("{0}", statusMsg);
                RecreateDialogs();
                Wait.Finish();
            });
            currentUser.RaiseMessage("{0}", logMsg);
            Wait.SetDescription(description);
        }

        public void Wait_OnRetry()
        {
            EnableMainWindow();
            tabController.ShowTab(ChangesetTabPage.Name, 1);
        }

        public void Wait_OnOk()
        {
            EnableMainWindow();
            HideWaitDialog();
        }
    }
}
