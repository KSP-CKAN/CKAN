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
    public partial class CloneKspDialog : Form
    {
        private GUIUser user = new GUIUser();
        private KSPManager manager;

        public CloneKspDialog(KSPManager manager)
        {
            this.manager = manager;

            InitializeComponent();

            // Populate the combobox for fake instance.
            List<Versioning.KspVersion> knownVersions = new GameVersionProviders.KspBuildMap(new Win32Registry()).KnownVersions;
            knownVersions.Reverse();
            comboBoxKspVersion.DataSource = knownVersions;
        }

        #region clone

        /// <summary>
        /// The KSP object for the instance the user selected to clone.
        /// Fills the path textbox automatically.
        /// </summary>
        private KSP selectedInstance
        {
            set
            {
                _selectedInstance = value;
                textBoxClonePath.Text = selectedInstance.GameDir();
            }
            get { return _selectedInstance; }
        }
        private KSP _selectedInstance;

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
                instancesAsStrings.Add(String.Format("{0} ({1}) at {2}", instance.Name, instance.Version().ToString(), instance.GameDir()));
            }
            
            // Raise the selection dialog.
            int selection = user.RaiseSelectionDialog("Choose an existing instance:", instancesAsStrings.ToArray());

            // Now set selectedInstance to the picked one.
            selectedInstance = knownInstances[selection];            
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
            
			var path = Path.GetDirectoryName(instanceDialog.FileName);

            // Create a new KSP object and set selectedInstance
			var instanceName = Path.GetFileName(path);
		    instanceName = manager.GetNextValidInstanceName(instanceName);
			selectedInstance = new KSP(path, instanceName, user);
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
        /// Close the window afterwards.
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
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

            // Clone the specified instance.
            // Done in a new task to not block the GUI thread.
            if (radioButtonClone.Checked)
            {
                try
                {
                    Task.Run(() => {
                        user.RaiseProgress("Cloning instance...", 0);
                        manager.CloneInstance(selectedInstance, newName, newPath);

                        if (checkBoxSetAsDefault.Checked)
                        {
                            manager.SetAutoStart(newName);
                        }

                        user.RaiseProgress("Successfully cloned instance.", 100);
                    });
                }
                catch (NotKSPDirKraken)
                {
                    user.RaiseError("The instance you wanted to clone is not valid.");
                    return;
                }
                catch (IOException exception)
                {
                    user.RaiseError("The destination folder is not empty or invalid: " + exception.Message);
                    return;
                }
                           
            }
            // Create a new dummy instance.
            // Also in a separate task.
            else if (radioButtonFake.Checked)
            {
                Versioning.KspVersion kspVersion = Versioning.KspVersion.Parse(comboBoxKspVersion.Text);
                string dlcVersion = textBoxDlcVersion.Text;

                try
                {
                    Task.Run(() => {
                        user.RaiseProgress("Creating new instance...", 0);
                        manager.FakeInstance(newName, newPath, kspVersion, dlcVersion);

                        if (checkBoxSetAsDefault.Checked)
                        {
                            manager.SetAutoStart(newName);
                        }

                        user.RaiseProgress("Successfully created instance.", 100);
                    });
                }
                catch (BadInstallLocationKraken)
                {
                    user.RaiseError("The destination folder is not empty or invalid.");
                    return;
                }
                catch (ArgumentException)
                {
                    user.RaiseError("This name is already used.");
                    return;
                }
            }

            if (checkBoxSwitchInstance.Checked)
            {
                manager.SetCurrentInstance(newName);
            }

            DialogResult = DialogResult.OK;
            this.Close();
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
