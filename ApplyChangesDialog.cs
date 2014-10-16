using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    public partial class ApplyChangesDialog : Form
    {
        
        public ApplyChangesDialog()
        {
            InitializeComponent();
        }

        public void ShowApplyChangesDialog(List<KeyValuePair<CkanModule, GUIModChangeType>> changeset)
        {
            ChangesListView.Items.Clear();

            foreach (var change in changeset)
            {
                if (change.Value == GUIModChangeType.None)
                {
                    continue;
                }

                ListViewItem item = new ListViewItem();
                item.Text = change.Key.name;

                var subChangeType = new ListViewItem.ListViewSubItem();
                subChangeType.Text = change.Value.ToString();

                item.SubItems.Add(subChangeType);
                ChangesListView.Items.Add(item);
            }

            ShowDialog();
        }
    }
}
