using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Versioning;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class ManageGameInstancesDialog : Form
    {
        private static GameInstanceManager manager => Main.Instance.Manager;
        private readonly IUser _user;
        private RenameInstanceDialog _renameInstanceDialog;
        private readonly OpenFileDialog _instanceDialog = new OpenFileDialog()
        {
            AddExtension     = false,
            CheckFileExists  = false,
            CheckPathExists  = false,
            InitialDirectory = Environment.CurrentDirectory,
            Filter           = GameFolderFilter(manager),
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
        => Properties.Resources.GameProgramFileDescription
            + "|" + string.Join(";", mgr.AllInstanceAnchorFiles);

        public bool HasSelections => GameInstancesListView.SelectedItems.Count > 0;

        /// <summary>
        /// Initialize the game instance selection window
        /// </summary>
        /// <param name="centerScreen">true to center the window on the screen, false to center it on the parent</param>
        public ManageGameInstancesDialog(bool centerScreen, IUser user)
        {
            _user = user;
            InitializeComponent();
            DialogResult = DialogResult.Cancel;

            if (centerScreen)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }

            if (!manager.Instances.Any())
            {
                manager.FindAndRegisterDefaultInstances();
            }

            // Set the renderer for the AddNewMenu
            if (Platform.IsMono)
            {
                AddNewMenu.Renderer = new FlatToolStripRenderer();
                InstanceListContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }

            UpdateInstancesList();
            UpdateButtonState();
        }

        public void UpdateInstancesList()
        {
            GameInstancesListView.Items.Clear();
            UpdateButtonState();

            var allSameGame = manager.Instances.Select(i => i.Value.game).Distinct().Count() <= 1;
            var hasPlayTime = manager.Instances.Any(instance => (instance.Value.playTime?.Time ?? TimeSpan.Zero) > TimeSpan.Zero);

            AddOrRemoveColumn(GameInstancesListView, Game, !allSameGame, GameInstallVersion.Index);
            AddOrRemoveColumn(GameInstancesListView, GamePlayTime, hasPlayTime, GameInstallPath.Index);

            GameInstancesListView.Items.AddRange(
                manager.Instances.OrderByDescending(instance => instance.Value.game.FirstReleaseDate)
                                  .ThenByDescending(instance => instance.Value.Version())
                                  .ThenBy(instance => instance.Key)
                                  .Select(instance => new ListViewItem(
                                      rowItems(instance.Value, !allSameGame, hasPlayTime))
                                  {
                                      Tag = instance.Key
                                  })
                                  .ToArray());

            GameInstancesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            GameInstancesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddOrRemoveColumn(ListView listView, ColumnHeader column, bool condition, int index)
        {
            if (condition && !listView.Columns.Contains(column))
            {
                listView.Columns.Insert(index, column);
            }
            else if (!condition && listView.Columns.Contains(column))
            {
                listView.Columns.Remove(column);
            }
        }

        private string[] rowItems(GameInstance instance, bool includeGame, bool includePlayTime)
        {
            var list = new List<string>
            {
                !instance.Valid
                    ? string.Format(Properties.Resources.ManageGameInstancesNameColumnInvalid, instance.Name)
                    : !(manager.CurrentInstance?.Equals(instance) ?? false) && instance.IsMaybeLocked
                        ? string.Format(Properties.Resources.ManageGameInstancesNameColumnLocked, instance.Name)
                        : instance.Name
            };

            if (includeGame)
            {
                list.Add(instance.game.ShortName);
            }

            list.Add(FormatVersion(instance.Version()));

            if (includePlayTime)
            {
                list.Add(instance.playTime?.ToString() ?? "");
            }

            list.Add(Platform.FormatPath(instance.GameDir()));
            return list.ToArray();
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.ManageInstances);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.ManageInstances);
        }

        private static string FormatVersion(GameVersion v)
        {
            return v == null
                ? Properties.Resources.CompatibleGameVersionsDialogNone
                // The BUILD component is not useful visually
                : new GameVersion(v.Major, v.Minor, v.Patch).ToString();
        }

        private void AddToCKANMenuItem_Click(object sender, EventArgs e)
        {
            if (_instanceDialog.ShowDialog(this) != DialogResult.OK
                    || !File.Exists(_instanceDialog.FileName))
            {
                return;
            }

            var path = Path.GetDirectoryName(_instanceDialog.FileName);
            try
            {
                var instanceName = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(instanceName))
                {
                    instanceName = path;
                }
                instanceName = manager.GetNextValidInstanceName(instanceName);
                manager.AddInstance(path, instanceName, _user);
                UpdateInstancesList();
            }
            catch (NotKSPDirKraken k)
            {
                _user.RaiseError(Properties.Resources.ManageGameInstancesNotValid,
                    new object[] { k.path });
            }
            catch (Exception exc)
            {
                _user.RaiseError(exc.Message);
            }
        }

        private void ImportFromSteamMenuItem_Click(object sender, EventArgs e)
        {
            var currentDirs = manager.Instances.Values
                                                .Select(inst => inst.GameDir())
                                                .ToHashSet(Platform.PathComparer);
            var toAdd = manager.FindDefaultInstances()
                               .Where(inst => !currentDirs.Contains(inst.GameDir()));
            foreach (var inst in toAdd)
            {
                manager.AddInstance(inst);
            }
            UpdateInstancesList();
        }

        private void CloneGameInstanceMenuItem_Click(object sender, EventArgs e)
        {
            var old_instance = manager.CurrentInstance;

            var result = new CloneGameInstanceDialog(manager, _user, (string)GameInstancesListView.SelectedItems[0].Tag).ShowDialog(this);
            if (result == DialogResult.OK && !Equals(old_instance, manager.CurrentInstance))
            {
                DialogResult = DialogResult.OK;
                Close();
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
            if (selected?.Tag is string instName)
            {
                try
                {
                    manager.SetCurrentInstance(instName);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (NotKSPDirKraken k)
                {
                    _user.RaiseError(Properties.Resources.ManageGameInstancesNotValid, k.path);
                }
            }
        }

        private void SetAsDefaultCheckbox_Click(object sender, EventArgs e)
        {
            if (SetAsDefaultCheckbox.Checked)
            {
                manager.ClearAutoStart();
                SetAsDefaultCheckbox.Checked = false;
                return;
            }

            var selected = GameInstancesListView.SelectedItems[0];
            if (selected?.Tag is string instName)
            {
                try
                {
                    manager.SetAutoStart(instName);
                    SetAsDefaultCheckbox.Checked = true;
                }
                catch (NotKSPDirKraken k)
                {
                    _user.RaiseError(Properties.Resources.ManageGameInstancesNotValid, k.path);
                }
            }
        }

        private void GameInstancesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();

            if (GameInstancesListView.SelectedItems.Count == 0)
            {
                return;
            }

            string instName = (string)GameInstancesListView.SelectedItems[0].Tag;
            SetAsDefaultCheckbox.Checked = manager.AutoStartInstance?.Equals(instName) ?? false;
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
                InstanceListContextMenuStrip.Show(this, new Point(e.X, e.Y));
            }
        }

        private void GameInstancesListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Apps:
                    InstanceListContextMenuStrip.Show(Cursor.Position);
                    e.Handled = true;
                    break;
            }
        }

        private void OpenDirectoryMenuItem_Click(object sender, EventArgs e)
        {
            string path = manager.Instances[(string) GameInstancesListView.SelectedItems[0].Tag].GameDir();

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
            {
                return;
            }

            // proceed with instance rename
            manager.RenameInstance(instance, _renameInstanceDialog.GetResult());
            UpdateInstancesList();
        }

        private void Forget_Click(object sender, EventArgs e)
        {
            foreach (var instance in GameInstancesListView.SelectedItems.OfType<ListViewItem>().Select(item => item.Tag as string))
            {
                manager.RemoveInstance(instance);
                UpdateInstancesList();
            }
        }

        private void UpdateButtonState()
        {
            RenameButton.Enabled = SelectButton.Enabled = SetAsDefaultCheckbox.Enabled = CloneGameInstanceMenuItem.Enabled = HasSelections;
            ForgetButton.Enabled = HasSelections && (string)GameInstancesListView.SelectedItems[0].Tag != manager.CurrentInstance?.Name;
            ImportFromSteamMenuItem.Enabled = manager.SteamLibrary.Games.Length > 0;
        }
    }
}
