using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using log4net;
using Autofac;
using CKAN.Versioning;
using CKAN.Configuration;

namespace CKAN.GUI
{
    public partial class SettingsDialog : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsDialog));

        private IUser m_user;
        private IConfiguration config;
        private RegistryManager regMgr;

        /// <summary>
        /// Initialize a settings window
        /// </summary>
        public SettingsDialog(RegistryManager regMgr, IUser user)
        {
            InitializeComponent();
            m_user        = user;
            this.regMgr   = regMgr;
            if (Platform.IsMono)
            {
                this.ClearCacheMenu.Renderer = new FlatToolStripRenderer();
            }
            config = ServiceLocator.Container.Resolve<IConfiguration>();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        public void UpdateDialog()
        {
            RefreshReposListBox(false);
            RefreshAuthTokensListBox();
            UpdateLanguageSelectionComboBox();

            LocalVersionLabel.Text = Meta.GetVersion();

            CheckUpdateOnLaunchCheckbox.Checked = Main.Instance.configuration.CheckForUpdatesOnLaunch;
            RefreshOnStartupCheckbox.Checked = Main.Instance.configuration.RefreshOnStartup;
            HideEpochsCheckbox.Checked = Main.Instance.configuration.HideEpochs;
            HideVCheckbox.Checked = Main.Instance.configuration.HideV;
            AutoSortUpdateCheckBox.Checked = Main.Instance.configuration.AutoSortByUpdate;
            EnableTrayIconCheckBox.Checked = MinimizeToTrayCheckBox.Enabled = Main.Instance.configuration.EnableTrayIcon;
            MinimizeToTrayCheckBox.Checked = Main.Instance.configuration.MinimizeToTray;
            PauseRefreshCheckBox.Checked = Main.Instance.configuration.RefreshPaused;

            UpdateRefreshRate();

            UpdateCacheInfo(config.DownloadCacheDir);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (CachePath.Text != config.DownloadCacheDir
                && !Main.Instance.Manager.TrySetupCache(CachePath.Text, out string failReason))
            {
                m_user.RaiseError(Properties.Resources.SettingsDialogSummaryInvalid, failReason);
                e.Cancel = true;
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        private void UpdateRefreshRate()
        {
            int rate = config.RefreshRate;
            RefreshTextBox.Text = rate.ToString();
            PauseRefreshCheckBox.Enabled = rate != 0;
            Main.Instance.pauseToolStripMenuItem.Enabled = config.RefreshRate != 0;
            Main.Instance.UpdateRefreshTimer();
        }

        private void RefreshReposListBox(bool saveChanges = true)
        {
            ReposListBox.BeginUpdate();
            ReposListBox.Items.Clear();
            ReposListBox.Items.AddRange(regMgr.registry.Repositories.Values
                // SortedDictionary just sorts by name
                .OrderBy(r => r.priority)
                .Select(r => new ListViewItem(new string[]
                    {
                        r.name, r.uri.ToString(),
                    })
                    {
                        Tag = r,
                    })
                .ToArray());
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            ReposListBox.EndUpdate();
            EnableDisableRepoButtons();

            if (saveChanges)
            {
                UseWaitCursor = true;
                // Save registry in background thread to keep GUI responsive
                Task.Factory.StartNew(() =>
                {
                    // Visual cue that we're doing something
                    regMgr.Save();
                    Util.Invoke(this, () => UseWaitCursor = false);
                });
            }
        }

        private void UpdateLanguageSelectionComboBox()
        {
            LanguageSelectionComboBox.Items.Clear();

            LanguageSelectionComboBox.Items.AddRange(Utilities.AvailableLanguages);
            // If the current language is supported by CKAN, set is as selected.
            // Else display a blank field.
            LanguageSelectionComboBox.SelectedIndex = LanguageSelectionComboBox.FindStringExact(config.Language);
        }

        private void UpdateCacheInfo(string newPath)
        {
            CachePath.Text = newPath;
            // Background thread in case GetSizeInfo takes a while
            Task.Factory.StartNew(() =>
            {
                try
                {
                    // Make a temporary cache object to validate the path without changing the setting till close
                    var cache = new NetModuleCache(newPath);
                    cache.GetSizeInfo(out int cacheFileCount, out long cacheSize, out long cacheFreeSpace);

                    Util.Invoke(this, () =>
                    {
                        if (config.CacheSizeLimit.HasValue)
                        {
                            // Show setting in MiB
                            CacheLimit.Text = (config.CacheSizeLimit.Value / 1024 / 1024).ToString();
                        }
                        CacheSummary.Text = string.Format(
                            Properties.Resources.SettingsDialogSummmary,
                            cacheFileCount, CkanModule.FmtSize(cacheSize), CkanModule.FmtSize(cacheFreeSpace));
                        CacheSummary.ForeColor   = SystemColors.ControlText;
                        OpenCacheButton.Enabled  = true;
                        ClearCacheButton.Enabled = (cacheSize > 0);
                        PurgeToLimitMenuItem.Enabled = (config.CacheSizeLimit.HasValue
                            && cacheSize > config.CacheSizeLimit.Value);
                    });

                }
                catch (Exception ex)
                {
                    Util.Invoke(this, () =>
                    {
                        CacheSummary.Text        = string.Format(Properties.Resources.SettingsDialogSummaryInvalid,
                                                                 ex.Message);
                        CacheSummary.ForeColor   = Color.Red;
                        OpenCacheButton.Enabled  = false;
                        ClearCacheButton.Enabled = false;
                    });
                }
            });
        }

        private void CachePath_TextChanged(object sender, EventArgs e)
        {
            UpdateCacheInfo(CachePath.Text);
        }

        private void CacheLimit_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CacheLimit.Text))
            {
                config.CacheSizeLimit = null;
            }
            else
            {
                // Translate from MB to bytes
                config.CacheSizeLimit = Convert.ToInt64(CacheLimit.Text) * 1024 * 1024;
            }
            UpdateCacheInfo(CachePath.Text);
        }

        private void CacheLimit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void ChangeCacheButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog cacheChooser = new FolderBrowserDialog()
            {
                Description         = Properties.Resources.SettingsDialogCacheDescrip,
                RootFolder          = Environment.SpecialFolder.MyComputer,
                SelectedPath        = config.DownloadCacheDir,
                ShowNewFolderButton = true
            };
            DialogResult result = cacheChooser.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                UpdateCacheInfo(cacheChooser.SelectedPath);
            }
        }

        private void PurgeToLimitMenuItem_Click(object sender, EventArgs e)
        {
            // Purge old downloads if we're over the limit
            if (config.CacheSizeLimit.HasValue)
            {
                // Switch main cache since user seems committed to this path
                if (CachePath.Text != config.DownloadCacheDir
                    && !Main.Instance.Manager.TrySetupCache(CachePath.Text, out string failReason))
                {
                    m_user.RaiseError(Properties.Resources.SettingsDialogSummaryInvalid, failReason);
                    return;
                }

                Main.Instance.Manager.Cache.EnforceSizeLimit(
                    config.CacheSizeLimit.Value,
                    regMgr.registry);
                UpdateCacheInfo(config.DownloadCacheDir);
            }
        }

        private void PurgeAllMenuItem_Click(object sender, EventArgs e)
        {
            // Switch main cache since user seems committed to this path
            if (CachePath.Text != config.DownloadCacheDir
                && !Main.Instance.Manager.TrySetupCache(CachePath.Text, out string failReason))
            {
                m_user.RaiseError(Properties.Resources.SettingsDialogSummaryInvalid, failReason);
                return;
            }

            Main.Instance.Manager.Cache.GetSizeInfo(
                out int cacheFileCount, out long cacheSize, out long cacheFreeSpace);

            YesNoDialog deleteConfirmationDialog = new YesNoDialog();
            string confirmationText = String.Format(
                Properties.Resources.SettingsDialogDeleteConfirm,
                cacheFileCount,
                CkanModule.FmtSize(cacheSize));

            if (deleteConfirmationDialog.ShowYesNoDialog(this, confirmationText) == DialogResult.Yes)
            {
                // tell the cache object to nuke itself
                Main.Instance.Manager.Cache.RemoveAll();

                // forcibly tell all mod rows to re-check cache state
                foreach (DataGridViewRow row in Main.Instance.ManageMods.ModGrid.Rows)
                {
                    var mod = row.Tag as GUIMod;
                    mod?.UpdateIsCached();
                }

                // finally, clear the preview contents list
                Main.Instance.RefreshModContentsTree();

                UpdateCacheInfo(config.DownloadCacheDir);
            }
        }

        private void ResetCacheButton_Click(object sender, EventArgs e)
        {
            // Reset to default cache path
            UpdateCacheInfo(JsonConfiguration.DefaultDownloadCacheDir);
        }

        private void OpenCacheButton_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(config.DownloadCacheDir);
        }

        private void ReposListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableRepoButtons();
        }

        private void EnableDisableRepoButtons()
        {
            DeleteRepoButton.Enabled = ReposListBox.SelectedIndices.Count > 0
                // Don't allow deletion of the last repo; the default will be
                // re-added automatically at load, so empty repos isn't a valid state.
                // To remove the last one, add its replacement first.
                && ReposListBox.Items.Count > 1;
            UpRepoButton.Enabled = ReposListBox.SelectedIndices.Count > 0
                && ReposListBox.SelectedIndices[0] > 0
                && ReposListBox.SelectedIndices[0] < ReposListBox.Items.Count;
            DownRepoButton.Enabled = ReposListBox.SelectedIndices.Count > 0
                && ReposListBox.SelectedIndices[0] < ReposListBox.Items.Count - 1
                && ReposListBox.SelectedIndices[0] >= 0;
        }

        private void DeleteRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItems.Count == 0)
            {
                return;
            }

            var repo = ReposListBox.SelectedItems[0].Tag as Repository;
            YesNoDialog deleteConfirmationDialog = new YesNoDialog();
            if (deleteConfirmationDialog.ShowYesNoDialog(this,
                string.Format(Properties.Resources.SettingsDialogRepoDeleteConfirm,
                              repo.name),
                Properties.Resources.SettingsDialogRepoDeleteDelete,
                Properties.Resources.SettingsDialogRepoDeleteCancel)
                    == DialogResult.Yes)
            {
                var registry = regMgr.registry;
                registry.RepositoriesRemove(repo.name);
                RefreshReposListBox();
                DeleteRepoButton.Enabled = false;
            }
        }

        private void NewRepoButton_Click(object sender, EventArgs e)
        {
            var dialog = new NewRepoDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var repo = dialog.Selection;
                var registry = regMgr.registry;
                if (registry.Repositories.Values.Any(other => other.uri == repo.uri))
                {
                    m_user.RaiseError(Properties.Resources.SettingsDialogRepoAddDuplicateURL, repo.uri);
                    return;
                }
                if (registry.Repositories.TryGetValue(repo.name, out Repository existing))
                {
                    repo.priority = existing.priority;
                    registry.RepositoriesRemove(repo.name);
                }
                else
                {
                    repo.priority = registry.Repositories.Count;
                }
                registry.RepositoriesAdd(repo);

                RefreshReposListBox();
            }
        }

        private void UpRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedIndices.Count == 0
                || ReposListBox.SelectedIndices[0] == 0)
            {
                return;
            }

            var selected = ReposListBox.SelectedItems[0].Tag as Repository;
            var prev     = ReposListBox.Items.Cast<ListViewItem>()
                                             .Select(item => item.Tag as Repository)
                                             .FirstOrDefault(r => r.priority == selected.priority - 1);
            --selected.priority;
            if (prev != null)
            {
                ++prev.priority;
            }
            RefreshReposListBox();
        }

        private void DownRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedIndices.Count == 0
                || ReposListBox.SelectedIndices[0] == ReposListBox.Items.Count - 1)
            {
                return;
            }

            var selected = ReposListBox.SelectedItems[0].Tag as Repository;
            var next     = ReposListBox.Items.Cast<ListViewItem>()
                                             .Select(item => item.Tag as Repository)
                                             .FirstOrDefault(r => r.priority == selected.priority + 1);
            ++selected.priority;
            if (next != null)
            {
                --next.priority;
            }
            RefreshReposListBox();
        }

        private void RefreshAuthTokensListBox()
        {
            AuthTokensListBox.Items.Clear();
            foreach (string host in config.GetAuthTokenHosts())
            {
                string token;
                if (config.TryGetAuthToken(host, out token))
                {
                    AuthTokensListBox.Items.Add(new ListViewItem(
                        new string[] { host, token })
                    {
                        Tag = $"{host}|{token}"
                    });
                }
            }
        }

        private void AuthTokensListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeleteAuthTokenButton.Enabled = AuthTokensListBox.SelectedItems.Count > 0;
        }

        private void NewAuthTokenButton_Click(object sender, EventArgs e)
        {
            // Inspired by https://stackoverflow.com/a/17546909/2422988
            Form newAuthTokenPopup = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                StartPosition   = FormStartPosition.CenterParent,
                ClientSize      = new Size(300, 100),
                Text            = Properties.Resources.AddAuthTokenTitle
            };
            Label hostLabel = new Label()
            {
                AutoSize = true,
                Location = new Point(3, 8),
                Size     = new Size(65, 13),
                Text     = Properties.Resources.AddAuthTokenHost
            };
            TextBox hostTextBox = new TextBox()
            {
                Location = new Point(3 + 65 + 3, 6),
                Size     = new Size(newAuthTokenPopup.ClientSize.Width - 65 - 10, 23),
                Text     = ""
            };
            Label tokenLabel = new Label()
            {
                AutoSize = true,
                Location = new Point(3, 38),
                Size     = new Size(65, 13),
                Text     = Properties.Resources.AddAuthTokenToken
            };
            TextBox tokenTextBox = new TextBox()
            {
                Location = new Point(3 + 65 + 3, 35),
                Size     = new Size(newAuthTokenPopup.ClientSize.Width - 65 - 10, 23),
                Text     = ""
            };
            Button acceptButton = new Button()
            {
                DialogResult = DialogResult.OK,
                Name         = "okButton",
                Size         = new Size(75, 23),
                Text         = Properties.Resources.AddAuthTokenAccept,
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
                Text         = Properties.Resources.AddAuthTokenCancel,
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
                    config.SetAuthToken(hostTextBox.Text, tokenTextBox.Text);
                    RefreshAuthTokensListBox();
                    break;
            }
        }

        private bool validNewAuthToken(string host, string token)
        {
            if (host.Length <= 0)
            {
                m_user.RaiseError(Properties.Resources.AddAuthTokenHostRequired);
                return false;
            }
            if (token.Length <= 0)
            {
                m_user.RaiseError(Properties.Resources.AddAuthTokenTokenRequired);
                return false;
            }
            if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
            {
                m_user.RaiseError(Properties.Resources.AddAuthTokenInvalidHost, host);
                return false;
            }
            string oldToken;
            if (ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(host, out oldToken))
            {
                m_user.RaiseError(Properties.Resources.AddAuthTokenDupHost, host);
                return false;
            }

            return true;
        }

        private void DeleteAuthTokenButton_Click(object sender, EventArgs e)
        {
            if (AuthTokensListBox.SelectedItems.Count > 0)
            {
                string item = AuthTokensListBox.SelectedItems[0].Tag as string;
                string host = item?.Split('|')[0].Trim();

                config.SetAuthToken(host, null);
                RefreshAuthTokensListBox();
                DeleteAuthTokenButton.Enabled = false;
            }
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            try
            {
                AutoUpdate.Instance.FetchLatestReleaseInfo();
                var latestVersion = AutoUpdate.Instance.latestUpdate.Version;
                if (latestVersion.IsGreaterThan(new ModuleVersion(Meta.GetVersion(VersionFormat.Short))) && AutoUpdate.Instance.IsFetched())
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
            if (AutoUpdate.CanUpdate)
            {
                Hide();
                Main.Instance.UpdateCKAN();
            }
            else
            {
                m_user.RaiseError(Properties.Resources.SettingsDialogUpdateFailed);
            }

        }

        private void CheckUpdateOnLaunchCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.CheckForUpdatesOnLaunch = CheckUpdateOnLaunchCheckbox.Checked;
        }

        private void RefreshOnStartupCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.RefreshOnStartup = RefreshOnStartupCheckbox.Checked;
        }

        private void HideEpochsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.HideEpochs = HideEpochsCheckbox.Checked;
        }

        private void HideVCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.HideV = HideVCheckbox.Checked;
        }

        private void LanguageSelectionComboBox_SelectionChanged(object sender, EventArgs e)
        {
            config.Language = LanguageSelectionComboBox.SelectedItem.ToString();
        }

        private void AutoSortUpdateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.AutoSortByUpdate = AutoSortUpdateCheckBox.Checked;
        }

        private void EnableTrayIconCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            MinimizeToTrayCheckBox.Enabled = Main.Instance.configuration.EnableTrayIcon = EnableTrayIconCheckBox.Checked;
            Main.Instance.CheckTrayState();
        }

        private void MinimizeToTrayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.MinimizeToTray = MinimizeToTrayCheckBox.Checked;
            Main.Instance.CheckTrayState();
        }

        private void RefreshTextBox_TextChanged(object sender, EventArgs e)
        {
            config.RefreshRate = string.IsNullOrEmpty(RefreshTextBox.Text) ? 0 : int.Parse(RefreshTextBox.Text);
            UpdateRefreshRate();
        }

        private void RefreshTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void PauseRefreshCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Main.Instance.configuration.RefreshPaused = PauseRefreshCheckBox.Checked;

            if (Main.Instance.configuration.RefreshPaused)
                Main.Instance.refreshTimer.Stop();
            else
                Main.Instance.refreshTimer.Start();
        }
    }
}
