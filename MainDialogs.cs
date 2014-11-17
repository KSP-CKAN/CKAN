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
        private YesNoDialog m_YesNoDialog = null;

        public void RecreateDialogs()
        {
            m_ApplyChangesDialog = controlFactory.CreateControl<ApplyChangesDialog>();
            m_RecommendsDialog = controlFactory.CreateControl<RecommendsDialog>();
            m_SettingsDialog = controlFactory.CreateControl<SettingsDialog>();
            m_YesNoDialog = controlFactory.CreateControl<YesNoDialog>();
        }

        public void AddStatusMessage(string text, params object[] args)
        {
            Util.Invoke(StatusLabel, () => StatusLabel.Text = String.Format(text, args));
            AddLogMessage(String.Format(text, args));
        }

        public void ErrorDialog(string text, params object[] args)
        {
            m_ErrorDialog.ShowErrorDialog(String.Format(text, args));
        }

        public bool YesNoDialog(string text)
        {
            return m_YesNoDialog.ShowYesNoDialog(text) == DialogResult.Yes;
        }

    }
}