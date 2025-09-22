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

            var compatibleVersions = inst.CompatibleVersions;

            ActualGameVersionLabel.Text  = inst.Version()
                                               ?.ToString()
                                               ?? Properties.Resources.CompatibleGameVersionsDialogNone;
            ActualInstancePathLabel.Text = Platform.FormatPath(inst.GameDir);
            var knownVersions = inst.Game.KnownVersions;
            var majorVersions = MajorVersions(knownVersions).ToArray();
            var compatibleVersionsLeftOthers = compatibleVersions.Except(knownVersions)
                                                                 .Except(majorVersions)
                                                                 .ToArray();

            SortAndAddVersionsToList(compatibleVersionsLeftOthers, compatibleVersions);
            SortAndAddVersionsToList(majorVersions,                compatibleVersions);
            SortAndAddVersionsToList(knownVersions,                compatibleVersions);
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

        private void ShowMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                MessageLabel.Text = "";
                MessageLabel.Visible = false;
            }
            else
            {
                MessageLabel.Text = msg;
                MessageLabel.Visible = true;
                MessageLabel.Height = Util.LabelStringHeight(CreateGraphics(),
                                                             MessageLabel);
                Height += MessageLabel.Height;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (MessageLabel.Visible)
            {
                MessageLabel.Height = Util.LabelStringHeight(CreateGraphics(),
                                                             MessageLabel);
            }
        }

        private void CompatibleGameVersionsDialog_Shown(object? sender, EventArgs? e)
        {
            if (_inst.CompatibleVersionsAreFromDifferentGameVersion)
            {
                CancelChooseCompatibleVersionsButton.Visible = false;
                if (_inst.GameVersionWhenCompatibleVersionsWereStored == null)
                {
                    ShowMessage(Properties.Resources.CompatibleGameVersionsDialogDefaultedDetails);
                }
                else
                {
                    ActualGameVersionLabel.Text = string.Format(
                        Properties.Resources.CompatibleGameVersionsDialogVersionDetails,
                        _inst.Version(),
                        _inst.GameVersionWhenCompatibleVersionsWereStored);
                    ActualGameVersionLabel.ForeColor = Color.Red;
                    ShowMessage(Properties.Resources.CompatibleGameVersionsDialogGameUpdatedDetails);
                }
            }
        }

        private static IEnumerable<GameVersion> MajorVersions(IReadOnlyCollection<GameVersion> knownVersions)
            => knownVersions.Select(v => v.ToVersionRange().Lower.Value)
                            .Select(v => new GameVersion(v.Major, v.Minor))
                            .Distinct();

        private void SortAndAddVersionsToList(IReadOnlyCollection<GameVersion> versions,
                                              IReadOnlyCollection<GameVersion> compatibleVersions)
        {
            foreach (var version in versions.Where(v => v != _inst.Version())
                                            .Reverse())
            {
                SelectedVersionsCheckedListBox.Items.Add(version, compatibleVersions.Contains(version));
            }
        }

        private void AddVersionToListButton_Click(object? sender, EventArgs? e)
        {
            if (AddVersionToListTextBox.Text.Length == 0)
            {
                return;
            }
            if (AddVersionToListTextBox.Text.Equals("any",
                                                    StringComparison.CurrentCultureIgnoreCase))
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

        private void ClearSelectionButton_Click(object? sender, EventArgs? e)
        {
            foreach (int index in SelectedVersionsCheckedListBox.CheckedIndices
                                                                .OfType<int>()
                                                                .ToArray())
            {
                SelectedVersionsCheckedListBox.SetItemChecked(index, false);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs? e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SaveButton_Click(object? sender, EventArgs? e)
        {
            _inst.SetCompatibleVersions(SelectedVersionsCheckedListBox.CheckedItems
                                                                      .Cast<GameVersion>()
                                                                      .ToList());

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
