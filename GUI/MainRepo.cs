using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Timers;
using System.Linq;
using Newtonsoft.Json;
using CKAN.Versioning;
using CKAN.Win32Registry;
using Autofac;

namespace CKAN
{

    public partial class Main
    {
        private BackgroundWorker m_UpdateRepoWorker;
        public Timer refreshTimer;

        public static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            if (master_uri == null)
            {
                master_uri = Repository.default_repo_master_list;
            }

            string json = Net.DownloadText(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        public void UpdateRepo()
        {
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainRepoWaitTitle);

            try
            {
                m_UpdateRepoWorker.RunWorkerAsync();
            }
            catch { }

            Util.Invoke(this, SwitchEnabledState);

            SetDescription(Properties.Resources.MainRepoContacting);
            ClearLog();
            ShowWaitDialog();
        }

        private bool _enabled = true;
        private void SwitchEnabledState()
        {
            _enabled = !_enabled;
            menuStrip1.Enabled = _enabled;
            MainTabControl.Enabled = _enabled;
            /* Windows (7 & 8 only?) bug #1548 has extra facets.
             * parent.childcontrol.Enabled = false seems to disable the parent,
             * if childcontrol had focus. Depending on optimization steps,
             * parent.childcontrol.Enabled = true does not necessarily
             * re-enable the parent.*/
            if (_enabled)
                this.Focus();
        }


        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            try
            {
                AddStatusMessage(Properties.Resources.MainRepoScanning);
                bool scanChanged = CurrentInstance.ScanGameData();
    
                AddStatusMessage(Properties.Resources.MainRepoUpdating);

                // Note the current mods' compatibility for the NewlyCompatible filter
                KspVersionCriteria versionCriteria = CurrentInstance.VersionCriteria();
                IRegistryQuerier registry = RegistryManager.Instance(CurrentInstance).registry;
                Dictionary<string, bool> oldModules = registry.Available(versionCriteria)
                    .ToDictionary(m => m.identifier, m => false);
                registry.Incompatible(versionCriteria)
                    .Where(m => !oldModules.ContainsKey(m.identifier))
                    .ToList()
                    .ForEach(m => oldModules.Add(m.identifier, true));

                RepoUpdateResult result = Repo.UpdateAllRepositories(
                    RegistryManager.Instance(CurrentInstance),
                    CurrentInstance, Manager.Cache, GUI.user);
                if (result == RepoUpdateResult.NoChanges && scanChanged)
                {
                    result = RepoUpdateResult.Updated;
                }
                e.Result = new KeyValuePair<RepoUpdateResult, Dictionary<string, bool>>(
                    result, oldModules);
            }
            catch (UriFormatException ex)
            {
                errorDialog.ShowErrorDialog(ex.Message);
            }
            catch (MissingCertificateKraken ex)
            {
                errorDialog.ShowErrorDialog(ex.ToString());
            }
            catch (Exception ex)
            {
                errorDialog.ShowErrorDialog(string.Format(Properties.Resources.MainRepoFailedToConnect, ex.Message));
            }
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            var resultPair = e.Result as KeyValuePair<RepoUpdateResult, Dictionary<string, bool>>?;
            RepoUpdateResult? result = resultPair?.Key;
            Dictionary<string, bool> oldModules = resultPair?.Value;

            switch (result)
            {
                case RepoUpdateResult.NoChanges:
                    AddStatusMessage(Properties.Resources.MainRepoUpToDate);
                    HideWaitDialog(true);
                    // Load rows if grid empty, otherwise keep current
                    if (ModList.Rows.Count < 1)
                    {
                        UpdateModsList(ChangeSet);
                    }
                    break;

                case RepoUpdateResult.Failed:
                    AddStatusMessage(Properties.Resources.MainRepoFailed);
                    break;

                case RepoUpdateResult.Updated:
                default:
                    UpdateModsList(ChangeSet, oldModules);
                    AddStatusMessage(Properties.Resources.MainRepoSuccess);
                    ShowRefreshQuestion();
                    HideWaitDialog(true);
                    UpgradeNotification();
                    break;
            }

            Util.Invoke(this, SwitchEnabledState);
            Util.Invoke(this, RecreateDialogs);
            Util.Invoke(this, ModList.Select);
        }

        private void ShowRefreshQuestion()
        {
            if (!configuration.RefreshOnStartupNoNag)
            {
                configuration.RefreshOnStartupNoNag = true;
                if (!currentUser.RaiseYesNoDialog(Properties.Resources.MainRepoAutoRefreshPrompt))
                {
                    configuration.RefreshOnStartup = false;
                }
                configuration.Save();
            }
        }

        public void InitRefreshTimer()
        {
            if (refreshTimer == null)
            {
                refreshTimer = new Timer
                {
                    AutoReset = true,
                    Enabled = true
                };
                refreshTimer.Elapsed += OnRefreshTimer;
            }
            UpdateRefreshTimer();
        }

        public void UpdateRefreshTimer()
        {
            refreshTimer.Stop();
            IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();

            // Interval is set to 1 minute * RefreshRate
            if (winReg.RefreshRate > 0)
            {
                refreshTimer.Interval = 1000 * 60 * winReg.RefreshRate;
                refreshTimer.Start();
            }
        }

        private void OnRefreshTimer(object sender, ElapsedEventArgs e)
        {
            if (!configuration.RefreshPaused)
            {
                // Just a safety check
                UpdateRepo();
            }
        }

        private void UpgradeNotification()
        {
            int numUpgradeable = mainModList.Modules.Count(mod => mod.HasUpdate);
            if (numUpgradeable > 0)
            {
                Util.Invoke(this, () =>
                {
                    minimizeNotifyIcon.ShowBalloonTip(
                        10000,
                        string.Format(Properties.Resources.MainRepoBalloonTipDetails, numUpgradeable),
                        Properties.Resources.MainRepoBalloonTipTooltip,
                        System.Windows.Forms.ToolTipIcon.Info
                    );
                });
            }
        }

        private void minimizeNotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // Unminimize
            OpenWindow();

            // Check all the upgrade checkboxes
            MarkAllUpdatesToolButton_Click(null, null);

            // Click Apply
            ApplyToolButton_Click(null, null);

            // Click Continue
            ConfirmChangesButton_Click(null, null);
        }

    }
}
