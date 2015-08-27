using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {

        private List<ModChange> m_Changeset;

        public void UpdateChangesDialog(List<ModChange> changeset, BackgroundWorker installWorker)
        {
            m_Changeset = changeset;
            m_InstallWorker = installWorker;
            ChangesListView.Items.Clear();

            if (changeset == null)
            {
                return;
            }

            // We're going to split our change-set into two parts: updated/removed mods,
            // and everything else (which right now is installing mods, but we may have
            // other types in the future).

            m_Changeset = new List<ModChange>();
            m_Changeset.AddRange(changeset.Where(change => change.ChangeType == GUIModChangeType.Remove));
            m_Changeset.AddRange(changeset.Where(change => change.ChangeType == GUIModChangeType.Update));

            IEnumerable<ModChange> leftOver = changeset.Where(change => change.ChangeType != GUIModChangeType.Remove
                                                && change.ChangeType != GUIModChangeType.Update);
            
            // Now make our list more human-friendly (dependencies for a mod are listed directly
            // after it.)
            CreateSortedModList(leftOver);

            changeset = m_Changeset;

            foreach (var change in changeset)
            {
                if (change.ChangeType == GUIModChangeType.None)
                {
                    continue;
                }

                var item = new ListViewItem {Text = String.Format("{0} {1}", change.Mod.Name, change.Mod.Version)};

                var sub_change_type = new ListViewItem.ListViewSubItem {Text = change.ChangeType.ToString()};

                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem();
                description.Text = change.Reason.Reason.Trim();

                if (change.ChangeType == GUIModChangeType.Update)
                {
                    description.Text = String.Format("Update selected by user to version {0}.", change.Mod.LatestVersion);
                }

                if (change.ChangeType == GUIModChangeType.Install && change.Reason is SelectionReason.UserRequested)
                {
                    description.Text = "New mod install selected by user.";
                }

                item.SubItems.Add(sub_change_type);
                item.SubItems.Add(description);
                ChangesListView.Items.Add(item);
            }
        }

        /// <summary>
        /// This method creates the Install part of the changeset
        /// It arranges the changeset in a human-friendly order
        /// The requested mod is listed first, it's dependencies right after it
        /// So we get for example "ModuleRCSFX" directly after "USI Exploration Pack"
        /// 
        /// It is very likely that this is forward-compatible with new ChangeTypes's,
        /// like a a "reconfigure" changetype, but only the future will tell
        /// </summary>
        /// <param name="changes">Every leftover ModChange that should be sorted</param>
        /// <param name="parent"></param>
        private void CreateSortedModList(IEnumerable<ModChange> changes, ModChange parent=null)
        {
            foreach (ModChange change in changes)
            {
                bool goDeeper = parent == null || change.Reason.Parent.identifier == parent.Mod.Identifier;

                if (goDeeper)
                {
                    if (!m_Changeset.Any(c => c.Mod.Identifier == change.Mod.Identifier))
                        m_Changeset.Add(change);
                    CreateSortedModList(changes.Where(c => !(c.Reason is SelectionReason.UserRequested)), change);
                }
            }
        }

        private void CancelChangesButton_Click(object sender, EventArgs e)
        {
            UpdateModsList();
            UpdateChangesDialog(null, m_InstallWorker);
            m_TabController.ShowTab("ManageModsTabPage");
            m_TabController.HideTab("ChangesetTabPage");
            ApplyToolButton.Enabled = false;
        }

        private void ConfirmChangesButton_Click(object sender, EventArgs e)
        {
            if (m_Changeset == null)
                return;

            menuStrip1.Enabled = false;

            RelationshipResolverOptions install_ops = RelationshipResolver.DefaultOpts();
            install_ops.with_recommends = false;
            //Using the changeset passed in can cause issues with versions.
            // An example is Mechjeb for FAR at 25/06/2015 with a 1.0.2 install.
            // TODO Work out why this is.
            var user_change_set = mainModList.ComputeUserChangeSet().ToList();
            m_InstallWorker.RunWorkerAsync(
                new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                    user_change_set, install_ops));
            m_Changeset = null;

            UpdateChangesDialog(null, m_InstallWorker);
            ShowWaitDialog();
        }

    }
}
