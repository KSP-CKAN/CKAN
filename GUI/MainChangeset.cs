using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {

        private List<KeyValuePair<CkanModule, GUIModChangeType>> m_Changeset;

        public void UpdateChangesDialog(List<KeyValuePair<CkanModule, GUIModChangeType>> changeset, BackgroundWorker installWorker)
        {
            m_Changeset = changeset;
            main_install_gui.install_worker = installWorker;
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

                var item = new ListViewItem {Text = String.Format("{0} {1}", change.Key.name, change.Key.version)};

                var subChangeType = new ListViewItem.ListViewSubItem {Text = change.Value.ToString()};

                item.SubItems.Add(subChangeType);
                ChangesListView.Items.Add(item);
            }
        }

        private void CancelChangesButton_Click(object sender, EventArgs e)
        {
            UpdateModsList();
            UpdateChangesDialog(null, main_install_gui.install_worker);
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

            main_install_gui.install_worker.RunWorkerAsync(
                new KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>(
                    m_Changeset, install_ops));
            m_Changeset = null;

            UpdateChangesDialog(null, main_install_gui.install_worker);
            ShowWaitDialog();
        }

    }
}
