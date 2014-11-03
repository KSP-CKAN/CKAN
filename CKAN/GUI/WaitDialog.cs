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
            AutoCloseCheckbox.Checked = Main.Instance.m_Configuration.AutoCloseWaitDialog;
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

        public void HideWaitDialog(bool success)
        {
            Util.Invoke(MessageTextBox, () => MessageTextBox.Text = "All done!");
            Util.Invoke(DialogProgressBar, () => DialogProgressBar.Value = 100);
            Util.Invoke(DialogProgressBar, () => DialogProgressBar.Style = ProgressBarStyle.Continuous);
            Util.Invoke(CloseWindowButton, () => CloseWindowButton.Enabled = true);
            Util.Invoke(CloseWindowButton, () => CancelCurrentActionButton.Enabled = false);

            if (AutoCloseCheckbox.Checked && success)
            {
                Util.Invoke(this, Close);
            }

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
                HideWaitDialog(true);
            }
        }

        private void CloseWindowButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AutoCloseCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.m_Configuration.AutoCloseWaitDialog = AutoCloseCheckbox.Checked;
            Main.Instance.m_Configuration.Save();
        }

    }
}