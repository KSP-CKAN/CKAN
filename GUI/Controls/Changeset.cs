using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Extensions;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Changeset : UserControl
    {
        public Changeset()
        {
            InitializeComponent();
            // Reduce the grid's flickering
            ChangesGrid.GetType()
                       .GetProperty("DoubleBuffered",
                                    BindingFlags.Instance | BindingFlags.NonPublic)
                       .SetValue(ChangesGrid, true, null);
        }

        public void LoadChangeset(List<ModChange>                changes,
                                  List<ModuleLabel>              AlertLabels,
                                  Dictionary<CkanModule, string> conflicts)
        {
            changeset = changes;
            ConfirmChangesButton.Enabled = conflicts == null || !conflicts.Any();
            CloseTheGameLabel.Visible = changes?.Any(ch => DeletingChanges.Contains(ch.ChangeType))
                                        ?? false;
            ChangesGrid.DataSource = new BindingList<ChangesetRow>(
                changes?.Select(ch => new ChangesetRow(ch, AlertLabels, conflicts))
                        .ToList()
                       ?? new List<ChangesetRow>());
        }

        public CkanModule SelectedItem => SelectedRow?.Change.Mod;

        public event Action<CkanModule> OnSelectedItemsChanged;
        public event Action<ModChange>  OnRemoveItem;

        public event Action<List<ModChange>> OnConfirmChanges;
        public event Action<bool>            OnCancelChanges;

        private void ChangesGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ChangesGrid.ClearSelection();
            foreach (var row in ChangesGrid.Rows.OfType<DataGridViewRow>())
            {
                var obj = row.DataBoundItem as ChangesetRow;
                row.DefaultCellStyle.ForeColor = obj?.WarningLabel != null
                                                 ? Color.Red : SystemColors.WindowText;
                row.DefaultCellStyle.BackColor = obj?.Conflict != null
                                                 ? Color.LightCoral : Color.Empty;
                if (obj?.Change.IsRemovable ?? false)
                {
                    foreach (var icon in row.Cells.OfType<DataGridViewImageCell>())
                    {
                        icon.ToolTipText = string.Format(Properties.Resources.ChangesetDeleteTooltip,
                                                         obj.ChangeType, obj.Change.Mod);
                    }
                }
            }
            ChangesGrid.AutoResizeColumns();
        }

        private void ChangesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (ChangesGrid.CurrentCell?.OwningColumn == DeleteColumn
                && ChangesGrid.CurrentRow?.DataBoundItem is ChangesetRow row
                && row.Change.IsRemovable
                && row.ConfirmUncheck())
            {
                (ChangesGrid.DataSource as BindingList<ChangesetRow>)?.Remove(row);
                changeset.Remove(row.Change);
                OnRemoveItem?.Invoke(row.Change);
            }
        }

        private void ConfirmChangesButton_Click(object sender, EventArgs e)
        {
            OnConfirmChanges?.Invoke(changeset);
        }

        private void CancelChangesButton_Click(object sender, EventArgs e)
        {
            changeset = null;
            OnCancelChanges?.Invoke(true);
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            OnCancelChanges?.Invoke(false);
        }

        private void ChangesGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (ChangesGrid.SelectedRows.Count > 0 && !Visible)
            {
                // Suppress selection while inactive
                ChangesGrid.ClearSelection();
            }
            // Don't pop up mod info when they click the X icons
            else if (ChangesGrid.CurrentCell?.OwningColumn is DataGridViewTextBoxColumn)
            {
                OnSelectedItemsChanged?.Invoke(SelectedRow?.Change.Mod);
            }
        }

        private static readonly HashSet<GUIModChangeType> DeletingChanges = new HashSet<GUIModChangeType>
        {
            GUIModChangeType.Remove,
            GUIModChangeType.Update,
            GUIModChangeType.Replace,
        };

        private ChangesetRow SelectedRow
            => ChangesGrid.SelectedRows
                          .OfType<DataGridViewRow>()
                          .FirstOrDefault()
                          ?.DataBoundItem as ChangesetRow;

        private List<ModChange> changeset;
    }

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class ChangesetRow
    {
        public ChangesetRow(ModChange                      change,
                            List<ModuleLabel>              alertLabels,
                            Dictionary<CkanModule, string> conflicts)
        {
            Change  = change;
            WarningLabel = alertLabels?.FirstOrDefault(l =>
                l.ContainsModule(Main.Instance.CurrentInstance.game,
                                 Change.Mod.identifier));
            conflicts?.TryGetValue(Change.Mod, out Conflict);
            Reasons = Conflict != null
                        ? string.Format("{0} ({1})", Conflict, Change.Description)
                    : WarningLabel != null
                        ? string.Format(
                            Properties.Resources.MainChangesetWarningInstallingModuleWithLabel,
                            WarningLabel.Name, Change.Description)
                    : Change.Description;
        }

        public readonly ModChange   Change;
        public readonly ModuleLabel WarningLabel = null;
        public readonly string      Conflict     = null;

        public string Mod         => Change.NameAndStatus;
        public string ChangeType  => Change.ChangeType.Localize();
        public string Reasons     { get; private set; }
        public Bitmap DeleteImage => Change.IsRemovable ? EmbeddedImages.textClear
                                                        : EmptyBitmap;

        public bool ConfirmUncheck()
            => Change.IsAutoRemoval
                ? Main.Instance.YesNoDialog(
                    string.Format(Properties.Resources.ChangesetConfirmRemoveAutoRemoval, Change.Mod),
                    Properties.Resources.ChangesetConfirmRemoveAutoRemovalYes,
                    Properties.Resources.ChangesetConfirmRemoveAutoRemovalNo)
                : Main.Instance.YesNoDialog(
                    string.Format(Properties.Resources.ChangesetConfirmRemoveUserRequested, ChangeType, Change.Mod),
                    Properties.Resources.ChangesetConfirmRemoveUserRequestedYes,
                    Properties.Resources.ChangesetConfirmRemoveUserRequestedNo);

        private static readonly Bitmap EmptyBitmap = new Bitmap(1, 1);
    }
}
