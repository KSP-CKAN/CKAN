using System;
using System.Windows.Forms;
using log4net;

namespace CKAN
{

    public partial class ErrorDialog : FormCompatibility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ErrorDialog));

        public ErrorDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public void ShowErrorDialog(string text, params object[] args)
        {
            if (Main.Instance.InvokeRequired)
            {
                Main.Instance.Invoke(new MethodInvoker(() => ShowErrorDialog(text, args)));
            }
            else
            {
                log.ErrorFormat(text, args);
                ErrorMessage.Text = String.Format(text, args);
                ShowDialog();
            }
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