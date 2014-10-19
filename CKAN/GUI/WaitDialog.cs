using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class WaitDialog : Form
    {
        public WaitDialog()
        {
            InitializeComponent();
            DialogProgressBar.Minimum = 0;
            DialogProgressBar.Maximum = 100;
        }

        public void ShowWaitDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            ShowDialog();
        }

        public void HideWaitDialog()
        {
            if (MessageTextBox.InvokeRequired)
            {
                MessageTextBox.Invoke(
                    new MethodInvoker(delegate { MessageTextBox.Text = "Waiting for operation to complete"; }));
            }
            else
            {
                MessageTextBox.Text = "Waiting for operation to complete";
            }

            Close();
        }

        public void SetProgress(int progress)
        {
            if (DialogProgressBar.InvokeRequired)
            {
                DialogProgressBar.Invoke(new MethodInvoker(delegate
                {
                    DialogProgressBar.Value = progress;
                    DialogProgressBar.Style = ProgressBarStyle.Continuous;
                }));
            }
            else
            {
                DialogProgressBar.Value = progress;
                DialogProgressBar.Style = ProgressBarStyle.Continuous;
            }
        }

        public void ResetProgress()
        {
            if (DialogProgressBar.InvokeRequired)
            {
                DialogProgressBar.Invoke(
                    new MethodInvoker(delegate { DialogProgressBar.Style = ProgressBarStyle.Marquee; }));
            }
            else
            {
                DialogProgressBar.Style = ProgressBarStyle.Marquee;
            }
        }

        public void SetDescription(string message)
        {
            if (MessageTextBox.InvokeRequired)
            {
                MessageTextBox.Invoke(new MethodInvoker(delegate { MessageTextBox.Text = "(" + message + ")"; }));
            }
            else
            {
                MessageTextBox.Text = "(" + message + ")";
            }
        }

        public void ClearLog()
        {
            if (LogTextBox.InvokeRequired)
            {
                LogTextBox.Invoke(new MethodInvoker(delegate { LogTextBox.Text = ""; }));
            }
            else
            {
                LogTextBox.Text = "";
            }
        }

        public void AddLogMessage(string message)
        {
            if (LogTextBox.InvokeRequired)
            {
                LogTextBox.Invoke(new MethodInvoker(delegate { LogTextBox.AppendText(message + "\r\n"); }));
            }
            else
            {
                LogTextBox.AppendText(message + "\r\n");
            }
        }

        private void ActionDescriptionLabel_Click(object sender, EventArgs e)
        {
        }
    }
}