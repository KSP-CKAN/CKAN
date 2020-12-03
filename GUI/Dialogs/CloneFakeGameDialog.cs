using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using CKAN.Versioning;
using CKAN.Games;

namespace CKAN
{
    /// <summary>
    /// The GUI implementation of clone and fake.
    /// It's a separate window, handling the whole process.
    /// </summary>
    public partial class CloneFakeGameDialog : Form
    {
        private GameInstanceManager manager;
        private IUser      user;

        public CloneFakeGameDialog(GameInstanceManager manager, IUser user)
            : base()
        {
            this.manager = manager;
            this.user    = user;

            InitializeComponent();

            // Populate the version combobox for fake instance.
            List<GameVersion> knownVersions = new KerbalSpaceProgram().KnownVersions;
            knownVersions.Reverse();
            comboBoxGameVersion.DataSource = knownVersions;

            // Populate the instances combobox with names of known instances
            comboBoxKnownInstance.DataSource = new string[] { "" }
                .Concat(manager.Instances.Values
                    .Where(i => i.Valid)
                    .OrderBy(i => i.Version())
                    .Reverse()
                    .Select(i => i.Name))
                .ToList();
            comboBoxKnownInstance.Text = manager.CurrentInstance?.Name
                ?? manager.AutoStartInstance
                ?? "";
            this.radioButtonClone.Checked = true;
        }

        #region clone

        private void comboBoxKnownInstance_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sel = comboBoxKnownInstance.SelectedItem as string;
            textBoxClonePath.Text = string.IsNullOrEmpty(sel)
                ? ""
                : manager.Instances[sel].GameDir();
        }

        /// <summary>
        /// Open an file dialog to search for a KSP instance, like in <code>ManageKspInstances</code>.
        /// </summary>
        private void buttonInstancePathSelection_Click(object sender, EventArgs e)
        {
            // Create a new FileDialog object
            OpenFileDialog instanceDialog = new OpenFileDialog()
            {
                AddExtension     = false,
                CheckFileExists  = false,
                CheckPathExists  = false,
                InitialDirectory = Environment.CurrentDirectory,
                Filter           = ManageGameInstancesDialog.GameFolderFilter(manager),
                Multiselect      = false
            };

            // Show the FileDialog and let the user search for the KSP directory.
		    if (instanceDialog.ShowDialog() != DialogResult.OK || !File.Exists(instanceDialog.FileName))
			    return;

            // Write the path to the textbox
            textBoxClonePath.Text = Path.GetDirectoryName(instanceDialog.FileName);
        }

        #endregion
        #region radio buttons

        /// <summary>
        /// The radio buttons are in different GroupBoxes, so they need to be unset manually.
        /// </summary>
        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton clickedRadioButton = (RadioButton)sender;

            if (clickedRadioButton.Checked)
            {
                if (clickedRadioButton == radioButtonClone)
                {
                    radioButtonFake.Checked = false;
                    cloneGroupBox.Enabled = !(fakeGroupBox.Enabled = false);
                }
                else
                {
                    radioButtonClone.Checked = false;
                    fakeGroupBox.Enabled = !(cloneGroupBox.Enabled = false);
                }
            }
        }

        #endregion

        /// <summary>
        /// User is done. Start cloning or faking, depending on the clicked radio button.
        /// Close the window if everything went right.
        /// </summary>
        private async void buttonOK_Click(object sender, EventArgs e)
        {
            string newName = textBoxNewName.Text;
            string newPath = textBoxNewPath.Text;

            // Do some basic checks.
            if (String.IsNullOrWhiteSpace(newName))
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogEnterName);
                return;
            }
            if (String.IsNullOrWhiteSpace(newPath))
            {
                user.RaiseError(Properties.Resources.CloneFakeKspDialogEnterPath);
                return;
            }

            // Show progress bar and deactivate controls.
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Show();
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Enabled = false;
            }

            // Clone the specified instance.
            // Done in a new task to not block the GUI thread.
            if (radioButtonClone.Checked)
            {
                user.RaiseMessage(Properties.Resources.CloneFakeKspDialogCloningInstance);

                try
                {
                    await Task.Run(() =>
                    {
                        GameInstance sourceInstance = manager.Instances.Values
                            .FirstOrDefault(i => i.GameDir() == textBoxClonePath.Text);
                        GameInstance instanceToClone = new GameInstance(
                            sourceInstance.game,
                            textBoxClonePath.Text,
                            "irrelevant",
                            user
                        );

                        if (instanceToClone.Valid)
                        {
                            manager.CloneInstance(instanceToClone, newName, newPath);
                        }
                        else
                        {
                            throw new NotKSPDirKraken(instanceToClone.GameDir());
                        }
                    });
                }
                catch (InstanceNameTakenKraken)
                {
                    user.RaiseError(Properties.Resources.CloneFakeKspDialogNameAlreadyUsed);
                    reactivateDialog();
                    return;
                }
                catch (NotKSPDirKraken kraken)
                {
                    user.RaiseError(string.Format(Properties.Resources.CloneFakeKspDialogInstanceNotValid, kraken.path));
                    reactivateDialog();
                    return;
                }
                catch (PathErrorKraken kraken)
                {
                    user.RaiseError(string.Format(Properties.Resources.CloneFakeKspDialogDestinationNotEmpty, kraken.path));
                    reactivateDialog();
                    return;
                }
                catch (IOException ex)
                {
                    user.RaiseError(string.Format(Properties.Resources.CloneFakeKspDialogCloneFailed, ex.Message));
                    reactivateDialog();
                    return;
                }
                catch (Exception ex)
                {
                    user.RaiseError(string.Format(Properties.Resources.CloneFakeKspDialogCloneFailed, ex.Message));
                    reactivateDialog();
                    return;
                }

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
                this.Close();
            }

            // Create a new dummy instance.
            // Also in a separate task.
            else if (radioButtonFake.Checked)
            {
                GameVersion GameVersion = GameVersion.Parse(comboBoxGameVersion.Text);

                Dictionary<DLC.IDlcDetector, GameVersion> dlcs = new Dictionary<DLC.IDlcDetector, GameVersion>();
                if (!String.IsNullOrWhiteSpace(textBoxMHDlcVersion.Text) && textBoxMHDlcVersion.Text.ToLower() != "none")
                {
                    if (GameVersion.TryParse(textBoxMHDlcVersion.Text, out GameVersion ver))
                    {
                        dlcs.Add(new DLC.MakingHistoryDlcDetector(), ver);
                    }
                    else
                    {
                        user.RaiseError(Properties.Resources.CloneFakeKspDialogDlcVersionMalformatted, "Making History");
                        reactivateDialog();
                        return;
                    }
                }
                if (!String.IsNullOrWhiteSpace(textBoxBGDlcVersion.Text) && textBoxBGDlcVersion.Text.ToLower() != "none")
                {
                    if (GameVersion.TryParse(textBoxBGDlcVersion.Text, out GameVersion ver))
                    {
                        dlcs.Add(new DLC.BreakingGroundDlcDetector(), ver);
                    }
                    else
                    {
                        user.RaiseError(Properties.Resources.CloneFakeKspDialogDlcVersionMalformatted, "Breaking Ground");
                        reactivateDialog();
                        return;
                    }
                }

                user.RaiseMessage(Properties.Resources.CloneFakeKspDialogCreatingInstance);

                try
                {
                    await Task.Run(() =>
                    {
                        manager.FakeInstance(new KerbalSpaceProgram(), newName, newPath, GameVersion, dlcs);
                    });
                }
                catch (InstanceNameTakenKraken)
                {
                    user.RaiseError(Properties.Resources.CloneFakeKspDialogNameAlreadyUsed);
                    reactivateDialog();
                    return;
                }
                catch (BadInstallLocationKraken)
                {
                    user.RaiseError(Properties.Resources.CloneFakeKspDialogDestinationNotEmpty, newPath);
                    reactivateDialog();
                    return;
                }
                catch (Exception ex)
                {
                    user.RaiseError(string.Format(Properties.Resources.CloneFakeKspDialogFakeFailed, ex.Message));
                    reactivateDialog();
                    return;
                }

                if (checkBoxSetAsDefault.Checked)
                {
                    manager.SetAutoStart(newName);
                }

                if (checkBoxSwitchInstance.Checked)
                {
                    manager.SetCurrentInstance(newName);
                }

                user.RaiseMessage(Properties.Resources.CloneFakeKspDialogSuccessfulCreate);

                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Activate all controls, shrink window and hide progress bar.
        /// </summary>
        private void reactivateDialog()
        {
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Enabled = true;
            }
            // Conditionally enable/disable the fake/clone fields
            radioButton_CheckedChanged(radioButtonClone, null);
            radioButton_CheckedChanged(radioButtonFake,  null);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            progressBar.Hide();
        }

        private void buttonPathBrowser_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogNewPath.ShowDialog().Equals(DialogResult.OK))
            {
                textBoxNewPath.Text = folderBrowserDialogNewPath.SelectedPath;
            }

        }
    }
}
