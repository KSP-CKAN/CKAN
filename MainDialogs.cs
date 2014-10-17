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

        private WaitDialog m_WaitDialog = new WaitDialog();
        private ApplyChangesDialog m_ApplyChangesDialog = new ApplyChangesDialog();
        private SettingsDialog m_SettingsDialog = new SettingsDialog();
        private ErrorDialog m_ErrorDialog = new ErrorDialog();
        private YesNoDialog m_YesNoDialog = new YesNoDialog();
        private RecommendsDialog m_RecommendsDialog = new RecommendsDialog();

        public void AddStatusMessage(string text, params object[] args)
        {
            if (StatusLabel.InvokeRequired)
            {
                StatusLabel.Invoke(new MethodInvoker(delegate
                {
                    StatusLabel.Text = String.Format(text, args);
                }));
            }
            else
            {
                StatusLabel.Text = String.Format(text, args);
            }

            m_WaitDialog.AddLogMessage(String.Format(text, args));
        }

        public void ErrorDialog(string text, params object[] args)
        {
            m_ErrorDialog.ShowErrorDialog(String.Format(text, args));
        }

        public bool YesNoDialog(string text)
        {
            return m_YesNoDialog.ShowYesNoDialog(text) == DialogResult.Yes;
        }

        public void ShowWaitDialog()
        {
            Enabled = false;
            m_WaitDialog.ShowWaitDialog();
        }

        public void HideWaitDialog()
        {
            m_WaitDialog.Close();
            Enabled = true;
        }

    }

}
