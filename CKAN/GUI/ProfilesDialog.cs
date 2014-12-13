using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ProfilesDialog : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProfilesDialog));

        public ProfilesDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void ProfilesDialog_Load(object sender, EventArgs e)
        {
            repopulateProfileComboBoxes();
        }

        private void repopulateProfileComboBoxes()
        {
            string activeProfile = Main.Instance.m_Configuration.ActiveProfileName;
            List<string> profileNames = Main.Instance.m_Configuration.Profiles.Select(entry => entry.Name).ToList();
            repopulateProfileComboBoxes(profileNames, activeProfile);
        }

        private void repopulateProfileComboBoxes(List<string> profileNames, string activeProfile)
        {
            if (profileNames.Count <= 0)
            {
                log.Warn("There are no profiles.");
                return;
            }

            CKANActiveProfileComboBox.Items.Clear();
            CKANCopyFromProfileComboBox.Items.Clear();

            foreach (string profileName in profileNames)
            {
                CKANActiveProfileComboBox.Items.Add(profileName);
                CKANCopyFromProfileComboBox.Items.Add(profileName);

                if (profileName == activeProfile)
                {
                    CKANActiveProfileComboBox.SelectedIndex = CKANActiveProfileComboBox.Items.Count - 1;
                }
            }

            if (CKANActiveProfileComboBox.SelectedIndex < 0)
            {
                CKANActiveProfileComboBox.SelectedIndex = 0;
            }

            if (CKANCopyFromProfileComboBox.SelectedIndex < 0)
            {
                CKANCopyFromProfileComboBox.SelectedIndex = 0;
            }
        }

        private void CKANNewProfileCreateButton_Click(object sender, EventArgs e)
        {
            string newProfileName = CKANNewProfileTextBox.Text;
            string copyFrom = CKANCopyFromProfileComboBox.SelectedItem as string;
            List<string> profileNames = Main.Instance.m_Configuration.Profiles.Select(entry => entry.Name).ToList();

            if (newProfileName.Length <= 0)
            {
                Main.Instance.ErrorDialog("Please enter a new profile name.");
                return;
            }

            if (profileNames.Contains(newProfileName))
            {
                Main.Instance.ErrorDialog(String.Format("Profile with name '{0}' already exists!", newProfileName));
                return;
            }

            ProfilesEntry newEntry;
            if (profileNames.Contains(copyFrom))
            {
                ProfilesEntry existingEntry = Main.Instance.m_Configuration.Profiles.First(entry => entry.Name == copyFrom);
                newEntry = ProfilesEntry.Create(newProfileName, existingEntry);
            }
            else
            {
                newEntry = ProfilesEntry.Create(newProfileName);
            }

            Main.Instance.m_Configuration.Profiles.Add(newEntry);
            Main.Instance.m_Configuration.ActiveProfileName = newProfileName;
            Main.Instance.m_Configuration.Save();
            repopulateProfileComboBoxes();
        }

        private void CKANActiveProfileApplyButton_Click(object sender, EventArgs e)
        {
            string newActiveProfileName = CKANActiveProfileComboBox.SelectedItem as string;
            Main.Instance.m_Configuration.ActiveProfileName = newActiveProfileName;
            Main.Instance.m_Configuration.Save();
            Main.Instance.LoadActiveProfile();
        }
    }
}
