using System;
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
                tabController.ShowTab("WaitTabPage", 2);
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

                tabController.HideTab("WaitTabPage");
                tabController.SetActiveTab("ManageModsTabPage");

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
            Util.Invoke(statusStrip1, () => {
                StatusProgress.Visible = false;
                AddStatusMessage(statusMsg);
            });
            Util.Invoke(WaitTabPage, () => {
                RecreateDialogs();
                Wait.Finish();
                SetProgress(100);
            });
            Wait.AddLogMessage(logMsg);
            Wait.SetDescription(description);
        }

        public void SetProgress(int progress)
        {
            Wait.ProgressValue = progress;
            Wait.ProgressIndeterminate = false;
            Util.Invoke(statusStrip1, () =>
            {
                StatusProgress.Value =
                    Math.Max(StatusProgress.Minimum,
                        Math.Min(StatusProgress.Maximum, progress));
                StatusProgress.Style = ProgressBarStyle.Continuous;
            });
        }

        [ForbidGUICalls]
        public void ResetProgress()
        {
            Wait.ProgressIndeterminate = true;
            Util.Invoke(statusStrip1, () =>
            {
                StatusProgress.Style = ProgressBarStyle.Marquee;
            });
        }

        public void Wait_OnRetry()
        {
            tabController.ShowTab("ChangesetTabPage", 1);
        }

        public void Wait_OnOk()
        {
            EnableMainWindow();
            HideWaitDialog();
        }
    }
}
