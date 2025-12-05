using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Games;

namespace CKAN.GUI
{
    /// <summary>
    /// The GUI implementation of clone.
    /// It's a separate window, handling the whole process.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class CloneGameInstanceDialog : Forms.Form
    {
        private readonly GameInstanceManager manager;
        private readonly IUser               user;

        public CloneGameInstanceDialog(GameInstanceManager manager,
                                       IUser               user,
                                       string?             selectedInstanceName = null)
            : base()
        {
            this.manager = manager;
            this.user    = user;

            InitializeComponent();

            ToolTip.SetToolTip(checkBoxShareStock, Properties.Resources.CloneGameInstanceToolTipShareStock);

            // Populate the instances combobox with names of known instances
            comboBoxKnownInstance.DataSource =
                Enumerable.Repeat("", 1)
                          .Concat(manager.Instances.Values
                                                   .Where(i => i.Valid)
                                                   .OrderByDescending(i => i.Game.FirstReleaseDate)
                                                   .ThenByDescending(i => i.Version())
                                                   .ThenBy(i => i.Name)
                                                   .Select(i => i.Name))
                          .ToList();
            comboBoxKnownInstance.Text = selectedInstanceName
                                             ?? manager.CurrentInstance?.Name
                                             ?? manager.Configuration.AutoStartInstance
                                             ?? "";

            if (Platform.IsWindows)
            {
                checkBoxShareStock.Checked = true;
            }
            else
            {
                // Symbolic links break cloned instances on Linux
                // (Games use paths like <GameRoot>/KSP_Data/../saves/,
                //  which follow the symlinks to the original instance)
                checkBoxShareStock.Visible = false;
            }
        }

        #region clone

        private void comboBoxKnownInstance_SelectedIndexChanged(object? sender, EventArgs? e)
        {
            if (comboBoxKnownInstance.SelectedItem is string sel
                && !string.IsNullOrEmpty(sel))
            {
                var inst = manager.Instances[sel];
                textBoxClonePath.Text = Platform.FormatPath(inst.GameDir);
                OptionalPathsListView.Items.Clear();
                OptionalPathsListView.Items.AddRange(
                    inst.Game.LeaveEmptyInClones
                             .OrderBy(path => path,
                                      StringComparer.OrdinalIgnoreCase)
                             .Select(path => new ListViewItem(path) { Tag = path })
                             .ToArray());
                OptionalPathsLabel.Visible = OptionalPathsListView.Visible =
                    OptionalPathsListView.Items.Count > 0;
            }
        }

        /// <summary>
        /// Open an file dialog to search for a game instance, like in <code>ManageGameInstancesDialog</code>.
        /// </summary>
        private void buttonInstancePathSelection_Click(object? sender, EventArgs? e)
        {
            // Create a new FileDialog object
            OpenFileDialog instanceDialog = new OpenFileDialog()
            {
                AddExtension     = false,
                CheckFileExists  = false,
                CheckPathExists  = false,
                InitialDirectory = Environment.CurrentDirectory,
                Filter           = ManageGameInstancesDialog.GameFolderFilter(),
                Multiselect      = false
            };

            // Show the FileDialog and let the user search for the game directory.
            if (instanceDialog.ShowDialog(this) != DialogResult.OK || !File.Exists(instanceDialog.FileName))
            {
                return;
            }

            // Write the path to the textbox
            textBoxClonePath.Text = Path.GetDirectoryName(instanceDialog.FileName);
        }

        #endregion

        /// <summary>
        /// User is done. Start cloning or faking, depending on the clicked radio button.
        /// Close the window if everything went right.
        /// </summary>
        private async void buttonOK_Click(object? sender, EventArgs? e)
        {
            string existingPath = textBoxClonePath.Text;
            string newName = textBoxNewName.Text;
            string newPath = textBoxNewPath.Text;

            // Do some basic checks.
            if (string.IsNullOrWhiteSpace(newName))
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogEnterName);
                return;
            }
            if (string.IsNullOrWhiteSpace(newPath))
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogEnterPath);
                return;
            }

            // Show progress bar and deactivate controls.
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Show();
            foreach (Control ctrl in Controls)
            {
                ctrl.Enabled = false;
            }

            user.RaiseMessage(Properties.Resources.CloneFakeKspDialogCloningInstance);

            try
            {
                var instanceToClone = comboBoxKnownInstance.SelectedItem is string s
                                      && manager.Instances.TryGetValue(s, out GameInstance? instFromBox)
                                      && existingPath == Platform.FormatPath(instFromBox.GameDir)
                                          ? instFromBox
                                          : manager.DetermineGame(new DirectoryInfo(existingPath), user) is IGame sourceGame
                                              ? new GameInstance(sourceGame, existingPath,
                                                                 "irrelevant", user)
                                              : null;
                if (instanceToClone == null)
                {
                    // User cancelled, let them try again
                    reactivateDialog();
                    return;
                }
                await Task.Run(() =>
                {
                    if (instanceToClone.Valid)
                    {
                        manager.CloneInstance(instanceToClone, newName, newPath,
                                              OptionalPathsListView.Items
                                                                   .OfType<ListViewItem>()
                                                                   .Where(lvi => !lvi.Checked)
                                                                   .Select(lvi => lvi.Tag)
                                                                   .OfType<string>()
                                                                   .ToArray(),
                                              checkBoxShareStock.Checked);
                    }
                    else
                    {
                        throw new NotGameDirKraken(instanceToClone.GameDir);
                    }
                });

                if (checkBoxSetAsDefault.Checked)
                {
                    manager.SetAutoStart(newName);
                }

                if (checkBoxSwitchInstance.Checked)
                {
                    manager.SetCurrentInstance(newName);
                }

                user.RaiseMessage(Properties.Resources.CloneFakeKspDialogSuccessfulClone);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InstanceNameTakenKraken)
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogNameAlreadyUsed);
                reactivateDialog();
            }
            catch (NotGameDirKraken kraken)
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogInstanceNotValid,
                                Platform.FormatPath(kraken.path));
                reactivateDialog();
            }
            catch (PathErrorKraken kraken)
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogDestinationNotEmpty,
                                Platform.FormatPath(kraken?.path ?? ""));
                reactivateDialog();
            }
            catch (IOException ex)
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogCloneFailed, ex.Message);
                reactivateDialog();
            }
            catch (Exception ex)
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogCloneFailed, ex.Message);
                reactivateDialog();
            }
        }

        private void buttonCancel_Click(object? sender, EventArgs? e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Activate all controls and hide progress bar.
        /// </summary>
        private void reactivateDialog()
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.Enabled = true;
            }
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            progressBar.Hide();
        }

        private void buttonPathBrowser_Click(object? sender, EventArgs? e)
        {
            if (folderBrowserDialogNewPath.ShowDialog(this).Equals(DialogResult.OK))
            {
                textBoxNewPath.Text = folderBrowserDialogNewPath.SelectedPath;
            }
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.CloneFakeInstances);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.CloneFakeInstances);
        }

    }
}
