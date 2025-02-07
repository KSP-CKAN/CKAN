using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
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
            emptyHeight = VerticalSplitter.SplitterDistance;
            bgWorker.DoWork             += DoWork;
            bgWorker.RunWorkerCompleted += RunWorkerCompleted;
        }

        [ForbidGUICalls]
        public void StartWaiting(Action<object?, DoWorkEventArgs?>             mainWork,
                                 Action<object?, RunWorkerCompletedEventArgs?> postWork,
                                 bool                                          cancelable,
                                 object?                                       param)
        {
            bgLogic   = mainWork;
            postLogic = postWork;
            Reset(cancelable);
            ClearLog();
            RetryEnabled = false;
            bgWorker.RunWorkerAsync(param);
        }

        public event Action? OnRetry;
        public event Action? OnCancel;
        public event Action? OnOk;

        public bool Busy => bgWorker.IsBusy;

        #pragma warning disable IDE0027

        public bool RetryEnabled
        {
            [ForbidGUICalls]
            set
            {
                Util.Invoke(this, () => RetryCurrentActionButton.Visible = value);
            }
        }

        public int ProgressValue
        {
            set
            {
                Util.Invoke(this,
                            () => DialogProgressBar.Value = Math.Max(DialogProgressBar.Minimum,
                                                                     Math.Min(DialogProgressBar.Maximum,
                                                                              value)));
            }
        }

        public bool ProgressIndeterminate
        {
            [ForbidGUICalls]
            set
            {
                Util.Invoke(this,
                            () => DialogProgressBar.Style = value ? ProgressBarStyle.Marquee
                                                                  : ProgressBarStyle.Continuous);
            }
        }

        #pragma warning restore IDE0027

        public void SetProgress(string label,
                                long remaining, long total)
        {
            // download_size is allowed to be 0
            if (total > 0)
            {
                Util.Invoke(this, () =>
                {
                    if (progressBars.TryGetValue(label, out LabeledProgressBar? pb))
                    {
                        var rateCounter = rateCounters[label];
                        rateCounter.BytesLeft = remaining;
                        rateCounter.Size      = total;

                        // download_size is allowed to be 0
                        var newVal = Math.Max(pb.Minimum,
                                              Math.Min(pb.Maximum,
                                                       (int)(100 * (total - remaining) / total)));
                        pb.Value = newVal;
                        pb.Text = rateCounter.Summary;
                        if (newVal >= 100)
                        {
                            var myLbl = progressLabels[label];
                            rateCounter.Stop();
                            ProgressBarTable.SuspendLayout();
                            for (int row = ProgressBarTable.GetPositionFromControl(pb).Row; row > 0; --row)
                            {
                                if (ProgressBarTable.GetControlFromPosition(0, row - 1) is Label prevLbl
                                    && ProgressBarTable.GetControlFromPosition(1, row - 1) is LabeledProgressBar prevPb)
                                {
                                    if (prevPb.Value >= 100)
                                    {
                                        // Previous row is completed, done
                                        break;
                                    }
                                    else
                                    {
                                        // Previous row is in progress, swap
                                        ProgressBarTable.SetRow(myLbl,   row - 1);
                                        ProgressBarTable.SetRow(pb,      row - 1);
                                        ProgressBarTable.SetRow(prevLbl, row);
                                        ProgressBarTable.SetRow(prevPb,  row);
                                    }
                                }
                            }
                            ProgressBarTable.ResumeLayout();
                        }
                    }
                    else
                    {
                        var rateCounter = new ByteRateCounter
                        {
                            BytesLeft = remaining,
                            Size      = total
                        };
                        rateCounters.Add(label, rateCounter);
                        rateCounter.Start();

                        var scrollToBottom = AtEnd(ProgressBarTable);
                        var newLb = new Label()
                        {
                            AutoSize = true,
                            Text     = label,
                            Margin   = new Padding(0, 8, 0, 0),
                        };
                        progressLabels.Add(label, newLb);
                        // download_size is allowed to be 0
                        var newVal = Math.Max(0,
                                              Math.Min(100,
                                                       (int)(100 * (total - remaining) / total)));
                        var newPb = new LabeledProgressBar()
                        {
                            Anchor  = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                            Minimum = 0,
                            Maximum = 100,
                            Value   = newVal,
                            Style   = ProgressBarStyle.Continuous,
                            Text    = rateCounter.Summary,
                        };
                        progressBars.Add(label, newPb);
                        // Make room before adding
                        var newHeight = progressBars.Values
                                                    .Take(1)
                                                    .Concat(progressBars.Count == 1
                                                                // If 1 row, show 1
                                                                ? Enumerable.Empty<LabeledProgressBar>()
                                                                // If >1 rows, show 1 + active
                                                                : progressBars.Values
                                                                              .Where(pb => pb.Value < 100))
                                                    .Sum(pb => pb.GetPreferredSize(Size.Empty).Height
                                                               + pb.Margin.Vertical);
                        if (ProgressBarTable.Height < newHeight)
                        {
                            VerticalSplitter.SplitterDistance = ProgressBarTable.Top
                                                                + newHeight
                                                                + ProgressBarTable.Margin.Vertical;
                            // Never show the horizontal scrollbar
                            ProgressBarTable.HorizontalScroll.Visible = false;
                        }
                        // Now add the new row
                        ProgressBarTable.Controls.Add(newLb, 0, -1);
                        ProgressBarTable.Controls.Add(newPb, 1, -1);
                        ProgressBarTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        ProgressBarTable.HorizontalScroll.Visible = false;
                        // If previously scrolled to the bottom, stay there, otherwise let user scroll up without interrupting
                        if (scrollToBottom)
                        {
                            ProgressBarTable.ScrollControlIntoView(newLb);
                            ProgressBarTable.ScrollControlIntoView(newPb);
                        }
                    }
                });
            }
        }

        private static bool AtEnd(ScrollableControl control)
            => control.DisplayRectangle.Height < control.Height
                || control.DisplayRectangle.Height
                   + control.DisplayRectangle.Y
                   - control.VerticalScroll.LargeChange < control.Height;

        private Action<object?, DoWorkEventArgs?>?             bgLogic;
        private Action<object?, RunWorkerCompletedEventArgs?>? postLogic;

        private readonly BackgroundWorker bgWorker = new BackgroundWorker()
        {
            WorkerReportsProgress      = true,
            WorkerSupportsCancellation = true,
        };

        private void DoWork(object? sender, DoWorkEventArgs? e)
        {
            bgLogic?.Invoke(sender, e);
        }

        private void RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs? e)
        {
            postLogic?.Invoke(sender, e);
        }

        private readonly int emptyHeight;

        private readonly Dictionary<string, Label>              progressLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, LabeledProgressBar> progressBars   = new Dictionary<string, LabeledProgressBar>();
        private readonly Dictionary<string, ByteRateCounter>    rateCounters   = new Dictionary<string, ByteRateCounter>();

        private void ClearProgressBars()
        {
            VerticalSplitter.SplitterDistance = emptyHeight;
            foreach (var rc in rateCounters.Values)
            {
                rc.Stop();
            }
            rateCounters.Clear();
            ProgressBarTable.SuspendLayout();
            ProgressBarTable.Controls.Clear();
            ProgressBarTable.RowStyles.Clear();
            ProgressBarTable.ResumeLayout();
            progressLabels.Clear();
            progressBars.Clear();
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
                DialogProgressBar.Text = Properties.Resources.MainWaitPleaseWait;
            });
        }

        public void Finish()
        {
            OnCancel = null;
            Util.Invoke(this, () =>
            {
                DialogProgressBar.Text = Properties.Resources.MainWaitDone;
                ProgressValue = 100;
                ProgressIndeterminate = false;
                CancelCurrentActionButton.Enabled = false;
                OkButton.Enabled = true;
            });
        }

        public void SetDescription(string message)
        {
            Util.Invoke(this, () => DialogProgressBar.Text = "(" + message + ")");
        }

        public void SetMainProgress(string message, int percent)
        {
            Util.Invoke(this, () =>
            {
                DialogProgressBar.Text = $"{message} - {percent}%";
                ProgressIndeterminate = false;
                ProgressValue = percent;
                if (message != lastProgressMessage)
                {
                    AddLogMessage(message);
                    lastProgressMessage = message;
                }
            });
        }

        public void SetMainProgress(ByteRateCounter rateCounter)
        {
            Util.Invoke(this, () =>
            {
                DialogProgressBar.Text = rateCounter.Summary;
                ProgressIndeterminate  = false;
                ProgressValue          = rateCounter.Percent;
            });
        }

        private string? lastProgressMessage;

        [ForbidGUICalls]
        private void ClearLog()
        {
            Util.Invoke(this, () => LogTextBox.Text = "");
        }

        public void AddLogMessage(string message)
        {
            LogTextBox.AppendText(message + "\r\n");
        }

        private void RetryCurrentActionButton_Click(object? sender, EventArgs? e)
        {
            OnRetry?.Invoke();
        }

        private void CancelCurrentActionButton_Click(object? sender, EventArgs? e)
        {
            bgWorker.CancelAsync();
            if (OnCancel != null)
            {
                OnCancel.Invoke();
                Util.Invoke(this, () => CancelCurrentActionButton.Enabled = false);
            }
        }

        private void OkButton_Click(object? sender, EventArgs? e)
        {
            OnOk?.Invoke();
        }
    }
}
