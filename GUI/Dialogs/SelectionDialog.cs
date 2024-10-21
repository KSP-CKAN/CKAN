using System;
using System.Linq;
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
    public partial class SelectionDialog : Form
    {
        public SelectionDialog()
        {
            InitializeComponent();
            currentSelected = 0;
        }

        /// <summary>
        /// Shows the selection dialog
        /// </summary>
        /// <returns>The selected index, -1 if canceled</returns>
        /// <param name="message">Message.</param>
        /// <param name="args">Array of items to select from. If first is an int, it will be interpreted as the index of the default option.</param>
        [ForbidGUICalls]
        public int ShowSelectionDialog(string message, params object[] args)
        {
            int defaultSelection = -1;
            int return_cancel    = -1;

            // Validate input
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new Kraken("Passed message string must be non-empty.");
            }

            // Check if we have a default option
            if (//args is [int v, ..]
                args.Length > 0
                && args[0] is int v)
            {
                // Check that the default selection makes sense
                defaultSelection = v;

                if (defaultSelection < 0 || defaultSelection > args.Length - 1)
                {
                    throw new Kraken("Passed default argument is out of range of the selection candidates.");
                }

                // Extract the relevant arguments.
                args = args.Skip(1).ToArray();

                // Show the default button
                Util.Invoke(this, DefaultButton.Show);
            }
            else
            {
                // Hide the default button unless we have a default option
                Util.Invoke(this, DefaultButton.Hide);
            }

            if (args.Length == 0)
            {
                throw new Kraken("Passed list of selection candidates must be non-empty.");
            }

            // Further data validation
            var argStrs = args.Select(arg => arg.ToString() ?? "").ToArray();
            if (argStrs.Any(string.IsNullOrWhiteSpace))
            {
                throw new Kraken("Candidate may not be empty.");
            }

            DialogResult result = DialogResult.Cancel;

            // Validation completed, set up the UI
            Util.Invoke(this, () =>
            {
                // Write the message to the label
                MessageLabel.Text = message;

                // Clear the item list.
                OptionsList.Items.Clear();

                // Add all items to the OptionsList
                OptionsList.Items.AddRange(
                    argStrs.Select((arg, i) =>
                                defaultSelection == i
                                    ? string.Format(Properties.Resources.SelectionDialogDefault,
                                                    arg)
                                    : arg)
                           .ToArray());

                if (defaultSelection >= 0)
                {
                    OptionsList.SetSelected(defaultSelection, true);
                }

                Height = Height - OptionsList.Height
                         + (OptionsList.ItemHeight * (OptionsList.Items.Count + 2));

                result = ShowDialog(ActiveForm);
            });

            // Show the dialog and get the return values
            return result switch
            {
                // Lots of dialog results we don't care about
                DialogResult.Abort or DialogResult.Retry or DialogResult.Ignore
                or DialogResult.No or DialogResult.None
                    => throw new NotImplementedException(),

                // If pressed Default button
                DialogResult.Yes => defaultSelection,

                // If pressed Cancel button
                DialogResult.Cancel => return_cancel,

                // If pressed Select button or double clicked
                DialogResult.OK or _ => currentSelected,
            };
        }

        private void OptionsList_SelectedIndexChanged(object? sender, EventArgs? e)
        {
            currentSelected = OptionsList.SelectedIndex;
        }

        private void OptionsList_DoubleClick(object? sender, EventArgs? e)
        {
            currentSelected = OptionsList.SelectedIndex;
            DialogResult    = SelectButton.DialogResult;
            Close();
        }

        private void OptionsList_KeyDown(object? sender, KeyEventArgs? e)
        {
            switch (e?.KeyCode)
            {
                case Keys.Enter:
                    e.Handled = true;
                    currentSelected = OptionsList.SelectedIndex;
                    DialogResult    = SelectButton.DialogResult;
                    Close();
                    break;

                case Keys.Escape:
                    e.Handled = true;
                    DialogResult = CancelSelectionButton.DialogResult;
                    Close();
                    break;
            }
        }

        private int currentSelected;
    }
}
