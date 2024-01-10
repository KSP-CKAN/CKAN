using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    /// <summary>
    /// A popup form reporting one or more download errors where
    /// the user can choose whether to retry or skip each one or
    /// abort the whole changeset, looking kind of like this:
    ///
    /// +--------------------------------------------------------+
    /// |                   Downloads Failed                     |
    /// +--------------------------------------------------------+
    /// | The following mods failed to download:                 |
    /// |                                                        |
    /// |  +--------+-------+-----------------+---------------+  |
    /// |  | Retry? | Skip? | Mod             | Error         |  |
    /// |  +--------+-------+-----------------+---------------+  |
    /// |  |   X    |       | CoolMod 1.0     | Timed out     |  |
    /// |  |   X    |       | AwesomeMod 1.0  | 404 Not Found |  |
    /// |  |        |   X   | MediocreMod 1.0 | 403 Forbidden |  |
    /// |  +--------+-------+-----------------+---------------+  |
    /// |                                                        |
    /// | [Retry without "Skip" mods]                            |
    /// | [Abort whole changeset]                                |
    /// +--------------------------------------------------------+
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class DownloadsFailedDialog : Form
    {
        /// <summary>
        /// Initialize the form, loads the grid and sets the height to fit
        /// </summary>
        /// <param name="Exceptions">Sequence of arrays of objects representing downloads that failed and the exceptions they threw</param>
        public DownloadsFailedDialog(
            string TopLabelMessage,
            string ModuleColumnHeader,
            string AbortButtonCaption,
            IEnumerable<KeyValuePair<object[], Exception>> Exceptions,
            Func<object, object, bool> rowsLinked)
        {
            InitializeComponent();
            ExplanationLabel.Text = TopLabelMessage;
            ModColumn.HeaderText  = ModuleColumnHeader;
            AbortButton.Text      = AbortButtonCaption;
            this.rowsLinked       = rowsLinked;
            rows = Exceptions
                // One row per affected mod (mods can share downloads)
                .SelectMany(kvp => kvp.Key.Select(m => new DownloadRow(m, kvp.Value)))
                .ToList();
            DownloadsGrid.DataSource = new BindingList<DownloadRow>(rows);
            ClientSize = new Size(ClientSize.Width,
                ExplanationLabel.Height
                + ExplanationLabel.Padding.Vertical
                + DownloadsGrid.ColumnHeadersHeight
                + (DownloadsGrid.RowCount
                    * DownloadsGrid.RowTemplate.Height)
                + DownloadsGrid.Margin.Vertical
                + DownloadsGrid.Padding.Vertical
                + BottomButtonPanel.Height);
        }

        [ForbidGUICalls]
        public object[] Wait() => task.Task.Result;

        /// <summary>
        /// True if user clicked the abort button, false otherwise
        /// </summary>
        public bool     Abort { get; private set; } = false;
        /// <summary>
        /// Array of data objects with a checkmark in the Retry column
        /// </summary>
        public object[] Retry => rows.Where(r => r.Retry)
                                     .Select(r => r.Data)
                                     .ToArray();
        /// <summary>
        /// Array of data objects with a checkmark in the Skip column
        /// </summary>
        public object[] Skip  => rows.Where(r => r.Skip)
                                     .Select(r => r.Data)
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
            var binding = (BindingList<DownloadRow>)DownloadsGrid.DataSource;
            var retry   = rows[e.RowIndex].Retry;
            // Update all rows with this download
            for (int i = 0; i < rows.Count; ++i)
            {
                if (rowsLinked(rows[e.RowIndex].Data, rows[i].Data))
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
            task.SetResult(Skip);
            Close();
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            Abort = true;
            task.SetResult(null);
            Close();
        }

        private readonly List<DownloadRow> rows;
        private readonly Func<object, object, bool> rowsLinked;
        private readonly TaskCompletionSource<object[]> task = new TaskCompletionSource<object[]>();
    }

    /// <summary>
    /// Data object representing one row, for BindingList to examine and update
    /// </summary>
    public class DownloadRow
    {
        /// <summary>
        /// Initialize the row
        /// </summary>
        /// <param name="data">The data object for this row</param>
        /// <param name="exc">The exception thrown when this download failed</param>
        public DownloadRow(object data, Exception exc)
        {
            Retry = true;
            Data  = data;
            Error = exc.GetBaseException().Message;
        }

        /// <summary>
        /// True if Retry column has a checkmark
        /// </summary>
        public bool   Retry { get; set; }
        /// <summary>
        /// True if Skip column has a checkmark
        /// </summary>
        #pragma warning disable IDE0027
        public bool   Skip  { get => !Retry; set { Retry = !value; } }
        #pragma warning restore IDE0027
        /// <summary>
        /// This row's data object
        /// </summary>
        public object Data  { get; private set; }
        /// <summary>
        /// This row's download error
        /// </summary>
        public string Error { get; private set; }
    }
}
