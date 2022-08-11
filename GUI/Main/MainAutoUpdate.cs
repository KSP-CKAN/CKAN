using System;
using System.ComponentModel;
using System.Windows.Forms;
using CKAN.Versioning;

namespace CKAN.GUI
{
    public partial class Main
    {
        /// <summary>
        /// Look for a CKAN update and start installing it if found.
        /// Note that this will happen on a background thread!
        /// </summary>
        /// <returns>
        /// true if update found, false otherwise.
        /// </returns>
        private bool CheckForCKANUpdate()
        {
            if (configuration.CheckForUpdatesOnLaunch && AutoUpdate.CanUpdate)
            {
                try
                {
                    log.Info("Making auto-update call");
                    AutoUpdate.Instance.FetchLatestReleaseInfo();
                    var latest_version = AutoUpdate.Instance.latestUpdate.Version;
                    var current_version = new ModuleVersion(Meta.GetVersion());

                    if (AutoUpdate.Instance.IsFetched() && latest_version.IsGreaterThan(current_version))
                    {
                        log.Debug("Found higher ckan version");
                        var release_notes = AutoUpdate.Instance.latestUpdate.ReleaseNotes;
                        var dialog = new NewUpdateDialog(latest_version.ToString(), release_notes);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            UpdateCKAN();
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    currentUser.RaiseError(Properties.Resources.MainAutoUpdateFailed, exception.Message);
                    log.Error("Error in auto-update", exception);
                }
            }
            return false;
        }

        /// <summary>
        /// Download a CKAN update and start AutoUpdater.exe, then exit.
        /// Note it will return control and then interrupt whatever is happening to exit!
        /// </summary>
        public void UpdateCKAN()
        {
            ResetProgress();
            ShowWaitDialog(false);
            SwitchEnabledState();
            Wait.ClearLog();
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainUpgradingWaitTitle);
            Wait.SetDescription(string.Format(Properties.Resources.MainUpgradingTo, AutoUpdate.Instance.latestUpdate.Version));

            log.Info("Start ckan update");
            BackgroundWorker updateWorker = new BackgroundWorker();
            updateWorker.DoWork += (sender, args) => AutoUpdate.Instance.StartUpdateProcess(true, currentUser);
            updateWorker.RunWorkerCompleted += UpdateReady;
            updateWorker.RunWorkerAsync();
        }

        private void UpdateReady(object sender, RunWorkerCompletedEventArgs e)
        {
            // Close will be cancelled if the window is still disabled
            SwitchEnabledState();
            Close();
        }

    }
}
