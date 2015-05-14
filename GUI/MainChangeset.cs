using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {

        private List<KeyValuePair<GUIMod, GUIModChangeType>> m_Changeset;

        public void UpdateChangesDialog(List<KeyValuePair<GUIMod, GUIModChangeType>> changeset, BackgroundWorker installWorker)
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
                if (change.Value == GUIModChangeType.None)
                {
                    continue;
                }

                var item = new ListViewItem {Text = String.Format("{0} {1}", change.Key.Name, change.Key.Version)};

                var sub_change_type = new ListViewItem.ListViewSubItem {Text = change.Value.ToString()};

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
            
            m_InstallWorker.RunWorkerAsync(
                new KeyValuePair<List<KeyValuePair<GUIMod, GUIModChangeType>>, RelationshipResolverOptions>(
                    m_Changeset, install_ops));
            m_Changeset = null;

            UpdateChangesDialog(null, m_InstallWorker);
            ShowWaitDialog();
        }

    }
}
