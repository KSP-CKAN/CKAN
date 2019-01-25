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
    public partial class CloneFakeKspDialog : FormCompatibility
    {
        private GUIUser user = new GUIUser();
        private KSPManager manager;

        public CloneFakeKspDialog(KSPManager manager)
        {
            this.manager = manager;

            InitializeComponent();

            // Populate the version combobox for fake instance.
            List<Versioning.KspVersion> knownVersions = new GameVersionProviders.KspBuildMap(new Win32Registry()).KnownVersions;
            knownVersions.Reverse();
            comboBoxKspVersion.DataSource = knownVersions;
        }

        new private void ApplyFormCompatibilityFixes ()
        {
            const int formWidthDifference  = 71;
            const int formHeightDifference = 22;

            if (!Platform.IsWindows)
            {
                ClientSize = new System.Drawing.Size(ClientSize.Width + formWidthDifference, ClientSize.Height + formHeightDifference);
            }
        }

        #region clone

        /// <summary>
        /// Click event for the OpenInstanceSelection button, which is used to raise a selection dialog
        /// to choose which known KSP instance the user wants to clone.
        /// </summary>
        private void buttonOpenInstanceSelection_Click(object sender, EventArgs e)
        {
            // Get all to the regisrty known instances.
            KSP[] knownInstances = manager.Instances.Values.ToArray();
            List<string> instancesAsStrings = new List<string>();
            
            // Now turn them into a list of nice, readable strings.
            foreach (KSP instance in knownInstances)
            {
                string instanceString = String.Format("{0} ({1}) at {2}", instance.Name, instance.Version()?.ToString() ?? "N/D", instance.GameDir() );
                instancesAsStrings.Add(instanceString);
            }
            
            // Raise the selection dialog.
            int selection = user.RaiseSelectionDialog("Choose an existing instance:", instancesAsStrings.ToArray());

            // Now set the textbox text to the path of the picked one.
            if (selection != -1)
            {
                textBoxClonePath.Text = knownInstances[selection].GameDir();
            }     
        }

        /// <summary>
        /// Open an file dialog to search for a KSP instance, like in <code>ChooseKSPInstance</code>.
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
                }
                else
                {
                    radioButtonClone.Checked = false;
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
            this.ClientSize = new System.Drawing.Size(424, 317);
            ApplyFormCompatibilityFixes();
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
                catch (IOException exception)
                {
                    user.RaiseError("The destination folder is not empty or invalid: " + exception.Message);
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

        /// <summary>
        /// Activate all controls, shrink window and hide progress bar.
        /// </summary>
        private void reactivateDialog()
        {
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Enabled = true;
            }
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            progressBar.Hide();
            this.ClientSize = new System.Drawing.Size(424, 287);
            ApplyFormCompatibilityFixes();
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
