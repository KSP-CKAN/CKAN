using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using log4net;

namespace CKAN.GUI
{
    public partial class DownloadsFailedDialog : Form
    {
        public DownloadsFailedDialog(IEnumerable<KeyValuePair<CkanModule[], Exception>> Exceptions)
        {
            InitializeComponent();
            rows = Exceptions
                // One row per affected mod (mods can share downloads)
                .SelectMany(kvp => kvp.Key.Select(m => new DownloadRow(m, kvp.Value)))
                .ToList();
            DownloadsGrid.DataSource = new BindingList<DownloadRow>(rows);
            ClientSize = new Size(ClientSize.Width,
                ExplanationLabel.Height
                + DownloadsGrid.RowCount
                    * DownloadsGrid.RowTemplate.Height
                + BottomButtonPanel.Height);
        }

        public bool         Abort { get; private set; } = false;
        public CkanModule[] Retry => rows.Where(r => r.Retry)
                                         .Select(r => r.Module)
                                         .ToArray();
        public CkanModule[] Skip  => rows.Where(r => r.Skip)
                                         .Select(r => r.Module)
                                         .ToArray();

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.DownloadsFailed);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.DownloadsFailed);
        }

        private void DownloadsGrid_SelectionChanged(object sender, EventArgs e)
        {
            // Don't clutter the screen with a highlight we don't use
            DownloadsGrid.ClearSelection();
        }

        /// <summary>
        /// React to checkboxes by triggering CellValueChanged
        /// </summary>
        private void DownloadsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DownloadsGrid.EndEdit();
        }

        /// <summary>
        /// Have to react to double clicks separately because CellContentClick doesn't fire
        /// </summary>
        private void DownloadsGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DownloadsGrid.EndEdit();
        }

        /// <summary>
        /// Luckily the data object's properties are always correct,
        /// so all we have to do is force a refresh
        /// </summary>
        private void DownloadsGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var binding  = (BindingList<DownloadRow>) DownloadsGrid.DataSource;
            var download = rows[e.RowIndex].Module.download;
            var retry    = rows[e.RowIndex].Retry;
            // Update all rows with this download
            for (int i = 0; i < rows.Count; ++i)
            {
                if (rows[i].Module.download == download)
                {
                    if (i != e.RowIndex)
                    {
                        rows[i].Retry = retry;
                    }
                    binding.ResetItem(i);
                }
            }
        }

        private void RetryButton_Click(object sender, EventArgs e)
        {
            Abort = false;
            Close();
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            Abort = true;
            Close();
        }

        private List<DownloadRow> rows;

        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadsFailedDialog));
    }

    public class DownloadRow
    {
        public DownloadRow(CkanModule module, Exception exc)
        {
            Retry   = true;
            Module  = module;
            Error   = exc.Message;
        }

        public bool       Retry   { get; set; }
        public bool       Skip    { get => !Retry; set { Retry = !value; } }
        public CkanModule Module  { get; private set; }
        public string     Error   { get; private set; }
    }
}
