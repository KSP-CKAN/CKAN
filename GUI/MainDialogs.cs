using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        private ErrorDialog errorDialog;
        private SettingsDialog settingsDialog;
        private PluginsDialog pluginsDialog;
        private YesNoDialog yesNoDialog;
        private SelectionDialog selectionDialog;

        public void RecreateDialogs()
        {
            errorDialog = controlFactory.CreateControl<ErrorDialog>();
            settingsDialog = controlFactory.CreateControl<SettingsDialog>();
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
            AddLogMessage(msg);
        }

        public void ErrorDialog(string text, params object[] args)
        {
            errorDialog.ShowErrorDialog(String.Format(text, args));
        }

        public bool YesNoDialog(string text, string yesText = null, string noText = null)
        {
            return yesNoDialog.ShowYesNoDialog(text, yesText, noText) == DialogResult.Yes;
        }

        public int SelectionDialog(string message, params object[] args)
        {
            return selectionDialog.ShowSelectionDialog(message, args);
        }
    }
}
