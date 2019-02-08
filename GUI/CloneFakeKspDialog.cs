using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    /// <summary>
    /// The GUI implementation of clone and fake.
    /// It's a seperate window, handling the whole process.
    /// </summary>
    public partial class CloneFakeKspDialog : Form
    {
        private GUIUser user = new GUIUser();
        private KSPManager manager;

        public CloneFakeKspDialog(KSPManager manager)
            : base()
        {
            this.manager = manager;

            InitializeComponent();

            // Populate the version combobox for fake instance.
            List<Versioning.KspVersion> knownVersions = new GameVersionProviders.KspBuildMap(new Win32Registry()).KnownVersions;
            knownVersions.Reverse();
            comboBoxKspVersion.DataSource = knownVersions;

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
                AddExtension = false,
                CheckFileExists = false,
                CheckPathExists = false,
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "Build metadata file (buildID*.txt)|buildID*.txt",
                Multiselect = false
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
            // Do some basic checks.
            if (textBoxNewName.TextLength == 0)
            {
                user.RaiseError("Please enter a name for the new instance.");
                return;
            }
            if (textBoxNewPath.TextLength == 0)
            {
                user.RaiseError("Please enter a path for the new instance.");
                return;
            }

            string newName = textBoxNewName.Text;
            string newPath = textBoxNewPath.Text;

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
                user.RaiseMessage("Cloning instance...");

                try
                {
                    await Task.Run(() =>
                    {
                        KSP instanceToClone = new KSP(textBoxClonePath.Text, "irrelevant", user);

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
                catch (NotKSPDirKraken kraken)
                {
                    user.RaiseError("The instance you wanted to clone is not valid: " + kraken.path);
                    reactivateDialog();
                    return;
                }
                catch (PathErrorKraken kraken)
                {
                    user.RaiseError("The destination folder is not empty: " + kraken.path);
                    reactivateDialog();
                    return;
                }
                catch (IOException ex)
                {
                    user.RaiseError($"Clone failed: {ex.Message}");
                    reactivateDialog();
                    return;
                }
                catch (Exception ex)
                {
                    user.RaiseError($"Clone failed: {ex.Message}");
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

                user.RaiseMessage("Successfully cloned instance.");

                DialogResult = DialogResult.OK;
                this.Close();
            }

            // Create a new dummy instance.
            // Also in a separate task.
            else if (radioButtonFake.Checked)
            {
                Versioning.KspVersion kspVersion = Versioning.KspVersion.Parse(comboBoxKspVersion.Text);
                string dlcVersion = textBoxDlcVersion.Text;

                user.RaiseMessage("Creating new instance...");

                try
                {
                    await Task.Run(() =>
                    {
                        manager.FakeInstance(newName, newPath, kspVersion, dlcVersion);
                    });
                }
                catch (BadInstallLocationKraken)
                {
                    user.RaiseError("The destination folder is not empty or invalid.");
                    reactivateDialog();
                    return;
                }
                catch (ArgumentException)
                {
                    user.RaiseError("This name is already used.");
                    reactivateDialog();
                    return;
                }
                catch (Exception ex)
                {
                    user.RaiseError($"Fake instance creation failed: {ex.Message}");
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

                user.RaiseMessage("Successfully created instance.");

                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private async void buttonCancel_Click(object sender, EventArgs e)
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
