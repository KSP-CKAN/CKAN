using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    public partial class WaitDialog : Form
    {
        public WaitDialog()
        {
            InitializeComponent();
        }

        public void ShowWaitDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            this.ShowDialog();
        }

        public void HideWaitDialog()
        {
            if (MessageTextBox.InvokeRequired)
            {
                MessageTextBox.Invoke(new MethodInvoker(delegate
                {
                    MessageTextBox.Text = "Waiting for operation to complete";
                }));
            }
            else
            {
                MessageTextBox.Text = "Waiting for operation to complete";
            }

            Close();
        }

        public void SetDescription(string message)
        {
            if (MessageTextBox.InvokeRequired)
            {
                MessageTextBox.Invoke(new MethodInvoker(delegate
                {
                    MessageTextBox.Text = "(" + message + ")";
                }));
            }
            else
            {
                MessageTextBox.Text = "(" + message + ")";
            }
        }

        private void ActionDescriptionLabel_Click(object sender, EventArgs e)
        {

        }

    }
}
