﻿using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace CKAN
{
    public struct RepositoryList
    {
        public Repository[] repositories;
    }


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

            CurrentInstance.ScanGameData();

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
            try
            {
                KSP current_instance1 = CurrentInstance;
                Repo.UpdateAllRepositories(RegistryManager.Instance(CurrentInstance), current_instance1, GUI.user);
            }
            catch (UriFormatException ex)
            {
                errorDialog.ShowErrorDialog(ex.Message);
            }
            catch (MissingCertificateKraken ex)
            {
                errorDialog.ShowErrorDialog(ex.ToString());
            }
            catch (Exception ex)
            {
                errorDialog.ShowErrorDialog("Failed to connect to repository. Exception: " + ex.Message);
            }

            currentUser.displayYesNo = null;
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList(repo_updated: true);

            HideWaitDialog(true);
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
