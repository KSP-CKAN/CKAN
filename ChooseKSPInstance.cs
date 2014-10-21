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

        public ChooseKSPInstance()
        {
            InitializeComponent();

            m_BrowseKSPFolder = new FolderBrowserDialog();

            if (!KSP.Instances.Any())
            {
                KSP.AddDefaultInstance();
            }

            UpdateInstancesList();

            SelectButton.Enabled = false;
        }

        private void UpdateInstancesList()
        {
            KSPInstancesListView.Items.Clear();

            foreach (var instance in KSP.Instances)
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
                var instance = new KSP();
                instance.SetGameDir(m_BrowseKSPFolder.SelectedPath);
                KSP.Instances.Add("New instance", instance);
                UpdateInstancesList();
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            var instance = (string) KSPInstancesListView.SelectedItems[0].Tag;
            KSP.InitializeInstance(instance);
            Hide();
            Main.Instance.Show();
        }

        private void KSPInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectButton.Enabled = true;
        }
    }
}
