using System;
using System.Windows.Forms;
using log4net;

namespace CKAN
{
    public partial class ErrorDialog : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ErrorDialog));

        public ErrorDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public void ShowErrorDialog(string text, params object[] args)
        {
            log.ErrorFormat(text, args);
            Util.Invoke(ErrorMessage, () => ErrorMessage.Text = String.Format(text, args));
            Util.Invoke(this, () => ShowDialog());
        }

        public void HideErrorDialog()
        {
            Util.Invoke(this, () => Close());
        }

        private void DismissButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}