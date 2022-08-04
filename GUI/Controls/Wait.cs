using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CKAN
{
    public partial class Wait : UserControl
    {
        public Wait()
        {
            InitializeComponent();
            progressTimer.Tick += (sender, evt) => ReflowProgressBars();
        }

        public event Action OnRetry;
        public event Action OnCancel;
        public event Action OnOk;

        public bool RetryEnabled
        {
            set
            {
                Util.Invoke(this, () =>
                    RetryCurrentActionButton.Visible = value);
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

        /// <summary>
        /// React to data received for a module,
        /// adds or updates a label and progress bar so user can see how each download is going
        /// </summary>
        /// <param name="module">The module that is being downloaded</param>
        /// <param name="remaining">Number of bytes left to download</param>
        /// <param name="total">Number of bytes in complete download</param>
        public void SetModuleProgress(CkanModule module, long remaining, long total)
        {
            Util.Invoke(this, () =>
            {
                if (moduleBars.TryGetValue(module, out ProgressBar pb))
                {
                    // download_size is allowed to be 0
                    pb.Value = Math.Max(pb.Minimum, Math.Min(pb.Maximum,
                        (int) (100 * (total - remaining) / total)
                    ));
                }
                else
                {
                    var rowTop = TopPanel.Height - padding;
                    var newLb = new Label()
                    {
                        AutoSize = true,
                        Location = new Point(2 * padding, rowTop),
                        Size     = new Size(labelWidth, progressHeight),
                        Text     = string.Format(
                                Properties.Resources.MainChangesetHostSize,
                                module.name,
                                module.version,
                                module.download.Host ?? "",
                                CkanModule.FmtSize(module.download_size)),
                    };
                    moduleLabels.Add(module, newLb);
                    var newPb = new ProgressBar()
                    {
                        Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                        Location = new Point(labelWidth + 3 * padding, rowTop),
                        Size     = new Size(TopPanel.Width - labelWidth - 5 * padding, progressHeight),
                        Minimum  = 0,
                        Maximum  = 100,
                        // download_size is allowed to be 0
                        Value    = Math.Max(0, Math.Min(100,
                                       (int) (100 * (total - remaining) / total)
                                   )),
                        Style    = ProgressBarStyle.Continuous,
                    };
                    moduleBars.Add(module, newPb);
                }
                progressTimer.Start();
            });
        }

        private const int padding        = 5;
        private const int labelWidth     = 400;
        private const int progressHeight = 20;
        private const int emptyHeight    = 85;

        private Dictionary<CkanModule, Label>       moduleLabels = new Dictionary<CkanModule, Label>();
        private Dictionary<CkanModule, ProgressBar> moduleBars   = new Dictionary<CkanModule, ProgressBar>();
        private Timer progressTimer = new Timer() { Interval = 3000 };

        /// <summary>
        /// Add new progress bars and remove completed ones (100%) in a single scheduled pass,
        /// so they don't constantly flicker and jump around.
        /// </summary>
        private void ReflowProgressBars()
        {
            Util.Invoke(this, () =>
            {
                int rowTop = emptyHeight - padding;
                var theBars = moduleBars.OrderBy(kvp => kvp.Value.Top).ToList();
                foreach (var kvp in theBars)
                {
                    if (kvp.Value.Value == 100)
                    {
                        // Finished, remove in this pass
                        TopPanel.Controls.Remove(moduleLabels[kvp.Key]);
                        TopPanel.Controls.Remove(kvp.Value);
                        // rowTop is unchanged, so next row will replace this one
                    }
                    else if (!TopPanel.Controls.Contains(kvp.Value))
                    {
                        // Just started, add it in this pass
                        moduleLabels[kvp.Key].Top = rowTop;
                        kvp.Value.Top = rowTop;
                        TopPanel.Controls.Add(moduleLabels[kvp.Key]);
                        TopPanel.Controls.Add(kvp.Value);
                        rowTop += progressHeight + padding;
                    }
                    else
                    {
                        // Not finished, already displayed, just make sure it's in the right position
                        moduleLabels[kvp.Key].Top = rowTop;
                        kvp.Value.Top = rowTop;
                        rowTop += progressHeight + padding;
                    }
                }
                // Make room for everything that's still visible
                TopPanel.Height = rowTop + padding;

                var removedModules = moduleBars
                    .Where(kvp => !TopPanel.Controls.Contains(kvp.Value))
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var module in removedModules)
                {
                    moduleLabels.Remove(module);
                    moduleBars.Remove(module);
                }
            });
        }

        /// <summary>
        /// React to completion of all downloads,
        /// removes all the module progress bars since we don't need them anymore
        /// </summary>
        public void DownloadsComplete()
        {
            ClearModuleBars();
            progressTimer.Stop();
        }

        private void ClearModuleBars()
        {
            Util.Invoke(this, () =>
            {
                foreach (var kvp in moduleLabels)
                {
                    TopPanel.Controls.Remove(kvp.Value);
                }
                foreach (var kvp in moduleBars)
                {
                    TopPanel.Controls.Remove(kvp.Value);
                }
                moduleLabels.Clear();
                moduleBars.Clear();
                TopPanel.Height = emptyHeight;
            });
        }

        public void Reset(bool cancelable)
        {
            Util.Invoke(this, () =>
            {
                ClearModuleBars();
                ProgressValue = DialogProgressBar.Minimum;
                ProgressIndeterminate = true;
                RetryCurrentActionButton.Visible = false;
                CancelCurrentActionButton.Visible = cancelable;
                OkButton.Enabled = false;
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
                RetryCurrentActionButton.Visible = !success;
                CancelCurrentActionButton.Visible = false;
                OkButton.Enabled = true;
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

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (OnOk != null)
            {
                OnOk();
            }
        }
    }
}
