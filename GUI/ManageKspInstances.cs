using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CKAN
{
    public partial class ManageKspInstances : Form
    {
        private readonly KSPManager _manager;
        private RenameInstanceDialog _renameInstanceDialog;
        private readonly OpenFileDialog _instanceDialog = new OpenFileDialog()
        {
            AddExtension = false,
            CheckFileExists = false,
            CheckPathExists = false,
            InitialDirectory = Environment.CurrentDirectory,
            Filter = Properties.Resources.CloneFakeKspDialogOpenFilter,
            Multiselect = false
        };

        public bool HasSelections => KSPInstancesListView.SelectedItems.Count > 0;

        /// <summary>
        /// Initialize the game instance selection window
        /// </summary>
        /// <param name="centerScreen">true to center the window on the screen, false to center it on the parent</param>
        public ManageKspInstances(bool centerScreen)
        {
            _manager = Main.Instance.Manager;
            InitializeComponent();

            if (centerScreen)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }

            if (!_manager.Instances.Any())
            {
                _manager.FindAndRegisterDefaultInstance();
            }

            // Set the renderer for the AddNewMenu
            if (Platform.IsMono)
            {
                this.AddNewMenu.Renderer = new FlatToolStripRenderer();
                this.InstanceListContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }

            UpdateInstancesList();
            UpdateButtonState();
        }

        public void UpdateInstancesList()
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
                            Text = instance.Value.Version()?.ToString() ?? Properties.Resources.CompatibleKspVersionsDialogNone
                        },
                        new ListViewItem.ListViewSubItem {
                            Text = instance.Value.GameDir().Replace('/', Path.DirectorySeparatorChar)
                        }
                    }, 0)
                    {
                        Tag = instance.Key
                    });
            }
        }

        private void AddToCKANMenuItem_Click(object sender, EventArgs e)
        {
            if (_instanceDialog.ShowDialog() != DialogResult.OK
                    || !File.Exists(_instanceDialog.FileName))
                return;

            var path = Path.GetDirectoryName(_instanceDialog.FileName);
            try
            {
                var instanceName = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(instanceName))
                {
                    instanceName = path;
                }
                instanceName = _manager.GetNextValidInstanceName(instanceName);
                KSP instance = new KSP(path, instanceName, GUI.user);
                _manager.AddInstance(instance);
                UpdateInstancesList();
            }
            catch (NotKSPDirKraken k)
            {
                GUI.user.RaiseError(Properties.Resources.ManageKspInstancesNotValid,
                    new object[] { k.path });
                return;
            }
        }

        private void CloneFakeInstanceMenuItem_Click(object sender, EventArgs e)
        {
            var old_instance = Main.Instance.CurrentInstance;

            var result = new CloneFakeKspDialog(_manager).ShowDialog();
            if (result == DialogResult.OK && !Equals(old_instance, Main.Instance.CurrentInstance))
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }

            UpdateInstancesList();
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
                _manager.SetCurrentInstance(instName);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (NotKSPDirKraken k)
            {
                GUI.user.RaiseError(Properties.Resources.ManageKspInstancesNotValid, k.path);
            }
        }

        private void SetAsDefaultCheckbox_Click(object sender, EventArgs e)
        {
            if (SetAsDefaultCheckbox.Checked)
            {
                _manager.ClearAutoStart();
                SetAsDefaultCheckbox.Checked = false;
                return;
            }

            var selected = KSPInstancesListView.SelectedItems[0];
            string instName = selected?.Tag as string;
            if (instName == null)
            {
                return;
            }

            try
            {
                _manager.SetAutoStart(instName);
                SetAsDefaultCheckbox.Checked = true;
            }
            catch (NotKSPDirKraken k)
            {
                GUI.user.RaiseError(Properties.Resources.ManageKspInstancesNotValid, k.path);
            }
        }

        private void KSPInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();

            if (KSPInstancesListView.SelectedItems.Count == 0)
                return;

            string instName = (string)KSPInstancesListView.SelectedItems[0].Tag;
            SetAsDefaultCheckbox.Checked = _manager.AutoStartInstance?.Equals(instName) ?? false;
        }

        private void KSPInstancesListView_DoubleClick(object sender, EventArgs r)
        {
            if (HasSelections)
            {
                UseSelectedInstance();
            }
        }

        private void KSPInstancesListView_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                InstanceListContextMenuStrip.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
        }

        private void OpenDirectoryMenuItem_Click(object sender, EventArgs e)
        {
            string path = KSPInstancesListView.SelectedItems[0].SubItems[2].Text;

            if (!Directory.Exists(path))
            {
                GUI.user.RaiseError(Properties.Resources.ManageKspInstancesDirectoryDeleted, path);
                return;
            }

            System.Diagnostics.Process.Start(path);
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
            RenameButton.Enabled = SelectButton.Enabled = SetAsDefaultCheckbox.Enabled = HasSelections;
            ForgetButton.Enabled = HasSelections && (string)KSPInstancesListView.SelectedItems[0].Tag != _manager.CurrentInstance?.Name;
        }
    }
}
