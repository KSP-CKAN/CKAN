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

        public void SetDescription(string message)
        {
            if (ActionDescriptionLabel.InvokeRequired)
            {
                ActionDescriptionLabel.Invoke(new MethodInvoker(delegate
                {
                    ActionDescriptionLabel.Text = "(" + message + ")";
                }));
            }
            else
            {
                ActionDescriptionLabel.Text = "(" + message + ")";
            }
        }

        private void ActionDescriptionLabel_Click(object sender, EventArgs e)
        {

        }

    }
}
