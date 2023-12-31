using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// A control for displaying and editing a search of mods.
    /// Contains several separate fields for searching different properties,
    /// plus a combined field that represents them all in a special syntax.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
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

        public event Action<string> ShowError;

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

        public void CloseSearch(Point screenCoords)
        {
            // Treat the entire main window as an uncheck button, and let
            // the actual checkbox handle unchecking itself
            var bounds = new Rectangle(ExpandButton.PointToScreen(new Point(0, 0)),
                                       ExpandButton.Size);
            if (!bounds.Contains(screenCoords))
            {
                ExpandButton.Checked = false;
            }
        }

        public void ParentMoved()
        {
            FormGeometryChanged();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            FormGeometryChanged();
        }

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
                ShowError?.Invoke(k.Message);
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
        private readonly EventHandler<EventArgs> handler;

        private void ExpandButton_CheckedChanged(object sender, EventArgs e)
        {
            ExpandButton.Text = ExpandButton.Checked ? "▴" : "▾";
            DoLayout(ExpandButton.Checked);
        }

        private void DoLayout(bool expanded)
        {
            FormGeometryChanged();
            SearchDetails.Visible = expanded;
            if (SearchDetails.Visible)
            {
                SearchDetails.SetFocus();
            }
        }

        private void FormGeometryChanged()
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
            SearchDetails.PopulateSearch(currentSearch);
            suppressSearch = false;
        }

        private void SearchDetails_ApplySearch(object sender, EventArgs e)
        {
            if (suppressSearch)
            {
                return;
            }

            currentSearch = SearchDetails.CurrentSearch();
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
