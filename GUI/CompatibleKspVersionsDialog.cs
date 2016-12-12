using CKAN.GameVersionProviders;
using System.Windows.Forms;
using System.Collections.Generic;
using Autofac;
using CKAN.Versioning;
using System;

namespace CKAN
{
    public partial class CompatibleKspVersionsDialog : Form
    {
        private KSP ksp;

        public CompatibleKspVersionsDialog(KSP ksp)
        {
            this.ksp = ksp;
            InitializeComponent();

            List<KspVersion> compatibleVersions = ksp.GetCompatibleVersions();

            gameVersionLabel.Text = ksp.Version().ToString();
            gameLocationLabel.Text = ksp.GameDir();
            List<KspVersion> knownVersions = new List<KspVersion>(ServiceLocator.Container.Resolve<IKspBuildMap>().getKnownVersions());
            List<KspVersion> majorVersionsList = createMajorVersionsList(knownVersions);
            List<KspVersion> compatibleVersionsLeftOthers = new List<KspVersion>(compatibleVersions);
            compatibleVersionsLeftOthers.RemoveAll((el)=>knownVersions.Contains(el) || majorVersionsList.Contains(el));

            SortAndAddVersionsToList(compatibleVersionsLeftOthers, compatibleVersions);            
            SortAndAddVersionsToList(majorVersionsList, compatibleVersions);            
            SortAndAddVersionsToList(knownVersions, compatibleVersions);
        }

        private static List<KspVersion> createMajorVersionsList(List<KspVersion> knownVersions)
        {
            Dictionary<KspVersion, bool> majorVersions = new Dictionary<KspVersion, bool>();
            foreach (var version in knownVersions)
            {
                KspVersion fullKnownVersion = version.ToVersionRange().Lower.Value;
                KspVersion toAdd = new KspVersion(fullKnownVersion.Major, fullKnownVersion.Minor);
                if (!majorVersions.ContainsKey(toAdd))
                {
                    majorVersions.Add(toAdd, true);
                }
            }
            return new List<KspVersion>(majorVersions.Keys);
        }

        private void SortAndAddVersionsToList(List<KspVersion> versions, List<KspVersion> compatibleVersions)
        {
            versions.Sort();
            versions.Reverse();
            foreach (var version in versions)
            {
                if (!version.Equals(ksp.Version()))
                {
                    selectedVersionsCheckedListBox.Items.Add(version, compatibleVersions.Contains(version));
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
