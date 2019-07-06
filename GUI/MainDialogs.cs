using System;
using System.Collections.Concurrent;
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

        // Ugly Hack. Possible fix is to alter the relationship provider so we can use a loop
        // over reason for to find a user requested mod. Or, you know, pass in a handler to it.
        private readonly ConcurrentStack<GUIMod> last_mod_to_have_install_toggled = new ConcurrentStack<GUIMod>();

        private async Task<CkanModule> TooManyModsProvideCore(TooManyModsProvideKraken kraken)
        {
            TaskCompletionSource<CkanModule> task = new TaskCompletionSource<CkanModule>();
            Util.Invoke(this, () =>
            {
                UpdateProvidedModsDialog(kraken, task);
                tabController.ShowTab("ChooseProvidedModsTabPage", 3);
                tabController.SetTabLock(true);
            });
            return await task.Task;
        }

        public async Task<CkanModule> TooManyModsProvide(TooManyModsProvideKraken kraken)
        {
            // We want LMtHIT to be the last user selection. If we alter this handling a too many provides
            // it needs to be reset so a potential second too many provides doesn't use the wrong mod.
            GUIMod mod;

            var module = await TooManyModsProvideCore(kraken);

            if (module == null
                    && last_mod_to_have_install_toggled.TryPeek(out mod))
            {
                MarkModForInstall(mod.Identifier, true);
            }
            Util.Invoke(this, () =>
            {
                tabController.SetTabLock(false);
                tabController.HideTab("ChooseProvidedModsTabPage");
                tabController.ShowTab("ManageModsTabPage");
            });

            last_mod_to_have_install_toggled.TryPop(out mod);
            return module;
        }
    }
}
