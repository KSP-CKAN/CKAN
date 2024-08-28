using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;

using Autofac;

using CKAN.Configuration;

namespace CKAN.GUI
{
    public partial class Main
    {
        #region Tray Behaviour

        public void CheckTrayState()
        {
            if (configuration != null)
            {
                pauseToolStripMenuItem.Enabled = ServiceLocator.Container.Resolve<IConfiguration>().RefreshRate != 0;
                pauseToolStripMenuItem.Text = configuration.RefreshPaused
                    ? Properties.Resources.MainTrayIconResume
                    : Properties.Resources.MainTrayIconPause;
                UpdateTrayState();
            }
        }

        private void UpdateTrayState()
        {
            if (configuration != null && configuration.EnableTrayIcon)
            {
                minimizeNotifyIcon.Visible = true;

                if (WindowState == FormWindowState.Minimized)
                {
                    if (configuration.MinimizeToTray)
                    {
                        // Remove our taskbar entry
                        Hide();
                    }
                }
                else
                {
                    // Save the window state
                    configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized;
                }
            }
            else
            {
                minimizeNotifyIcon.Visible = false;
            }
        }

        private void UpdateTrayInfo()
        {
            var count = ManageMods.mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable);

            if (count == 0)
            {
                updatesToolStripMenuItem.Enabled = false;
                updatesToolStripMenuItem.Text = Properties.Resources.MainTrayNoUpdates;
            }
            else
            {
                updatesToolStripMenuItem.Enabled = true;
                updatesToolStripMenuItem.Text = string.Format(Properties.Resources.MainTrayUpdatesAvailable, count);
            }
        }

        /// <summary>
        /// Open the GUI and set it to the correct state.
        /// </summary>
        public void OpenWindow()
        {
            Show();
            WindowState = configuration?.IsWindowMaximised ?? false ? FormWindowState.Maximized : FormWindowState.Normal;
        }

        private void minimizeNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenWindow();
        }

        private void updatesToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            OpenWindow();
            ManageMods.MarkAllUpdates();
        }

        private void refreshToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            UpdateRepo();
        }

        private void pauseToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (configuration != null)
            {
                configuration.RefreshPaused = !configuration.RefreshPaused;
                if (configuration.RefreshPaused)
                {
                    refreshTimer?.Stop();
                    pauseToolStripMenuItem.Text = Properties.Resources.MainTrayIconResume;
                }
                else
                {
                    refreshTimer?.Start();
                    pauseToolStripMenuItem.Text = Properties.Resources.MainTrayIconPause;
                }
            }
        }

        private void openCKANToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            OpenWindow();
        }

        private void cKANSettingsToolStripMenuItem1_Click(object? sender, EventArgs? e)
        {
            OpenWindow();
            if (configuration != null && CurrentInstance != null)
            {
                new SettingsDialog(ServiceLocator.Container.Resolve<IConfiguration>(),
                                   configuration,
                                   RegistryManager.Instance(CurrentInstance, repoData),
                                   updater,
                                   currentUser)
                    .ShowDialog(this);
            }
        }

        private void minimizedContextMenuStrip_Opening(object? sender, CancelEventArgs? e)
        {
            // The menu location can be partly off-screen by default.
            // Fix it.
            minimizedContextMenuStrip.Location = Util.ClampedLocation(
                minimizedContextMenuStrip.Location,
                minimizedContextMenuStrip.Size);
        }

        private void minimizeNotifyIcon_BalloonTipClicked(object? sender, EventArgs? e)
        {
            // Unminimize
            OpenWindow();

            // Check all the upgrade checkboxes
            ManageMods.MarkAllUpdates();

            // Install
            Wait.StartWaiting(InstallMods, PostInstallMods, true,
                new InstallArgument(
                    ManageMods.ComputeUserChangeSet()
                              .ToList(),
                    RelationshipResolverOptions.DependsOnlyOpts())
            );
        }
        #endregion
    }
}
