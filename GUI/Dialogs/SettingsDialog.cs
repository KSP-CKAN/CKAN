using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;

using CKAN.Versioning;
using CKAN.Configuration;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class SettingsDialog : Form
    {
        /// <summary>
        /// Initialize a settings window
        /// </summary>
        public SettingsDialog(IConfiguration   coreConfig,
                              GUIConfiguration guiConfig,
                              RegistryManager  regMgr,
                              AutoUpdate       updater,
                              IUser            user,
                              string?          userAgent)
        {
            InitializeComponent();

            ToolTip.SetToolTip(RefreshTextBox,    Properties.Resources.SettingsToolTipRefreshTextBox);
            ToolTip.SetToolTip(ChangeCacheButton, Properties.Resources.SettingsToolTipChangeCacheButton);
            ToolTip.SetToolTip(ResetCacheButton,  Properties.Resources.SettingsToolTipResetCacheButton);
            ToolTip.SetToolTip(OpenCacheButton,   Properties.Resources.SettingsToolTipOpenCacheButton);
            ToolTip.SetToolTip(ClearCacheButton,  Properties.Resources.SettingsToolTipClearCacheButton);

            this.coreConfig = coreConfig;
            this.guiConfig  = guiConfig;
            this.regMgr     = regMgr;
            this.updater    = updater;
            this.user       = user;
            this.userAgent  = userAgent;
            if (Platform.IsMono)
            {
                ClearCacheMenu.Renderer = new FlatToolStripRenderer();
            }
            CachePathEditButton.Height = CachePathSaveButton.Height =
                CachePathCancelButton.Height = CachePathTextBox.Height;
        }

        private void SettingsDialog_Load(object? sender, EventArgs? e)
        {
            UpdateDialog();
        }

        public void UpdateDialog()
        {
            RefreshReposListBox(false);
            RefreshAuthTokensListBox();
            UpdateLanguageSelectionComboBox();
            UpdateAutoUpdate();

            CheckUpdateOnLaunchCheckbox.Checked = guiConfig.CheckForUpdatesOnLaunch;
            DevBuildsCheckbox.Checked = coreConfig.DevBuilds ?? false;
            RefreshOnStartupCheckbox.Checked = guiConfig.RefreshOnStartup;
            HideEpochsCheckbox.Checked = guiConfig.HideEpochs;
            HideVCheckbox.Checked = guiConfig.HideV;
            AutoSortUpdateCheckBox.Checked = guiConfig.AutoSortByUpdate;
            EnableTrayIconCheckBox.Checked = MinimizeToTrayCheckBox.Enabled = guiConfig.EnableTrayIcon;
            MinimizeToTrayCheckBox.Checked = guiConfig.MinimizeToTray;
            PauseRefreshCheckBox.Checked = guiConfig.RefreshPaused;

            UpdateRefreshRate();

            UpdateCacheInfo(coreConfig.DownloadCacheDir ?? JsonConfiguration.DefaultDownloadCacheDir);
            UpdateStabilityToleranceComboBox();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!RepositoryGroupBox.Enabled)
            {
                // Don't close the window if still editing the cache path
                e.Cancel = true;
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        #region Cache path and limit

        private void UpdateCacheInfo(string newPath)
        {
            CachePathTextBox.Text = newPath;
            EnableDisableCachePath(false);
            if (manager?.Cache != null)
            {
                // Background thread in case GetSizeInfo takes a while
                Task.Run(() =>
                {
                    try
                    {
                        manager.Cache.GetSizeInfo(out int   cacheFileCount,
                                                  out long  cacheSize,
                                                  out long? cacheFreeSpace);

                        Util.Invoke(this, () =>
                        {
                            if (coreConfig.CacheSizeLimit.HasValue)
                            {
                                // Show setting in MiB
                                CacheLimit.Text = (coreConfig.CacheSizeLimit.Value / 1024 / 1024).ToString();
                            }
                            CacheSummary.Text = cacheFreeSpace.HasValue
                                ? string.Format(Properties.Resources.SettingsDialogSummmary,
                                                cacheFileCount,
                                                CkanModule.FmtSize(cacheSize),
                                                CkanModule.FmtSize(cacheFreeSpace.Value))
                                : string.Format(Properties.Resources.SettingsDialogSummmaryFreeSpaceUnknown,
                                                cacheFileCount,
                                                CkanModule.FmtSize(cacheSize));
                            CacheSummary.ForeColor   = SystemColors.ControlText;
                            OpenCacheButton.Enabled  = true;
                            ClearCacheButton.Enabled = (cacheSize > 0);
                            PurgeToLimitMenuItem.Enabled = (coreConfig.CacheSizeLimit.HasValue
                                                            && cacheSize > coreConfig.CacheSizeLimit.Value);
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
        }

        private async void CachePathTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    UpdateCacheInfo(coreConfig.DownloadCacheDir ?? JsonConfiguration.DefaultDownloadCacheDir);
                    break;

                case Keys.Enter:
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    await TrySaveCachePath(CachePathTextBox.Text);
                    break;
            }
        }

        private void CachePathEditButton_Click(object? sender, EventArgs? e)
        {
            EnableDisableCachePath(true);
            CachePathTextBox.Focus();
        }

        private async void CachePathSaveButton_Click(object? sender, EventArgs? e)
        {
            await TrySaveCachePath(CachePathTextBox.Text);
        }

        private void CachePathCancelButton_Click(object? sender, EventArgs? e)
        {
            UpdateCacheInfo(coreConfig.DownloadCacheDir
                            ?? JsonConfiguration.DefaultDownloadCacheDir);
        }

        private async Task<bool> TrySaveCachePath(string newPath)
        {
            CachePathTextBox.Enabled      = false;
            CachePathSaveButton.Enabled   = false;
            CachePathCancelButton.Enabled = false;
            return await Task.Run(() =>
            {
                if (newPath != coreConfig.DownloadCacheDir
                    && manager != null
                    && !manager.TrySetupCache(newPath,
                                              new ProgressImmediate<int>(UpdateCacheProgress),
                                              out string? failReason))
                {
                    Util.Invoke(this, () =>
                    {
                        MoveCacheProgressBar.Visible = false;
                        if (failReason.Length > 0)
                        {
                            user.RaiseError(Properties.Resources.SettingsDialogSummaryInvalid,
                                            failReason);
                        }
                        else
                        {
                            // User cancelled the choice popup, reset UI
                            UpdateCacheInfo(coreConfig.DownloadCacheDir
                                            ?? JsonConfiguration.DefaultDownloadCacheDir);
                        }
                    });
                    return false;
                }
                else
                {
                    Util.Invoke(this, () =>
                    {
                        MoveCacheProgressBar.Visible = false;
                        UpdateCacheInfo(newPath);
                    });
                    return true;
                }
            });
        }

        private void UpdateCacheProgress(int percent)
        {
            Util.Invoke(CachePathTextBox, () =>
            {
                MoveCacheProgressBar.Visible = true;
                MoveCacheProgressBar.Value = percent;
                MoveCacheProgressBar.Text = string.Format(Properties.Resources.SettingsDialogCacheProgress,
                                                          percent);
            });
        }

        private void EnableDisableCachePath(bool enable)
        {
            ControlBox                    = !enable;
            BehaviourGroupBox.Enabled     = !enable;
            AuthTokensGroupBox.Enabled    = !enable;
            AutoUpdateGroupBox.Enabled    = !enable;
            RepositoryGroupBox.Enabled    = !enable;
            MoreSettingsGroupBox.Enabled  = !enable;
            CachePathTextBox.Enabled      = enable;
            CachePathEditButton.Visible   = !enable;
            CachePathSaveButton.Enabled   = true;
            CachePathSaveButton.Visible   = enable;
            CachePathCancelButton.Enabled = true;
            CachePathCancelButton.Visible = enable;
            ChangeCacheButton.Enabled     = !enable;
            ResetCacheButton.Enabled      = !enable
                                            && CachePathTextBox.Text != JsonConfiguration.DefaultDownloadCacheDir;
            ClearCacheButton.Enabled      = !enable;
            if (enable)
            {
                CachePathCancelButton.Left = CachePathSaveButton.Left - CachePathCancelButton.Width;
            }
            CachePathTextBox.Width = enable ? CachePathCancelButton.Left - CachePathTextBox.Left
                                     : CachePathEditButton.Left   - CachePathTextBox.Left;
        }

        private void CacheLimit_TextChanged(object? sender, EventArgs? e)
        {
            if (string.IsNullOrEmpty(CacheLimit.Text))
            {
                coreConfig.CacheSizeLimit = null;
            }
            else
            {
                // Translate from MB to bytes
                coreConfig.CacheSizeLimit = Convert.ToInt64(CacheLimit.Text) * 1024 * 1024;
            }
            UpdateCacheInfo(CachePathTextBox.Text);
        }

        private void CacheLimit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private async void ChangeCacheButton_Click(object? sender, EventArgs? e)
        {
            var cacheChooser = new FolderBrowserDialog()
            {
                Description         = Properties.Resources.SettingsDialogCacheDescrip,
                RootFolder          = Environment.SpecialFolder.MyComputer,
                SelectedPath        = coreConfig.DownloadCacheDir
                                      ?? JsonConfiguration.DefaultDownloadCacheDir,
                ShowNewFolderButton = true
            };
            if (cacheChooser.ShowDialog(this) == DialogResult.OK)
            {
                await TrySaveCachePath(cacheChooser.SelectedPath);
            }
        }

        private void PurgeToLimitMenuItem_Click(object? sender, EventArgs? e)
        {
            // Purge old downloads if we're over the limit
            if (coreConfig.CacheSizeLimit.HasValue
                && manager?.Cache != null
                && coreConfig.DownloadCacheDir != null)
            {
                manager.Cache.EnforceSizeLimit(coreConfig.CacheSizeLimit.Value,
                                               regMgr.registry);
                UpdateCacheInfo(coreConfig.DownloadCacheDir);
            }
        }

        private void PurgeAllMenuItem_Click(object? sender, EventArgs? e)
        {
            if (manager?.Cache != null)
            {
                manager.Cache.GetSizeInfo(out int cacheFileCount,
                                          out long cacheSize,
                                          out _);

                var deleteConfirmationDialog = new YesNoDialog();
                var confirmationText = string.Format(
                    Properties.Resources.SettingsDialogDeleteConfirm,
                    cacheFileCount,
                    CkanModule.FmtSize(cacheSize));

                if (deleteConfirmationDialog.ShowYesNoDialog(this, confirmationText) == DialogResult.Yes)
                {
                    // Tell the cache object to nuke itself
                    manager.Cache.RemoveAll();

                    if (coreConfig.DownloadCacheDir != null)
                    {
                        UpdateCacheInfo(coreConfig.DownloadCacheDir);
                    }
                }
            }
        }

        private async void ResetCacheButton_Click(object? sender, EventArgs? e)
        {
            // Reset to default cache path
            await TrySaveCachePath(JsonConfiguration.DefaultDownloadCacheDir);
        }

        private void OpenCacheButton_Click(object? sender, EventArgs? e)
        {
            Utilities.ProcessStartURL(coreConfig.DownloadCacheDir
                                      ?? JsonConfiguration.DefaultDownloadCacheDir);
        }

        #endregion

        #region Repositories

        public bool RepositoryAdded   { get; private set; } = false;
        public bool RepositoryRemoved { get; private set; } = false;
        public bool RepositoryMoved   { get; private set; } = false;

        private void RefreshReposListBox(bool        saveChanges = true,
                                         Repository? toSelect    = null)
        {
            ReposListBox.BeginUpdate();
            ReposListBox.Items.Clear();
            ReposListBox.Items.AddRange(regMgr.registry.Repositories.Values
                // SortedDictionary just sorts by name
                .OrderBy(r => r.priority)
                .Select(r => new ListViewItem(new string[]
                    {
                        r.name, r.uri?.ToString() ?? "",
                    })
                    {
                        Tag = r,
                        Selected = toSelect == r,
                    })
                .ToArray());
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            ReposListBox.Focus();
            ReposListBox.EndUpdate();
            EnableDisableRepoButtons();

            if (saveChanges)
            {
                UseWaitCursor = true;
                // Save registry in background thread to keep GUI responsive
                Task.Run(() =>
                {
                    // Visual cue that we're doing something
                    regMgr.Save();
                    Util.Invoke(this, () => UseWaitCursor = false);
                });
            }
        }

        private void ReposListBox_SelectedIndexChanged(object? sender, EventArgs? e)
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
            UpRepoButton.Enabled =
                //ReposListBox.SelectedIndices is [int i and > 0, ..]
                ReposListBox.SelectedIndices.Count > 0
                && ReposListBox.SelectedIndices[0] is int i
                && i > 0
                && i < ReposListBox.Items.Count;
            DownRepoButton.Enabled =
                //ReposListBox.SelectedIndices is [int j and >= 0, ..]
                ReposListBox.SelectedIndices.Count > 0
                && ReposListBox.SelectedIndices[0] is int j
                && j >= 0
                && j < ReposListBox.Items.Count - 1;
        }

        private void DeleteRepoButton_Click(object? sender, EventArgs? e)
        {
            YesNoDialog deleteConfirmationDialog = new YesNoDialog();
            if (//ReposListBox.SelectedItems is [{Tag: Repository repo}, ..]
                ReposListBox.SelectedItems.Count > 0
                && ReposListBox.SelectedItems[0] is {Tag: Repository repo}
                && deleteConfirmationDialog.ShowYesNoDialog(this,
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
                RepositoryRemoved = true;
            }
        }

        private void NewRepoButton_Click(object? sender, EventArgs? e)
        {
            if (manager?.CurrentInstance != null
                && RepositoryList.DefaultRepositories(manager.CurrentInstance.game, userAgent)?.repositories
                   is Repository[] repos)
            {
                var dialog = new NewRepoDialog(repos);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var repo = dialog.Selection;
                    var registry = regMgr.registry;
                    if (registry.Repositories.Values.Any(other => other.uri == repo.uri))
                    {
                        user.RaiseError(Properties.Resources.SettingsDialogRepoAddDuplicateURL, repo.uri);
                        return;
                    }
                    if (registry.Repositories.TryGetValue(repo.name, out Repository? existing))
                    {
                        repo.priority = existing.priority;
                        registry.RepositoriesRemove(repo.name);
                    }
                    else
                    {
                        repo.priority = registry.Repositories.Count;
                    }
                    registry.RepositoriesAdd(repo);
                    RepositoryAdded = true;

                    RefreshReposListBox();
                }
            }
        }

        private void UpRepoButton_Click(object? sender, EventArgs? e)
        {
            if (//ReposListBox.SelectedIndices is [> 0, ..]
                ReposListBox.SelectedIndices.Count > 0
                && ReposListBox.SelectedIndices[0] > 0
                //&& ReposListBox.SelectedItems is [{Tag: Repository selected}, ..]
                && ReposListBox.SelectedItems.Count > 0
                && ReposListBox.SelectedItems[0] is {Tag: Repository selected})
            {
                var prev = ReposListBox.Items.OfType<ListViewItem>()
                                             .Select(item => item.Tag as Repository)
                                             .OfType<Repository>()
                                             .FirstOrDefault(r => r.priority == selected.priority - 1);
                --selected.priority;
                RepositoryMoved = true;
                if (prev != null)
                {
                    ++prev.priority;
                }
                RefreshReposListBox(true, selected);
            }
        }

        private void DownRepoButton_Click(object? sender, EventArgs? e)
        {
            if (//ReposListBox.SelectedIndices is [int i, ..]
                ReposListBox.SelectedIndices.Count > 0
                && ReposListBox.SelectedIndices[0] is int i
                && i < ReposListBox.Items.Count - 1
                //&& ReposListBox.SelectedItems is [{Tag: Repository selected}]
                && ReposListBox.SelectedItems.Count > 0
                && ReposListBox.SelectedItems[0] is {Tag: Repository selected})
            {
                var next = ReposListBox.Items.Cast<ListViewItem>()
                                             .Select(item => item.Tag as Repository)
                                             .OfType<Repository>()
                                             .FirstOrDefault(r => r.priority == selected.priority + 1);
                ++selected.priority;
                RepositoryMoved = true;
                if (next != null)
                {
                    --next.priority;
                }
                RefreshReposListBox(true, selected);
            }
        }

        #endregion

        #region Auth tokens

        private void RefreshAuthTokensListBox()
        {
            AuthTokensListBox.Items.Clear();
            foreach (string host in coreConfig.GetAuthTokenHosts())
            {
                if (coreConfig.TryGetAuthToken(host, out string? token))
                {
                    AuthTokensListBox.Items.Add(new ListViewItem(
                        new string[] { host, token })
                    {
                        Tag = $"{host}|{token}"
                    });
                }
            }
            AuthTokensListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            AuthTokensListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AuthTokensListBox_SelectedIndexChanged(object? sender, EventArgs? e)
        {
            DeleteAuthTokenButton.Enabled = AuthTokensListBox.SelectedItems.Count > 0;
        }

        private void NewAuthTokenButton_Click(object? sender, EventArgs? e)
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
                AutoSize     = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                DialogResult = DialogResult.OK,
                FlatStyle    = FlatStyle.Flat,
                Name         = "okButton",
                Size         = new Size(75, 23),
                Text         = Properties.Resources.AddAuthTokenAccept,
                UseVisualStyleBackColor = true,
                Location     = new Point((newAuthTokenPopup.Width - 80 - 80) / 2, 64)
            };
            acceptButton.Click += (origin, evt) =>
            {
                newAuthTokenPopup.DialogResult = validNewAuthToken(hostTextBox.Text, tokenTextBox.Text)
                    ? DialogResult.OK
                    : DialogResult.None;
            };
            Button cancelButton = new Button()
            {
                AutoSize     = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                DialogResult = DialogResult.Cancel,
                FlatStyle    = FlatStyle.Flat,
                Name         = "cancelButton",
                Size         = new Size(75, 23),
                Text         = Properties.Resources.AddAuthTokenCancel,
                UseVisualStyleBackColor = true,
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
                    coreConfig.SetAuthToken(hostTextBox.Text, tokenTextBox.Text);
                    RefreshAuthTokensListBox();
                    break;
            }
        }

        private bool validNewAuthToken(string host, string token)
        {
            if (host.Length <= 0)
            {
                user.RaiseError(Properties.Resources.AddAuthTokenHostRequired);
                return false;
            }
            if (token.Length <= 0)
            {
                user.RaiseError(Properties.Resources.AddAuthTokenTokenRequired);
                return false;
            }
            if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
            {
                user.RaiseError(Properties.Resources.AddAuthTokenInvalidHost, host);
                return false;
            }
            if (coreConfig.TryGetAuthToken(host, out _))
            {
                user.RaiseError(Properties.Resources.AddAuthTokenDupHost, host);
                return false;
            }

            return true;
        }

        private void DeleteAuthTokenButton_Click(object? sender, EventArgs? e)
        {
            if (//AuthTokensListBox.SelectedItems is [{Tag: string item}, ..]
                AuthTokensListBox.SelectedItems.Count > 0
                && AuthTokensListBox.SelectedItems[0] is {Tag: string item}
                //&& item.Split('|') is [var firstPiece, ..]
                && item.Split('|') is string[] pieces
                && pieces.Length > 0
                && pieces[0] is string firstPiece)
            {
                var host = firstPiece.Trim();
                coreConfig.SetAuthToken(host, null);
                RefreshAuthTokensListBox();
                DeleteAuthTokenButton.Enabled = false;
            }
        }

        #endregion

        #region CKAN updates

        private void UpdateAutoUpdate()
        {
            LocalVersionLabel.Text = Meta.GetVersion();
            try
            {
                var latestVersion = updater.GetUpdate(coreConfig.DevBuilds ?? false, userAgent)
                                           .Version;
                LatestVersionLabel.Text = latestVersion?.ToString() ?? "";
                // Allow downgrading in case they want to stop using dev builds
                InstallUpdateButton.Enabled = !latestVersion?.Equals(new ModuleVersion(Meta.GetVersion())) ?? false;
            }
            catch
            {
                // Can't get the version, reset the label
                var resources = new SingleAssemblyComponentResourceManager(typeof(SettingsDialog));
                resources.ApplyResources(LatestVersionLabel,
                                         LatestVersionLabel.Name);
                InstallUpdateButton.Enabled = false;
            }
        }

        private void CheckForUpdatesButton_Click(object? sender, EventArgs? e)
        {
            try
            {
                UpdateAutoUpdate();
            }
            catch (Exception exc)
            {
                log.Warn("Exception caught in CheckForUpdates:\r\n" + exc);
            }
        }

        private void InstallUpdateButton_Click(object? sender, EventArgs? e)
        {
            if (Main.Instance?.CheckForCKANUpdate() ?? false)
            {
                Hide();
                Main.Instance.UpdateCKAN();
            }
            else
            {
                user.RaiseError(Properties.Resources.SettingsDialogUpdateFailed);
            }
        }

        private void CheckUpdateOnLaunchCheckbox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.CheckForUpdatesOnLaunch = CheckUpdateOnLaunchCheckbox.Checked;
        }

        private void DevBuildsCheckbox_CheckedChanged(object? sender, EventArgs? e)
        {
            coreConfig.DevBuilds = DevBuildsCheckbox.Checked;
            UpdateAutoUpdate();
        }

        #endregion

        #region More settings

        private void UpdateLanguageSelectionComboBox()
        {
            LanguageSelectionComboBox.Items.Clear();
            LanguageSelectionComboBox.Items.AddRange(Utilities.AvailableLanguages);
            // If the current language is supported by CKAN, set is as selected.
            // Else display a blank field.
            LanguageSelectionComboBox.SelectedIndex = LanguageSelectionComboBox.FindStringExact(coreConfig.Language);
        }

        private void RefreshOnStartupCheckbox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.RefreshOnStartup = RefreshOnStartupCheckbox.Checked;
        }

        private void HideEpochsCheckbox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.HideEpochs = HideEpochsCheckbox.Checked;
        }

        private void HideVCheckbox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.HideV = HideVCheckbox.Checked;
        }

        private void LanguageSelectionComboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            // Don't change values on scroll
            if (e is HandledMouseEventArgs me)
            {
                me.Handled = true;
            }
        }

        private void LanguageSelectionComboBox_SelectionChanged(object? sender, EventArgs? e)
        {
            coreConfig.Language = LanguageSelectionComboBox.SelectedItem?.ToString();
        }

        private void AutoSortUpdateCheckBox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.AutoSortByUpdate = AutoSortUpdateCheckBox.Checked;
        }

        private void UpdateStabilityToleranceComboBox()
        {
            StabilityToleranceComboBox.Items.Clear();
            StabilityToleranceComboBox.Items.AddRange(Enum.GetValues(typeof(ReleaseStatus))
                                                          .OfType<ReleaseStatus>()
                                                          .OrderBy(relStat => (int)relStat)
                                                          .Select(relStat => new ReleaseStatusItem(relStat))
                                                          .ToArray());
            if (manager?.CurrentInstance != null)
            {
                StabilityToleranceComboBox.SelectedIndex = (int)manager.CurrentInstance.StabilityToleranceConfig.OverallStabilityTolerance;
            }
        }

        public bool StabilityToleranceChanged { get; private set; } = false;

        private void StabilityToleranceComboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            // Don't change values on scroll
            if (e is HandledMouseEventArgs me)
            {
                me.Handled = true;
            }
        }

        private void StabilityToleranceComboBox_SelectionChanged(object? sender, EventArgs? e)
        {
            if (manager?.CurrentInstance != null
                && StabilityToleranceComboBox.SelectedItem is ReleaseStatusItem item
                && item.Value is ReleaseStatus relStat)
            {
                manager.CurrentInstance.StabilityToleranceConfig.OverallStabilityTolerance = relStat;
                StabilityToleranceChanged = true;
            }
        }

        #endregion

        #region Tray icon

        private void UpdateRefreshRate()
        {
            if (Main.Instance != null)
            {
                int rate = coreConfig.RefreshRate;
                RefreshTextBox.Text = rate.ToString();
                PauseRefreshCheckBox.Enabled = rate != 0;
                Main.Instance.pauseToolStripMenuItem.Enabled = coreConfig.RefreshRate != 0;
                Main.Instance.UpdateRefreshTimer();
            }
        }

        private void EnableTrayIconCheckBox_CheckedChanged(object? sender, EventArgs? e)
        {
            MinimizeToTrayCheckBox.Enabled = guiConfig.EnableTrayIcon = EnableTrayIconCheckBox.Checked;
            Main.Instance?.CheckTrayState();
        }

        private void MinimizeToTrayCheckBox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.MinimizeToTray = MinimizeToTrayCheckBox.Checked;
            Main.Instance?.CheckTrayState();
        }

        private void RefreshTextBox_TextChanged(object? sender, EventArgs? e)
        {
            coreConfig.RefreshRate = string.IsNullOrEmpty(RefreshTextBox.Text) ? 0 : int.Parse(RefreshTextBox.Text);
            UpdateRefreshRate();
        }

        private void RefreshTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void PauseRefreshCheckBox_CheckedChanged(object? sender, EventArgs? e)
        {
            guiConfig.RefreshPaused = PauseRefreshCheckBox.Checked;

            if (guiConfig.RefreshPaused)
            {
                Main.Instance?.refreshTimer?.Stop();
            }
            else
            {
                Main.Instance?.refreshTimer?.Start();
            }
        }

        #endregion

        private static GameInstanceManager? manager => Main.Instance?.Manager;

        private readonly IConfiguration   coreConfig;
        private readonly GUIConfiguration guiConfig;
        private readonly RegistryManager  regMgr;
        private readonly AutoUpdate       updater;
        private readonly IUser            user;
        private readonly string?          userAgent;

        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsDialog));
    }
}
