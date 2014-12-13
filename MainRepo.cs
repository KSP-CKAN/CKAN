using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {
        private BackgroundWorker m_UpdateRepoWorker;

        public void UpdateRepo()
        {
            m_TabController.RenameTab("WaitTabPage", "Updating repository");

            CurrentInstance.ScanGameData();

            m_UpdateRepoWorker.RunWorkerAsync();

            Util.Invoke(this, () => Enabled = false);

            SetDescription("Contacting repository..");
            ClearLog();
            ShowWaitDialog();
        }

        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            try
            {
                KSP current_instance1 = CurrentInstance;
                Repo.Update(RegistryManager.Instance(CurrentInstance), current_instance1.Version(), (Uri)null);
            }
            catch (MissingCertificateKraken ex)
            {
                m_ErrorDialog.ShowErrorDialog(ex.ToString());
            }
            catch (Exception)
            {
                m_ErrorDialog.ShowErrorDialog("Failed to connect to repository");
            }
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            SetDescription("Scanning for manually installed mods");
            CurrentInstance.ScanGameData();

            UpdateModsList();

            HideWaitDialog(true);
            AddStatusMessage("Repository successfully updated");

            Util.Invoke(ModList, () => ModList.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells));
            Util.Invoke(this, () => Enabled = true);
            Util.Invoke(this, () => RecreateDialogs());
        }
    }
}