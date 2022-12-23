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

        public void LoadChangeset(List<ModChange> changes, List<ModuleLabel> AlertLabels)
        {
            changeset = changes;
            alertLabels = AlertLabels;
            ChangesListView.Items.Clear();
            if (changes != null)
            {
                // Changeset sorting is handled upstream in the resolver
                ChangesListView.Items.AddRange(changes
                    .Where(ch => ch.ChangeType != GUIModChangeType.None)
                    .Select(makeItem)
                    .ToArray());
                ChangesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                ChangesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && Platform.IsMono)
            {
                // Workaround: make sure the ListView headers are drawn
                Util.Invoke(ChangesListView, () => ChangesListView.EndUpdate());
            }
        }

        public ListView.SelectedListViewItemCollection SelectedItems
            => ChangesListView.SelectedItems;

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        public event Action<List<ModChange>> OnConfirmChanges;
        public event Action<bool> OnCancelChanges;

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

        private ListViewItem makeItem(ModChange change)
        {
            var descr = change.Description;
            CkanModule m = change.Mod;
            ModuleLabel warnLbl = alertLabels?.FirstOrDefault(l => l.ModuleIdentifiers.Contains(m.identifier));
            return new ListViewItem(new string[]
            {
                change.NameAndStatus,
                change.ChangeType.ToI18nString(),
                warnLbl != null
                    ? string.Format(
                        Properties.Resources.MainChangesetWarningInstallingModuleWithLabel,
                        warnLbl.Name,
                        descr)
                    : descr
            })
            {
                Tag = m,
                ForeColor = warnLbl != null ? Color.Red : SystemColors.WindowText,
                ToolTipText = descr,
            };
        }

        private List<ModChange> changeset;
        private List<ModuleLabel> alertLabels;
    }
}
