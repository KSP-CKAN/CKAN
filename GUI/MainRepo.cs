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
            var old_dialog = m_User.displayYesNo;
            m_User.displayYesNo = YesNoDialog;

            m_TabController.RenameTab("WaitTabPage", "Updating repositories");

            CurrentInstance.ScanGameData();

            try
            {
                m_UpdateRepoWorker.RunWorkerAsync();
            }
            finally
            {
                m_User.displayYesNo = old_dialog;
            }

            Util.Invoke(this, delegate { SetEnabledState(false); });

            SetDescription("Contacting repository..");
            ClearLog();
            ShowWaitDialog();
        }

        private void SetEnabledState(bool enabled)
        {
            menuStrip1.Enabled = enabled;
            MainTabControl.Enabled = enabled;
            /* Windows (7 & 8 only?) bug #1548 has extra facets. 
             * parent.childcontrol.Enabled = false seems to disable the parent,
             * if childcontrol had focus. Depending on optimization steps,
             * parent.childcontrol.Enabled = true does not necessarily
             * re-enable the parent.*/
            if (enabled)
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
                m_ErrorDialog.ShowErrorDialog(ex.Message);
            }
            catch (MissingCertificateKraken ex)
            {
                m_ErrorDialog.ShowErrorDialog(ex.ToString());
            }
            catch (Exception ex)
            {
                m_ErrorDialog.ShowErrorDialog("Failed to connect to repository. Exception: " + ex.Message);
            }

            m_User.displayYesNo = null;
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList(repo_updated: true);

            HideWaitDialog(true);
            AddStatusMessage("Repository successfully updated");
            ShowRefreshQuestion();

            Util.Invoke(this, delegate { SetEnabledState(true); });
            Util.Invoke(this, RecreateDialogs);
            Util.Invoke(this, ModList.Select);
        }

        private void ShowRefreshQuestion()
        {
            if (!m_Configuration.RefreshOnStartupNoNag)
            {
                m_User.displayYesNo = YesNoDialog;
                m_Configuration.RefreshOnStartupNoNag = true;
                if (!m_User.displayYesNo("Would you like CKAN to refresh the modlist every time it is loaded? (You can always manually refresh using the button up top.)"))
                {
                    m_Configuration.RefreshOnStartup = false;
                }
                m_Configuration.Save();
                m_User.displayYesNo = null;
            }
        }
    }
}
