using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Wait : UserControl
    {
        public Wait()
        {
            InitializeComponent();
        }

        public event Action OnRetry;
        public event Action OnCancel;

        public bool RetryEnabled
        {
            set
            {
                Util.Invoke(this, () =>
                    RetryCurrentActionButton.Enabled = value);
            }
        }

        public int ProgressValue
        {
            set
            {
                Util.Invoke(this, () =>
                    DialogProgressBar.Value =
                        Math.Max(DialogProgressBar.Minimum,
                            Math.Min(DialogProgressBar.Maximum, value)));
            }
        }

        public bool ProgressIndeterminate
        {
            set
            {
                Util.Invoke(this, () =>
                    DialogProgressBar.Style = value
                        ? ProgressBarStyle.Marquee
                        : ProgressBarStyle.Continuous);
            }
        }

        public void Reset(bool cancelable)
        {
            Util.Invoke(this, () =>
            {
                ProgressValue = DialogProgressBar.Minimum;
                ProgressIndeterminate = true;
                RetryCurrentActionButton.Enabled = false;
                CancelCurrentActionButton.Enabled = cancelable;
                MessageTextBox.Text = Properties.Resources.MainWaitPleaseWait;
            });
        }

        public void Finish(bool success)
        {
            Util.Invoke(this, () =>
            {
                MessageTextBox.Text = Properties.Resources.MainWaitDone;
                ProgressValue = 100;
                ProgressIndeterminate = false;
                RetryCurrentActionButton.Enabled = !success;
                CancelCurrentActionButton.Enabled = false;
            });
        }

        public void SetDescription(string message)
        {
            Util.Invoke(this, () =>
                MessageTextBox.Text = "(" + message + ")");
        }

        public void ClearLog()
        {
            Util.Invoke(this, () =>
                LogTextBox.Text = "");
        }

        public void AddLogMessage(string message)
        {
            Util.Invoke(this, () =>
                LogTextBox.AppendText(message + "\r\n"));
        }

        private void RetryCurrentActionButton_Click(object sender, EventArgs e)
        {
            if (OnRetry != null)
            {
                OnRetry();
            }
        }

        private void CancelCurrentActionButton_Click(object sender, EventArgs e)
        {
            if (OnCancel != null)
            {
                OnCancel();
            }
            Util.Invoke(this, () =>
                CancelCurrentActionButton.Enabled = false);
        }
    }
}
