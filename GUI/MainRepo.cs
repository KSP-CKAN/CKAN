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
            var old_dialog = m_User.displayYesNo;
            m_User.displayYesNo = YesNoDialog;

            m_TabController.RenameTab("WaitTabPage", "Updating repositories");

            try
            {
                m_UpdateRepoWorker.RunWorkerAsync();
            }
            finally
            {
                m_User.displayYesNo = old_dialog;
            }

            Util.Invoke(this, () => Enabled = false);

            SetDescription("Contacting repository..");
            ClearLog();
            ShowWaitDialog();
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
                m_User.displayMessage("Install Cancelled", new object[0]);
            }
            else if (e.Error != null)
            {
                m_User.displayError("Failed to connect to repository. Exception: "+e.Error.ToString(), new object[0]);
            }
            else
            {
                UpdateModsList(repo_updated: true);
            }

            HideWaitDialog(true);
            if(!e.Cancelled && e.Error==null)
                AddStatusMessage("Repository successfully updated");

            Util.Invoke(ModList, () => ModList.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells));
            Util.Invoke(this, () => Enabled = true);
            Util.Invoke(this, RecreateDialogs);
        }
    }
}