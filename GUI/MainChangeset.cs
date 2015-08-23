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

            m_Changeset = new List<ModChange>();
            m_Changeset.AddRange(changeset.Where(change => change.ChangeType == GUIModChangeType.Remove));
            m_Changeset.AddRange(changeset.Where(change => change.ChangeType == GUIModChangeType.Update));

            IEnumerable<ModChange> left = changeset.Where(change => change.ChangeType == GUIModChangeType.Install);
            CreateInstallList(left);

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
                    description.Text = String.Format("Update to version {0}", change.Mod.LatestVersion);
                }

                item.SubItems.Add(sub_change_type);
                item.SubItems.Add(description);
                ChangesListView.Items.Add(item);
            }
        }

        private void CreateInstallList(IEnumerable<ModChange> changes, ModChange parent=null)
        {
            foreach (ModChange change in changes)
            {
                bool goDeeper = parent == null;
                if (!goDeeper && change.Reason.Parent.identifier == parent.Mod.Identifier)
                {
                    goDeeper = true;
                }

                if (goDeeper)
                {
                    if (!m_Changeset.Any(c => c.Mod.Identifier == change.Mod.Identifier))
                        m_Changeset.Add(change);
                    CreateInstallList(changes.Where(c => c.Reason is SelectionReason.Depends), change);
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
