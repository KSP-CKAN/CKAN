using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using log4net;

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
            RefreshAuthTokensListBox();

            LocalVersionLabel.Text = Meta.GetVersion();

            CheckUpdateOnLaunchCheckbox.Checked = Main.Instance.configuration.CheckForUpdatesOnLaunch;
            RefreshOnStartupCheckbox.Checked = Main.Instance.configuration.RefreshOnStartup;
            HideEpochsCheckbox.Checked = Main.Instance.configuration.HideEpochs;

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
            var cachePath = Main.Instance.CurrentInstance.DownloadCacheDir();
            var cacheDirectory = new DirectoryInfo(cachePath);
            foreach (var file in cacheDirectory.GetFiles())
            {
                m_cacheFileCount++;
                m_cacheSize += file.Length;
            }

            CKANCacheLabel.Text = String.Format
            (
                "There are currently {0} cached files using {1} in total",
                m_cacheFileCount,
                CkanModule.FmtSize(m_cacheSize)
            );
        }

        private void ClearCKANCacheButton_Click(object sender, EventArgs e)
        {
            YesNoDialog deleteConfirmationDialog = new YesNoDialog();
            string confirmationText = String.Format
            (
                "Do you really want to delete {0} cached files, freeing {1}?",
                m_cacheFileCount,
                CkanModule.FmtSize(m_cacheSize)
            );

            if (deleteConfirmationDialog.ShowYesNoDialog(confirmationText) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (var file in Directory.GetFiles(Main.Instance.CurrentInstance.DownloadCacheDir()))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                    }
                }

                // tell the cache object to nuke itself
                Main.Instance.CurrentInstance.Cache.Clear();

                // forcibly tell all mod rows to re-check cache state
                foreach (DataGridViewRow row in Main.Instance.ModList.Rows)
                {
                    var mod = row.Tag as GUIMod;
                    mod?.UpdateIsCached();
                }

                // finally, clear the preview contents list
                Main.Instance.UpdateModContentsTree(null, true);

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

        private void RefreshAuthTokensListBox()
        {
            AuthTokensListBox.Items.Clear();
            foreach (string host in Win32Registry.GetAuthTokenHosts())
            {
                string token;
                if (Win32Registry.TryGetAuthToken(host, out token))
                {
                    AuthTokensListBox.Items.Add(string.Format("{0} | {1}", host, token));
                }
            }
        }

        private void AuthTokensListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeleteAuthTokenButton.Enabled = AuthTokensListBox.SelectedItem != null;
        }

        private void NewAuthTokenButton_Click(object sender, EventArgs e)
        {
            // Inspired by https://stackoverflow.com/a/17546909/2422988
            Form newAuthTokenPopup = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                StartPosition   = FormStartPosition.CenterParent,
                ClientSize      = new Size(300, 100),
                Text            = "Add Authentication Token"
            };
            Label hostLabel = new Label()
            {
                AutoSize = true,
                Location = new Point(3, 6),
                Size     = new Size(271, 13),
                Text     = "Host:"
            };
            TextBox hostTextBox = new TextBox()
            {
                Location = new Point(45, 6),
                Size     = new Size(newAuthTokenPopup.ClientSize.Width - 40 - 10, 23),
                Text     = ""
            };
            Label tokenLabel = new Label()
            {
                AutoSize = true,
                Location = new Point(3, 35),
                Size     = new Size(271, 13),
                Text     = "Token:"
            };
            TextBox tokenTextBox = new TextBox()
            {
                Location = new Point(45, 35),
                Size     = new Size(newAuthTokenPopup.ClientSize.Width - 40 - 10, 23),
                Text     = ""
            };
            Button acceptButton = new Button()
            {
                DialogResult = DialogResult.OK,
                Name         = "okButton",
                Size         = new Size(75, 23),
                Text         = "&Accept",
                Location     = new Point((newAuthTokenPopup.ClientSize.Width - 80 - 80) / 2, 64)
            };
            acceptButton.Click += (origin, evt) =>
            {
                newAuthTokenPopup.DialogResult = validNewAuthToken(hostTextBox.Text, tokenTextBox.Text)
                    ? DialogResult.OK
                    : DialogResult.None;
            };
            Button cancelButton = new Button()
            {
                DialogResult = DialogResult.Cancel,
                Name         = "cancelButton",
                Size         = new Size(75, 23),
                Text         = "&Cancel",
                Location     = new Point(acceptButton.Location.X + acceptButton.Size.Width + 5, 64)
            };

            newAuthTokenPopup.Controls.Add(hostLabel);
            newAuthTokenPopup.Controls.Add(hostTextBox);
            newAuthTokenPopup.Controls.Add(tokenLabel);
            newAuthTokenPopup.Controls.Add(tokenTextBox);
            newAuthTokenPopup.Controls.Add(acceptButton);
            newAuthTokenPopup.Controls.Add(cancelButton);
            newAuthTokenPopup.AcceptButton = acceptButton;
            newAuthTokenPopup.CancelButton = cancelButton;

            switch (newAuthTokenPopup.ShowDialog(this))
            {
                case DialogResult.Abort:
                case DialogResult.Cancel:
                case DialogResult.Ignore:
                case DialogResult.No:
                    // User cancelled out, so do nothing
                    break;

                case DialogResult.OK:
                case DialogResult.Yes:
                    Win32Registry.SetAuthToken(hostTextBox.Text, tokenTextBox.Text);
                    RefreshAuthTokensListBox();
                    break;
            }
        }

        private static bool validNewAuthToken(string host, string token)
        {
            if (host.Length <= 0)
            {
                GUI.user.RaiseError("Host field is required.");
                return false;
            }
            if (token.Length <= 0)
            {
                GUI.user.RaiseError("Token field is required.");
                return false;
            }
            if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
            {
                GUI.user.RaiseError("{0} is not a valid host name.", host);
                return false;
            }
            string oldToken;
            if (Win32Registry.TryGetAuthToken(host, out oldToken))
            {
                GUI.user.RaiseError("{0} already has an authentication token.", host);
                return false;
            }

            return true;
        }

        private void DeleteAuthTokenButton_Click(object sender, EventArgs e)
        {
            if (AuthTokensListBox.SelectedItem != null)
            {
                string item = AuthTokensListBox.SelectedItem as string;
                string host = item?.Split('|')[0].Trim();

                Win32Registry.SetAuthToken(host, null);
                RefreshAuthTokensListBox();
                DeleteRepoButton.Enabled = false;
            }
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                AutoUpdate.Instance.FetchLatestReleaseInfo();
                var latestVersion = AutoUpdate.Instance.LatestVersion;
                if (latestVersion.IsGreaterThan(new Version(Meta.GetVersion(VersionFormat.Short))) && AutoUpdate.Instance.IsFetched())
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
                log.Warn("Exception caught in CheckForUpdates:\r\n" + ex);
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

        private void HideEpochsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.HideEpochs = HideEpochsCheckbox.Checked;
            Main.Instance.configuration.Save();
        }
    }
}
