using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Wait : UserControl
    {
        public Wait()
        {
            InitializeComponent();
            progressTimer.Tick += (sender, evt) => ReflowProgressBars();

            bgWorker.DoWork             += DoWork;
            bgWorker.RunWorkerCompleted += RunWorkerCompleted;
        }


        [ForbidGUICalls]
        public void StartWaiting(Action<object, DoWorkEventArgs>             mainWork,
                                 Action<object, RunWorkerCompletedEventArgs> postWork,
                                 bool cancelable,
                                 object param)
        {
            bgLogic   = mainWork;
            postLogic = postWork;
            Reset(cancelable);
            ClearLog();
            RetryEnabled = false;
            bgWorker.RunWorkerAsync(param);
        }

        public event Action OnRetry;
        public event Action OnCancel;
        public event Action OnOk;

        #pragma warning disable IDE0027

        public bool RetryEnabled
        {
            [ForbidGUICalls]
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
            [ForbidGUICalls]
            set
            {
                Util.Invoke(this, () =>
                    DialogProgressBar.Style = value
                        ? ProgressBarStyle.Marquee
                        : ProgressBarStyle.Continuous);
            }
        }

        #pragma warning restore IDE0027

        public void SetProgress(string label, long remaining, long total)
        {
            if (total > 0)
            {
                Util.Invoke(this, () =>
                {
                    if (progressBars.TryGetValue(label, out ProgressBar pb))
                    {
                        // download_size is allowed to be 0
                        pb.Value = Math.Max(pb.Minimum, Math.Min(pb.Maximum,
                            (int) (100 * (total - remaining) / total)));
                    }
                    else
                    {
                        var newLb = new Label()
                        {
                            AutoSize = true,
                            Text     = label,
                            Margin   = new Padding(0, 8, 0, 0),
                        };
                        progressLabels.Add(label, newLb);
                        var newPb = new ProgressBar()
                        {
                            Anchor  = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                            Minimum = 0,
                            Maximum = 100,
                            // download_size is allowed to be 0
                            Value   = Math.Max(0, Math.Min(100,
                                           (int) (100 * (total - remaining) / total))),
                            Style   = ProgressBarStyle.Continuous,
                        };
                        progressBars.Add(label, newPb);
                    }
                    progressTimer.Start();
                });
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
            SetProgress(module.ToString(), remaining, total);
        }

        private Action<object, DoWorkEventArgs>             bgLogic;
        private Action<object, RunWorkerCompletedEventArgs> postLogic;

        private readonly BackgroundWorker bgWorker = new BackgroundWorker()
        {
            WorkerReportsProgress      = true,
            WorkerSupportsCancellation = true,
        };

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            bgLogic?.Invoke(sender, e);
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            postLogic?.Invoke(sender, e);
        }

        private const int padding     = 5;
        private const int emptyHeight = 85;

        private readonly Dictionary<string, Label>       progressLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, ProgressBar> progressBars   = new Dictionary<string, ProgressBar>();
        private readonly Timer progressTimer = new Timer() { Interval = 3000 };

        /// <summary>
        /// Add new progress bars and remove completed ones (100%) in a single scheduled pass,
        /// so they don't constantly flicker and jump around.
        /// </summary>
        private void ReflowProgressBars()
        {
            Util.Invoke(this, () =>
            {
                foreach (var kvp in progressBars)
                {
                    var lbl = progressLabels[kvp.Key];
                    var pb  = kvp.Value;

                    if (pb.Value >= 100)
                    {
                        if (ProgressBarTable.Controls.Contains(pb))
                        {
                            // Finished, remove in this pass
                            ProgressBarTable.Controls.Remove(lbl);
                            ProgressBarTable.Controls.Remove(pb);
                            ProgressBarTable.RowStyles.RemoveAt(0);
                        }
                    }
                    else if (!ProgressBarTable.Controls.Contains(pb))
                    {
                        // Just started, add it in this pass
                        ProgressBarTable.Controls.Add(lbl, 0, -1);
                        ProgressBarTable.Controls.Add(pb,  1, -1);
                        ProgressBarTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    }
                }

                // Remove completed rows from our dicts
                var removedKeys = progressBars
                    .Where(kvp => !ProgressBarTable.Controls.Contains(kvp.Value))
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var key in removedKeys)
                {
                    progressLabels.Remove(key);
                    progressBars.Remove(key);
                }

                // Fit table to its contents (it assumes we will give it a size)
                var cellPadding = ProgressBarTable.Padding.Vertical;
                ProgressBarTable.Height = progressBars.Values
                    .Select(pb => pb.GetPreferredSize(Size.Empty).Height + cellPadding)
                    .Sum();
                TopPanel.Height = ProgressBarTable.Top + ProgressBarTable.Height + padding;
            });
        }

        /// <summary>
        /// React to completion of all downloads,
        /// removes all the module progress bars since we don't need them anymore
        /// </summary>
        public void DownloadsComplete()
        {
            ClearProgressBars();
            progressTimer.Stop();
        }

        private void ClearProgressBars()
        {
            Util.Invoke(this, () =>
            {
                ProgressBarTable.Controls.Clear();
                ProgressBarTable.RowStyles.Clear();
                progressLabels.Clear();
                progressBars.Clear();
                TopPanel.Height = emptyHeight;
            });
        }

        [ForbidGUICalls]
        public void Reset(bool cancelable)
        {
            Util.Invoke(this, () =>
            {
                ClearProgressBars();
                ProgressValue = DialogProgressBar.Minimum;
                ProgressIndeterminate = true;
                CancelCurrentActionButton.Visible = cancelable;
                CancelCurrentActionButton.Enabled = true;
                OkButton.Enabled = false;
                MessageTextBox.Text = Properties.Resources.MainWaitPleaseWait;
            });
        }

        public void Finish()
        {
            OnCancel = null;
            Util.Invoke(this, () =>
            {
                MessageTextBox.Text = Properties.Resources.MainWaitDone;
                ProgressValue = 100;
                ProgressIndeterminate = false;
                CancelCurrentActionButton.Enabled = false;
                OkButton.Enabled = true;
            });
        }

        public void SetDescription(string message)
        {
            Util.Invoke(this, () =>
                MessageTextBox.Text = "(" + message + ")");
        }

        public void SetMainProgress(string message, int percent)
        {
            Util.Invoke(this, () =>
            {
                MessageTextBox.Text = $"{message} - {percent}%";
                ProgressIndeterminate = false;
                ProgressValue = percent;
                if (message != lastProgressMessage)
                {
                    AddLogMessage(message);
                    lastProgressMessage = message;
                }
            });
        }

        public void SetMainProgress(int percent, long bytesPerSecond, long bytesLeft)
        {
            var fullMsg = string.Format(CKAN.Properties.Resources.NetAsyncDownloaderProgress,
                                        CkanModule.FmtSize(bytesPerSecond),
                                        CkanModule.FmtSize(bytesLeft));
            Util.Invoke(this, () =>
            {
                MessageTextBox.Text = $"{fullMsg} - {percent}%";
                ProgressIndeterminate = false;
                ProgressValue = percent;
            });
        }

        private string lastProgressMessage;

        [ForbidGUICalls]
        private void ClearLog()
        {
            Util.Invoke(this, () =>
                LogTextBox.Text = "");
        }

        public void AddLogMessage(string message)
        {
            LogTextBox.AppendText(message + "\r\n");
        }

        private void RetryCurrentActionButton_Click(object sender, EventArgs e)
        {
            OnRetry?.Invoke();
        }

        private void CancelCurrentActionButton_Click(object sender, EventArgs e)
        {
            bgWorker.CancelAsync();
            if (OnCancel != null)
            {
                OnCancel.Invoke();
                Util.Invoke(this, () =>
                    CancelCurrentActionButton.Enabled = false);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            OnOk?.Invoke();
        }
    }
}
