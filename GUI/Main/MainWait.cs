using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        private Action cancelCallback;

        public void ShowWaitDialog(bool cancelable = true)
        {
            Util.Invoke(this, () =>
            {
                tabController.ShowTab("WaitTabPage", 2);
                Wait.Reset(cancelable);
                StatusProgress.Value = 0;
                StatusProgress.Style = ProgressBarStyle.Marquee;
                StatusProgress.Visible = true;
            });
        }

        public void HideWaitDialog(bool success)
        {
            Util.Invoke(this, () =>
            {
                Wait.Finish(success);
                RecreateDialogs();

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
        public void FailWaitDialog(string statusMsg, string logMsg, string description, bool success)
        {
            Util.Invoke(statusStrip1, () => {
                StatusProgress.Visible = false;
                AddStatusMessage(statusMsg);
            });
            Util.Invoke(WaitTabPage, () => {
                RecreateDialogs();
                Wait.Finish(false);
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
                StatusProgress.Value = progress;
                StatusProgress.Style = ProgressBarStyle.Continuous;
            });
        }

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

        public void Wait_OnCancel()
        {
            if (cancelCallback != null)
            {
                cancelCallback();
            }
        }
    }
}
