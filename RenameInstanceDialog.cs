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
    public partial class RenameInstanceDialog : Form
    {
        public RenameInstanceDialog()
        {
            InitializeComponent();
        }

        public DialogResult ShowRenameInstanceDialog(string name)
        {
            NameTextBox.Text = name;
            return ShowDialog();
        }

        public string GetResult()
        {
            return NameTextBox.Text;
        }

        private void RenameInstanceDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
