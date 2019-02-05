using System;
using System.Windows.Forms;

namespace CKAN
{

    internal delegate void MainCancelCallback();

    public partial class Main
    {

        private MainCancelCallback cancelCallback;

        public void ShowWaitDialog(bool cancelable = true)
        {
            Util.Invoke(DialogProgressBar, () =>
            {
                tabController.ShowTab("WaitTabPage", 2);

                DialogProgressBar.Value = 0;
                DialogProgressBar.Minimum = 0;
                DialogProgressBar.Maximum = 100;
                DialogProgressBar.Style = ProgressBarStyle.Marquee;
            });
            Util.Invoke(statusStrip1, () =>
            {
                StatusProgress.Value = 0;
                StatusProgress.Style = ProgressBarStyle.Marquee;
                StatusProgress.Visible = true;
            });
            Util.Invoke(CancelCurrentActionButton, () => CancelCurrentActionButton.Enabled = cancelable);
            Util.Invoke(MessageTextBox, () => MessageTextBox.Text = "Please wait");
        }

        public void HideWaitDialog(bool success)
        {
            Util.Invoke(statusStrip1, () =>
            {
                StatusProgress.Value = 100;
                StatusProgress.Style = ProgressBarStyle.Continuous;
            });
            Util.Invoke(DialogProgressBar, () =>
            {
                MessageTextBox.Text = "All done!";
                DialogProgressBar.Value = 100;
                DialogProgressBar.Style = ProgressBarStyle.Continuous;
                RecreateDialogs();

                tabController.SetActiveTab("ManageModsTabPage");

                CancelCurrentActionButton.Enabled = false;
                DialogProgressBar.Value = 0;
                DialogProgressBar.Style = ProgressBarStyle.Continuous;
            });
            Util.Invoke(statusStrip1, () =>
            {
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
                RetryCurrentActionButton.Visible = !success;
                CancelCurrentActionButton.Enabled = false;
                SetProgress(100);
            });
            tabController.HideTab("ChangesetTabPage");
            AddLogMessage(logMsg);
            SetDescription(description);
        }

        public void SetProgress(int progress)
        {
            if (progress < 0)
            {
                progress = 0;
            }
            else if (progress > 100)
            {
                progress = 100;
            }

            Util.Invoke(DialogProgressBar, () =>
            {
                DialogProgressBar.Value = progress;
                DialogProgressBar.Style = ProgressBarStyle.Continuous;
            });
            Util.Invoke(statusStrip1, () =>
            {
                StatusProgress.Value = progress;
                StatusProgress.Style = ProgressBarStyle.Continuous;
            });
        }

        public void ResetProgress()
        {
            Util.Invoke(DialogProgressBar, () =>
            {
                DialogProgressBar.Style = ProgressBarStyle.Marquee;
            });
            Util.Invoke(statusStrip1, () =>
            {
                StatusProgress.Style    = ProgressBarStyle.Marquee;
            });
        }

        public void SetDescription(string message)
        {
            Util.Invoke(MessageTextBox, () => MessageTextBox.Text = "(" + message + ")");
        }

        public void ClearLog()
        {
            Util.Invoke(LogTextBox, () => LogTextBox.Text = "");
        }

        public void AddLogMessage(string message)
        {
            Util.Invoke(LogTextBox, () => LogTextBox.AppendText(message + "\r\n"));
        }

        private void RetryCurrentActionButton_Click(object sender, EventArgs e)
        {
            Util.Invoke(DialogProgressBar, () =>
            {
                tabController.ShowTab("ChangesetTabPage", 1);
            });
        }

        private void CancelCurrentActionButton_Click(object sender, EventArgs e)
        {
            if (cancelCallback != null)
            {
                cancelCallback();
            }
            Util.Invoke(DialogProgressBar, () =>
            {
                CancelCurrentActionButton.Enabled = false;
            });
        }

    }

}
