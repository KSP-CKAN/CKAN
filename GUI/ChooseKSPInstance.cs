using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CKAN
{
    public partial class ChooseKSPInstance : Form
    {
        private readonly KSPManager _manager;
        private RenameInstanceDialog _renameInstanceDialog;
        private readonly OpenFileDialog _instanceDialog = new OpenFileDialog()
        {
            AddExtension = false,
            CheckFileExists = false,
            CheckPathExists = false,
            InitialDirectory = Environment.CurrentDirectory,
            Filter = "KSP binaries (KSP*.exe)|KSP*.exe",
            Multiselect = false
        };

        public bool HasSelections => KSPInstancesListView.SelectedItems.Count > 0;

        public ChooseKSPInstance()
        {
            _manager = Main.Instance.Manager;
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;

            if (!_manager.Instances.Any())
            {
                _manager.FindAndRegisterDefaultInstance();
            }

            UpdateInstancesList();
            UpdateButtonState();
        }

        private void UpdateInstancesList()
        {
            KSPInstancesListView.Items.Clear();
            UpdateButtonState();

            foreach (var instance in _manager.Instances)
            {
                var item = new ListViewItem { Text = instance.Key, Tag = instance.Key };

                item.SubItems.Add(new ListViewItem.ListViewSubItem { Text = instance.Value.Version().ToString() });

                item.SubItems.Add(new ListViewItem.ListViewSubItem { Text = instance.Value.GameDir() });

                KSPInstancesListView.Items.Add(item);
            }
        }

        private void AddNewButton_Click(object sender, EventArgs e)
        {
            if (_instanceDialog.ShowDialog() != DialogResult.OK) return;
            if (!File.Exists(_instanceDialog.FileName)) return;

            KSP instance;
            var path = Path.GetDirectoryName(_instanceDialog.FileName);
            try
            {
                instance = new KSP(path, GUI.user);
            }
            catch (NotKSPDirKraken)
            {
                GUI.user.displayError("Directory {0} is not valid KSP directory.", new object[] { path });
                return;
            }

            var instanceName = Path.GetFileName(path);
            instanceName = _manager.GetNextValidInstanceName(instanceName);
            _manager.AddInstance(instanceName, instance);
            UpdateInstancesList();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            UseSelectedInstance();
        }

        private void UseSelectedInstance()
        {
            var instance = (string)KSPInstancesListView.SelectedItems[0].Tag;

            if (SetAsDefaultCheckbox.Checked)
            {
                _manager.SetAutoStart(instance);
            }

            _manager.SetCurrentInstance(instance);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void KSPInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void KSPInstancesListView_DoubleClick(object sender, EventArgs r)
        {
            if (HasSelections) UseSelectedInstance();
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            var instance = (string)KSPInstancesListView.SelectedItems[0].Tag;

            // show the dialog, and only continue if the user selected "OK"
            _renameInstanceDialog = new RenameInstanceDialog();
            if (_renameInstanceDialog.ShowRenameInstanceDialog(instance) != DialogResult.OK) return;

            // proceed with instance rename
            _manager.RenameInstance(instance, _renameInstanceDialog.GetResult());
            UpdateInstancesList();
        }

        private void Forget_Click(object sender, EventArgs e)
        {
            foreach (var instance in KSPInstancesListView.SelectedItems.OfType<ListViewItem>().Select(item => item.Tag as string))
            {
                _manager.RemoveInstance(instance);
                UpdateInstancesList();
            }
        }

        private void UpdateButtonState()
        {
            ForgetButton.Enabled = RenameButton.Enabled = SelectButton.Enabled = SetAsDefaultCheckbox.Enabled = _manager.Instances.Count > 0;
        }
    }
}
