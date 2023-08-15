using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

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
                return;

            ShowWaitDialog();
            if (downloader != null)
            {
                Task.Factory.StartNew(() =>
                {
                    // Just pass to the existing worker
                    downloader.DownloadModules(new List<CkanModule> { module.ToCkanModule() });
                    UpdateCachedByDownloads(module);
                });
            }
            else
            {
                // Start up a new worker
                Wait.StartWaiting(CacheMod, PostModCaching, true, module);
            }
        }

        private void CacheMod(object sender, DoWorkEventArgs e)
        {
            ResetProgress();

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
                Util.Invoke(this, () => _PostModCaching((GUIMod)e.Result));
            }
        }

        private void UpdateCachedByDownloads(GUIMod module)
        {
            // Update all mods that share the same ZIP
            var allGuiMods = ManageMods.AllGUIMods();
            foreach (var otherMod in module.ToModule().GetDownloadsGroup(
                allGuiMods.Values.Select(guiMod => guiMod.ToModule())))
            {
                allGuiMods[otherMod.identifier].UpdateIsCached();
            }
        }

        private void _PostModCaching(GUIMod module)
        {
            UpdateCachedByDownloads(module);

            // Reapply searches in case is:cached or not:cached is active
            ManageMods.UpdateFilters();

            // User might have selected another row. Show current in tree.
            RefreshModContentsTree();

            // Close progress tab and switch back to mod list
            HideWaitDialog();
            EnableMainWindow();
        }
    }
}
