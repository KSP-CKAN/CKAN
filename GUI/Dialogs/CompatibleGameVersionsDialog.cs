using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Autofac;
using CKAN.Versioning;
using CKAN.GameVersionProviders;

namespace CKAN
{
    public partial class CompatibleGameVersionsDialog : Form
    {
        private GameInstance _inst;

        /// <summary>
        /// Initialize the compatible game versions dialog
        /// </summary>
        /// <param name="inst">Game instance</param>
        /// <param name="centerScreen">true to center the dialog on the screen, false to center on the parent</param>
        public CompatibleGameVersionsDialog(GameInstance inst, bool centerScreen)
        {
            this._inst = inst;
            InitializeComponent();

            if (centerScreen)
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }

            List<GameVersion> compatibleVersions = inst.GetCompatibleVersions();

            GameVersionLabel.Text  = inst.Version()?.ToString() ?? Properties.Resources.CompatibleGameVersionsDialogNone;
            GameLocationLabel.Text = inst.GameDir();
            List<GameVersion> knownVersions = inst.game.KnownVersions;
            List<GameVersion> majorVersionsList = CreateMajorVersionsList(knownVersions);
            List<GameVersion> compatibleVersionsLeftOthers = new List<GameVersion>(compatibleVersions);
            compatibleVersionsLeftOthers.RemoveAll((el)=>knownVersions.Contains(el) || majorVersionsList.Contains(el));

            SortAndAddVersionsToList(compatibleVersionsLeftOthers, compatibleVersions);
            SortAndAddVersionsToList(majorVersionsList, compatibleVersions);
            SortAndAddVersionsToList(knownVersions, compatibleVersions);
        }

        private void CompatibleGameVersionsDialog_Shown(object sender, EventArgs e)
        {
            if (_inst.CompatibleVersionsAreFromDifferentGameVersion)
            {
                MessageBox.Show(Properties.Resources.CompatibleGameVersionsDialogGameUpdated);
                CancelChooseCompatibleVersionsButton.Visible = false;
                GameVersionLabel.Text = string.Format(
                    Properties.Resources.CompatibleGameVersionsDialogVersionDetails,
                    _inst.Version(),
                    _inst.GameVersionWhenCompatibleVersionsWereStored
                );
                GameVersionLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private static List<GameVersion> CreateMajorVersionsList(List<GameVersion> knownVersions)
        {
            Dictionary<GameVersion, bool> majorVersions = new Dictionary<GameVersion, bool>();
            foreach (var version in knownVersions)
            {
                GameVersion fullKnownVersion = version.ToVersionRange().Lower.Value;
                GameVersion toAdd = new GameVersion(fullKnownVersion.Major, fullKnownVersion.Minor);
                if (!majorVersions.ContainsKey(toAdd))
                {
                    majorVersions.Add(toAdd, true);
                }
            }
            return new List<GameVersion>(majorVersions.Keys);
        }

        private void SortAndAddVersionsToList(List<GameVersion> versions, List<GameVersion> compatibleVersions)
        {
            versions.Sort();
            versions.Reverse();
            foreach (GameVersion version in versions)
            {
                if (!version.Equals(_inst.Version()))
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
                    Properties.Resources.CompatibleGameVersionsDialogInvalidFormat,
                    Properties.Resources.CompatibleGameVersionsDialogErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            try
            {
                var version = GameVersion.Parse(AddVersionToListTextBox.Text);
                SelectedVersionsCheckedListBox.Items.Insert(0, version);
            }
            catch (FormatException)
            {
                MessageBox.Show(
                    Properties.Resources.CompatibleGameVersionsDialogInvalidFormat,
                    Properties.Resources.CompatibleGameVersionsDialogErrorTitle,
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
            _inst.SetCompatibleVersions(
                SelectedVersionsCheckedListBox.CheckedItems.Cast<GameVersion>().ToList()
            );

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
