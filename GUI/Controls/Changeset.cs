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
            alertLabels = AlertLabels;
            ChangesListView.Items.Clear();
            if (changes != null)
            {
                // We're going to split our change-set into two parts: removed mods,
                // and everything else (right now, replacing, upgrading, and installing mods, but we may have
                // other types in the future).

                sortedChangeSet.Clear();
                sortedChangeSet.AddRange(changes.Where(change => change.ChangeType == GUIModChangeType.Remove));

                // Now make our list more human-friendly (dependencies for a mod are listed directly
                // after it.)
                CreateSortedModList(changes
                    .Where(change => change.ChangeType != GUIModChangeType.Remove)
                    .ToList());

                ChangesListView.Items.AddRange(sortedChangeSet
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
        {
            get
            {
                return ChangesListView.SelectedItems;
            }
        }

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        public event Action OnConfirmChanges;
        public event Action<bool> OnCancelChanges;

        private void ChangesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnSelectedItemsChanged != null)
            {
                OnSelectedItemsChanged(ChangesListView.SelectedItems);
            }
        }

        private void ConfirmChangesButton_Click(object sender, EventArgs e)
        {
            if (OnConfirmChanges != null)
            {
                OnConfirmChanges();
            }
        }

        private void CancelChangesButton_Click(object sender, EventArgs e)
        {
            if (OnCancelChanges != null)
            {
                OnCancelChanges(true);
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (OnCancelChanges != null)
            {
                OnCancelChanges(false);
            }
        }

        private ListViewItem makeItem(ModChange change)
        {
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
                        change.Description
                      )
                    : change.Description
            })
            {
                Tag = m,
                ForeColor = warnLbl != null ? Color.Red : SystemColors.WindowText
            };
        }

        /// <summary>
        /// This method creates the Install part of the changeset
        /// It arranges the changeset in a human-friendly order
        /// The requested mod is listed first, its dependencies right after it
        /// So we get for example "ModuleRCSFX" directly after "USI Exploration Pack"
        ///
        /// It is very likely that this is forward-compatible with new ChangeTypes's,
        /// like a "reconfigure" changetype, but only the future will tell
        /// </summary>
        /// <param name="changes">Every leftover ModChange that should be sorted</param>
        /// <param name="parent"></param>
        private void CreateSortedModList(IEnumerable<ModChange> changes, ModChange parent = null)
        {
            var notUserReq = changes
                .Where(c => !(c.Reason is SelectionReason.UserRequested))
                .Memoize();
            foreach (ModChange change in changes)
            {
                bool goDeeper = parent == null || change.Reason.Parent.identifier == parent.Mod.identifier;

                if (goDeeper)
                {
                    if (!sortedChangeSet.Any(c => c.Mod.identifier == change.Mod.identifier && c.ChangeType != GUIModChangeType.Remove))
                        sortedChangeSet.Add(change);
                    CreateSortedModList(notUserReq, change);
                }
            }
        }

        private List<ModChange>   sortedChangeSet = new List<ModChange>();
        private List<ModuleLabel> alertLabels;
    }
}
