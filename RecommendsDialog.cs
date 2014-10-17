using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{
    public partial class RecommendsDialog : Form
    {
        public RecommendsDialog()
        {
            InitializeComponent();
        }

        public List<string> ShowRecommendsDialog(string message, List<string> recommended)
        {
            if (MessageLabel.InvokeRequired)
            {
                MessageLabel.Invoke(new MethodInvoker(delegate
                {
                    MessageLabel.Text = message;
                }));
            }
            else
            {
                MessageLabel.Text = message;
            }

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

            if (ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> selected = new List<string>();

                foreach (ListViewItem item in RecommendedListView.Items)
                {
                    if (item.Checked)
                    {
                        selected.Add(item.SubItems[0].Text);
                    }
                }

                return selected;
            }
            else
            {
                return null;
            }
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
