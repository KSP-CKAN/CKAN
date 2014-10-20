using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{
    public partial class RecommendsDialog : Form
    {
        public RecommendsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public List<string> ShowRecommendsDialog(string message, List<string> recommended)
        {
            Util.Invoke(MessageLabel, () => MessageLabel.Text = message);

            if (RecommendedListView.InvokeRequired)
            {
                RecommendedListView.Invoke(new MethodInvoker(delegate
                {
                    RecommendedListView.Items.Clear();
                    foreach (string mod in recommended)
                    {
                        var item = new ListViewItem();
                        var subName = new ListViewItem.ListViewSubItem();
                        subName.Text = mod;
                        item.SubItems.Add(subName);
                        RecommendedListView.Items.Add(item);
                    }
                }));
            }
            else
            {
                RecommendedListView.Items.Clear();
                foreach (string mod in recommended)
                {
                    var item = new ListViewItem();
                    var subName = new ListViewItem.ListViewSubItem();
                    subName.Text = mod;
                    item.SubItems.Add(subName);
                    RecommendedListView.Items.Add(item);
                }
            }


            if (ShowDialog() == DialogResult.OK)
            {
                var selected = new List<string>();

                foreach (ListViewItem item in RecommendedListView.Items)
                {
                    if (item.Checked)
                    {
                        string name = item.SubItems[1].Text;
                        selected.Add(name);
                    }
                }

                return selected;
            }
            return null;
        }

        private void SelectAllButton_Click(object sender, EventArgs e)
        {
            if (RecommendedListView.InvokeRequired)
            {
                RecommendedListView.Invoke(new MethodInvoker(delegate
                {
                    foreach (ListViewItem item in RecommendedListView.Items)
                    {
                        item.Checked = true;
                    }
                }));
            }
            else
            {
                foreach (ListViewItem item in RecommendedListView.Items)
                {
                    item.Checked = true;
                }
            }
        }
    }
}