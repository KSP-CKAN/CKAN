using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public partial class Main
    {
        private NetAsyncModulesDownloader downloader;

        private void ModInfo_OnDownloadClick(GUIMod gmod)
        {
            StartDownload(gmod);
        }

        public void StartDownload(GUIMod module)
        {
            if (module == null || !module.IsCKAN)
            {
                return;
            }

            ShowWaitDialog();
            if (downloader != null)
            {
                Task.Factory.StartNew(() =>
                {
                    // Just pass to the existing worker
                    downloader.DownloadModules(new List<CkanModule> { module.ToCkanModule() });
                });
            }
            else
            {
                // Start up a new worker
                Wait.StartWaiting(CacheMod, PostModCaching, true, module);
            }
        }

        [ForbidGUICalls]
        private void CacheMod(object sender, DoWorkEventArgs e)
        {
            GUIMod gm = e.Argument as GUIMod;
            downloader = new NetAsyncModulesDownloader(currentUser, Manager.Cache);
            downloader.Progress      += Wait.SetModuleProgress;
            downloader.AllComplete   += Wait.DownloadsComplete;
            downloader.StoreProgress += (module, remaining, total) =>
                Wait.SetProgress(string.Format(Properties.Resources.ValidatingDownload, module),
                    remaining, total);
            Wait.OnCancel += downloader.CancelDownload;
            downloader.DownloadModules(new List<CkanModule> { gm.ToCkanModule() });
            e.Result = e.Argument;
        }

        public void PostModCaching(object sender, RunWorkerCompletedEventArgs e)
        {
            Wait.OnCancel -= downloader.CancelDownload;
            downloader = null;
            // Can't access e.Result if there's an error
            if (e.Error != null)
            {
                switch (e.Error)
                {

                    case CancelledActionKraken exc:
                        // User already knows they cancelled, get out
                        HideWaitDialog();
                        EnableMainWindow();
                        break;

                    default:
                        FailWaitDialog(Properties.Resources.DownloadFailed,
                                       e.Error.ToString(),
                                       Properties.Resources.DownloadFailed);
                        break;

                }
            }
            else
            {
                // Close progress tab and switch back to mod list
                HideWaitDialog();
                EnableMainWindow();
            }
        }

        [ForbidGUICalls]
        private void UpdateCachedByDownloads(CkanModule module)
        {
            var allGuiMods = ManageMods.AllGUIMods();
            var affectedMods =
                module?.GetDownloadsGroup(allGuiMods.Values
                                                    .Select(guiMod => guiMod.ToModule())
                                                    .Where(mod => mod != null))
                       .Select(other => allGuiMods[other.identifier])
                ?? allGuiMods.Values;
            foreach (var otherMod in affectedMods)
            {
                otherMod.UpdateIsCached();
            }
        }

        [ForbidGUICalls]
        private void OnModStoredOrPurged(CkanModule module)
        {
            UpdateCachedByDownloads(module);

            // Reapply searches in case is:cached or not:cached is active
            ManageMods.UpdateFilters();

            if (module == null
                || ModInfo.SelectedModule?.Identifier == module.identifier)
            {
                ModInfo.RefreshModContentsTree();
            }
        }
    }
}
