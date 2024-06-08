using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Versioning;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class CompatibleGameVersionsDialog : Form
    {
        private readonly GameInstance _inst;

        /// <summary>
        /// Initialize the compatible game versions dialog
        /// </summary>
        /// <param name="inst">Game instance</param>
        /// <param name="centerScreen">true to center the dialog on the screen, false to center on the parent</param>
        public CompatibleGameVersionsDialog(GameInstance inst, bool centerScreen)
        {
            _inst = inst;
            InitializeComponent();

            if (centerScreen)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }

            var compatibleVersions = inst.GetCompatibleVersions();

            GameVersionLabel.Text  = inst.Version()?.ToString() ?? Properties.Resources.CompatibleGameVersionsDialogNone;
            GameLocationLabel.Text = Platform.FormatPath(inst.GameDir());
            var knownVersions = inst.game.KnownVersions;
            var majorVersionsList = CreateMajorVersionsList(knownVersions);
            var compatibleVersionsLeftOthers = compatibleVersions.Except(knownVersions)
                                                                 .Except(majorVersionsList)
                                                                 .ToList();

            SortAndAddVersionsToList(compatibleVersionsLeftOthers, compatibleVersions);
            SortAndAddVersionsToList(majorVersionsList, compatibleVersions);
            SortAndAddVersionsToList(knownVersions, compatibleVersions);
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.CompatibleGameVersions);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.CompatibleGameVersions);
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
                    _inst.GameVersionWhenCompatibleVersionsWereStored);
                GameVersionLabel.ForeColor = Color.Red;
            }
        }

        private static List<GameVersion> CreateMajorVersionsList(List<GameVersion> knownVersions)
            => knownVersions.Select(v => v.ToVersionRange().Lower.Value)
                            .Select(v => new GameVersion(v.Major, v.Minor))
                            .Distinct()
                            .ToList();

        private void SortAndAddVersionsToList(List<GameVersion> versions, List<GameVersion> compatibleVersions)
        {
            foreach (var version in versions.Where(v => v != _inst.Version())
                                            .Reverse())
            {
                SelectedVersionsCheckedListBox.Items.Add(version, compatibleVersions.Contains(version));
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
                MessageBox.Show(Properties.Resources.CompatibleGameVersionsDialogInvalidFormat,
                                Properties.Resources.CompatibleGameVersionsDialogErrorTitle,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }
            try
            {
                var version = GameVersion.Parse(AddVersionToListTextBox.Text);
                SelectedVersionsCheckedListBox.Items.Insert(0, version);
            }
            catch (FormatException)
            {
                MessageBox.Show(Properties.Resources.CompatibleGameVersionsDialogInvalidFormat,
                                Properties.Resources.CompatibleGameVersionsDialogErrorTitle,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
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
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            _inst.SetCompatibleVersions(SelectedVersionsCheckedListBox.CheckedItems
                                                                      .Cast<GameVersion>()
                                                                      .ToList());

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
