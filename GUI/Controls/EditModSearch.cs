using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using log4net;

namespace CKAN.GUI
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

            ToolTip.SetToolTip(ExpandButton, Properties.Resources.EditModSearchTooltipExpandButton);

            // TextBox resizes unpredictably at runtime, so we need special logic
            // to line up the button with it
            ExpandButton.Top    = FilterCombinedTextBox.Top;
            ExpandButton.Height = ExpandButton.Width = FilterCombinedTextBox.Height;
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
        public event Action<EditModSearch, ModSearch> ApplySearch;

        /// <summary>
        /// Event fired when user wants to switch focus away from this control.
        /// </summary>
        public event Action SurrenderFocus;

        public ModSearch Search
        {
            get => currentSearch;
            set
            {
                currentSearch = value;
                SearchToEditor();
                FilterCombinedTextBox.Text = currentSearch?.Combined ?? "";
            }
        }

        public bool ShowLabel
        {
            set
            {
                FilterCombinedLabel.Visible = value;
                FilterOrLabel.Visible = !value;
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(EditModSearch));
        private Timer filterTimer;
        private bool suppressSearch = false;
        private ModSearch currentSearch = null;

        private void ExpandButton_CheckedChanged(object sender, EventArgs e)
        {
            ExpandButton.Text = ExpandButton.Checked ? "▴" : "▾";
            DoLayout(ExpandButton.Checked);
        }

        private void DoLayout(bool expanded)
        {
            FormGeometryChanged(null, null);
            SearchDetails.Visible = expanded;
            if (SearchDetails.Visible)
            {
                SearchDetails.FilterByNameTextBox.Focus();
                if (Main.Instance != null)
                {
                    Main.Instance.Move   += FormGeometryChanged;
                    Resize += FormGeometryChanged;
                }
            }
            else if (Main.Instance != null)
            {
                Main.Instance.Move   -= FormGeometryChanged;
                Resize -= FormGeometryChanged;
            }
        }

        private void FormGeometryChanged(object sender, EventArgs e)
        {
            SearchDetails.Location = PointToScreen(new Point(
                FilterCombinedTextBox.Left,
                FilterCombinedTextBox.Top + FilterCombinedTextBox.Height - 1
            ));
            // Fit dropdown from left edge of text box to right edge of button
            SearchDetails.Width = Math.Max(200,
                ExpandButton.Left + ExpandButton.Width
                    - FilterCombinedTextBox.Left);
        }

        private void FilterCombinedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (suppressSearch)
                return;

            try
            {
                currentSearch = ModSearch.Parse(FilterCombinedTextBox.Text,
                    Main.Instance.ManageMods.mainModList.ModuleLabels.LabelsFor(Main.Instance.CurrentInstance.Name).ToList()
                );
                SearchToEditor();
                TriggerSearchOrTimer();
            }
            catch (Kraken k)
            {
                Main.Instance.AddStatusMessage(k.Message);
            }
        }

        private void SearchToEditor()
        {
            suppressSearch = true;
                SearchDetails.FilterByNameTextBox.Text        = currentSearch?.Name
                                                                    ?? "";
                SearchDetails.FilterByAuthorTextBox.Text      = currentSearch?.Authors.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterByDescriptionTextBox.Text = currentSearch?.Description
                                                                    ?? "";
                SearchDetails.FilterByLanguageTextBox.Text    = currentSearch?.Localizations.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterByDependsTextBox.Text     = currentSearch?.DependsOn.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterByRecommendsTextBox.Text  = currentSearch?.Recommends.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterByConflictsTextBox.Text   = currentSearch?.ConflictsWith.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterBySuggestsTextBox.Text    = currentSearch?.Suggests.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterByTagsTextBox.Text        = currentSearch?.TagNames.Aggregate("", combinePieces)
                                                                    ?? "";
                SearchDetails.FilterByLabelsTextBox.Text      = currentSearch?.Labels
                                                                    .Select(lb => lb.Name)
                                                                    .Aggregate("", combinePieces)
                                                                    ?? "";

                SearchDetails.CompatibleToggle.Value      = currentSearch?.Compatible;
                SearchDetails.InstalledToggle.Value       = currentSearch?.Installed;
                SearchDetails.CachedToggle.Value          = currentSearch?.Cached;
                SearchDetails.NewlyCompatibleToggle.Value = currentSearch?.NewlyCompatible;
                SearchDetails.UpgradeableToggle.Value     = currentSearch?.Upgradeable;
                SearchDetails.ReplaceableToggle.Value     = currentSearch?.Replaceable;
            suppressSearch = false;
        }

        private static string combinePieces(string joined, string piece)
        {
            return string.IsNullOrEmpty(joined) ? piece : $"{joined} {piece}";
        }

        private void SearchDetails_ApplySearch(bool immediately)
        {
            if (suppressSearch)
                return;

            var knownLabels = Main.Instance.ManageMods.mainModList.ModuleLabels.LabelsFor(Main.Instance.CurrentInstance.Name).ToList();

            currentSearch = new ModSearch(
                SearchDetails.FilterByNameTextBox.Text,
                SearchDetails.FilterByAuthorTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterByDescriptionTextBox.Text,
                SearchDetails.FilterByLanguageTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterByDependsTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterByRecommendsTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterBySuggestsTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterByConflictsTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterByTagsTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList(),
                SearchDetails.FilterByLabelsTextBox.Text.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                    .Select(ln => knownLabels.FirstOrDefault(lb => lb.Name == ln))
                    .Where(lb => lb != null)
                    .ToList(),
                SearchDetails.CompatibleToggle.Value,
                SearchDetails.InstalledToggle.Value,
                SearchDetails.CachedToggle.Value,
                SearchDetails.NewlyCompatibleToggle.Value,
                SearchDetails.UpgradeableToggle.Value,
                SearchDetails.ReplaceableToggle.Value
            );
            suppressSearch = true;
                FilterCombinedTextBox.Text = currentSearch?.Combined ?? "";
            suppressSearch = false;
            if (immediately)
            {
                TriggerSearch();
            }
            else
            {
                TriggerSearchOrTimer();
            }
        }

        private void SearchDetails_SurrenderFocus()
        {
            if (SurrenderFocus != null)
            {
                SurrenderFocus();
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            ExpandButton.Checked = false;
        }

        private void FilterTextBox_Enter(object sender, EventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Switch focus from filters to mod list on enter, down, or pgdn
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    // Bypass the timer for immediate update
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    filterTimer?.Stop();
                    TriggerSearch();
                    break;

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
            if (Platform.IsMono && !string.IsNullOrEmpty(FilterCombinedTextBox.Text))
            {
                // Delay updating to improve typing performance on OS X and Linux
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
                filterTimer.Interval = 500;
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
            ApplySearch?.Invoke(this, currentSearch);
        }

    }
}
