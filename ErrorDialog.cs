using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        public void ShowErrorDialog(string message)
        {
            if (ErrorMessage.InvokeRequired)
            {
                ErrorMessage.Invoke(new MethodInvoker(delegate
                {
                    ErrorMessage.Text = message;
                }));
            }
            else
            {
                ErrorMessage.Text = message;
            }

            ShowDialog();
        }

        public void HideErrorDialog()
        {
            Close();
        }

        private void DismissButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
