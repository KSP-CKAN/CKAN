using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {

        private ErrorDialog m_ErrorDialog;
        private SettingsDialog m_SettingsDialog;
       // private YesNoDialog m_YesNoDialog = null;

        public void RecreateDialogs()
        {
            m_SettingsDialog = controlFactory.CreateControl<SettingsDialog>();
          //  m_YesNoDialog = controlFactory.CreateControl<YesNoDialog>();
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
    }
}