using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Autofac;
using CKAN.Versioning;
using CKAN.GameVersionProviders;

namespace CKAN
{
    public partial class CompatibleKspVersionsDialog : Form
    {
        private KSP _ksp;

        /// <summary>
        /// Initialize the compatible game versions dialog
        /// </summary>
        /// <param name="ksp">Game instance</param>
        /// <param name="centerScreen">true to center the dialog on the screen, false to center on the parent</param>
        public CompatibleKspVersionsDialog(KSP ksp, bool centerScreen)
        {
            this._ksp = ksp;
            InitializeComponent();

            if (centerScreen)
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }

            List<KspVersion> compatibleVersions = ksp.GetCompatibleVersions();

            GameVersionLabel.Text  = ksp.Version()?.ToString() ?? Properties.Resources.CompatibleKspVersionsDialogNone;
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
                MessageBox.Show(Properties.Resources.CompatibleKspVersionsDialogKSPUpdated);
                CancelChooseCompatibleVersionsButton.Visible = false;
                GameVersionLabel.Text = string.Format(
                    Properties.Resources.CompatibleKspVersionsDialogVersionDetails,
                    _ksp.Version(),
                    _ksp.VersionOfKspWhenCompatibleVersionsWereStored
                );
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
            foreach (KspVersion version in versions)
            {
                if (!version.Equals(_ksp.Version()))
                {
                    SelectedVersionsCheckedListBox.Items.Add(version, compatibleVersions.Contains(version));
                }
            }
        }

        private void AddVersionToListButton_Click(object sender, EventArgs e)
        {
            if (AddVersionToListTextBox.Text.Length == 0)
            {
                return;
            }
            if (AddVersionToListTextBox.Text.ToLower() == "any") 
            {
                MessageBox.Show(
                    Properties.Resources.CompatibleKspVersionsDialogInvalidFormat,
                    Properties.Resources.CompatibleKspVersionsDialogErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            try
            {
                var version = KspVersion.Parse(AddVersionToListTextBox.Text);
                SelectedVersionsCheckedListBox.Items.Insert(0, version);
            }
            catch (FormatException)
            {
                MessageBox.Show(
                    Properties.Resources.CompatibleKspVersionsDialogInvalidFormat,
                    Properties.Resources.CompatibleKspVersionsDialogErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            _ksp.SetCompatibleVersions(
                SelectedVersionsCheckedListBox.CheckedItems.Cast<KspVersion>().ToList()
            );

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
