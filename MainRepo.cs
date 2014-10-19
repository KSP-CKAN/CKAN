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
            m_UpdateRepoWorker.RunWorkerAsync();

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { Enabled = false; }));
            }
            else
            {
                Enabled = false;
            }

            m_WaitDialog.SetDescription("Contacting repository..");
            m_WaitDialog.ClearLog();
            m_WaitDialog.ShowWaitDialog();
        }

        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            try
            {
                Repo.Update(m_Configuration.Repository);
            }
            catch (Exception)
            {
                m_ErrorDialog.ShowErrorDialog("Failed to connect to repository");
            }
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            UpdateModFilterList();

            m_WaitDialog.SetDescription("Scanning for manually installed mods");
            KSP.ScanGameData();

            m_WaitDialog.HideWaitDialog();
            AddStatusMessage("Repository successfully updated");

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { Enabled = true; }));
            }
            else
            {
                Enabled = true;
            }
        }
    }
}