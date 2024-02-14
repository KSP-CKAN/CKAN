using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Linq;
using System.Windows.Forms;
using System.Transactions;
using Timer = System.Timers.Timer;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Newtonsoft.Json;
using Autofac;

using CKAN.Configuration;
using CKAN.Extensions;
using CKAN.GUI.Attributes;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    using RepoArgument = Tuple<bool, bool>;
    using RepoResult   = Tuple<RepositoryDataManager.UpdateResult, Dictionary<string, bool>, bool>;

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
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

        public void UpdateRepo(bool forceFullRefresh = false, bool refreshWithoutChanges = false)
        {
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainRepoWaitTitle);

            try
            {
                Wait.StartWaiting(UpdateRepo, PostUpdateRepo, true,
                                  new RepoArgument(forceFullRefresh, refreshWithoutChanges));
            }
            catch (Exception exc)
            {
                log.Error("Failed to start repo update!", exc);
            }

            DisableMainWindow();

            Wait.SetDescription(Properties.Resources.MainRepoContacting);
            ShowWaitDialog();
        }

        [ForbidGUICalls]
        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            (bool forceFullRefresh, bool refreshWithoutChanges) = (RepoArgument)e.Argument;
            // Don't repeat this stuff if downloads fail
            currentUser.RaiseMessage(Properties.Resources.MainRepoScanning);
            log.Debug("Scanning before repo update");
            var regMgr = RegistryManager.Instance(CurrentInstance, repoData);
            bool scanChanged = regMgr.ScanUnmanagedFiles();

            // Note the current mods' compatibility for the NewlyCompatible filter
            var registry = regMgr.registry;

            // Load cached data with progress bars instead of without if not already loaded
            // (which happens if auto-update is enabled, otherwise this is a no-op).
            // We need the old data to alert the user of newly compatible modules after update.
            repoData.Prepopulate(
                registry.Repositories.Values.ToList(),
                new Progress<int>(p => currentUser.RaiseProgress(Properties.Resources.LoadingCachedRepoData, p)));

            var versionCriteria = CurrentInstance.VersionCriteria();
            var oldModules = registry.CompatibleModules(versionCriteria)
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
                            var repo = repos.Where(r => target.urls.Contains(r.uri))
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

                        currentUser.RaiseMessage(Properties.Resources.MainRepoUpdating);

                        var updateResult = repoData.Update(repos, CurrentInstance.game,
                                                           forceFullRefresh, downloader, currentUser);

                        if (canceled)
                        {
                            throw new CancelledActionKraken();
                        }

                        if (updateResult == RepositoryDataManager.UpdateResult.NoChanges && scanChanged)
                        {
                            updateResult = RepositoryDataManager.UpdateResult.Updated;
                        }
                        e.Result = new RepoResult(updateResult, oldModules, refreshWithoutChanges);

                        // If we make it to the end, we are done
                        transaction.Complete();
                        return;
                    }
                    catch (DownloadErrorsKraken k)
                    {
                        log.Debug("Caught download errors kraken");
                        DownloadsFailedDialog dfd = null;
                        Util.Invoke(this, () =>
                        {
                            dfd = new DownloadsFailedDialog(
                                Properties.Resources.RepoDownloadsFailedMessage,
                                Properties.Resources.RepoDownloadsFailedColHdr,
                                Properties.Resources.RepoDownloadsFailedAbortBtn,
                                k.Exceptions.Select(kvp => new KeyValuePair<object[], Exception>(
                                    new object[] { repos[kvp.Key] }, kvp.Value)),
                                // Rows are only linked to themselves
                                (r1, r2) => r1 == r2);
                            dfd.ShowDialog(this);
                        });
                        var skip  = dfd.Wait()?.Select(r => r as Repository).ToArray();
                        var abort = dfd.Abort;
                        dfd.Dispose();
                        if (abort)
                        {
                            e.Result = new RepoResult(RepositoryDataManager.UpdateResult.Failed,
                                                      oldModules, refreshWithoutChanges);
                            throw new CancelledActionKraken();
                        }
                        if (skip.Length > 0)
                        {
                            foreach (var r in skip)
                            {
                                registry.RepositoriesRemove(r.name);
                                needRegistrySave = true;
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

                    case TransactionException texc:
                        // "Failed to roll back" is useless by itself,
                        // so show all inner exceptions too
                        foreach (var exc in texc.TraverseNodes<Exception>(ex => ex.InnerException)
                                                .Reverse())
                        {
                            log.Error(exc.Message, exc);
                            currentUser.RaiseMessage(exc.Message);
                        }
                        currentUser.RaiseMessage(Properties.Resources.MainRepoFailed);
                        Wait.Finish();
                        EnableMainWindow();
                        break;

                    case AggregateException exc:
                        foreach (var inner in exc.InnerExceptions
                                                 .SelectMany(inner =>
                                                     inner.TraverseNodes(ex => ex.InnerException)
                                                          .Reverse()))
                        {
                            log.Error(inner.Message, inner);
                            currentUser.RaiseMessage(inner.Message);
                        }
                        currentUser.RaiseMessage(Properties.Resources.MainRepoFailed);
                        Wait.Finish();
                        EnableMainWindow();
                        break;

                    case Exception exc:
                        log.Error(exc.Message, exc);
                        currentUser.RaiseMessage(exc.Message);
                        currentUser.RaiseMessage(Properties.Resources.MainRepoFailed);
                        Wait.Finish();
                        EnableMainWindow();
                        break;
                }
            }
            else
            {
                (RepositoryDataManager.UpdateResult updateResult,
                 Dictionary<string, bool>           oldModules,
                 bool                               refreshWithoutChanges) = (RepoResult)e.Result;

                switch (updateResult)
                {
                    case RepositoryDataManager.UpdateResult.NoChanges:
                        currentUser.RaiseMessage(Properties.Resources.MainRepoUpToDate);
                        // Reload rows if user added a cached repo repo
                        if (refreshWithoutChanges)
                        {
                            RefreshModList(false, oldModules);
                        }
                        else
                        {
                            // Nothing changed, just go back
                            HideWaitDialog();
                            EnableMainWindow();
                            Util.Invoke(this, ManageMods.ModGrid.Select);
                        }
                        break;


                    case RepositoryDataManager.UpdateResult.OutdatedClient:
                        currentUser.RaiseMessage(Properties.Resources.MainRepoOutdatedClient);
                        if (CheckForCKANUpdate())
                        {
                            UpdateCKAN();
                        }
                        else
                        {
                            // No update available or user said no. Proceed as normal.
                            ShowRefreshQuestion();
                            UpgradeNotification();
                            RefreshModList(false, oldModules);
                        }
                        break;

                    case RepositoryDataManager.UpdateResult.Updated:
                    default:
                        currentUser.RaiseMessage(Properties.Resources.MainRepoSuccess);
                        ShowRefreshQuestion();
                        UpgradeNotification();
                        RefreshModList(false, oldModules);
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
                        ToolTipIcon.Info
                    );
                });
            }
        }
    }
}
