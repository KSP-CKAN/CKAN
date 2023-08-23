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

            handler = Util.Debounce<EventArgs>(ImmediateHandler,
                                               SkipDelayIf,
                                               AbortIf,
                                               DelayedHandler);

            // Sharing handler combines the delay from both events
            FilterCombinedTextBox.TextChanged += (sender, e) => handler(sender, e);
            FilterCombinedTextBox.KeyDown     += (sender, e) => handler(sender, e);
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
        private bool suppressSearch = false;
        private ModSearch currentSearch = null;

        private void ImmediateHandler(object sender, EventArgs e)
        {
            try
            {
                switch (e)
                {
                    case KeyEventArgs keyEvt:
                        switch (keyEvt.KeyCode)
                        {
                            // Switch focus from filters to mod list on enter, down, or pgdn
                            case Keys.Up:
                            case Keys.Down:
                            case Keys.PageUp:
                            case Keys.PageDown:
                                keyEvt.Handled = true;
                                SurrenderFocus?.Invoke();
                                return;
                        }
                        break;
                }
                // Sync the search boxes immediately
                currentSearch = ModSearch.Parse(FilterCombinedTextBox.Text,
                    Main.Instance.ManageMods.mainModList.ModuleLabels.LabelsFor(Main.Instance.CurrentInstance.Name).ToList());
                SearchToEditor();
            }
            catch (Kraken k)
            {
                Main.Instance.AddStatusMessage(k.Message);
            }
        }

        private bool SkipDelayIf(object sender, EventArgs e)
        {
            if (e == null)
            {
                // The tri state search controls don't pass their events to us,
                // and we want immediate handling for them
                return true;
            }
            switch (e) {
                case KeyEventArgs keyEvt:
                    switch (keyEvt.KeyCode)
                    {
                        // Refresh immediately if user presses enter
                        case Keys.Enter:
                            keyEvt.Handled = true;
                            keyEvt.SuppressKeyPress = true;
                            return true;

                        // Let the runtime update the text box first for any other keys
                        default:
                            return false;
                    }
            }
            // Always refresh immediately on clear
            return string.IsNullOrEmpty(FilterCombinedTextBox.Text)
                // Or if the plain text search is long enough to return few enough results to be fast
                || (currentSearch?.Name.Length ?? 0) > 4;
        }

        private bool AbortIf(object sender, EventArgs e) => suppressSearch;

        private void DelayedHandler(object sender, EventArgs e)
        {
            ApplySearch?.Invoke(this, currentSearch);
        }

        /// <summary>
        /// Allow multiple changes to the filter criteria before automatically
        /// refreshing the display. Refresh may take longer than the time between key strokes,
        /// which makes the UI seem unresponsive or buggy:
        /// http://mono.1490590.n4.nabble.com/Incorrect-missing-and-duplicate-keypress-events-td4658863.html
        /// </summary>
        private EventHandler<EventArgs> handler;

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

        private void SearchDetails_ApplySearch(object sender, EventArgs e)
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
            handler(sender, e);
        }

        private void SearchDetails_SurrenderFocus()
        {
            SurrenderFocus?.Invoke();
        }

        protected override void OnLeave(EventArgs e)
        {
            ExpandButton.Checked = false;
        }

        private void FilterTextBox_Enter(object sender, EventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }
    }
}
