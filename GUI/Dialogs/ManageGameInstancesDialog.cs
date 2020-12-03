using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CKAN
{
    public partial class ManageGameInstancesDialog : Form
    {
        private readonly GameInstanceManager _manager = Main.Instance.Manager;
        private readonly IUser _user;
        private RenameInstanceDialog _renameInstanceDialog;
        private readonly OpenFileDialog _instanceDialog = new OpenFileDialog()
        {
            AddExtension     = false,
            CheckFileExists  = false,
            CheckPathExists  = false,
            InitialDirectory = Environment.CurrentDirectory,
            Filter           = GameFolderFilter(Main.Instance.Manager),
            Multiselect      = false
        };
        
        /// <summary>
        /// Generate filter string for OpenFileDialog
        /// </summary>
        /// <param name="mgr">Game instance manager that can tell us about the build ID files</param>
        /// <returns>
        /// "Build metadata files (buildID.txt;buildID64.txt)|buildID.txt;buildID64.txt"
        /// </returns>
        public static string GameFolderFilter(GameInstanceManager mgr)
        {
            return Properties.Resources.BuildIDFilterDescription
                + "|" + string.Join(";", mgr.AllBuildIDFiles);
        }

        public bool HasSelections => GameInstancesListView.SelectedItems.Count > 0;

        /// <summary>
        /// Initialize the game instance selection window
        /// </summary>
        /// <param name="centerScreen">true to center the window on the screen, false to center it on the parent</param>
        public ManageGameInstancesDialog(bool centerScreen, IUser user)
        {
            _user = user;
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
            GameInstancesListView.Items.Clear();
            UpdateButtonState();

            GameInstancesListView.Items.AddRange(_manager.Instances
                .OrderByDescending(instance => instance.Value.Version())
                .Select(instance => new ListViewItem(new string[]
                {
                    instance.Key,
                    instance.Value.game.ShortName,
                    instance.Value.Version()?.ToString() ?? Properties.Resources.CompatibleGameVersionsDialogNone,
                    instance.Value.GameDir().Replace('/', Path.DirectorySeparatorChar)
                })
                {
                    Tag = instance.Key
                })
                .ToArray()
            );
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
                _manager.AddInstance(path, instanceName, _user);
                UpdateInstancesList();
            }
            catch (NotKSPDirKraken k)
            {
                _user.RaiseError(Properties.Resources.ManageGameInstancesNotValid,
                    new object[] { k.path });
                return;
            }
        }

        private void CloneFakeInstanceMenuItem_Click(object sender, EventArgs e)
        {
            var old_instance = Main.Instance.CurrentInstance;

            var result = new CloneFakeGameDialog(_manager, _user).ShowDialog();
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
            if (GameInstancesListView.SelectedItems.Count == 0)
            {
                return;
            }

            var selected = GameInstancesListView.SelectedItems[0];
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
                _user.RaiseError(Properties.Resources.ManageGameInstancesNotValid, k.path);
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

            var selected = GameInstancesListView.SelectedItems[0];
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
                _user.RaiseError(Properties.Resources.ManageGameInstancesNotValid, k.path);
            }
        }

        private void GameInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();

            if (GameInstancesListView.SelectedItems.Count == 0)
                return;

            string instName = (string)GameInstancesListView.SelectedItems[0].Tag;
            SetAsDefaultCheckbox.Checked = _manager.AutoStartInstance?.Equals(instName) ?? false;
        }

        private void GameInstancesListView_DoubleClick(object sender, EventArgs r)
        {
            if (HasSelections)
            {
                UseSelectedInstance();
            }
        }

        private void GameInstancesListView_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                InstanceListContextMenuStrip.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
        }

        private void OpenDirectoryMenuItem_Click(object sender, EventArgs e)
        {
            string path = GameInstancesListView.SelectedItems[0].SubItems[2].Text;

            if (!Directory.Exists(path))
            {
                _user.RaiseError(Properties.Resources.ManageGameInstancesDirectoryDeleted, path);
                return;
            }

            Utilities.ProcessStartURL(path);
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            var instance = (string)GameInstancesListView.SelectedItems[0].Tag;

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
            foreach (var instance in GameInstancesListView.SelectedItems.OfType<ListViewItem>().Select(item => item.Tag as string))
            {
                _manager.RemoveInstance(instance);
                UpdateInstancesList();
            }
        }

        private void UpdateButtonState()
        {
            RenameButton.Enabled = SelectButton.Enabled = SetAsDefaultCheckbox.Enabled = HasSelections;
            ForgetButton.Enabled = HasSelections && (string)GameInstancesListView.SelectedItems[0].Tag != _manager.CurrentInstance?.Name;
        }
    }
}
