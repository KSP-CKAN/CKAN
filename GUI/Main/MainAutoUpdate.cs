using System;
using System.ComponentModel;
using System.Windows.Forms;

using Autofac;

using CKAN.Configuration;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void AutoUpdatePrompts(IConfiguration   coreConfig,
                                       GUIConfiguration guiConfig)
        {
            if (!guiConfig.CheckForUpdatesOnLaunchNoNag && AutoUpdate.CanUpdate)
            {
                log.Debug("Asking user if they wish for auto-updates");
                if (new AskUserForAutoUpdatesDialog().ShowDialog(this) == DialogResult.OK)
                {
                    guiConfig.CheckForUpdatesOnLaunch = true;
                }
                guiConfig.CheckForUpdatesOnLaunchNoNag = true;
            }

            if (!coreConfig.DevBuilds.HasValue && guiConfig.CheckForUpdatesOnLaunch)
            {
                coreConfig.DevBuilds = !YesNoDialog(Properties.Resources.MainReleasesOrDevBuildsPrompt,
                                                    Properties.Resources.MainReleasesOrDevBuildsYes,
                                                    Properties.Resources.MainReleasesOrDevBuildsNo);
                if (coreConfig.DevBuilds.Value && Platform.IsWindows)
                {
                    // Tell Windows users about malware scanner's false positives
                    // and how to disable it, if they feel safe doing it
                    Utilities.ProcessStartURL(HelpURLs.WindowsDevBuilds);
                }
            }
        }

        /// <summary>
        /// Look for a CKAN update and start installing it if found.
        /// Note that this will happen on a background thread!
        /// </summary>
        /// <returns>
        /// true if update found, false otherwise.
        /// </returns>
        public bool CheckForCKANUpdate()
        {
            if (AutoUpdate.CanUpdate)
            {
                try
                {
                    log.Info("Making auto-update call");
                    var mainConfig = ServiceLocator.Container.Resolve<IConfiguration>();
                    var update = updater.GetUpdate(mainConfig.DevBuilds ?? false);
                    var latestVersion = update.Version;

                    if (latestVersion.IsGreaterThan(Meta.ReleaseVersion))
                    {
                        log.DebugFormat("Found higher CKAN version: {0}", latestVersion);
                        var releaseNotes = update.ReleaseNotes;
                        var dialog = new NewUpdateDialog(latestVersion.ToString(), releaseNotes);
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    currentUser.RaiseError(Properties.Resources.MainAutoUpdateFailed,
                                           exception.Message);
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
            ShowWaitDialog();
            DisableMainWindow();
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainUpgradingWaitTitle);
            var mainConfig = ServiceLocator.Container.Resolve<IConfiguration>();
            var update = updater.GetUpdate(mainConfig.DevBuilds ?? false);
            Wait.SetDescription(string.Format(Properties.Resources.MainUpgradingTo,
                                              update.Version));

            log.Info("Starting CKAN update");
            Wait.StartWaiting((sender, args) => updater.StartUpdateProcess(true, mainConfig.DevBuilds ?? false, currentUser),
                              UpdateReady,
                              false,
                              null);
        }

        private void UpdateReady(object sender, RunWorkerCompletedEventArgs e)
        {
            // Close will be cancelled if the window is still disabled
            EnableMainWindow();
            Close();
        }

    }
}
