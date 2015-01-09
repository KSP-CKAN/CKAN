using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CKAN
{
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            RefreshReposListBox();

            KSPInstallPathLabel.Text = Main.Instance.CurrentInstance.GameDir();
            UpdateCacheInfo();
        }

        private void RefreshReposListBox()
        {
            ReposListBox.Items.Clear();

            foreach (var item in Main.Instance.CurrentInstance.Registry.Repositories)
            {
                var name = item.Value.name;
                var url = item.Value.uri;
                ReposListBox.Items.Add(String.Format("{0} | {1}", name, url));
            }
        }

        private void UpdateCacheInfo()
        {
            long cacheSize = 0;
            var cachePath = Path.Combine(Main.Instance.CurrentInstance.CkanDir(), "downloads");

            var cacheDirectory = new DirectoryInfo(cachePath);
            int count = 0;
            foreach (var file in cacheDirectory.GetFiles())
            {
                count++;
                cacheSize += file.Length;
            }

            CKANCacheLabel.Text = String.Format
            (
                "There are currently {0} files in the cache for a total of {1} MiB",
                count,
                cacheSize / 1024 / 1024
            );
        }

        private void ClearCKANCacheButton_Click(object sender, EventArgs e)
        {
            var cachePath = Path.Combine(Main.Instance.CurrentInstance.CkanDir(), "downloads");
            foreach (var file in Directory.GetFiles(cachePath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }

            UpdateCacheInfo();
        }

        private void ResetAutoStartChoice_Click(object sender, EventArgs e)
        {
            Main.Instance.Manager.ClearAutoStart();

            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Application.Exit();
        }

        private void ReposListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeleteRepoButton.Enabled = ReposListBox.SelectedItem != null;
        }

        private void DeleteRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            var item = (string)ReposListBox.SelectedItem;
            var repo = item.Split('|')[0].Trim();
            // Main.Instance.CurrentInstance.Registry.Repositories.Remove(repo);
            RefreshReposListBox();
            DeleteRepoButton.Enabled = false;
        }

        private void NewRepoButton_Click(object sender, EventArgs e)
        {
            var dialog = new NewRepoDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var repo = dialog.RepoUrlTextBox.Text.Split('|');
                    var name = repo[0].Trim();
                    var url = repo[1].Trim();

                    SortedDictionary<string, Repository> repositories = Main.Instance.CurrentInstance.Registry.Repositories;
                    if (repositories.ContainsKey(name))
                    {
                        repositories.Remove(name);
                    }

                    repositories.Add(name, new Repository(name, url));
                    Main.Instance.CurrentInstance.Registry.Repositories = repositories;

                    RefreshReposListBox();
                }
                catch (Exception ex)
                {
                    Main.Instance.m_User.RaiseError("Invalid repo format - should be \"<name> | <url>\"");
                }
            }
        }

    }
}