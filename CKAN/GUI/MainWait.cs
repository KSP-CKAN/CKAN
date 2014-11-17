using System;
using System.Windows.Forms;

namespace CKAN
{

    internal delegate void MainCancelCallback();

    public partial class Main : Form
    {

        private TabPage waitTabPage = null;
        private MainCancelCallback cancelCallback = null;

        public void ShowWaitDialog(bool cancelable = true)
        {
            if (waitTabPage != null)
            {
                MainTabControl.TabPages.Add(waitTabPage);
                waitTabPage = null;
            }

            MainTabControl.SelectTab(2);

            ManageModsTabPage.Enabled = false;
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

            ManageModsTabPage.Enabled = true;

            MainTabControl.SelectTab(0);
            MainTabControl.TabPages[2].Text = "Log";
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

        private void CancelCurrentActionButton_Click(object sender, EventArgs e)
        {
            if (cancelCallback != null)
            {
                cancelCallback();
                CancelCurrentActionButton.Enabled = false;
                HideWaitDialog(true);
            }
        }

    }

}
