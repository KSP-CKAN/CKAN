using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Linq;
using Newtonsoft.Json;
using CKAN.Versioning;
using CKAN.Configuration;
using Autofac;

namespace CKAN.GUI
{
    public partial class Main
    {
        public Timer refreshTimer;

        public RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            if (master_uri == null)
            {
                master_uri = CurrentInstance.game.RepositoryListURL;
            }

            string json = Net.DownloadText(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        public void UpdateRepo()
        {
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainRepoWaitTitle);

            try
            {
                Wait.StartWaiting(UpdateRepo, PostUpdateRepo, true, null);
            }
            catch { }

            DisableMainWindow();

            Wait.SetDescription(Properties.Resources.MainRepoContacting);
            ShowWaitDialog();
        }

        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            // Don't repeat this stuff if downloads fail
            AddStatusMessage(Properties.Resources.MainRepoScanning);
            log.Debug("Scanning before repo update");
            bool scanChanged = CurrentInstance.Scan();

            AddStatusMessage(Properties.Resources.MainRepoUpdating);

            // Note the current mods' compatibility for the NewlyCompatible filter
            GameVersionCriteria versionCriteria = CurrentInstance.VersionCriteria();
            var registry = RegistryManager.Instance(CurrentInstance).registry;
            Dictionary<string, bool> oldModules = registry.CompatibleModules(versionCriteria)
                .ToDictionary(m => m.identifier, m => false);
            registry.IncompatibleModules(versionCriteria)
                .Where(m => !oldModules.ContainsKey(m.identifier))
                .ToList()
                .ForEach(m => oldModules.Add(m.identifier, true));

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                // Only way out is to return or throw
                while (true)
                {
                    var repos = registry.Repositories.Values.ToArray();
                    try
                    {
                        bool canceled = false;
                        var downloader = new NetAsyncDownloader(currentUser);
                        downloader.Progress += (target, remaining, total) =>
                        {
                            var repo = repos
                                .Where(r => r.uri == target.url)
                                .FirstOrDefault();
                            if (repo != null)
                            {
                                Wait.SetProgress(repo.name, remaining, total);
                            }
                        };
                        Wait.OnCancel += () =>
                        {
                            canceled = true;
                            downloader.CancelDownload();
                        };

                        RepoUpdateResult result = Repo.UpdateAllRepositories(
                            RegistryManager.Instance(CurrentInstance),
                            CurrentInstance, downloader, Manager.Cache, currentUser);

                        if (canceled)
                        {
                            throw new CancelledActionKraken();
                        }

                        if (result == RepoUpdateResult.NoChanges && scanChanged)
                        {
                            result = RepoUpdateResult.Updated;
                        }
                        e.Result = new KeyValuePair<RepoUpdateResult, Dictionary<string, bool>>(
                            result, oldModules);

                        // If we make it to the end, we are done
                        transaction.Complete();
                        return;
                    }
                    catch (DownloadErrorsKraken k)
                    {
                        var dfd = new DownloadsFailedDialog(
                            Properties.Resources.RepoDownloadsFailedMessage,
                            Properties.Resources.RepoDownloadsFailedColHdr,
                            Properties.Resources.RepoDownloadsFailedAbortBtn,
                            k.Exceptions.Select(kvp => new KeyValuePair<object[], Exception>(
                                new object[] { repos[kvp.Key] }, kvp.Value)),
                            // Rows are only linked to themselves
                            (r1, r2) => r1 == r2);
                        dfd.ShowDialog(this);
                        var abort = dfd.Abort;
                        var skip  = dfd.Skip.Select(r => r as Repository).ToArray();
                        dfd.Dispose();
                        if (abort)
                        {
                            e.Result = new KeyValuePair<RepoUpdateResult, Dictionary<string, bool>>(
                                RepoUpdateResult.Failed, oldModules);
                            throw new CancelledActionKraken();
                        }
                        if (skip.Length > 0)
                        {
                            foreach (var r in skip)
                            {
                                registry.Repositories.Remove(r.name);
                            }
                        }

                        // Loop back around to retry
                    }
                }
            }
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                switch (e.Error)
                {
                    case CancelledActionKraken k:
                        HideWaitDialog();
                        EnableMainWindow();
                        break;

                    case ReinstallModuleKraken rmk:
                        // Re-enable the UI for the install flow
                        EnableMainWindow();
                        Wait.StartWaiting(InstallMods, PostInstallMods, true,
                            new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                                rmk.Modules
                                    .Select(m => new ModChange(m, GUIModChangeType.Update, null))
                                    .ToList(),
                                RelationshipResolver.DependsOnlyOpts()
                            )
                        );
                        // Don't mess with the UI, let the install flow control it
                        break;

                    case Exception exc:
                        AddStatusMessage(Properties.Resources.MainRepoFailed);
                        currentUser.RaiseMessage(exc.Message);
                        Wait.Finish();
                        EnableMainWindow();
                        break;
                }
            }
            else
            {
                var resultPair = e.Result as KeyValuePair<RepoUpdateResult, Dictionary<string, bool>>?;
                RepoUpdateResult? result = resultPair?.Key;
                Dictionary<string, bool> oldModules = resultPair?.Value;

                switch (result)
                {
                    case RepoUpdateResult.NoChanges:
                        AddStatusMessage(Properties.Resources.MainRepoUpToDate);
                        HideWaitDialog();
                        // Load rows if grid empty, otherwise keep current
                        if (ManageMods.ModGrid.Rows.Count < 1)
                        {
                            RefreshModList();
                        }
                        else
                        {
                            EnableMainWindow();
                            Util.Invoke(this, ManageMods.ModGrid.Select);
                        }
                        SetupDefaultSearch();
                        break;

                    case RepoUpdateResult.Updated:
                    default:
                        AddStatusMessage(Properties.Resources.MainRepoSuccess);
                        ShowRefreshQuestion();
                        UpgradeNotification();
                        EnableMainWindow();
                        RefreshModList(oldModules);
                        break;
                }
            }
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
            IConfiguration cfg = ServiceLocator.Container.Resolve<IConfiguration>();

            // Interval is set to 1 minute * RefreshRate
            if (cfg.RefreshRate > 0)
            {
                refreshTimer.Interval = 1000 * 60 * cfg.RefreshRate;
                refreshTimer.Start();
            }
        }

        private void OnRefreshTimer(object sender, ElapsedEventArgs e)
        {
            if (menuStrip1.Enabled && !configuration.RefreshPaused)
            {
                // Just a safety check
                UpdateRepo();
            }
        }

        private void UpgradeNotification()
        {
            int numUpgradeable = ManageMods.mainModList.Modules.Count(mod => mod.HasUpdate);
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
    }
}
