using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ApplyChangesDialog : Form
    {
        private List<KeyValuePair<CkanModule, GUIModChangeType>> m_Changeset;
        private BackgroundWorker m_InstallWorker;

        public ApplyChangesDialog()
        {
            InitializeComponent();
        }

        public void ShowApplyChangesDialog(List<KeyValuePair<CkanModule, GUIModChangeType>> changeset,
            BackgroundWorker installWorker)
        {
            m_Changeset = changeset;
            m_InstallWorker = installWorker;

            ChangesListView.Items.Clear();

            foreach (var change in changeset)
            {
                if (change.Value == GUIModChangeType.None)
                {
                    continue;
                }

                var item = new ListViewItem();
                item.Text = String.Format("{0} v{1}", change.Key.name, change.Key.version);

                var subChangeType = new ListViewItem.ListViewSubItem();
                subChangeType.Text = change.Value.ToString();

                item.SubItems.Add(subChangeType);
                ChangesListView.Items.Add(item);
            }

            ShowDialog();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            m_Changeset = null;
            m_InstallWorker = null;
            Close();
            Main.Instance.UpdateModsList();
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            RelationshipResolverOptions install_ops = RelationshipResolver.DefaultOpts();
            install_ops.with_recommends = false;
            
            m_InstallWorker.RunWorkerAsync(
                new KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>(
                    m_Changeset, install_ops));
            m_InstallWorker = null;
            m_Changeset = null;
            Close();

            Main.Instance.ShowWaitDialog();
        }
    }
}