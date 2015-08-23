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

            foreach (var change in changeset)
            {
                if (change.ChangeType == GUIModChangeType.None)
                {
                    continue;
                }

                var item = new ListViewItem {Text = String.Format("{0} {1}", change.Mod.Name, change.Mod.Version)};

                var sub_change_type = new ListViewItem.ListViewSubItem {Text = change.ChangeType.ToString()};

                item.SubItems.Add(sub_change_type);
                ChangesListView.Items.Add(item);
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
