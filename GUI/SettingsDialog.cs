using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        private long m_cacheSize;
        private int m_cacheFileCount;

        private List<Repository> _sortedRepos = new List<Repository>();

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

            CheckUpdateOnLaunchCheckbox.Checked = Main.Instance.configuration.CheckForUpdatesOnLaunch;
            RefreshOnStartupCheckbox.Checked = Main.Instance.configuration.RefreshOnStartup;

            UpdateCacheInfo();
        }

        private void RefreshReposListBox()
        {
            // Give the Repository the priority it
            // currently has in the gui
            for (int i = 0; i < _sortedRepos.Count; i++)
            {
                _sortedRepos[i].priority = i;
            }

            var manager = RegistryManager.Instance(Main.Instance.CurrentInstance);
            var registry = manager.registry;
            _sortedRepos = new List<Repository>(registry.Repositories.Values);

            _sortedRepos.Sort((repo1, repo2) => repo1.priority.CompareTo(repo2.priority));
            ReposListBox.Items.Clear();
            foreach (var repo in _sortedRepos)
            {
                ReposListBox.Items.Add(string.Format("{0} | {1}", repo.name, repo.uri));
            }

            manager.Save();
        }

        private void UpdateCacheInfo()
        {
            m_cacheSize = 0;
            m_cacheFileCount = 0;
            var cachePath = Path.Combine(Main.Instance.CurrentInstance.CkanDir(), "downloads");

            var cacheDirectory = new DirectoryInfo(cachePath);
            foreach (var file in cacheDirectory.GetFiles())
            {
                m_cacheFileCount++;
                m_cacheSize += file.Length;
            }

            CKANCacheLabel.Text = String.Format
            (
                "There are currently {0} cached files using {1} MB in total",
                m_cacheFileCount,
                m_cacheSize / 1024 / 1024
            );
        }

        private void ClearCKANCacheButton_Click(object sender, EventArgs e)
        {
            YesNoDialog deleteConfirmationDialog = new YesNoDialog();
            string confirmationText = String.Format
            (
                "Do you really want to delete {0} cached files, freeing {1} MB?",
                m_cacheFileCount, 
                m_cacheSize / 1024 / 1024
            );

            if (deleteConfirmationDialog.ShowYesNoDialog(confirmationText) == System.Windows.Forms.DialogResult.Yes)
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

            var item = _sortedRepos[ReposListBox.SelectedIndex];
            var registry = RegistryManager.Instance(Main.Instance.CurrentInstance).registry;
            registry.Repositories.Remove(item.name);
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

                    var registry = RegistryManager.Instance(Main.Instance.CurrentInstance).registry;
                    SortedDictionary<string, Repository> repositories = registry.Repositories;
                    if (repositories.ContainsKey(name))
                    {
                        repositories.Remove(name);
                    }

                    repositories.Add(name, new Repository(name, url, _sortedRepos.Count));
                    registry.Repositories = repositories;

                    RefreshReposListBox();
                }
                catch (Exception)
                {
                    Main.Instance.currentUser.RaiseError("Invalid repo format - should be \"<name> | <url>\"");
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

            var item = _sortedRepos[ReposListBox.SelectedIndex];
            _sortedRepos.RemoveAt(ReposListBox.SelectedIndex);
            _sortedRepos.Insert(ReposListBox.SelectedIndex - 1, item);
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

            var item = _sortedRepos[ReposListBox.SelectedIndex];
            _sortedRepos.RemoveAt(ReposListBox.SelectedIndex);
            _sortedRepos.Insert(ReposListBox.SelectedIndex + 1, item);
            RefreshReposListBox();
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                AutoUpdate.Instance.FetchLatestReleaseInfo();
                var latestVersion = AutoUpdate.Instance.LatestVersion;
                if (latestVersion.IsGreaterThan(new Version(Meta.Version())) && AutoUpdate.Instance.IsFetched())
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
                log.Warn("Exception caught in CheckForUpdates:\r\n"+ex);
            }
        }

        private void InstallUpdateButton_Click(object sender, EventArgs e)
        {
            Hide();
            Main.Instance.UpdateCKAN();
        }

        private void CheckUpdateOnLaunchCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.CheckForUpdatesOnLaunch = CheckUpdateOnLaunchCheckbox.Checked;
            Main.Instance.configuration.Save();
        }

        private void RefreshOnStartupCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.RefreshOnStartup = RefreshOnStartupCheckbox.Checked;
            Main.Instance.configuration.Save();
        }
    }
}