using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public partial class Main
    {
        private NetAsyncModulesDownloader? downloader;

        private void ModInfo_OnDownloadClick(GUIMod gmod)
        {
            StartDownload(gmod);
        }

        public void StartDownloads(IEnumerable<GUIMod> modules)
        {
            ShowWaitDialog();
            if (downloader != null)
            {
                Task.Run(() =>
                {
                    // Just pass to the existing worker
                    downloader.DownloadModules(modules.Select(m => m.ToModule()));
                });
            }
            else
            {
                // Start up a new worker
                Wait.StartWaiting(CacheMods, PostModCaching, true, modules.ToArray());
            }
        }

        public void StartDownload(GUIMod module)
        {
            StartDownloads(Enumerable.Repeat(module, 1));
        }

        [ForbidGUICalls]
        private void CacheMods(object? sender, DoWorkEventArgs? e)
        {
            if (e != null
                && e.Argument is ICollection<GUIMod> modules
                && Manager?.Cache != null)
            {
                var cancelTokenSrc = new CancellationTokenSource();
                Wait.OnCancel += cancelTokenSrc.Cancel;
                downloader = new NetAsyncModulesDownloader(currentUser, Manager.Cache, userAgent,
                                                           cancelTokenSrc.Token);
                downloader.DownloadProgress += OnModDownloading;
                downloader.StoreProgress    += OnModValidating;
                downloader.OverallDownloadProgress += currentUser.RaiseProgress;
                for (bool done = false; !done; )
                {
                    try
                    {
                        downloader.DownloadModules(modules.Select(m => m.ToModule()));
                        done = true;
                    }
                    catch (ModuleDownloadErrorsKraken k)
                    {
                        DownloadsFailedDialog? dfd = null;
                        Util.Invoke(this, () =>
                        {
                            dfd = new DownloadsFailedDialog(
                                Properties.Resources.ModDownloadsFailedMessage,
                                Properties.Resources.ModDownloadsFailedColHdr,
                                Properties.Resources.ModDownloadsFailedAbortBtn,
                                k.Exceptions.Select(kvp => new KeyValuePair<object[], Exception>(
                                    modules.Select(m => m.ToModule()).ToArray(), kvp.Value)),
                                (m1, m2) => (m1 as CkanModule)?.download == (m2 as CkanModule)?.download);
                             dfd.ShowDialog(this);
                        });
                        var skip  = (dfd?.Wait()?.OfType<CkanModule>() ?? Enumerable.Empty<CkanModule>())
                                                 .ToArray();
                        var abort = dfd?.Abort ?? false;
                        dfd?.Dispose();
                        if (abort || skip.Length > 0)
                        {
                            throw new CancelledActionKraken();
                        }
                    }
                }
                e.Result = e.Argument;
            }
        }

        public void PostModCaching(object? sender, RunWorkerCompletedEventArgs? e)
        {
            if (downloader != null)
            {
                downloader = null;
            }
            // Can't access e.Result if there's an error
            if (e?.Error != null)
            {
                switch (e.Error)
                {

                    case CancelledActionKraken:
                        // User already knows they cancelled, get out
                        HideWaitDialog();
                        EnableMainWindow();
                        break;

                    default:
                        FailWaitDialog(Properties.Resources.DownloadFailed,
                                       e.Error.Message,
                                       Properties.Resources.DownloadFailed);
                        break;

                }
            }
            else
            {
                // Close progress tab and switch back to mod list
                HideWaitDialog();
                EnableMainWindow();
                ModInfo.SwitchTab("ContentTabPage");
            }
        }

        [ForbidGUICalls]
        private void UpdateCachedByDownloads(CkanModule? module)
        {
            var allGuiMods = ManageMods.AllGUIMods();
            var affectedMods =
                module?.GetDownloadsGroup(allGuiMods.Values
                                                    .Select(guiMod => guiMod.ToModule())
                                                    .OfType<CkanModule>())
                       .Select(other => allGuiMods[other.identifier])
                      ?? allGuiMods.Values;
            foreach (var otherMod in affectedMods)
            {
                otherMod.UpdateIsCached();
            }
        }

        [ForbidGUICalls]
        private void OnCacheChanged(NetModuleCache? prev)
        {
            if (prev != null)
            {
                prev.ModStored -= OnModStoredOrPurged;
                prev.ModPurged -= OnModStoredOrPurged;
            }
            if (Manager.Cache != null)
            {
                Manager.Cache.ModStored += OnModStoredOrPurged;
                Manager.Cache.ModPurged += OnModStoredOrPurged;
            }
            UpdateCachedByDownloads(null);
        }

        [ForbidGUICalls]
        private void OnModStoredOrPurged(CkanModule? module)
        {
            UpdateCachedByDownloads(module);
        }
    }
}
