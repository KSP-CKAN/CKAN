using CKAN.GameVersionProviders;
using System.Windows.Forms;
using System.Collections.Generic;
using Autofac;
using CKAN.Versioning;
using System;

namespace CKAN
{
    public partial class KspInstanceSettingsDialog : Form
    {
        private KSP ksp;

        public KspInstanceSettingsDialog(KSP ksp)
        {
            this.ksp = ksp;
            InitializeComponent();

            gameVersionLabel.Text = ksp.Version().ToString();
            gameLocationLabel.Text = ksp.GameDir();
            var knownVersions = ServiceLocator.Container.Resolve<IKspBuildMap>().getKnownVersions();
            knownVersions.Reverse();
            foreach (var version in knownVersions)
            {
                if (!version.Equals(ksp.Version()))
                {
                    selectedVersionsCheckedListBox.Items.Add(version, false);
                }
            }
        }

        private void addVersionToListButton_Click(object sender, System.EventArgs e)
        {
            if(addVersionToListTextBox.Text.Length == 0)
            {
                return;
            }
            try
            {
                var version = KspVersion.Parse(addVersionToListTextBox.Text);
                selectedVersionsCheckedListBox.Items.Insert(0, version);
            }
            catch(FormatException ex)
            {
                MessageBox.Show("Version has invalid format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clearSelectionButton_Click(object sender, EventArgs e)
        {
            foreach (int index in selectedVersionsCheckedListBox.CheckedIndices)
            {
                selectedVersionsCheckedListBox.SetItemChecked(index, false);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            List<KspVersion> selectedVersion = new List<KspVersion>();
            foreach (KspVersion item in selectedVersionsCheckedListBox.CheckedItems)
            {
                selectedVersion.Add(item);
            }
            ksp.SetCompatibleVersions(selectedVersion);

            this.Close();
        }
    }
}
