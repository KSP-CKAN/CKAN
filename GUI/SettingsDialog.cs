using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Repository.Hierarchy;

namespace CKAN
{
    public partial class SettingsDialog : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsDialog));

        public static readonly string DefaultBuildUrl =
            "https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/builds.json";

        public Configuration Configuration;
        public KSP Instance;
        public string BuildUrl => BuildJsonTextBox.Text;
        public List<Repository> Repos { get; private set; } = new List<Repository>();

        private long _cacheSize;
        private int _cacheFileCount;

        public SettingsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            // allows later mocking of these variables
            // todo: get rid of static Main.Instance
            if (Main.Instance != null)
            {
                Configuration = Main.Instance.configuration;
                Instance = Main.Instance.CurrentInstance;
            }
        }

        public void Initialize()
        {
            UpdateDialog();
            UpdateBuildMapUrl();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public async void UpdateBuildMapUrl()
        {
            // only bother checking for validity if the value has been changed
            if (Configuration.BuildMapUrl.Equals(BuildJsonTextBox.Text)) return;

            if (!await EnsureValidBuildUrl())
            {
                // revert value to known default
                BuildJsonTextBox.Text = DefaultBuildUrl;

                // todo: warning dialog telling the user to fix the URL
            }

            Configuration.BuildMapUrl = string.IsNullOrWhiteSpace(BuildUrl)
                ? BuildUrl
                : DefaultBuildUrl;
        }

        public void UpdateDialog()
        {
            RefreshReposListBox();

            BuildJsonTextBox.Text = string.IsNullOrWhiteSpace(Configuration.BuildMapUrl)
                ? Configuration.BuildMapUrl
                : DefaultBuildUrl;
            LocalVersionLabel.Text = Meta.Version();

            CheckUpdateOnLaunchCheckbox.Checked = Configuration.CheckForUpdatesOnLaunch;
            RefreshOnStartupCheckbox.Checked = Configuration.RefreshOnStartup;

            UpdateCacheInfo();
        }

        private async Task<bool> EnsureValidBuildUrl()
        {
            // give ourselves a fighting chance at making the HTTP request
            if (string.IsNullOrWhiteSpace(BuildJsonTextBox.Text))
            {
                BuildJsonTextBox.Text = DefaultBuildUrl;
            }

            // perform a dumb read of the configured URL
            try
            {
                var simpleValidator = new WebClient();
                await simpleValidator.DownloadStringTaskAsync(BuildJsonTextBox.Text);
                // no need to validate the contents of the file, just whether or not the URL yields a response
                return true;
            }
            catch (WebException e)
            {
                Log.Warn("Build map URL failed validation!", e);
            }

            return false;
        }

        private void RefreshReposListBox()
        {
            // Give the Repository the priority it
            // currently has in the gui
            for (var i = 0; i < Repos.Count; i++)
            {
                Repos[i].priority = i;
            }

            var manager = RegistryManager.Instance(Instance);
            var registry = manager.registry;
            Repos = new List<Repository>(registry.Repositories.Values);
            Repos.Sort((repo1, repo2) => repo1.priority.CompareTo(repo2.priority));
            ReposListBox.Items.Clear();
            foreach (var repo in Repos)
            {
                ReposListBox.Items.Add(string.Format("{0} | {1}", repo.name, repo.uri));
            }

            manager.Save();
        }

        private void UpdateCacheInfo()
        {
            _cacheSize = 0;
            _cacheFileCount = 0;

            var cacheDirectory = new DirectoryInfo(Instance.DownloadCacheDir());
            foreach (var file in cacheDirectory.GetFiles())
            {
                _cacheFileCount++;
                _cacheSize += file.Length;
            }

            CKANCacheLabel.Text = string.Format
            (
                "There are currently {0} cached files using {1} MB in total",
                _cacheFileCount,
                _cacheSize / 1024 / 1024
            );
        }

        private void ClearCKANCacheButton_Click(object sender, EventArgs e)
        {
            var deleteConfirmationDialog = new YesNoDialog();
            var confirmationText = string.Format
            (
                "Do you really want to delete {0} cached files, freeing {1} MB?",
                _cacheFileCount, 
                _cacheSize / 1024 / 1024
            );

            if (deleteConfirmationDialog.ShowYesNoDialog(confirmationText) == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    var cacheDir = Instance.DownloadCacheDir();
                    Directory.Delete(cacheDir, true);
                    Directory.CreateDirectory(cacheDir);
                }
                catch (Exception ex)
                {
                    Log.Info("Exception thrown trying to clean up cache directory", ex);
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

            var item = Repos[ReposListBox.SelectedIndex];
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

                    repositories.Add(name, new Repository(name, url, Repos.Count));
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

            var item = Repos[ReposListBox.SelectedIndex];
            Repos.RemoveAt(ReposListBox.SelectedIndex);
            Repos.Insert(ReposListBox.SelectedIndex - 1, item);
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

            var item = Repos[ReposListBox.SelectedIndex];
            Repos.RemoveAt(ReposListBox.SelectedIndex);
            Repos.Insert(ReposListBox.SelectedIndex + 1, item);
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
                Log.Warn("Exception caught in CheckForUpdates:\r\n"+ex);
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

        private void SettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateBuildMapUrl();
        }
    }
}