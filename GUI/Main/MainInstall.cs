using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Transactions;

using CKAN.Extensions;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    using ModChanges = List<ModChange>;
    public partial class Main
    {
        /// <summary>
        /// Initiate the GUI installer flow for one specific module
        /// </summary>
        /// <param name="registry">Reference to the registry</param>
        /// <param name="module">Module to install</param>
        public void InstallModuleDriver(IRegistryQuerier registry, IEnumerable<CkanModule> modules)
        {
            try
            {
                DisableMainWindow();
                var userChangeSet = new List<ModChange>();
                foreach (var module in modules)
                {
                    InstalledModule installed = registry.InstalledModule(module.identifier);
                    if (installed != null)
                    {
                        // Already installed, remove it first
                        userChangeSet.Add(new ModChange(installed.Module, GUIModChangeType.Remove));
                    }
                    // Install the selected mod
                    userChangeSet.Add(new ModChange(module, GUIModChangeType.Install));
                }
                if (userChangeSet.Count > 0)
                {
                    // Resolve the provides relationships in the dependencies
                    Wait.StartWaiting(InstallMods, PostInstallMods, true,
                        new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                            userChangeSet,
                            RelationshipResolver.DependsOnlyOpts()));
                }
            }
            catch
            {
                // If we failed, do the clean-up normally done by PostInstallMods.
                HideWaitDialog();
                EnableMainWindow();
            }
        }

        // this probably needs to be refactored
        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            bool canceled = false;
            var opts = (KeyValuePair<ModChanges, RelationshipResolverOptions>) e.Argument;

            RegistryManager registry_manager = RegistryManager.Instance(manager.CurrentInstance);
            Registry registry = registry_manager.registry;
            ModuleInstaller installer = new ModuleInstaller(CurrentInstance, Manager.Cache, currentUser);
            // Avoid accumulating multiple event handlers
            installer.onReportModInstalled -= OnModInstalled;
            installer.onReportModInstalled += OnModInstalled;
            // setup progress callback

            // this will be the final list of mods we want to install
            var toInstall   = new List<CkanModule>();
            var toUninstall = new HashSet<CkanModule>();
            var toUpgrade   = new HashSet<CkanModule>();

            // First compose sets of what the user wants installed, upgraded, and removed.
            foreach (ModChange change in opts.Key)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.Remove:
                        toUninstall.Add(change.Mod);
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
                            toUninstall.Add(repl.ToReplace);
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
                    registry, toInstall, toUninstall,
                    CurrentInstance.VersionCriteria(), Manager.Cache,
                    recommendations, suggestions, supporters);
                tabController.SetTabLock(true);
                var result = ChooseRecommendedMods.Wait();
                tabController.SetTabLock(false);
                tabController.HideTab("ChooseRecommendedModsTabPage");
                if (result == null)
                {
                    e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                    throw new CancelledActionKraken();
                }
                else
                {
                    toInstall = toInstall.Concat(result).Distinct().ToList();
                }
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
            downloader.Progress      += Wait.SetModuleProgress;
            downloader.AllComplete   += Wait.DownloadsComplete;
            downloader.StoreProgress += (module, remaining, total) =>
                Wait.SetProgress(string.Format(Properties.Resources.ValidatingDownload, module),
                    remaining, total);

            Wait.OnCancel += () =>
            {
                canceled = true;
                downloader.CancelDownload();
            };

            HashSet<string> possibleConfigOnlyDirs = null;

            // Treat whole changeset as atomic
            using (TransactionScope transaction = CkanTransaction.CreateTransactionScope())
            {
                // Checks if all actions were successful
                // Uninstall/installs/upgrades until every list is empty
                // If the queue is NOT empty, resolvedAllProvidedMods is false until the action is done
                for (bool resolvedAllProvidedMods = false; !resolvedAllProvidedMods;)
                {
                    try
                    {
                        e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                        if (!canceled && toUninstall.Count > 0)
                        {
                            installer.UninstallList(toUninstall.Select(m => m.identifier),
                                ref possibleConfigOnlyDirs, registry_manager, false, toInstall);
                            toUninstall.Clear();
                        }
                        if (!canceled && toInstall.Count > 0)
                        {
                            installer.InstallList(toInstall, opts.Value, registry_manager, ref possibleConfigOnlyDirs, downloader, false);
                            toInstall.Clear();
                        }
                        if (!canceled && toUpgrade.Count > 0)
                        {
                            installer.Upgrade(toUpgrade, downloader, ref possibleConfigOnlyDirs, registry_manager, true, true, false);
                            toUpgrade.Clear();
                        }
                        if (canceled)
                        {
                            e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                            throw new CancelledActionKraken();
                        }
                        resolvedAllProvidedMods = true;
                    }
                    catch (ModuleDownloadErrorsKraken k)
                    {
                        // Get full changeset (toInstall only includes user's selections, not dependencies)
                        var crit = CurrentInstance.VersionCriteria();
                        var fullChangeset = new RelationshipResolver(toInstall, null, opts.Value, registry, crit).ModList().ToList();
                        var dfd = new DownloadsFailedDialog(
                            Properties.Resources.ModDownloadsFailedMessage,
                            Properties.Resources.ModDownloadsFailedColHdr,
                            Properties.Resources.ModDownloadsFailedAbortBtn,
                            k.Exceptions.Select(kvp => new KeyValuePair<object[], Exception>(
                                fullChangeset.Where(m => m.download == kvp.Key.download).ToArray(),
                                kvp.Value)),
                            (m1, m2) => (m1 as CkanModule)?.download == (m2 as CkanModule)?.download);
                        Util.Invoke(this, () => dfd.ShowDialog(this));
                        var skip = dfd.Wait()?.Select(m => m as CkanModule).ToArray();
                        var abort = dfd.Abort;
                        dfd.Dispose();
                        if (abort)
                        {
                            canceled = true;
                            e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                            throw new CancelledActionKraken();
                        }

                        if (skip.Length > 0)
                        {
                            // Remove mods from changeset that user chose to skip
                            // and any mods depending on them
                            var dependers = registry.FindReverseDependencies(
                                skip.Select(s => s.identifier).ToList(),
                                fullChangeset,
                                // Consider virtual dependencies satisfied so user can make a new choice if they skip
                                rel => rel.LatestAvailableWithProvides(registry, crit).Count > 1)
                                .ToHashSet();
                            toInstall.RemoveAll(m => dependers.Contains(m.identifier));
                        }

                        // Now we loop back around again
                    }
                    catch (TooManyModsProvideKraken k)
                    {
                        // Prompt user to choose which mod to use
                        tabController.ShowTab("ChooseProvidedModsTabPage", 3);
                        ChooseProvidedMods.LoadProviders(k.Message, k.modules, Manager.Cache);
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
                            e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                            throw new CancelledActionKraken();
                        }
                    }
                }
                transaction.Complete();
            }
            HandlePossibleConfigOnlyDirs(registry, possibleConfigOnlyDirs);
            e.Result = new KeyValuePair<bool, ModChanges>(true, opts.Key);
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

                    case NotEnoughSpaceKraken exc:
                        currentUser.RaiseMessage(exc.Message);
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
                        // User already knows they cancelled, get out
                        HideWaitDialog();
                        EnableMainWindow();
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
                            new SettingsDialog(currentUser).ShowDialog(this);
                            Enabled = true;
                        }
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

                Wait.RetryEnabled = true;
                FailWaitDialog(
                    Properties.Resources.MainInstallErrorInstalling,
                    Properties.Resources.MainInstallKnownError,
                    Properties.Resources.MainInstallFailed);
            }
            else
            {
                // The Result property throws if InstallMods threw (!!!)
                KeyValuePair<bool, ModChanges> result = (KeyValuePair<bool, ModChanges>) e.Result;
                AddStatusMessage(Properties.Resources.MainInstallSuccess);
                // Rebuilds the list of GUIMods
                RefreshModList();
            }
        }
    }
}
