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
    public partial class YesNoDialog : Form
    {
        public YesNoDialog()
        {
            InitializeComponent();
        }

        public DialogResult ShowYesNoDialog(string text)
        {
            if (DescriptionLabel.InvokeRequired)
            {
                DescriptionLabel.Invoke(new MethodInvoker(delegate
                {
                    DescriptionLabel.Text = text;
                }));
            }
            else
            {
                DescriptionLabel.Text = text;
            }

            StartPosition = FormStartPosition.CenterScreen;
            return ShowDialog();
        }

        public void HideYesNoDialog()
        {
            Close();
        }

    }
}
