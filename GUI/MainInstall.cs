using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CKAN.Types;

namespace CKAN
{
    using ModChanges = List<ModChange>;
    public partial class Main
    {
        private BackgroundWorker installWorker;

        // used to signal the install worker that the user canceled the install process
        // this may happen on the recommended/suggested mods dialogs
        private volatile bool installCanceled;

        /// <summary>
        /// Initiate the GUI installer flow for one specific module
        /// </summary>
        /// <param name="registry">Reference to the registry</param>
        /// <param name="module">Module to install</param>
        public async void InstallModuleDriver(IRegistryQuerier registry, CkanModule module)
        {
            RelationshipResolverOptions install_ops = RelationshipResolver.DefaultOpts();
            install_ops.with_recommends = false;

            try
            {
                var initialChangeSet = new HashSet<ModChange>();
                // Install the selected mod
                initialChangeSet.Add(new ModChange(
                    new GUIMod(module, registry, CurrentInstance.VersionCriteria()),
                    GUIModChangeType.Install,
                    null
                ));
                InstalledModule installed = registry.InstalledModule(module.identifier);
                if (installed != null)
                {
                    // Already installed, remove it first
                    initialChangeSet.Add(new ModChange(
                        new GUIMod(installed.Module, registry, CurrentInstance.VersionCriteria()),
                        GUIModChangeType.Remove,
                        null
                    ));
                }
                List<ModChange> fullChangeSet = new List<ModChange>(
                    await mainModList.ComputeChangeSetFromModList(
                        registry,
                        initialChangeSet,
                        ModuleInstaller.GetInstance(CurrentInstance, Manager.Cache, GUI.user),
                        CurrentInstance.VersionCriteria()
                    )
                );
                if (fullChangeSet != null && fullChangeSet.Count > 0)
                {
                    // Resolve the provides relationships in the dependencies
                    installWorker.RunWorkerAsync(
                        new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                            fullChangeSet,
                            install_ops
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
                changeSet = null;
            }
        }

        private void InstallMods(object sender, DoWorkEventArgs e) // this probably needs to be refactored
        {
            installCanceled = false;
            ClearLog();

            var opts = (KeyValuePair<ModChanges, RelationshipResolverOptions>) e.Argument;

            IRegistryQuerier registry  = RegistryManager.Instance(manager.CurrentInstance).registry;
            ModuleInstaller  installer = ModuleInstaller.GetInstance(CurrentInstance, Manager.Cache, GUI.user);
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
                        toUninstall.Add(change.Mod.Identifier);
                        break;
                    case GUIModChangeType.Update:
                        toUpgrade.Add(change.Mod.Identifier);
                        break;
                    case GUIModChangeType.Install:
                        toInstall.Add(change.Mod.ToModule());
                        break;
                }
            }

            // Now work on satisifying dependencies.

            var recommended = new Dictionary<CkanModule, List<string>>();
            var suggested   = new Dictionary<CkanModule, List<string>>();

            foreach (var change in opts.Key)
            {
                if (change.ChangeType == GUIModChangeType.Install)
                {
                    AddMod(change.Mod.ToModule().recommends, recommended, change.Mod.Identifier, registry, toInstall);
                    AddMod(change.Mod.ToModule().suggests,   suggested,   change.Mod.Identifier, registry, toInstall);
                }
            }

            ShowSelection(recommended, toInstall);
            ShowSelection(suggested,   toInstall, true);

            tabController.HideTab("ChooseRecommendedModsTabPage");

            if (installCanceled)
            {
                tabController.ShowTab("ManageModsTabPage");
                e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                return;
            }

            // Now let's make all our changes.
            tabController.RenameTab("WaitTabPage", "Status log");
            ShowWaitDialog();
            tabController.SetTabLock(true);

            IDownloader downloader = new NetAsyncModulesDownloader(GUI.user);
            cancelCallback = () =>
            {
                downloader.CancelDownload();
                installCanceled = true;
            };

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
                            installer.UninstallList(toUninstall, false, toInstall.Select(m => m.identifier));
                            processSuccessful = true;
                        }
                    }
                    if (toUpgrade.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.Upgrade(toUpgrade, downloader);
                            processSuccessful = true;
                        }
                    }
                    if (toInstall.Count > 0)
                    {
                        processSuccessful = false;
                        if (!installCanceled)
                        {
                            installer.InstallList(toInstall, opts.Value, downloader, false);
                            processSuccessful = true;
                        }
                    }
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
                    CkanModule chosen = TooManyModsProvideCore(k).Result;
                    // Close the selection prompt
                    Util.Invoke(this, () =>
                    {
                        tabController.ShowTab("WaitTabPage");
                        tabController.HideTab("ChooseProvidedModsTabPage");
                    });
                    if (chosen != null)
                    {
                        // User picked a mod, queue it up for installation
                        toInstall.Add(chosen);
                        // DON'T return so we can loop around and try the above InstallList call again
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
                    GUI.user.RaiseMessage(
                        "{0} requires {1} but it is not listed in the index, or not available for your version of KSP.",
                        ex.parent, ex.module);
                    return;
                }
                catch (ModuleNotFoundKraken ex)
                {
                    GUI.user.RaiseMessage(
                        "Module {0} required but it is not listed in the index, or not available for your version of KSP.",
                        ex.module);
                    return;
                }
                catch (BadMetadataKraken ex)
                {
                    GUI.user.RaiseMessage("Bad metadata detected for module {0}: {1}", ex.module, ex.Message);
                    return;
                }
                catch (FileExistsKraken ex)
                {
                    if (ex.owningModule != null)
                    {
                        GUI.user.RaiseMessage(
                            "\r\nOh no! We tried to overwrite a file owned by another mod!\r\n" +
                            "Please try a `ckan update` and try again.\r\n\r\n" +
                            "If this problem re-occurs, then it maybe a packaging bug.\r\n" +
                            "Please report it at:\r\n\r\n" +
                            "https://github.com/KSP-CKAN/NetKAN/issues/new\r\n\r\n" +
                            "Please including the following information in your report:\r\n\r\n" +
                            "File           : {0}\r\n" +
                            "Installing Mod : {1}\r\n" +
                            "Owning Mod     : {2}\r\n" +
                            "CKAN Version   : {3}\r\n",
                            ex.filename, ex.installingModule, ex.owningModule,
                            Meta.GetVersion()
                        );
                    }
                    else
                    {
                        GUI.user.RaiseMessage(
                            "\r\n\r\nOh no!\r\n\r\n" +
                            "It looks like you're trying to install a mod which is already installed,\r\n" +
                            "or which conflicts with another mod which is already installed.\r\n\r\n" +
                            "As a safety feature, the CKAN will *never* overwrite or alter a file\r\n" +
                            "that it did not install itself.\r\n\r\n" +
                            "If you wish to install {0} via the CKAN,\r\n" +
                            "then please manually uninstall the mod which owns:\r\n\r\n" +
                            "{1}\r\n\r\n" + "and try again.\r\n",
                            ex.installingModule, ex.filename
                        );
                    }

                    GUI.user.RaiseMessage("Your GameData has been returned to its original state.\r\n");
                    return;
                }
                catch (InconsistentKraken ex)
                {
                    // The prettiest Kraken formats itself for us.
                    GUI.user.RaiseMessage(ex.InconsistenciesPretty);
                    return;
                }
                catch (CancelledActionKraken)
                {
                    return;
                }
                catch (MissingCertificateKraken kraken)
                {
                    // Another very pretty kraken.
                    GUI.user.RaiseMessage(kraken.ToString());
                    return;
                }
                catch (DownloadThrottledKraken kraken)
                {
                    string msg = kraken.ToString();
                    GUI.user.RaiseMessage(msg);
                    if (YesNoDialog($"{msg}\r\n\r\nOpen settings now?"))
                    {
                        // Launch the URL describing this host's throttling practices, if any
                        if (kraken.infoUrl != null)
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                UseShellExecute = true,
                                FileName        = kraken.infoUrl.ToString()
                            });
                        }
                        // Now pretend they clicked the menu option for the settings
                        Enabled = false;
                        settingsDialog.ShowDialog();
                        Enabled = true;
                    }
                    return;
                }
                catch (ModuleDownloadErrorsKraken kraken)
                {
                    GUI.user.RaiseMessage(kraken.ToString());
                    GUI.user.RaiseError(kraken.ToString());
                    return;
                }
                catch (DirectoryNotFoundKraken kraken)
                {
                    GUI.user.RaiseMessage("\r\n{0}", kraken.Message);
                    return;
                }
                catch (DllNotFoundException)
                {
                    if (GUI.user.RaiseYesNoDialog("libcurl installation not found. Open wiki page for help?"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            UseShellExecute = true,
                            FileName        = "https://github.com/KSP-CKAN/CKAN/wiki/libcurl"
                        });
                    }
                    throw;
                }
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            AddStatusMessage("Module \"{0}\" successfully installed", mod.name);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            KeyValuePair<bool, ModChanges> result = (KeyValuePair<bool, ModChanges>) e.Result;

            tabController.SetTabLock(false);

            if (result.Key && !installCanceled)
            {
                // Rebuilds the list of GUIMods
                UpdateModsList(false, result.Value);

                if (modChangedCallback != null)
                {
                    foreach (var mod in result.Value)
                    {
                        modChangedCallback(mod.Mod, mod.ChangeType);
                    }
                }

                // install successful
                AddStatusMessage("Success!");
                HideWaitDialog(true);
                ChangeSet = null;
                RetryCurrentActionButton.Visible = false;
                UpdateChangesDialog(null, installWorker);
            }
            else if(installCanceled)
            {
                // User cancelled the installation
                // Rebuilds the list of GUIMods
                UpdateModsList(false, result.Value);
                UpdateChangesDialog(null, installWorker);
                if (result.Key) {
                    FailWaitDialog("Cancellation to late, process complete!", "User canceled the process manually, but all mods already (un)installed/upgraded.", "Process complete!", result.Key);
                } else {
                    FailWaitDialog("Process canceled by user!", "User canceled the process manually!", "(Un)Install/Upgrade canceled!", result.Key);
                }
            }
            else
            {
                // There was an error
                FailWaitDialog("Error during installation!", "An unknown error occurred, please try again!", "Installation failed!", result.Key);
                UpdateChangesDialog(result.Value, installWorker);
            }

            Util.Invoke(this, () => Enabled = true);
            Util.Invoke(menuStrip1, () => menuStrip1.Enabled = true);
        }

        private TaskCompletionSource<CkanModule> toomany_source;
        private void UpdateProvidedModsDialog(TooManyModsProvideKraken tooManyProvides, TaskCompletionSource<CkanModule> task)
        {
            toomany_source = task;
            ChooseProvidedModsLabel.Text =
                String.Format(
                    "Module {0} is provided by more than one available module, please choose one of the following mods:",
                    tooManyProvides.requested);

            ChooseProvidedModsListView.Items.Clear();

            ChooseProvidedModsListView.ItemChecked += ChooseProvidedModsListView_ItemChecked;

            foreach (CkanModule module in tooManyProvides.modules)
            {
                ListViewItem item = new ListViewItem()
                {
                    Tag = module,
                    Checked = false,
                    Text = Manager.Cache.IsMaybeCachedZip(module)
                        ? $"{module.name} {module.version} (cached)"
                        : $"{module.name} {module.version} ({module.download.Host ?? ""}, {CkanModule.FmtSize(module.download_size)})"
                };
                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem()
                {
                    Text = module.@abstract
                };

                item.SubItems.Add(description);
                ChooseProvidedModsListView.Items.Add(item);
            }
            ChooseProvidedModsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ChooseProvidedModsContinueButton.Enabled = false;
        }


        private void ChooseProvidedModsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var any_item_selected = ChooseProvidedModsListView.Items.Cast<ListViewItem>().Any(item => item.Checked);
            ChooseProvidedModsContinueButton.Enabled = any_item_selected;
            if (!e.Item.Checked)
            {
                return;
            }

            foreach (ListViewItem item in ChooseProvidedModsListView.Items.Cast<ListViewItem>()
                .Where(item => item != e.Item && item.Checked))
                {
                    item.Checked = false;
                }

        }

        private void ChooseProvidedModsCancelButton_Click(object sender, EventArgs e)
        {
            toomany_source.SetResult(null);
        }

        private void ChooseProvidedModsContinueButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in ChooseProvidedModsListView.Items)
            {
                if (item.Checked)
                {
                    toomany_source.SetResult((CkanModule)item.Tag);
                }
            }
        }

    }
}
