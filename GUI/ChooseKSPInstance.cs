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
            Filter = "Build metadata file (buildID*.txt)|buildID*.txt",
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
                KSPInstancesListView.Items.Add(
                    new ListViewItem(new ListViewItem.ListViewSubItem[]
                    {
                        new ListViewItem.ListViewSubItem {
                            Text = instance.Key
                        },
                        new ListViewItem.ListViewSubItem {
                            Text = instance.Value.Version()?.ToString() ?? "<NONE>"
                        },
                        new ListViewItem.ListViewSubItem {
                            Text = instance.Value.GameDir()
                        }
                    }, 0)
                    {
                        Tag = instance.Key
                    });
            }
        }

        private void AddNewButton_Click(object sender, EventArgs e)
        {
            if (_instanceDialog.ShowDialog() != DialogResult.OK
                    || !File.Exists(_instanceDialog.FileName))
                return;

            var path = Path.GetDirectoryName(_instanceDialog.FileName);
            try
            {
                var instanceName = Path.GetFileName(path);
                instanceName = _manager.GetNextValidInstanceName(instanceName);
                KSP instance = new KSP(path, instanceName, GUI.user);
                _manager.AddInstance(instance);
                UpdateInstancesList();
            }
            catch (NotKSPDirKraken k)
            {
                GUI.user.displayError("Directory {0} is not valid KSP directory.",
                    new object[] { k.path });
                return;
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            UseSelectedInstance();
        }

        private void UseSelectedInstance()
        {
            if (KSPInstancesListView.SelectedItems.Count == 0)
            {
                return;
            }

            var selected = KSPInstancesListView.SelectedItems[0];
            var instName = selected?.Tag as string;
            if (instName == null)
            {
                return;
            }

            try
            {
                if (SetAsDefaultCheckbox.Checked)
                {
                    _manager.SetAutoStart(instName);
                }

                _manager.SetCurrentInstance(instName);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (NotKSPDirKraken k)
            {
                GUI.user.displayError("Directory {0} is not valid KSP directory.",
                    new object[] { k.path });
            }
        }

        private void KSPInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void KSPInstancesListView_DoubleClick(object sender, EventArgs r)
        {
            if (HasSelections)
            {
                UseSelectedInstance();
            }
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            var instance = (string)KSPInstancesListView.SelectedItems[0].Tag;

            // show the dialog, and only continue if the user selected "OK"
            _renameInstanceDialog = new RenameInstanceDialog();
            if (_renameInstanceDialog.ShowRenameInstanceDialog(instance) != DialogResult.OK)
                return;

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
            ForgetButton.Enabled = RenameButton.Enabled = SelectButton.Enabled = SetAsDefaultCheckbox.Enabled = HasSelections;
        }
    }
}
