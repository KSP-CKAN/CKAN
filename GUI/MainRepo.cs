using System;
using System.ComponentModel;
using System.Net;
using System.Timers;
using System.Linq;
using Newtonsoft.Json;

namespace CKAN
{

    public partial class Main
    {
        private BackgroundWorker m_UpdateRepoWorker;
        public Timer refreshTimer;

        public static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            WebClient client = new WebClient();

            if (master_uri == null)
            {
                master_uri = Repository.default_repo_master_list;
            }

            string json = client.DownloadString(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        public void UpdateRepo()
        {
            var old_dialog = currentUser.displayYesNo;
            currentUser.displayYesNo = YesNoDialog;

            tabController.RenameTab("WaitTabPage", "Updating repositories");

            CurrentInstance.ScanGameData();

            try
            {
                m_UpdateRepoWorker.RunWorkerAsync();
            }
            finally
            {
                currentUser.displayYesNo = old_dialog;
            }

            Util.Invoke(this, SwitchEnabledState);

            SetDescription("Contacting repository..");
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
                AddStatusMessage("Updating repositories...");
                e.Result = Repo.UpdateAllRepositories(RegistryManager.Instance(CurrentInstance), CurrentInstance, Manager.Cache, GUI.user);
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
                errorDialog.ShowErrorDialog("Failed to connect to repository. Exception: " + ex.Message);
            }

            currentUser.displayYesNo = null;
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Result as int? ?? 0) > 0)
            {
                UpdateModsList(true, ChangeSet);
                AddStatusMessage("Repositories successfully updated.");
                ShowRefreshQuestion();
                HideWaitDialog(true);
                UpgradeNotification();
            }
            else
            {
                AddStatusMessage("Repository update failed!");
            }

            Util.Invoke(this, SwitchEnabledState);
            Util.Invoke(this, RecreateDialogs);
            Util.Invoke(this, ModList.Select);
        }

        private void ShowRefreshQuestion()
        {
            if (!configuration.RefreshOnStartupNoNag)
            {
                currentUser.displayYesNo = YesNoDialog;
                configuration.RefreshOnStartupNoNag = true;
                if (!currentUser.displayYesNo("Would you like CKAN to refresh the modlist every time it is loaded? (You can always manually refresh using the button up top.)"))
                {
                    configuration.RefreshOnStartup = false;
                }
                configuration.Save();
                currentUser.displayYesNo = null;
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
            Win32Registry winReg = new Win32Registry();

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
                        $"{numUpgradeable} update{(numUpgradeable > 1 ? "s" : "")} available",
                        $"Click to upgrade",
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
