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
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            CKANRepositoryTextBox.Text = Main.Instance.m_Configuration.Repository;
        }

        private void CKANRepositoryApplyButton_Click(object sender, EventArgs e)
        {
            Main.Instance.m_Configuration.Repository = CKANRepositoryTextBox.Text;
            Main.Instance.UpdateRepo();
            Main.Instance.m_Configuration.Save();
            Close();
        }

        private void CKANRepositoryDefaultButton_Click(object sender, EventArgs e)
        {
            Main.Instance.m_Configuration.Repository = Repo.default_ckan_repo;
            Main.Instance.UpdateRepo();
            Main.Instance.m_Configuration.Save();
            Close();
        }
    }
}
