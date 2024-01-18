using System;
using System.Windows.Forms;

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public partial class Main
    {
        private ErrorDialog errorDialog;
        private PluginsDialog pluginsDialog;
        private YesNoDialog yesNoDialog;
        private SelectionDialog selectionDialog;
        public ControlFactory controlFactory;

        public void RecreateDialogs()
        {
            if (controlFactory == null)
            {
                controlFactory = new ControlFactory();
            }
            errorDialog = controlFactory.CreateControl<ErrorDialog>();
            pluginsDialog = controlFactory.CreateControl<PluginsDialog>();
            yesNoDialog = controlFactory.CreateControl<YesNoDialog>();
            selectionDialog = controlFactory.CreateControl<SelectionDialog>();
        }

        [ForbidGUICalls]
        public void ErrorDialog(string text, params object[] args)
        {
            errorDialog.ShowErrorDialog(this, text, args);
        }

        [ForbidGUICalls]
        public bool YesNoDialog(string text, string yesText = null, string noText = null)
            => yesNoDialog.ShowYesNoDialog(this, text, yesText, noText) == DialogResult.Yes;

        /// <summary>
        /// Show a yes/no dialog with a "don't show again" checkbox
        /// </summary>
        /// <returns>A tuple of the dialog result and a bool indicating whether
        /// the suppress-checkbox has been checked (true)</returns>
        [ForbidGUICalls]
        public Tuple<DialogResult, bool> SuppressableYesNoDialog(string text, string suppressText, string yesText = null, string noText = null)
            => yesNoDialog.ShowSuppressableYesNoDialog(this, text, suppressText, yesText, noText);

        [ForbidGUICalls]
        public int SelectionDialog(string message, params object[] args)
            => selectionDialog.ShowSelectionDialog(message, args);
    }
}
