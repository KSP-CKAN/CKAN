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
    public partial class ChooseKSPInstance : Form
    {
        private FolderBrowserDialog m_BrowseKSPFolder = null;
        private RenameInstanceDialog m_RenameInstanceDialog = null;

        public ChooseKSPInstance()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            m_BrowseKSPFolder = new FolderBrowserDialog();

            if (!KSPManager.Instances.Any())
            {
                KSPManager.FindAndRegisterDefaultInstance();
            }

            UpdateInstancesList();

            SelectButton.Enabled = false;
            RenameButton.Enabled = false;
            SetAsDefaultCheckbox.Enabled = false;
        }

        private void UpdateInstancesList()
        {
            SelectButton.Enabled = false;
            RenameButton.Enabled = false;
            SetAsDefaultCheckbox.Enabled = false;

            KSPInstancesListView.Items.Clear();

            foreach (var instance in KSPManager.Instances)
            {
                var item = new ListViewItem() { Text = instance.Key, Tag = instance.Key };

                item.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = instance.Value.Version().ToString() });

                item.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = instance.Value.GameDir() });

                KSPInstancesListView.Items.Add(item);
            }
        }

        private void AddNewButton_Click(object sender, EventArgs e)
        {
            if (m_BrowseKSPFolder.ShowDialog() == DialogResult.OK)
            {
                KSP instance;
                try
                {
                     instance = new KSP(m_BrowseKSPFolder.SelectedPath);
                }
                catch (NotKSPDirKraken){
                    User.displayError("Directory {0} is not valid KSP directory.", m_BrowseKSPFolder.SelectedPath);
                    return;
                }

                string instanceName = KSPManager.GetNextValidInstanceName("New instance");
                KSPManager.Instances.Add(instanceName, instance);
                UpdateInstancesList();
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            var instance = (string) KSPInstancesListView.SelectedItems[0].Tag;

            if (SetAsDefaultCheckbox.Checked)
            {
                KSPManager.SetAutoStart(instance);
            }

            KSPManager.SetCurrentInstance(instance);
            Hide();
            Main.Instance.Show();
        }

        private void KSPInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KSPInstancesListView.SelectedItems.Count == 0)
            {
                SelectButton.Enabled = false;
                RenameButton.Enabled = false;
                SetAsDefaultCheckbox.Enabled = false;
                return;
            }

            RenameButton.Enabled = true;
            SelectButton.Enabled = true;
            SetAsDefaultCheckbox.Enabled = true;
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            var instance = (string) KSPInstancesListView.SelectedItems[0].Tag;

            m_RenameInstanceDialog = new RenameInstanceDialog();
            if (m_RenameInstanceDialog.ShowRenameInstanceDialog(instance) == DialogResult.OK)
            {
                KSPManager.RenameInstance(instance, m_RenameInstanceDialog.GetResult());
                UpdateInstancesList();
            }
        }

    }
}
