using System;
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
                    module.UpdateIsCached();
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
            Wait.OnCancel += downloader.CancelDownload;
            downloader.DownloadModules(new List<CkanModule> { gm.ToCkanModule() });
            e.Result = e.Argument;
        }

        public void PostModCaching(object sender, RunWorkerCompletedEventArgs e)
        {
            downloader = null;
            // Can't access e.Result if there's an error
            if (e.Error != null)
            {
                switch (e.Error)
                {

                    case CancelledActionKraken exc:
                        // User already knows they cancelled, get out
                        HideWaitDialog(false);
                        tabController.SetTabLock(false);
                        Util.Invoke(this, () => Enabled = true);
                        Util.Invoke(menuStrip1, () => menuStrip1.Enabled = true);
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

        private void _PostModCaching(GUIMod module)
        {
            module.UpdateIsCached();
            // Update mod list in case is:cached or not:cached filters are active
            ManageMods_OnRefresh();
            // User might have selected another row. Show current in tree.
            UpdateModContentsTree(ModInfo.SelectedModule.ToCkanModule(), true);
        }
    }
}
