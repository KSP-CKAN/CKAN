using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {
        private ApplyChangesDialog m_ApplyChangesDialog = null;
        private ErrorDialog m_ErrorDialog = null;
        private RecommendsDialog m_RecommendsDialog = null;
        private SettingsDialog m_SettingsDialog = null;
        private WaitDialog m_WaitDialog = null;
        private YesNoDialog m_YesNoDialog = null;

        public void AddStatusMessage(string text, params object[] args)
        {
            Util.Invoke(StatusLabel, () => StatusLabel.Text = String.Format(text, args));
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
            Util.Invoke(this, () => Enabled = false);
            m_WaitDialog.ShowWaitDialog();
        }

        public void HideWaitDialog()
        {
            m_WaitDialog.Close();
            Util.Invoke(this, () => Enabled = true);
        }
    }
}