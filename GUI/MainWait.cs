using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {        
        public Button CancelCurrentActionButton1
        {
            set { CancelCurrentActionButton = value; }
            get { return CancelCurrentActionButton; }
        }

        public void ShowWaitDialog(bool cancelable = true)
        {
            m_TabController.ShowTab("WaitTabPage", 2);

            CancelCurrentActionButton.Enabled = cancelable;

            DialogProgressBar.Value = 0;
            DialogProgressBar.Minimum = 0;
            DialogProgressBar.Maximum = 100;
            DialogProgressBar.Style = ProgressBarStyle.Marquee;
            MessageTextBox.Text = "Please wait";
        }

        public void HideWaitDialog(bool success)
        {
            MessageTextBox.Text = "All done!";
            DialogProgressBar.Value = 100;
            DialogProgressBar.Style = ProgressBarStyle.Continuous;
            RecreateDialogs();

            m_TabController.SetActiveTab("ManageModsTabPage");

            CancelCurrentActionButton.Enabled = false;
            DialogProgressBar.Value = 0;
            DialogProgressBar.Style = ProgressBarStyle.Continuous;
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

            Util.Invoke(DialogProgressBar, () => DialogProgressBar.Value = progress);
            Util.Invoke(DialogProgressBar, () => DialogProgressBar.Style = ProgressBarStyle.Continuous);
        }

        public void ResetProgress()
        {
            Util.Invoke(DialogProgressBar, () => DialogProgressBar.Style = ProgressBarStyle.Marquee);
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
    }

}
