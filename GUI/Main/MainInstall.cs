using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Transactions;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Extensions;
using CKAN.GUI.Attributes;
using CKAN.Configuration;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    using InstallResult = Tuple<bool, List<ModChange>>;

    /// <summary>
    /// Type expected by InstallMods in DoWorkEventArgs.Argument
    /// Not a `using` because it's used by other files
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class InstallArgument : Tuple<List<ModChange>, RelationshipResolverOptions>
    {
        public InstallArgument(List<ModChange> changes, RelationshipResolverOptions options)
            : base(changes, options)
        { }
    }

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
                        new InstallArgument(userChangeSet,
                                            RelationshipResolverOptions.DependsOnlyOpts()));
                }
            }
            catch
            {
                // If we failed, do the clean-up normally done by PostInstallMods.
                HideWaitDialog();
                EnableMainWindow();
            }
        }

        [ForbidGUICalls]
        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            bool canceled = false;
            (List<ModChange> changes, RelationshipResolverOptions options) = (InstallArgument)e.Argument;

            var registry_manager = RegistryManager.Instance(Manager.CurrentInstance, repoData);
            var registry = registry_manager.registry;
            var installer = new ModuleInstaller(CurrentInstance, Manager.Cache, currentUser);
            // Avoid accumulating multiple event handlers
            installer.onReportModInstalled -= OnModInstalled;
            installer.onReportModInstalled += OnModInstalled;

            // this will be the final list of mods we want to install
            var toInstall   = new List<CkanModule>();
            var toUninstall = new HashSet<CkanModule>();
            var toUpgrade   = new HashSet<CkanModule>();

            // Check whether we need an explicit Remove call for auto-removals.
            // If there's an Upgrade or a user-initiated Remove, they'll take care of it.
            var needRemoveForAuto = changes.All(ch => ch.ChangeType == GUIModChangeType.Install
                                                      || ch.IsAutoRemoval);

            // First compose sets of what the user wants installed, upgraded, and removed.
            foreach (ModChange change in changes)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.Remove:
                        // Let Upgrade and Remove handle auto-removals to avoid cascade-removal of depending mods.
                        // Unless auto-removal is the ONLY thing in the changeset, in which case
                        // filtering these out would give us a completely empty changeset.
                        if (needRemoveForAuto || !change.IsAutoRemoval)
                        {
                            toUninstall.Add(change.Mod);
                        }
                        break;
                    case GUIModChangeType.Update:
                        toUpgrade.Add(change is ModUpgrade mu ? mu.targetMod
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
                            if (!toInstall.Contains(repl.ReplaceWith))
                            {
                                toInstall.Add(repl.ReplaceWith);
                            }
                        }
                        break;
                }
            }

            Util.Invoke(this, () => UseWaitCursor = true);
            try
            {
                // Prompt for recommendations and suggestions, if any
                if (installer.FindRecommendations(
                    changes.Where(ch => ch.ChangeType == GUIModChangeType.Install)
                           .Select(ch => ch.Mod)
                           .ToHashSet(),
                    toInstall,
                    registry,
                    out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                    out Dictionary<CkanModule, List<string>> suggestions,
                    out Dictionary<CkanModule, HashSet<string>> supporters))
                {
                    tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                    ChooseRecommendedMods.LoadRecommendations(
                        registry, toInstall, toUninstall,
                        CurrentInstance.VersionCriteria(), Manager.Cache,
                        CurrentInstance.game,
                        ManageMods.mainModList.ModuleLabels
                                              .LabelsFor(CurrentInstance.Name)
                                              .ToList(),
                        configuration,
                        recommendations, suggestions, supporters);
                    tabController.SetTabLock(true);
                    Util.Invoke(this, () => UseWaitCursor = false);
                    var result = ChooseRecommendedMods.Wait();
                    tabController.SetTabLock(false);
                    tabController.HideTab("ChooseRecommendedModsTabPage");
                    if (result == null)
                    {
                        e.Result = new InstallResult(false, changes);
                        throw new CancelledActionKraken();
                    }
                    else
                    {
                        toInstall = toInstall.Concat(result).Distinct().ToList();
                    }
                }
            }
            finally
            {
                // Make sure the progress tab always shows up with a normal cursor even if an exception is thrown
                Util.Invoke(this, () => UseWaitCursor = false);
                ShowWaitDialog();
            }

            // Now let's make all our changes.
            Util.Invoke(this, () =>
            {
                // Need to be on the GUI thread to get the translated string
                tabController.RenameTab("WaitTabPage", Properties.Resources.MainInstallWaitTitle);
            });
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
                        e.Result = new InstallResult(false, changes);
                        if (!canceled && toUninstall.Count > 0)
                        {
                            installer.UninstallList(toUninstall.Select(m => m.identifier),
                                ref possibleConfigOnlyDirs, registry_manager, false, toInstall);
                            toUninstall.Clear();
                        }
                        if (!canceled && toInstall.Count > 0)
                        {
                            installer.InstallList(toInstall, options, registry_manager, ref possibleConfigOnlyDirs, downloader, false);
                            toInstall.Clear();
                        }
                        if (!canceled && toUpgrade.Count > 0)
                        {
                            installer.Upgrade(toUpgrade, downloader, ref possibleConfigOnlyDirs, registry_manager, true, true, false);
                            toUpgrade.Clear();
                        }
                        if (canceled)
                        {
                            e.Result = new InstallResult(false, changes);
                            throw new CancelledActionKraken();
                        }
                        resolvedAllProvidedMods = true;
                    }
                    catch (ModuleDownloadErrorsKraken k)
                    {
                        // Get full changeset (toInstall only includes user's selections, not dependencies)
                        var crit = CurrentInstance.VersionCriteria();
                        var fullChangeset = new RelationshipResolver(
                            toInstall.Concat(toUpgrade), toUninstall, options, registry, crit
                        ).ModList().ToList();
                        DownloadsFailedDialog dfd = null;
                        Util.Invoke(this, () =>
                        {
                            dfd = new DownloadsFailedDialog(
                                Properties.Resources.ModDownloadsFailedMessage,
                                Properties.Resources.ModDownloadsFailedColHdr,
                                Properties.Resources.ModDownloadsFailedAbortBtn,
                                k.Exceptions.Select(kvp => new KeyValuePair<object[], Exception>(
                                    fullChangeset.Where(m => m.download == kvp.Key.download).ToArray(),
                                    kvp.Value)),
                                (m1, m2) => (m1 as CkanModule)?.download == (m2 as CkanModule)?.download);
                             dfd.ShowDialog(this);
                        });
                        var skip  = dfd.Wait()?.Select(m => m as CkanModule).ToArray();
                        var abort = dfd.Abort;
                        dfd.Dispose();
                        if (abort)
                        {
                            canceled = true;
                            e.Result = new InstallResult(false, changes);
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
                        Util.Invoke(this, () => StatusProgress.Visible = false);
                        var repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();
                        ChooseProvidedMods.LoadProviders(
                            k.Message,
                            k.modules.OrderByDescending(m => repoData.GetDownloadCount(registry.Repositories.Values,
                                                                                       m.identifier)
                                                             ?? 0)
                                     .ThenByDescending(m => m.identifier == k.requested)
                                     .ThenBy(m => m.name)
                                     .ToList(),
                            Manager.Cache);
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
                            Util.Invoke(this, () => StatusProgress.Visible = true);
                        }
                        else
                        {
                            e.Result = new InstallResult(false, changes);
                            throw new CancelledActionKraken();
                        }
                    }
                }
                transaction.Complete();
            }
            HandlePossibleConfigOnlyDirs(registry, possibleConfigOnlyDirs);
            e.Result = new InstallResult(true, changes);
        }

        [ForbidGUICalls]
        private void HandlePossibleConfigOnlyDirs(Registry registry, HashSet<string> possibleConfigOnlyDirs)
        {
            if (possibleConfigOnlyDirs != null)
            {
                // Check again for registered files, since we may
                // just have installed or upgraded some
                possibleConfigOnlyDirs.RemoveWhere(
                    d => !Directory.Exists(d)
                         || Directory.EnumerateFileSystemEntries(d, "*", SearchOption.AllDirectories)
                                     .Select(absF => CurrentInstance.ToRelativeGameDir(absF))
                                     .Any(relF => registry.FileOwner(relF) != null));
                if (possibleConfigOnlyDirs.Count > 0)
                {
                    Util.Invoke(this, () => StatusLabel.ToolTipText = StatusLabel.Text = "");
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
            currentUser.RaiseMessage(string.Format(Properties.Resources.MainInstallModSuccess,
                                     mod));
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
                        currentUser.RaiseMessage(exc.Message);
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
                            new SettingsDialog(ServiceLocator.Container.Resolve<IConfiguration>(),
                                               configuration,
                                               RegistryManager.Instance(CurrentInstance, repoData),
                                               updater,
                                               currentUser)
                                .ShowDialog(this);
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
                        // Thrown when the Registry tries to enlist with multiple different transactions
                        // Want to see the stack trace for this one
                        currentUser.RaiseMessage(exc.ToString());
                        currentUser.RaiseError(exc.ToString());
                        break;

                    case TransactionException texc:
                        // "Failed to roll back" is useless by itself,
                        // so show all inner exceptions too
                        foreach (var exc in texc.TraverseNodes<Exception>(ex => ex.InnerException)
                                                .Reverse())
                        {
                            log.Error(exc.Message, exc);
                            currentUser.RaiseMessage(exc.Message);
                        }
                        break;

                    default:
                        currentUser.RaiseMessage(e.Error.Message);
                        break;
                }

                Wait.RetryEnabled = true;
                FailWaitDialog(Properties.Resources.MainInstallErrorInstalling,
                               Properties.Resources.MainInstallKnownError,
                               Properties.Resources.MainInstallFailed);
            }
            else
            {
                // The Result property throws if InstallMods threw (!!!)
                (bool success, List<ModChange> changes) = (InstallResult)e.Result;
                currentUser.RaiseMessage(Properties.Resources.MainInstallSuccess);
                // Rebuilds the list of GUIMods
                RefreshModList(false);
            }
        }
    }
}
