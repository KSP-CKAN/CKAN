using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class WaitDialog : Form
    {
        public WaitDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            DialogProgressBar.Minimum = 0;
            DialogProgressBar.Maximum = 100;
        }

        public void ShowWaitDialog(bool asDialog = true)
        {
            if (asDialog)
            {
                Util.Invoke(this, () => ShowDialog());
            }
            else
            {
                Util.Invoke(this, () => Show());
            }
        }

        public void HideWaitDialog()
        {
            Util.Invoke(MessageTextBox, () => MessageTextBox.Text = "Waiting for operation to complete");
            Util.Invoke(this, () => Close());
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