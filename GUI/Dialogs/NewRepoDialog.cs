using System;
﻿using System.Linq;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class NewRepoDialog : Form
    {
        public NewRepoDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public Repository Selection
        {
            get
            {
                return new Repository(RepoNameTextBox.Text, RepoUrlTextBox.Text);
            }
        }

        private void NewRepoDialog_Load(object sender, EventArgs e)
        {
            RepositoryList repositories;

            try
            {
                repositories = Main.FetchMasterRepositoryList();
            }
            catch
            {
                ReposListBox.Items.Add(Properties.Resources.NewRepoDialogFailed);
                return;
            }

            ReposListBox.Items.Clear();

            if (repositories.repositories == null)
            {
                ReposListBox.Items.Add(Properties.Resources.NewRepoDialogFailed);
                return;
            }

            ReposListBox.Items.AddRange(repositories.repositories.Select(r =>
                new ListViewItem(new string[] { r.name, r.uri.ToString() })
                {
                    Tag = r
                }
            ).ToArray());
        }

        private void ReposListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItems.Count == 0)
            {
                return;
            }

            Repository r = ReposListBox.SelectedItems[0].Tag as Repository;
            RepoNameTextBox.Text = r.name;
            RepoUrlTextBox.Text = r.uri.ToString();
        }

        private void RepoUrlTextBox_TextChanged(object sender, EventArgs e)
        {
            RepoOK.Enabled = RepoNameTextBox.Text.Length > 0
                && RepoUrlTextBox.Text.Length > 0;
        }
    }
}
