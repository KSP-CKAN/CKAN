using System;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class Main
    {
        private ErrorDialog errorDialog;
        private PluginsDialog pluginsDialog;
        private YesNoDialog yesNoDialog;
        private SelectionDialog selectionDialog;

        public void RecreateDialogs()
        {
            errorDialog = controlFactory.CreateControl<ErrorDialog>();
            pluginsDialog = controlFactory.CreateControl<PluginsDialog>();
            yesNoDialog = controlFactory.CreateControl<YesNoDialog>();
            selectionDialog = controlFactory.CreateControl<SelectionDialog>();
        }

        public void AddStatusMessage(string text, params object[] args)
        {
            string msg = String.Format(text, args);
            // No newlines in status bar
            Util.Invoke(statusStrip1, () =>
                StatusLabel.ToolTipText = StatusLabel.Text = msg.Replace("\r\n", " ").Replace("\n", " ")
            );
            Wait.AddLogMessage(msg);
        }

        public void ErrorDialog(string text, params object[] args)
        {
            errorDialog.ShowErrorDialog(String.Format(text, args));
        }

        public bool YesNoDialog(string text, string yesText = null, string noText = null)
        {
            return yesNoDialog.ShowYesNoDialog(this, text, yesText, noText) == DialogResult.Yes;
        }

        /// <summary>
        /// Show a yes/no dialog with a "don't show again" checkbox
        /// </summary>
        /// <returns>A tuple of the dialog result and a bool indicating whether
        /// the suppress-checkbox has been checked (true)</returns>
        public Tuple<DialogResult, bool> SuppressableYesNoDialog(string text, string suppressText, string yesText = null, string noText = null)
        {
            return yesNoDialog.ShowSuppressableYesNoDialog(this, text, suppressText, yesText, noText);
        }

        public int SelectionDialog(string message, params object[] args)
        {
            return selectionDialog.ShowSelectionDialog(message, args);
        }
    }
}
