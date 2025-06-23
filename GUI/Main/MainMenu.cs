using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Autofac;

using CKAN.Configuration;

namespace CKAN.GUI
{
    public partial class Main
    {
        #region File menu

        private void ManageGameInstancesToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            var old_instance = CurrentInstance;
            var result = new ManageGameInstancesDialog(Manager, !actuallyVisible, currentUser).ShowDialog(this);
            if (result == DialogResult.OK && !Equals(old_instance, CurrentInstance))
            {
                for (bool done = false; !done;)
                {
                    try
                    {
                        ManageMods.ModGrid.ClearSelection();
                        CurrentInstanceUpdated();
                        if (old_instance != null)
                        {
                            old_instance.StabilityToleranceConfig.Changed -= StabilityToleranceConfig_Changed;
                        }
                        done = true;
                    }
                    catch (RegistryInUseKraken kraken)
                    {
                        if (YesNoDialog(
                            kraken.Message,
                            Properties.Resources.MainDeleteLockfileYes,
                            Properties.Resources.MainDeleteLockfileNo))
                        {
                            // Delete it
                            File.Delete(kraken.lockfilePath);
                        }
                        else
                        {
                            // Couldn't get the lock, revert to previous instance
                            Manager.SetCurrentInstance(old_instance);
                            CurrentInstanceUpdated();
                            done = true;
                        }
                    }
                    catch (RegistryVersionNotSupportedKraken kraken)
                    {
                        // Couldn't load the registry, revert to previous instance
                        currentUser.RaiseError("{0}", kraken.Message);
                        if (CheckForCKANUpdate())
                        {
                            UpdateCKAN();
                        }
                        else
                        {
                            Manager.SetCurrentInstance(old_instance);
                            CurrentInstanceUpdated();
                            done = true;
                        }
                    }
                }
            }
        }

        private void OpenGameDirectoryToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                Utilities.ProcessStartURL(CurrentInstance.GameDir());
            }
        }

        private void InstallFromckanToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog()
            {
                Filter      = Properties.Resources.CKANFileFilter,
                Multiselect = true,
            };

            if (open_file_dialog.ShowDialog(this) == DialogResult.OK)
            {
                InstallFromCkanFiles(open_file_dialog.FileNames);
            }
        }

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            Close();
        }

        #endregion

        #region Settings menu

        private void CKANSettingsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null && configuration != null)
            {
                // Flipping enabled here hides the main form itself.
                Enabled = false;
                var dialog = new SettingsDialog(ServiceLocator.Container.Resolve<IConfiguration>(),
                                                configuration,
                                                RegistryManager.Instance(CurrentInstance, repoData),
                                                updater,
                                                currentUser,
                                                userAgent);
                dialog.ShowDialog(this);
                Enabled = true;
                if (dialog.RepositoryAdded)
                {
                    UpdateRepo(refreshWithoutChanges: true);
                }
                else if (dialog.RepositoryRemoved || dialog.RepositoryMoved
                         || dialog.StabilityToleranceChanged)
                {
                    RefreshModList(false);
                }
            }
        }

        private void GameCommandlineToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            EditCommandLines();
        }

        private void CompatibleGameVersionsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                var dialog = new CompatibleGameVersionsDialog(CurrentInstance,
                                                              !actuallyVisible);
                if (dialog.ShowDialog(this) != DialogResult.Cancel)
                {
                    // This takes a while, so don't do it if they cancel out
                    RefreshModList(false);
                }
            }
        }

        private void preferredHostsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                Enabled = false;
                var dlg = new PreferredHostsDialog(
                    ServiceLocator.Container.Resolve<IConfiguration>(),
                    RegistryManager.Instance(CurrentInstance, repoData).registry);
                dlg.ShowDialog(this);
                Enabled = true;
            }
        }

        private void installFiltersToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                Enabled = false;
                var dlg = new InstallFiltersDialog(ServiceLocator.Container.Resolve<IConfiguration>(), CurrentInstance);
                dlg.ShowDialog(this);
                Enabled = true;
                if (dlg.Changed)
                {
                    // The Update checkbox might appear or disappear if missing files were or are filtered out
                    RefreshModList(false);
                }
            }
        }

        private void pluginsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            Enabled = false;
            pluginsDialog.ShowDialog(this);
            Enabled = true;
        }

        #endregion

        #region Help menu

        private void userGuideToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            Utilities.ProcessStartURL(HelpURLs.UserGuide);
        }

        private void discordToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            Utilities.ProcessStartURL(HelpURLs.CKANDiscord);
        }

        private void modSupportToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                Utilities.ProcessStartURL(CurrentInstance.game.ModSupportURL.ToString());
            }
        }

        private void reportClientIssueToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            Utilities.ProcessStartURL(HelpURLs.CKANIssues);
        }

        private void reportMetadataIssueToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                Utilities.ProcessStartURL(CurrentInstance.game.MetadataBugtrackerURL.ToString());
            }
        }

        private void aboutToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            new AboutDialog().ShowDialog(this);
        }

        #endregion

    }
}
