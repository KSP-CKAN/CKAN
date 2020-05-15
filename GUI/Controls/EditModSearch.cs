using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using log4net;

namespace CKAN
{
    /// <summary>
    /// A control for displaying and editing a search of mods.
    /// Contains several separate fields for searching different properties,
    /// plus a combined field that represents them all in a special syntax.
    /// </summary>
    public partial class EditModSearch : UserControl
    {
        /// <summary>
        /// Initialize a mod search editing control
        /// </summary>
        public EditModSearch()
        {
            InitializeComponent();

            this.ToolTip.SetToolTip(ExpandButton, Properties.Resources.EditModSearchTooltipExpandButton);

            labels = new Label[]
            {
                FilterByNameLabel,
                FilterByAuthorLabel,
                FilterByDescriptionLabel,
                FilterByLanguageLabel,
                FilterByDependsLabel,
                FilterByRecommendsLabel,
                FilterBySuggestsLabel,
                FilterByConflictsLabel,
                FilterCombinedLabel,
            };
            textboxes = new TextBox[]
            {
                FilterByNameTextBox,
                FilterByAuthorTextBox,
                FilterByDescriptionTextBox,
                FilterByLanguageTextBox,
                FilterByDependsTextBox,
                FilterByRecommendsTextBox,
                FilterBySuggestsTextBox,
                FilterByConflictsTextBox,
                FilterCombinedTextBox,
            };
            rowCount = labels.Length;
            maxLabelWidth = labels.Select(l => l.Width).Max();
            DoLayout(ExpandButton.Checked);
        }

        /// <summary>
        /// Clear the input fields
        /// </summary>
        public void Clear()
        {
            FilterCombinedTextBox.Text = "";
        }

        /// <summary>
        /// Toggle whether the detailed controls are shown or hidden.
        /// </summary>
        public void ExpandCollapse()
        {
            ExpandButton.Checked = !ExpandButton.Checked;
        }

        /// <summary>
        /// Event fired when a search needs to be executed.
        /// </summary>
        public event Action<ModSearch> ApplySearch;

        /// <summary>
        /// Event fired when user wants to switch focus away from this control.
        /// </summary>
        public event Action SurrenderFocus;

        private static readonly ILog log = LogManager.GetLogger(typeof(EditModSearch));
        private Label[] labels;
        private TextBox[] textboxes;
        private readonly int rowCount;
        private const int rowHeight = 26;
        private const int padding   = 4;
        private readonly int maxLabelWidth;
        private Timer filterTimer;
        private bool suppressSearch = false;
        private ModSearch currentSearch = null;

        private void ExpandButton_CheckedChanged(object sender, EventArgs e)
        {
            ExpandButton.Text = ExpandButton.Checked ? "▾" : "▴";
            DoLayout(ExpandButton.Checked);
        }

        private void DoLayout(bool expanded)
        {
            if (expanded)
            {
                for (int row = 0; row < rowCount; ++row)
                {
                    labels[row].Visible = true;
                    labels[row].Left = padding;
                    labels[row].Top = padding + row * (rowHeight + padding);
                    textboxes[row].Visible = true;
                    textboxes[row].Left = maxLabelWidth + 2 * padding;
                    textboxes[row].Top = padding + row * (rowHeight + padding);
                }
                Height = rowCount * (rowHeight + padding);
            }
            else
            {
                Height = rowHeight + padding;
                for (int row = 0; row < labels.Length; ++row)
                {
                    if (row < labels.Length - 1)
                    {
                        labels[row].Visible    = false;
                        textboxes[row].Visible = false;
                    }
                    else
                    {
                        labels[row].Top     = padding;
                        labels[row].Left    = padding;
                        textboxes[row].Top  = padding;
                        textboxes[row].Left = maxLabelWidth + 2 * padding;
                    }
                }
            }
            var botTB = textboxes[rowCount - 1];
            ExpandButton.Left   = botTB.Left + botTB.Width;
            ExpandButton.Top    = botTB.Top;
            ExpandButton.Height = botTB.Height;
            ExpandButton.Width  = botTB.Height;
        }

        private void FilterCombinedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (suppressSearch)
                return;

            try
            {
                currentSearch = ModSearch.Parse(FilterCombinedTextBox.Text);
                suppressSearch = true;
                    FilterByNameTextBox.Text        = currentSearch?.Name          ?? "";
                    FilterByAuthorTextBox.Text      = currentSearch?.Author        ?? "";
                    FilterByDescriptionTextBox.Text = currentSearch?.Description   ?? "";
                    FilterByLanguageTextBox.Text    = currentSearch?.Localization  ?? "";
                    FilterByDependsTextBox.Text     = currentSearch?.DependsOn     ?? "";
                    FilterByRecommendsTextBox.Text  = currentSearch?.Recommends    ?? "";
                    FilterBySuggestsTextBox.Text    = currentSearch?.Suggests      ?? "";
                    FilterByConflictsTextBox.Text   = currentSearch?.ConflictsWith ?? "";
                suppressSearch = false;
                TriggerSearchOrTimer();
            }
            catch (Kraken k)
            {
                Main.Instance.AddStatusMessage(k.Message);
            }
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (suppressSearch)
                return;

            currentSearch = new ModSearch(
                FilterByNameTextBox.Text,
                FilterByAuthorTextBox.Text,
                FilterByDescriptionTextBox.Text,
                FilterByLanguageTextBox.Text,
                FilterByDependsTextBox.Text,
                FilterByRecommendsTextBox.Text,
                FilterBySuggestsTextBox.Text,
                FilterByConflictsTextBox.Text
            );
            suppressSearch = true;
                FilterCombinedTextBox.Text = currentSearch?.Combined ?? "";
            suppressSearch = false;
            TriggerSearchOrTimer();
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Switch focus from filters to mod list on enter, down, or pgdn
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    if (SurrenderFocus != null)
                    {
                        SurrenderFocus();
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void TriggerSearchOrTimer()
        {
            if (Platform.IsMac)
            {
                // Delay updating to improve typing performance on OS X.
                RunFilterUpdateTimer();
            }
            else
            {
                TriggerSearch();
            }
        }

        /// <summary>
        /// Start or restart a timer to update the filter after an interval since the last keypress.
        /// On Mac OS X, this prevents the search field from locking up due to DataGridViews being
        /// slow and key strokes being interpreted incorrectly when slowed down:
        /// http://mono.1490590.n4.nabble.com/Incorrect-missing-and-duplicate-keypress-events-td4658863.html
        /// </summary>
        private void RunFilterUpdateTimer()
        {
            if (filterTimer == null)
            {
                filterTimer = new Timer();
                filterTimer.Tick += OnFilterUpdateTimer;
                filterTimer.Interval = 700;
                filterTimer.Start();
            }
            else
            {
                filterTimer.Stop();
                filterTimer.Start();
            }
        }

        /// <summary>
        /// Updates the filter after an interval of time has passed since the last keypress.
        /// </summary>
        private void OnFilterUpdateTimer(object source, EventArgs e)
        {
            TriggerSearch();
            filterTimer.Stop();
        }

        private void TriggerSearch()
        {
            if (ApplySearch != null)
            {
                ApplySearch(currentSearch);
            }
        }

    }
}
