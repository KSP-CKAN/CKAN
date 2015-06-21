using System;
using System.ComponentModel;
using System.Net;
using Newtonsoft.Json;

namespace CKAN
{

    public partial class Main
    {
        private BackgroundWorker m_UpdateRepoWorker;

        public static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            WebClient client = new WebClient();

            if (master_uri == null)
            {
                master_uri = Repository.default_repo_master_list;
            }

            string json = client.DownloadString(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        public void UpdateRepo()
        {
            var old_dialog = currentUser.displayYesNo;
            currentUser.displayYesNo = YesNoDialog;

            tabController.RenameTab("WaitTabPage", "Updating repositories");

            try
            {
                m_UpdateRepoWorker.RunWorkerAsync();
            }
            finally
            {
                currentUser.displayYesNo = old_dialog;
            }

            Util.Invoke(this, SwitchEnabledState);

            SetDescription("Contacting repository..");
            ClearLog();
            ShowWaitDialog();
        }

        private bool _enabled = true;
        private void SwitchEnabledState()
        {
            _enabled = !_enabled;
            menuStrip1.Enabled = _enabled;
            MainTabControl.Enabled = _enabled;
            /* Windows (7 & 8 only?) bug #1548 has extra facets.
             * parent.childcontrol.Enabled = false seems to disable the parent,
             * if childcontrol had focus. Depending on optimization steps,
             * parent.childcontrol.Enabled = true does not necessarily
             * re-enable the parent.*/
            if (_enabled)
                this.Focus();
        }


        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            KSP current_instance = CurrentInstance;
            Repo.UpdateAllRepositories(RegistryManager.Instance(CurrentInstance), current_instance, GUI.user);
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            SetDescription("Scanning for manually installed mods");
            CurrentInstance.ScanGameData();

            if (e.Cancelled)
            {
                currentUser.displayMessage("Install Cancelled", new object[0]);
            }
            else if (e.Error != null)
            {
                currentUser.displayError("Failed to connect to repository. Exception: "+e.Error.ToString(), new object[0]);
            }
            else
            {
                UpdateModsList(repo_updated: true);
            }

            HideWaitDialog(true);

            if(!e.Cancelled && e.Error==null)
                AddStatusMessage("Repository successfully updated");

            ShowRefreshQuestion();
            Util.Invoke(this, SwitchEnabledState);
            Util.Invoke(this, RecreateDialogs);
            Util.Invoke(this, ModList.Select);
        }

        private void ShowRefreshQuestion()
        {
            if (!configuration.RefreshOnStartupNoNag)
            {
                currentUser.displayYesNo = YesNoDialog;
                configuration.RefreshOnStartupNoNag = true;
                if (!currentUser.displayYesNo("Would you like CKAN to refresh the modlist every time it is loaded? (You can always manually refresh using the button up top.)"))
                {
                    configuration.RefreshOnStartup = false;
                }
                configuration.Save();
                currentUser.displayYesNo = null;
            }
        }
    }
}
