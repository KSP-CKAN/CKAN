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
            finally
            {
                ChangeSet = null;
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
            var toUpgrade   = new HashSet<string>();

            // First compose sets of what the user wants installed, upgraded, and removed.
            foreach (ModChange change in opts.Key)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.Remove:
                        toUninstall.Add(change.Mod.identifier);
                        break;
                    case GUIModChangeType.Update:
                        toUpgrade.Add(change.Mod.identifier);
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
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainInstallWaitTitle);
            ShowWaitDialog();
            tabController.SetTabLock(true);

            IDownloader downloader = new NetAsyncModulesDownloader(currentUser, Manager.Cache);
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
                            installer.UninstallList(toUninstall, ref possibleConfigOnlyDirs, registry_manager, false, toInstall.Select(m => m.identifier));
                            processSuccessful = true;
                        }
                    }
                    if (toUpgrade.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.Upgrade(toUpgrade, downloader, ref possibleConfigOnlyDirs, registry_manager);
                            processSuccessful = true;
                        }
                    }
                    if (toInstall.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.InstallList(toInstall, opts.Value, registry_manager, downloader, false);
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
                catch (DependencyNotSatisfiedKraken ex)
                {
                    currentUser.RaiseMessage(Properties.Resources.MainInstallDepNotSatisfied, ex.parent, ex.module);
                    return;
                }
                catch (ModuleNotFoundKraken ex)
                {
                    currentUser.RaiseMessage(Properties.Resources.MainInstallNotFound, ex.module);
                    return;
                }
                catch (BadMetadataKraken ex)
                {
                    currentUser.RaiseMessage(Properties.Resources.MainInstallBadMetadata, ex.module, ex.Message);
                    return;
                }
                catch (FileExistsKraken ex)
                {
                    if (ex.owningModule != null)
                    {
                        currentUser.RaiseMessage(
                            Properties.Resources.MainInstallFileExists,
                            ex.filename, ex.installingModule, ex.owningModule,
                            Meta.GetVersion()
                        );
                    }
                    else
                    {
                        currentUser.RaiseMessage(
                            Properties.Resources.MainInstallUnownedFileExists,
                            ex.installingModule, ex.filename
                        );
                    }
                    currentUser.RaiseMessage(Properties.Resources.MainInstallGameDataReverted);
                    return;
                }
                catch (InconsistentKraken ex)
                {
                    // The prettiest Kraken formats itself for us.
                    currentUser.RaiseMessage(ex.InconsistenciesPretty);
                    return;
                }
                catch (CancelledActionKraken)
                {
                    return;
                }
                catch (MissingCertificateKraken kraken)
                {
                    // Another very pretty kraken.
                    currentUser.RaiseMessage(kraken.ToString());
                    return;
                }
                catch (DownloadThrottledKraken kraken)
                {
                    string msg = kraken.ToString();
                    currentUser.RaiseMessage(msg);
                    if (YesNoDialog(string.Format(Properties.Resources.MainInstallOpenSettingsPrompt, msg),
                        Properties.Resources.MainInstallOpenSettings,
                        Properties.Resources.MainInstallNo))
                    {
                        // Launch the URL describing this host's throttling practices, if any
                        if (kraken.infoUrl != null)
                        {
                            Utilities.ProcessStartURL(kraken.infoUrl.ToString());
                        }
                        // Now pretend they clicked the menu option for the settings
                        Enabled = false;
                        new SettingsDialog(currentUser).ShowDialog();
                        Enabled = true;
                    }
                    return;
                }
                catch (ModuleDownloadErrorsKraken kraken)
                {
                    currentUser.RaiseMessage(kraken.ToString());
                    currentUser.RaiseError(kraken.ToString());
                    return;
                }
                catch (DirectoryNotFoundKraken kraken)
                {
                    currentUser.RaiseMessage("\r\n{0}", kraken.Message);
                    return;
                }
                catch (DllNotFoundException)
                {
                    if (currentUser.RaiseYesNoDialog(Properties.Resources.MainInstallLibCurlMissing))
                    {
                        Utilities.ProcessStartURL("https://github.com/KSP-CKAN/CKAN/wiki/libcurl");
                    }
                    throw;
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
            KeyValuePair<bool, ModChanges> result = (KeyValuePair<bool, ModChanges>) e.Result;

            tabController.SetTabLock(false);

            if (result.Key && !installCanceled)
            {
                // Rebuilds the list of GUIMods
                UpdateModsList(null);

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
            else if (installCanceled)
            {
                // User cancelled the installation
                // Rebuilds the list of GUIMods
                UpdateModsList(ChangeSet);
                if (result.Key) {
                    FailWaitDialog(
                        Properties.Resources.MainInstallCancelTooLate,
                        Properties.Resources.MainInstallCancelAfterInstall,
                        Properties.Resources.MainInstallProcessComplete,
                        result.Key
                    );
                } else {
                    FailWaitDialog(
                        Properties.Resources.MainInstallProcessCanceled,
                        Properties.Resources.MainInstallCanceledManually,
                        Properties.Resources.MainInstallInstallCanceled,
                        result.Key
                    );
                }
            }
            else if (e.Error == null)
            {
                // The install was unsuccessful, but we did catch the exception.
                FailWaitDialog(
                    Properties.Resources.MainInstallErrorInstalling,
                    Properties.Resources.MainInstallKnownError,
                    Properties.Resources.MainInstallFailed,
                    result.Key
                );
            }
            else
            {
                // An unknown error was thrown which we didn't catch.
                FailWaitDialog(
                    Properties.Resources.MainInstallErrorInstalling,
                    Properties.Resources.MainInstallUnknownError,
                    Properties.Resources.MainInstallFailed,
                    result.Key
                );
            }

            Util.Invoke(this, () => Enabled = true);
            Util.Invoke(menuStrip1, () => menuStrip1.Enabled = true);
        }
    }
}
