using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CKAN
{

    public partial class Main : Form
    {

        private BackgroundWorker m_UpdateRepoWorker = null;

        public void UpdateRepo()
        {
            m_UpdateRepoWorker.RunWorkerAsync();
            Enabled = false;
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
            Enabled = true;
        }
    }

}
