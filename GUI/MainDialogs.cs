using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        private ErrorDialog m_ErrorDialog;
        private SettingsDialog m_SettingsDialog;
        private PluginsDialog m_PluginsDialog;
        private YesNoDialog m_YesNoDialog = null;

        public void RecreateDialogs()
        {
            m_SettingsDialog = controlFactory.CreateControl<SettingsDialog>();
            m_PluginsDialog = controlFactory.CreateControl<PluginsDialog>();
            m_YesNoDialog = controlFactory.CreateControl<YesNoDialog>();
        }

        public void AddStatusMessage(string text, params object[] args)
        {
            Util.Invoke(StatusLabel, () => StatusLabel.Text = String.Format(text, args));
            AddLogMessage(String.Format(text, args));
        }

        public void ErrorDialog(string text, params object[] args)
        {
            m_ErrorDialog.ShowErrorDialog(String.Format(text, args));
        }

        public bool YesNoDialog(string text)
        {
            return m_YesNoDialog.ShowYesNoDialog(text) == DialogResult.Yes;
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
                m_TabController.ShowTab("ChooseProvidedModsTabPage", 3);
                m_TabController.SetTabLock(true);
            });
            var module = await task.Task;

            if (module == null)
            {
                last_mod_to_have_install_toggled.TryPeek(out mod);
                MarkModForInstall(mod.Identifier,uninstall:true);                
            }
            Util.Invoke(this, () =>
            {
                m_TabController.SetTabLock(false);

                m_TabController.HideTab("ChooseProvidedModsTabPage");

                m_TabController.ShowTab("ManageModsTabPage");
            });

            if(module!=null)
                MarkModForInstall(module.identifier);
            
            last_mod_to_have_install_toggled.TryPop(out mod);
            return module;
        }
    }
}