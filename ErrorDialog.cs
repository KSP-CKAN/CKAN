using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public void ShowErrorDialog(string message)
        {
            if (ErrorMessage.InvokeRequired)
            {
                ErrorMessage.Invoke(new MethodInvoker(delegate { ErrorMessage.Text = message; }));
            }
            else
            {
                ErrorMessage.Text = message;
            }

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { ShowDialog(); }));
            }
            else
            {
                ShowDialog();
            }
        }

        public void HideErrorDialog()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { Close(); }));
            }
            else
            {
                Close();
            }
        }

        private void DismissButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}