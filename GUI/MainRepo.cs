using System;
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
            m_User.displayYesNo = YesNoDialog;

            m_TabController.RenameTab("WaitTabPage", "Updating repositories");

            CurrentInstance.ScanGameData();

            m_UpdateRepoWorker.RunWorkerAsync();

            Util.Invoke(this, SwitchEnabledState);

            SetDescription("Contacting repository..");
            ClearLog();
            ShowWaitDialog();
        }

        //Todo: better name for this method
        private void SwitchEnabledState()
        {
            menuStrip1.Enabled = !menuStrip1.Enabled;
            MainTabControl.Enabled = !MainTabControl.Enabled;
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
            SetDescription("Scanning for manually installed mods");

            UpdateModsList(repo_updated: true);

            HideWaitDialog(true);
            AddStatusMessage("Repository successfully updated");
            ShowRefreshQuestion();

            Util.Invoke(this, SwitchEnabledState);
            Util.Invoke(this, RecreateDialogs);
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