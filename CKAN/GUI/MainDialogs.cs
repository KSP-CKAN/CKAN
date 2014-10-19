using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {
        private readonly ApplyChangesDialog m_ApplyChangesDialog = null;
        private readonly ErrorDialog m_ErrorDialog = null;
        private readonly RecommendsDialog m_RecommendsDialog = null;
        private readonly SettingsDialog m_SettingsDialog = null;
        private readonly WaitDialog m_WaitDialog = null;
        private readonly YesNoDialog m_YesNoDialog = null;

        public void AddStatusMessage(string text, params object[] args)
        {
            if (StatusLabel.InvokeRequired)
            {
                StatusLabel.Invoke(new MethodInvoker(delegate { StatusLabel.Text = String.Format(text, args); }));
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
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { Enabled = false; }));
            }
            else
            {
                Enabled = false;
            }
            
            m_WaitDialog.ShowWaitDialog();
        }

        public void HideWaitDialog()
        {
            m_WaitDialog.Close();

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