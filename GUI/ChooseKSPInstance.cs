using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CKAN
{
    public partial class ChooseKSPInstance : Form
    {
        private FolderBrowserDialog browseKspFolder;
        private RenameInstanceDialog renameInstanceDialog;
        private readonly KSPManager manager;

        public ChooseKSPInstance()
        {
            manager = Main.Instance.Manager;
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;

            browseKspFolder = new FolderBrowserDialog();

            if (!manager.Instances.Any())
            {
                manager.FindAndRegisterDefaultInstance();
            }

            UpdateInstancesList();

            SetButtonsEnabled(false);
        }

        private void UpdateInstancesList()
        {
            SetButtonsEnabled(false);
            KSPInstancesListView.Items.Clear();

            foreach (var instance in manager.Instances)
            {
                var item = new ListViewItem { Text = instance.Key, Tag = instance.Key };

                item.SubItems.Add(new ListViewItem.ListViewSubItem { Text = instance.Value.Version().ToString() });

                item.SubItems.Add(new ListViewItem.ListViewSubItem { Text = instance.Value.GameDir() });

                KSPInstancesListView.Items.Add(item);
            }
        }

        private void AddNewButton_Click(object sender, EventArgs e)
        {
            if (browseKspFolder.ShowDialog() == DialogResult.OK)
            {
                KSP instance;
                string path = browseKspFolder.SelectedPath;
                try
                {
                    instance = new KSP(path, GUI.user);
                }
                catch (NotKSPDirKraken){
                    GUI.user.displayError("Directory {0} is not valid KSP directory.", new object[] {path});
                    return;
                }

                string instanceName = Path.GetFileName(path);
                instanceName = manager.GetNextValidInstanceName(instanceName);
                manager.AddInstance(instanceName, instance);
                UpdateInstancesList();
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            UseSelectedInstance();
        }

        private void UseSelectedInstance()
        {
            var instance = (string) KSPInstancesListView.SelectedItems[0].Tag;

            if (SetAsDefaultCheckbox.Checked)
            {
                manager.SetAutoStart(instance);
            }

            manager.SetCurrentInstance(instance);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void KSPInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var has_instance = KSPInstancesListView.SelectedItems.Count != 0;
            SetButtonsEnabled(has_instance);
        }

        private void KSPInstancesListView_DoubleClick(object sender, EventArgs r)
        {
            var has_instance = KSPInstancesListView.SelectedItems.Count != 0;
            if(has_instance)
                UseSelectedInstance();
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            var instance = (string) KSPInstancesListView.SelectedItems[0].Tag;

            renameInstanceDialog = new RenameInstanceDialog();
            if (renameInstanceDialog.ShowRenameInstanceDialog(instance) == DialogResult.OK)
            {
                manager.RenameInstance(instance, renameInstanceDialog.GetResult());
                UpdateInstancesList();
            }
        }

        private void Forget_Click(object sender, EventArgs e)
        {
            var instance = (string)KSPInstancesListView.SelectedItems[0].Tag;
            manager.RemoveInstance(instance);
            UpdateInstancesList();

        }

        private void InstallSettingsButton_Click (object sender, EventArgs e){
            
        }

        private void SetButtonsEnabled(bool has_instance)
        {
            ForgetButton.Enabled = RenameButton.Enabled = SelectButton.Enabled = SetAsDefaultCheckbox.Enabled = InstallSettingsButton.Enabled = has_instance;
        }

    }
}
