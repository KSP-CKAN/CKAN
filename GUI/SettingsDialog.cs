using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using log4net;
using log4net.Repository.Hierarchy;

namespace CKAN
{
    public partial class SettingsDialog : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsDialog));
        public SettingsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        public void UpdateDialog()
        {
            RefreshReposListBox();

            LocalVersionLabel.Text = Meta.Version();

            CheckUpdateOnLaunchCheckbox.Checked = Main.Instance.m_Configuration.CheckForUpdatesOnLaunch;

            UpdateCacheInfo();
        }

        private void RefreshReposListBox()
        {
            List<Repository> sortedRepos = new List<Repository>();
            foreach (var item in Main.Instance.CurrentInstance.Registry.Repositories)
            {
                sortedRepos.Add(item.Value);
            }

            sortedRepos.Sort((repo1, repo2) => repo1.priority.CompareTo(repo2.priority));

            ReposListBox.Items.Clear();
            foreach (var item in sortedRepos)
            {
                ReposListBox.Items.Add(item);
            }

            Main.Instance.CurrentInstance.RegistryManager.Save();
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

        private void ReposListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeleteRepoButton.Enabled = ReposListBox.SelectedItem != null;

            if (ReposListBox.SelectedIndex > 0 && ReposListBox.SelectedIndex < ReposListBox.Items.Count)
            {
                UpRepoButton.Enabled = true;
            }
            else
            {
                UpRepoButton.Enabled = false;
            }

            if (ReposListBox.SelectedIndex  < ReposListBox.Items.Count - 1 && ReposListBox.SelectedIndex >= 0)
            {
                DownRepoButton.Enabled = true;
            }
            else
            {
                DownRepoButton.Enabled = false;
            }
        }

        private void DeleteRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            var item = (Repository)ReposListBox.SelectedItem;
            Main.Instance.CurrentInstance.Registry.Repositories.Remove(item.name);
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
                catch (Exception)
                {
                    Main.Instance.m_User.RaiseError("Invalid repo format - should be \"<name> | <url>\"");
                }
            }
        }

        private void UpRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            if (ReposListBox.SelectedIndex == 0)
            {
                return;
            }

            var item = (Repository)ReposListBox.SelectedItem;
            var aboveItem = (Repository)ReposListBox.Items[ReposListBox.SelectedIndex - 1];
            item.priority = aboveItem.priority - 1;
            RefreshReposListBox();
        }

        private void DownRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            if (ReposListBox.SelectedIndex == ReposListBox.Items.Count - 1)
            {
                return;
            }

            var item = (Repository)ReposListBox.SelectedItem;
            var belowItem = (Repository)ReposListBox.Items[ReposListBox.SelectedIndex + 1];
            item.priority = belowItem.priority + 1;
            RefreshReposListBox();
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                var latestVersion = AutoUpdate.FetchLatestCkanVersion();

                if (latestVersion.IsGreaterThan(new Version(Meta.Version())))
                {
                    InstallUpdateButton.Enabled = true;
                }
                else
                {
                    InstallUpdateButton.Enabled = false;
                }

                LatestVersionLabel.Text = latestVersion.ToString();
            }
            catch (Exception ex)
            {
                log.Warn("Exception caught in CheckForUpdates:\n"+ex);
            }
        }

        private void InstallUpdateButton_Click(object sender, EventArgs e)
        {
            AutoUpdate.StartUpdateProcess(true);
        }

        private void CheckUpdateOnLaunchCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.m_Configuration.CheckForUpdatesOnLaunch = CheckUpdateOnLaunchCheckbox.Checked;
            Main.Instance.m_Configuration.Save();
        }
    }
}