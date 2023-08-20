using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

using CKAN.Extensions;

namespace CKAN.GUI
{
    public partial class Changeset : UserControl
    {
        public Changeset()
        {
            InitializeComponent();
        }

        public void LoadChangeset(
            List<ModChange> changes,
            List<ModuleLabel> AlertLabels,
            Dictionary<CkanModule, string> conflicts)
        {
            changeset      = changes;
            alertLabels    = AlertLabels;
            this.conflicts = conflicts;
            ConfirmChangesButton.Enabled = conflicts == null || !conflicts.Any();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                // Update list on each refresh in case caching changed
                UpdateList();
            }
        }

        public ListView.SelectedListViewItemCollection SelectedItems
            => ChangesListView.SelectedItems;

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        public event Action<List<ModChange>> OnConfirmChanges;
        public event Action<bool> OnCancelChanges;

        private void UpdateList()
        {
            ChangesListView.BeginUpdate();
            ChangesListView.Items.Clear();
            if (changeset != null)
            {
                // Changeset sorting is handled upstream in the resolver
                ChangesListView.Items.AddRange(changeset
                    .Where(ch => ch.ChangeType != GUIModChangeType.None)
                    .Select(ch => makeItem(ch, conflicts))
                    .ToArray());
                ChangesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                ChangesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            ChangesListView.EndUpdate();
        }

        private void ChangesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedItemsChanged?.Invoke(ChangesListView.SelectedItems);
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

        private ListViewItem makeItem(ModChange change, Dictionary<CkanModule, string> conflicts)
        {
            var descr = change.Description;
            CkanModule m = change.Mod;
            ModuleLabel warnLbl = alertLabels?.FirstOrDefault(l =>
                l.ContainsModule(Main.Instance.CurrentInstance.game, m.identifier));
            return new ListViewItem(new string[]
            {
                change.NameAndStatus,
                change.ChangeType.ToI18nString(),
                conflicts != null && conflicts.TryGetValue(m, out string confDescr)
                    ? string.Format("{0} ({1})", confDescr, descr)
                    : warnLbl != null
                        ? string.Format(
                            Properties.Resources.MainChangesetWarningInstallingModuleWithLabel,
                            warnLbl.Name,
                            descr)
                        : descr
            })
            {
                Tag         = m,
                ForeColor   = warnLbl != null ? Color.Red : SystemColors.WindowText,
                BackColor   = conflicts != null && conflicts.ContainsKey(m) ? Color.LightCoral : Color.Empty,
                ToolTipText = descr,
            };
        }

        private List<ModChange>                changeset;
        private List<ModuleLabel>              alertLabels;
        private Dictionary<CkanModule, string> conflicts;
    }
}
