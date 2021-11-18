using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CKAN.GUI
{
    public partial class Main
    {
        private BackgroundWorker          cacheWorker;
        private NetAsyncModulesDownloader downloader;

        private void ModInfo_OnDownloadClick(GUIMod gmod)
        {
            StartDownload(gmod);
        }

        public void StartDownload(GUIMod module)
        {
            if (module == null || !module.IsCKAN)
                return;

            if (cacheWorker == null)
            {
                cacheWorker = new BackgroundWorker()
                {
                    WorkerReportsProgress      = true,
                    WorkerSupportsCancellation = true,
                };
                cacheWorker.DoWork += CacheMod;
                cacheWorker.RunWorkerCompleted += PostModCaching;
            }

            Main.Instance.ShowWaitDialog(false);
            if (cacheWorker.IsBusy)
            {
                Task.Factory.StartNew(() =>
                {
                    // Just pass to the existing worker
                    downloader.DownloadModules(new List<CkanModule> { module.ToCkanModule() });
                    module.UpdateIsCached();
                });
            }
            else
            {
                // Start up a new worker
                downloader = new NetAsyncModulesDownloader(Main.Instance.currentUser, Main.Instance.Manager.Cache);
                cacheWorker.RunWorkerAsync(module);
            }
        }

        // cacheWorker.DoWork
        private void CacheMod(object sender, DoWorkEventArgs e)
        {
            ResetProgress();
            Wait.ClearLog();

            GUIMod gm = e.Argument as GUIMod;
            downloader.DownloadModules(new List<CkanModule> { gm.ToCkanModule() });
            e.Result = e.Argument;
        }

        // cacheWorker.RunWorkerCompleted
        public void PostModCaching(object sender, RunWorkerCompletedEventArgs e)
        {
            Util.Invoke(this, () => _PostModCaching((GUIMod)e.Result));
        }

        private void _PostModCaching(GUIMod module)
        {
            module.UpdateIsCached();
            HideWaitDialog(true);
            // User might have selected another row. Show current in tree.
            UpdateModContentsTree(ModInfo.SelectedModule.ToCkanModule(), true);
        }
    }
}
