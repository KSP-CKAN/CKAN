using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        #region Tray Behaviour

        public void CheckTrayState()
        {
            enableTrayIcon = configuration.EnableTrayIcon;
            minimizeToTray = configuration.MinimizeToTray;
            pauseToolStripMenuItem.Enabled = winReg.RefreshRate != 0;
            pauseToolStripMenuItem.Text = configuration.RefreshPaused ? "Resume" : "Pause";
            UpdateTrayState();
        }

        private void UpdateTrayState()
        {
            if (enableTrayIcon)
            {
                minimizeNotifyIcon.Visible = true;

                if (WindowState == FormWindowState.Minimized)
                {
                    if (minimizeToTray)
                    {
                        // Remove our taskbar entry
                        Hide();
                    }
                }
                else
                {
                    // Save the window state
                    configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized;
                    configuration.Save();
                }
            }
            else
            {
                minimizeNotifyIcon.Visible = false;
            }
        }

        public void UpdateTrayInfo()
        {
            var count = mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable);

            if (count == 0)
            {
                updatesToolStripMenuItem.Enabled = false;
                updatesToolStripMenuItem.Text = "No available updates";
            }
            else
            {
                updatesToolStripMenuItem.Enabled = true;
                updatesToolStripMenuItem.Text = $"{count} available update" + (count == 1 ? "" : "s");
            }
        }

        /// <summary>
        /// Open the GUI and set it to the correct state.
        /// </summary>
        public void OpenWindow()
        {
            Show();
            WindowState = configuration.IsWindowMaximised ? FormWindowState.Maximized : FormWindowState.Normal;
        }

        private void minimizeNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenWindow();
        }

        private void updatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow();
            MarkAllUpdatesToolButton_Click(sender, e);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRepo();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configuration.RefreshPaused = !configuration.RefreshPaused;
            if (configuration.RefreshPaused)
            {
                refreshTimer.Stop();
                pauseToolStripMenuItem.Text = "Resume";
            }
            else
            {
                refreshTimer.Start();
                pauseToolStripMenuItem.Text = "Pause";
            }
        }

        private void openCKANToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow();
        }

        private void cKANSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenWindow();
            new SettingsDialog().ShowDialog();
        }

        private void minimizedContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // The menu location can be partly off-screen by default.
            // Fix it.
            minimizedContextMenuStrip.Location = Util.ClampedLocation(
                minimizedContextMenuStrip.Location,
                minimizedContextMenuStrip.Size
            );
        }

        #endregion

    }
}
