using System;
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
        private int currentSelected;

        public SelectionDialog ()
        {
            InitializeComponent();
            currentSelected = 0;
        }

        /// <summary>
        /// Shows the selection dialog.
        /// </summary>
        /// <returns>The selected index, -1 if canceled.</returns>
        /// <param name="message">Message.</param>
        /// <param name="args">Array of items to select from.</param>
        [ForbidGUICalls]
        public int ShowSelectionDialog (string message, params object[] args)
        {
            int defaultSelection = -1;
            int return_cancel = -1;

            // Validate input.
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new Kraken("Passed message string must be non-empty.");
            }

            if (args.Length == 0)
            {
                throw new Kraken("Passed list of selection candidates must be non-empty.");
            }

            // Hide the default button unless we have a default option
            Util.Invoke(DefaultButton, DefaultButton.Hide);
            // Clear the item list.
            Util.Invoke(OptionsList, OptionsList.Items.Clear);

            // Check if we have a default option.
            if (args[0] is int v)
            {
                // Check that the default selection makes sense.
                defaultSelection = v;

                if (defaultSelection < 0 || defaultSelection > args.Length - 1)
                {
                    throw new Kraken("Passed default arguments is out of range of the selection candidates.");
                }

                // Extract the relevant arguments.
                object[] newArgs = new object[args.Length - 1];

                for (int i = 1; i < args.Length; i++)
                {
                    newArgs[i - 1] = args[i];
                }

                args = newArgs;

                // Show the defaultButton.
                Util.Invoke(DefaultButton, DefaultButton.Show);
            }

            // Further data validation.
            foreach (object argument in args)
            {
                if (string.IsNullOrWhiteSpace(argument.ToString()))
                {
                    throw new Kraken("Candidate may not be empty.");
                }
            }

            // Add all items to the OptionsList.
            for (int i = 0; i < args.Length; i++)
            {
                if (defaultSelection == i)
                {
                    Util.Invoke(OptionsList, () => OptionsList.Items.Add(string.Concat(args[i].ToString(), "  -- Default")));

                }
                else
                {
                    Util.Invoke(OptionsList, () => OptionsList.Items.Add(args[i].ToString()));
                }
            }

            // Write the message to the label.
            Util.Invoke(MessageLabel, () => MessageLabel.Text = message);

            // Now show the dialog and get the return values.
            DialogResult result = ShowDialog();
            if (result == DialogResult.Yes)
            {
                // If pressed Defaultbutton
                return defaultSelection;
            }
            else if (result == DialogResult.Cancel)
            {
                // If pressed CancelButton
                return return_cancel;
            }
            else
            {
                return currentSelected;
            }
        }

        public void HideYesNoDialog ()
        {
            Util.Invoke(this, Close);
        }

        private void OptionsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentSelected = OptionsList.SelectedIndex;
        }
    }
}
