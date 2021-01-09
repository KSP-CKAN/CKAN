using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CKAN.Extensions;

namespace CKAN
{
    using ModChanges = List<ModChange>;
    public partial class Main
    {
        private BackgroundWorker installWorker;

        // Used to signal the install worker that the user canceled the install process.
        // This may happen on the recommended/suggested mods dialogs or during the download.
        private volatile bool installCanceled;

        /// <summary>
        /// Initiate the GUI installer flow for one specific module
        /// </summary>
        /// <param name="registry">Reference to the registry</param>
        /// <param name="module">Module to install</param>
        public void InstallModuleDriver(IRegistryQuerier registry, CkanModule module)
        {
            try
            {
                var userChangeSet = new List<ModChange>();
                InstalledModule installed = registry.InstalledModule(module.identifier);
                if (installed != null)
                {
                    // Already installed, remove it first
                    userChangeSet.Add(new ModChange(
                        installed.Module,
                        GUIModChangeType.Remove,
                        null
                    ));
                }
                // Install the selected mod
                userChangeSet.Add(new ModChange(
                    module,
                    GUIModChangeType.Install,
                    null
                ));
                if (userChangeSet.Count > 0)
                {
                    // Resolve the provides relationships in the dependencies
                    installWorker.RunWorkerAsync(
                        new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                            userChangeSet,
                            RelationshipResolver.DependsOnlyOpts()
                        )
                    );
                }
            }
            catch
            {
                // If we failed, do the clean-up normally done by PostInstallMods.
                HideWaitDialog(false);
                menuStrip1.Enabled = true;
            }
        }

        // this probably needs to be refactored
        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            installCanceled = false;
            Wait.ClearLog();

            var opts = (KeyValuePair<ModChanges, RelationshipResolverOptions>) e.Argument;

            RegistryManager registry_manager = RegistryManager.Instance(manager.CurrentInstance);
            Registry registry = registry_manager.registry;
            ModuleInstaller installer = ModuleInstaller.GetInstance(CurrentInstance, Manager.Cache, currentUser);
            // Avoid accumulating multiple event handlers
            installer.onReportModInstalled -= OnModInstalled;
            installer.onReportModInstalled += OnModInstalled;
            // setup progress callback

            // this will be the final list of mods we want to install
            HashSet<CkanModule> toInstall = new HashSet<CkanModule>();
            var toUninstall = new HashSet<string>();
            var toUpgrade   = new HashSet<CkanModule>();

            // First compose sets of what the user wants installed, upgraded, and removed.
            foreach (ModChange change in opts.Key)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.Remove:
                        toUninstall.Add(change.Mod.identifier);
                        break;
                    case GUIModChangeType.Update:
                        toUpgrade.Add(change is ModUpgrade mu
                            ? mu.targetMod
                            : change.Mod);
                        break;
                    case GUIModChangeType.Install:
                        toInstall.Add(change.Mod);
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod, CurrentInstance.VersionCriteria());
                        if (repl != null)
                        {
                            toUninstall.Add(repl.ToReplace.identifier);
                            toInstall.Add(repl.ReplaceWith);
                        }
                        break;
                }
            }

            // Prompt for recommendations and suggestions, if any
            if (installer.FindRecommendations(
                opts.Key.Where(ch => ch.ChangeType == GUIModChangeType.Install)
                    .Select(ch => ch.Mod)
                    .ToHashSet(),
                toInstall,
                registry,
                out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                out Dictionary<CkanModule, List<string>> suggestions,
                out Dictionary<CkanModule, HashSet<string>> supporters
            ))
            {
                tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                ChooseRecommendedMods.LoadRecommendations(
                    registry, CurrentInstance.VersionCriteria(),
                    Manager.Cache, recommendations, suggestions, supporters);
                tabController.SetTabLock(true);
                var result = ChooseRecommendedMods.Wait();
                if (result == null)
                {
                    installCanceled = true;
                }
                else
                {
                    toInstall.UnionWith(result);
                }
                tabController.SetTabLock(false);
                tabController.HideTab("ChooseRecommendedModsTabPage");
            }

            if (installCanceled)
            {
                tabController.ShowTab("ManageModsTabPage");
                e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                return;
            }

            // Now let's make all our changes.
            Util.Invoke(this, () =>
            {
                // Need to be on the GUI thread to get the translated string
                tabController.RenameTab("WaitTabPage", Properties.Resources.MainInstallWaitTitle);
            });
            ShowWaitDialog();
            tabController.SetTabLock(true);

            IDownloader downloader = new NetAsyncModulesDownloader(currentUser, Manager.Cache);
            downloader.Progress    += Wait.SetModuleProgress;
            downloader.AllComplete += Wait.DownloadsComplete;
            cancelCallback = () =>
            {
                downloader.CancelDownload();
                installCanceled = true;
            };

            HashSet<string> possibleConfigOnlyDirs = null;

            // checks if all actions were successfull
            bool processSuccessful = false;
            bool resolvedAllProvidedMods = false;
            // uninstall/installs/upgrades until every list is empty
            // if the queue is NOT empty, resolvedAllProvidedMods is set to false until the action is done
            while (!resolvedAllProvidedMods)
            {
                try
                {
                    e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                    if (toUninstall.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.UninstallList(toUninstall, ref possibleConfigOnlyDirs, registry_manager, false, toInstall);
                            processSuccessful = true;
                        }
                    }
                    if (toUpgrade.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.Upgrade(toUpgrade, downloader, ref possibleConfigOnlyDirs, registry_manager, true, true, false);
                            processSuccessful = true;
                        }
                    }
                    if (toInstall.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.InstallList(toInstall, opts.Value, registry_manager, ref possibleConfigOnlyDirs, downloader, false);
                            processSuccessful = true;
                        }
                    }

                    HandlePossibleConfigOnlyDirs(registry, possibleConfigOnlyDirs);

                    e.Result = new KeyValuePair<bool, ModChanges>(processSuccessful, opts.Key);
                    if (installCanceled)
                    {
                        return;
                    }
                    resolvedAllProvidedMods = true;
                }
                catch (TooManyModsProvideKraken k)
                {
                    // Prompt user to choose which mod to use
                    tabController.ShowTab("ChooseProvidedModsTabPage", 3);
                    ChooseProvidedMods.LoadProviders(k.requested, k.modules, Manager.Cache);
                    tabController.SetTabLock(true);
                    CkanModule chosen = ChooseProvidedMods.Wait();
                    // Close the selection prompt
                    tabController.SetTabLock(false);
                    tabController.HideTab("ChooseProvidedModsTabPage");
                    if (chosen != null)
                    {
                        // User picked a mod, queue it up for installation
                        toInstall.Add(chosen);
                        // DON'T return so we can loop around and try the above InstallList call again
                        tabController.ShowTab("WaitTabPage");
                    }
                    else
                    {
                        // User cancelled, get out
                        tabController.ShowTab("ManageModsTabPage");
                        e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                        return;
                    }
                }
            }
        }

        private void HandlePossibleConfigOnlyDirs(Registry registry, HashSet<string> possibleConfigOnlyDirs)
        {
            if (possibleConfigOnlyDirs != null)
            {
                // Check again for registered files, since we may
                // just have installed or upgraded some
                possibleConfigOnlyDirs.RemoveWhere(
                    d => Directory.EnumerateFileSystemEntries(d, "*", SearchOption.AllDirectories)
                        .Any(f => registry.FileOwner(CurrentInstance.ToRelativeGameDir(f)) != null));
                if (possibleConfigOnlyDirs.Count > 0)
                {
                    AddStatusMessage("");
                    tabController.ShowTab("DeleteDirectoriesTabPage", 4);
                    tabController.SetTabLock(true);

                    DeleteDirectories.LoadDirs(CurrentInstance, possibleConfigOnlyDirs);

                    // Wait here for the GUI process to finish dealing with the user
                    if (DeleteDirectories.Wait(out HashSet<string> toDelete))
                    {
                        foreach (string dir in toDelete)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                            }
                            catch
                            {
                                // Don't worry if it doesn't work, just keep going
                            }
                        }
                    }

                    tabController.ShowTab("WaitTabPage");
                    tabController.HideTab("DeleteDirectoriesTabPage");
                    tabController.SetTabLock(false);
                }
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            AddStatusMessage(string.Format(Properties.Resources.MainInstallModSuccess, mod.name));
            LabelsAfterInstall(mod);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            tabController.SetTabLock(false);

            if (e.Error != null)
            {
                switch (e.Error)
                {
                    case DependencyNotSatisfiedKraken exc:
                        currentUser.RaiseMessage(Properties.Resources.MainInstallDepNotSatisfied, exc.parent, exc.module);
                        break;

                    case ModuleNotFoundKraken exc:
                        currentUser.RaiseMessage(Properties.Resources.MainInstallNotFound, exc.module);
                        break;

                    case BadMetadataKraken exc:
                        currentUser.RaiseMessage(Properties.Resources.MainInstallBadMetadata, exc.module, exc.Message);
                        break;

                    case FileExistsKraken exc:
                        if (exc.owningModule != null)
                        {
                            currentUser.RaiseMessage(
                                Properties.Resources.MainInstallFileExists,
                                exc.filename, exc.installingModule, exc.owningModule,
                                Meta.GetVersion()
                            );
                        }
                        else
                        {
                            currentUser.RaiseMessage(
                                Properties.Resources.MainInstallUnownedFileExists,
                                exc.installingModule, exc.filename
                            );
                        }
                        currentUser.RaiseMessage(Properties.Resources.MainInstallGameDataReverted);
                        break;

                    case InconsistentKraken exc:
                        currentUser.RaiseMessage(exc.InconsistenciesPretty);
                        break;

                    case CancelledActionKraken exc:
                        currentUser.RaiseMessage(exc.Message);
                        installCanceled = true;
                        break;

                    case MissingCertificateKraken exc:
                        currentUser.RaiseMessage(exc.ToString());
                        break;

                    case DownloadThrottledKraken exc:
                        string msg = exc.ToString();
                        currentUser.RaiseMessage(msg);
                        if (YesNoDialog(string.Format(Properties.Resources.MainInstallOpenSettingsPrompt, msg),
                            Properties.Resources.MainInstallOpenSettings,
                            Properties.Resources.MainInstallNo))
                        {
                            // Launch the URL describing this host's throttling practices, if any
                            if (exc.infoUrl != null)
                            {
                                Utilities.ProcessStartURL(exc.infoUrl.ToString());
                            }
                            // Now pretend they clicked the menu option for the settings
                            Enabled = false;
                            new SettingsDialog(currentUser).ShowDialog();
                            Enabled = true;
                        }
                        break;

                    case ModuleDownloadErrorsKraken exc:
                        currentUser.RaiseMessage(exc.ToString());
                        currentUser.RaiseError(exc.ToString());
                        break;

                    case DirectoryNotFoundKraken exc:
                        currentUser.RaiseMessage("\r\n{0}", exc.Message);
                        break;

                    case ModuleIsDLCKraken exc:
                        string dlcMsg = string.Format(Properties.Resources.MainInstallCantInstallDLC, exc.module.name);
                        currentUser.RaiseMessage(dlcMsg);
                        currentUser.RaiseError(dlcMsg);
                        break;

                    case TransactionalKraken exc:
                        // Want to see the stack trace for this one
                        currentUser.RaiseMessage(exc.ToString());
                        currentUser.RaiseError(exc.ToString());
                        break;

                    default:
                        currentUser.RaiseMessage(e.Error.Message);
                        break;
                }

                FailWaitDialog(
                    Properties.Resources.MainInstallErrorInstalling,
                    Properties.Resources.MainInstallKnownError,
                    Properties.Resources.MainInstallFailed,
                    false
                );
            }
            else
            {
                // The Result property throws if InstallMods threw (!!!)
                KeyValuePair<bool, ModChanges> result = (KeyValuePair<bool, ModChanges>) e.Result;
                if (!installCanceled)
                {
                    // Rebuilds the list of GUIMods
                    ManageMods.UpdateModsList(null);

                    if (modChangedCallback != null)
                    {
                        foreach (var mod in result.Value)
                        {
                            modChangedCallback(mod.Mod, mod.ChangeType);
                        }
                    }

                    // install successful
                    AddStatusMessage(Properties.Resources.MainInstallSuccess);
                    HideWaitDialog(true);
                }
                else
                {
                    // User cancelled the installation
                    if (result.Key) {
                        FailWaitDialog(
                            Properties.Resources.MainInstallCancelTooLate,
                            Properties.Resources.MainInstallCancelAfterInstall,
                            Properties.Resources.MainInstallProcessComplete,
                            true
                        );
                    } else {
                        FailWaitDialog(
                            Properties.Resources.MainInstallProcessCanceled,
                            Properties.Resources.MainInstallCanceledManually,
                            Properties.Resources.MainInstallInstallCanceled,
                            false
                        );
                    }
                }
            }

            Util.Invoke(this, () => Enabled = true);
            Util.Invoke(menuStrip1, () => menuStrip1.Enabled = true);
        }
    }
}
