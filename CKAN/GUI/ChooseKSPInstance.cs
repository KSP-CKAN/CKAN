using System;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ChooseKSPInstance : Form
    {
        private FolderBrowserDialog m_BrowseKSPFolder;
        private RenameInstanceDialog m_RenameInstanceDialog;

        public ChooseKSPInstance()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            m_BrowseKSPFolder = new FolderBrowserDialog();


            if (!Main.Instance.Manager.GetInstances().Any())
            {
                Main.Instance.Manager.FindAndRegisterDefaultInstance();
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

            foreach (var instance in Main.Instance.Manager.GetInstances())
            {
                var item = new ListViewItem { Text = instance.Key, Tag = instance.Key };

                item.SubItems.Add(new ListViewItem.ListViewSubItem { Text = instance.Value.Version().ToString() });

                item.SubItems.Add(new ListViewItem.ListViewSubItem { Text = instance.Value.GameDir() });

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
                    instance = new KSP(m_BrowseKSPFolder.SelectedPath, GUI.user);
                }
                catch (NotKSPDirKraken){
                    GUI.user.displayError("Directory {0} is not valid KSP directory.", new object[] {m_BrowseKSPFolder.SelectedPath});
                    return;
                }

                string instanceName = Main.Instance.Manager.GetNextValidInstanceName("New instance");
                Main.Instance.Manager.GetInstances().Add(instanceName, instance);
                UpdateInstancesList();
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            var instance = (string) KSPInstancesListView.SelectedItems[0].Tag;

            if (SetAsDefaultCheckbox.Checked)
            {
                Main.Instance.Manager.SetAutoStart(instance);
            }

            Main.Instance.Manager.SetCurrentInstance(instance);
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
                Main.Instance.Manager.RenameInstance(instance, m_RenameInstanceDialog.GetResult());
                UpdateInstancesList();
            }
        }

    }
}
