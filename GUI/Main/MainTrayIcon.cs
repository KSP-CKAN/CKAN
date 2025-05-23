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
                    openCKANToolStripMenuItem.Visible = true;
                }
                else
                {
                    // Save the window state
                    configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized;
                    openCKANToolStripMenuItem.Visible = false;
                }
            }
            else
            {
                minimizeNotifyIcon.Visible = false;
            }
        }

        private void UpdateTrayInfo()
        {
            if (CurrentInstance != null)
            {
                var count = ManageMods.mainModList.CountModsByFilter(CurrentInstance,
                                                                     GUIModFilter.InstalledUpdateAvailable);
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
            toolStripSeparator4.Visible = true;
            updatesToolStripMenuItem.Visible = true;
        }

        /// <summary>
        /// Open the GUI and set it to the correct state.
        /// </summary>
        public void OpenWindow()
        {
            Show();
            WindowState = configuration?.IsWindowMaximised ?? false ? FormWindowState.Maximized : FormWindowState.Normal;
            openCKANToolStripMenuItem.Visible = false;
        }

        private void minimizeNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OpenWindow();
            }
        }

        private void minimizeNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OpenWindow();
            }
        }

        #region Menu

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

        private void openGameToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (configuration != null)
            {
                LaunchGame(configuration.CommandLines.First());
            }
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
                                   currentUser,
                                   userAgent)
                    .ShowDialog(this);
            }
        }

        #endregion

        private void minimizedContextMenuStrip_Opening(object? sender, CancelEventArgs? e)
        {
            // The menu location can be partly off-screen by default.
            // Fix it.
            minimizedContextMenuStrip.Location =
                Util.ClampedLocation(minimizedContextMenuStrip.Location,
                                     minimizedContextMenuStrip.Size);
        }

        private void minimizeNotifyIcon_BalloonTipClicked(object? sender, EventArgs? e)
        {
            // Unminimize
            OpenWindow();

            // Check all the upgrade checkboxes
            ManageMods.MarkAllUpdates();

            if (CurrentInstance != null)
            {
                // Install
                Wait.StartWaiting(InstallMods, PostInstallMods, true,
                                  new InstallArgument(ManageMods.ComputeUserChangeSet()
                                                                .ToList(),
                                                      RelationshipResolverOptions.DependsOnlyOpts(CurrentInstance.StabilityToleranceConfig)));
            }
        }
        #endregion
    }
}
