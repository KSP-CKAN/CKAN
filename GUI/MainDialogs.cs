using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public void RecreateDialogs()
        {
            errorDialog = controlFactory.CreateControl<ErrorDialog>();
            settingsDialog = controlFactory.CreateControl<SettingsDialog>();
            pluginsDialog = controlFactory.CreateControl<PluginsDialog>();
            yesNoDialog = controlFactory.CreateControl<YesNoDialog>();
        }

        public void AddStatusMessage(string text, params object[] args)
        {
            Util.Invoke(StatusLabel, () => StatusLabel.Text = String.Format(text, args));
            AddLogMessage(String.Format(text, args));
        }

        public void ErrorDialog(string text, params object[] args)
        {
            errorDialog.ShowErrorDialog(String.Format(text, args));
        }

        public bool YesNoDialog(string text)
        {
            return yesNoDialog.ShowYesNoDialog(text) == DialogResult.Yes;
        }

        //Ugly Hack. Possible fix is to alter the relationship provider so we can use a loop
        //over reason for to find a user requested mod. Or, you know, pass in a handler to it.
        private readonly ConcurrentStack<GUIMod> last_mod_to_have_install_toggled = new ConcurrentStack<GUIMod>();
        public async Task<CkanModule> TooManyModsProvide(TooManyModsProvideKraken kraken)
        {
            //We want LMtHIT to be the last user selection. If we alter this handling a too many provides
            // it needs to be reset so a potential second too many provides doesn't use the wrong mod.
            GUIMod mod;

            TaskCompletionSource<CkanModule> task = new TaskCompletionSource<CkanModule>();
            Util.Invoke(this, () =>
            {
                UpdateProvidedModsDialog(kraken, task);
                tabController.ShowTab("ChooseProvidedModsTabPage", 3);
                tabController.SetTabLock(true);
            });
            var module = await task.Task;

            if (module == null)
            {
                last_mod_to_have_install_toggled.TryPeek(out mod);
                MarkModForInstall(mod.Identifier);
            }
            Util.Invoke(this, () =>
            {
                tabController.SetTabLock(false);

                tabController.HideTab("ChooseProvidedModsTabPage");

                tabController.ShowTab("ManageModsTabPage");
            });

            if(module!=null)
                MarkModForInstall(module.identifier);

            last_mod_to_have_install_toggled.TryPop(out mod);
            return module;
        }
    }
}