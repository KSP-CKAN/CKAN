using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Autofac;

using CKAN.Configuration;
using CKAN.IO;

namespace CKAN.GUI
{
    public partial class Main
    {
        private string? pendingProfile;

        private void ManageProfilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (CurrentInstance == null || Manager.Cache == null)
            {
                return;
            }
            if (ManageMods.HasPendingChanges)
            {
                currentUser.RaiseError(Properties.Resources.ProfilesPendingChanges);
                return;
            }
            var manager = RegistryManager.Instance(CurrentInstance, repoData);
            using (var dialog = new ManageProfilesDialog(manager))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedProfile is not string name)
                {
                    UpdateStatusBar();
                    return;
                }
                try
                {
                    manager.ScanUnmanagedFiles();
                    var coreConfig = ServiceLocator.Container.Resolve<IConfiguration>();
                    var installer = new ModuleInstaller(CurrentInstance, Manager.Cache, coreConfig, currentUser);
                    var plan = installer.PlanProfile(name, manager.registry);
                    var changes = plan.Remove.Select(m => new ModChange(m.Module, GUIModChangeType.Remove, coreConfig))
                                      .Concat(plan.Install.Select(m => new ModChange(m, GUIModChangeType.Install, coreConfig)))
                                      .ToList();
                    UpdateChangesDialog(changes, null);
                    pendingProfile = name;
                    Changeset.AllowIndividualChanges = false;
                    tabController.RenameTab(ChangesetTabPage.Name, string.Format(Properties.Resources.ProfilesSwitchTitle, name));
                    tabController.ShowTab(ChangesetTabPage.Name, 1);
                    DisableMainWindow();
                }
                catch (Kraken exc)
                {
                    currentUser.RaiseError("{0}", exc.Message);
                }
            }
        }

        private void ApplyProfile(string name)
        {
            if (CurrentInstance == null || Manager.Cache == null)
            {
                return;
            }
            var instance = CurrentInstance;
            var manager = RegistryManager.Instance(instance, repoData);
            var cache = Manager.Cache;
            var coreConfig = ServiceLocator.Container.Resolve<IConfiguration>();
            var cancellation = new CancellationTokenSource();
            DisableMainWindow();
            ShowWaitDialog();
            Wait.StartWaiting((sender, e) =>
            {
                Wait.OnCancel += cancellation.Cancel;
                try
                {
                    var installer = new ModuleInstaller(instance, cache, coreConfig, currentUser, cancellation.Token);
                    installer.InstallProgress += OnModInstalling;
                    installer.RemoveProgress += OnModRemoving;
                    var downloader = new NetAsyncModulesDownloader(currentUser, cache, userAgent, cancellation.Token);
                    downloader.DownloadProgress += OnModDownloading;
                    downloader.StoreProgress += OnModValidating;
                    installer.SwitchProfile(name, manager, downloader);
                }
                finally
                {
                    Wait.OnCancel -= cancellation.Cancel;
                }
            }, (sender, e) =>
            {
                cancellation.Dispose();
                EnableMainWindow();
                if (e?.Error != null)
                {
                    currentUser.RaiseError("{0}", e.Error.Message);
                    currentUser.RaiseMessage(Properties.Resources.ProfilesSwitchFailed);
                    Wait.Finish();
                }
                else
                {
                    ManageMods.ClearChangeSet();
                    RefreshModList(false);
                }
                UpdateStatusBar();
            }, true, null);
        }
    }
}
