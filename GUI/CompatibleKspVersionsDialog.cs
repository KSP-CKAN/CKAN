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
        private KSP _ksp;

        public CompatibleKspVersionsDialog(KSP ksp)
        {

            this._ksp = ksp;
            InitializeComponent();

            List<KspVersion> compatibleVersions = ksp.GetCompatibleVersions();

            GameVersionLabel.Text  = ksp.Version()?.ToString() ?? "<NONE>";
            GameLocationLabel.Text = ksp.GameDir();
            List<KspVersion> knownVersions = new List<KspVersion>(ServiceLocator.Container.Resolve<IKspBuildMap>().KnownVersions);
            List<KspVersion> majorVersionsList = CreateMajorVersionsList(knownVersions);
            List<KspVersion> compatibleVersionsLeftOthers = new List<KspVersion>(compatibleVersions);
            compatibleVersionsLeftOthers.RemoveAll((el)=>knownVersions.Contains(el) || majorVersionsList.Contains(el));

            SortAndAddVersionsToList(compatibleVersionsLeftOthers, compatibleVersions);
            SortAndAddVersionsToList(majorVersionsList, compatibleVersions);
            SortAndAddVersionsToList(knownVersions, compatibleVersions);
        }

        private void CompatibleKspVersionsDialog_Shown(object sender, EventArgs e)
        {
            if (_ksp.CompatibleVersionsAreFromDifferentKsp)
            {
                MessageBox.Show("KSP has been updated since you last reviewed your compatible KSP versions. Please make sure that settings are correct.");
                CancelChooseCompatibleVersionsButton.Visible = false;
                GameVersionLabel.Text =  $"{_ksp.Version()} (previous game version: {_ksp.VersionOfKspWhenCompatibleVersionsWereStored})";
                GameVersionLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private static List<KspVersion> CreateMajorVersionsList(List<KspVersion> knownVersions)
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
                if (!version.Equals(_ksp.Version()))
                {
                    SelectedVersionsCheckedListBox.Items.Add(version, compatibleVersions.Contains(version));
                }
            }
        }

        private void AddVersionToListButton_Click(object sender, System.EventArgs e)
        {
            if(AddVersionToListTextBox.Text.Length == 0)
            {
                return;
            }
            try
            {
                var version = KspVersion.Parse(AddVersionToListTextBox.Text);
                SelectedVersionsCheckedListBox.Items.Insert(0, version);
            }
            catch(FormatException)
            {
                MessageBox.Show("Version has invalid format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearSelectionButton_Click(object sender, EventArgs e)
        {
            foreach (int index in SelectedVersionsCheckedListBox.CheckedIndices)
            {
                SelectedVersionsCheckedListBox.SetItemChecked(index, false);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            List<KspVersion> selectedVersion = new List<KspVersion>();
            foreach (KspVersion item in SelectedVersionsCheckedListBox.CheckedItems)
            {
                selectedVersion.Add(item);
            }
            _ksp.SetCompatibleVersions(selectedVersion);

            this.Close();
        }
    }
}
