using System;
using System.Windows.Forms;

namespace CKAN
{

    public delegate void WaitDialogCancelCallback();

    public partial class WaitDialog : Form
    {

        public WaitDialogCancelCallback cancelCallback = null;

        public WaitDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            DialogProgressBar.Minimum = 0;
            DialogProgressBar.Maximum = 100;
        }

        public void ShowWaitDialog(bool asDialog = true, bool cancelable = true)
        {
            Util.Invoke(CancelCurrentActionButton, () => CancelCurrentActionButton.Enabled = cancelable);
            Util.Invoke(CloseWindowButton, () => CloseWindowButton.Enabled = false);

            if (asDialog)
            {
                Util.Invoke(this, () => ShowDialog());
            }
            else
            {
                Util.Invoke(this, Show);
            }
        }

        public void HideWaitDialog()
        {
            Util.Invoke(MessageTextBox, () => MessageTextBox.Text = "All done!");
            Util.Invoke(DialogProgressBar, () => DialogProgressBar.Value = 100);
            Util.Invoke(CloseWindowButton, () => CloseWindowButton.Enabled = true);
            Main.Instance.RecreateDialogs();
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

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (cancelCallback != null)
            {
                cancelCallback();
                CancelCurrentActionButton.Enabled = false;
                HideWaitDialog();
            }
        }

        private void CloseWindowButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}