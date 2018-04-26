﻿using System;
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

        // signals if all mods installed correctly
        private volatile bool resolvedAllProvidedMods;

        // this will be the final list of mods we want to install
        private HashSet<CkanModule> toInstall = new HashSet<CkanModule>();

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
                // Resolve the provides relationships in the dependencies
                List<ModChange> fullChangeSet = new List<ModChange>(
                    await mainModList.ComputeChangeSetFromModList(
                        registry,
                        new HashSet<ModChange>()
                        {
                            new ModChange(
                                new GUIMod(
                                    module,
                                    registry,
                                    CurrentInstance.VersionCriteria()
                                ),
                                GUIModChangeType.Install,
                                null
                            )
                        },
                        ModuleInstaller.GetInstance(CurrentInstance, GUI.user),
                        CurrentInstance.VersionCriteria()
                    )
                );
                if (fullChangeSet != null && fullChangeSet.Count > 0)
                {
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
            ShowWaitDialog();
            installCanceled = false;

            ClearLog();

            var opts = (KeyValuePair<ModChanges, RelationshipResolverOptions>) e.Argument;

            IRegistryQuerier registry  = RegistryManager.Instance(manager.CurrentInstance).registry;
            ModuleInstaller  installer = ModuleInstaller.GetInstance(CurrentInstance, GUI.user);
            // Avoid accumulating multiple event handlers
            installer.onReportModInstalled -= OnModInstalled;
            installer.onReportModInstalled += OnModInstalled;
            // setup progress callback

            toInstall       = new HashSet<CkanModule>();
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
                    AddMod(change.Mod.ToModule().recommends, recommended, change.Mod.Identifier, registry);
                    AddMod(change.Mod.ToModule().suggests,   suggested,   change.Mod.Identifier, registry);
                }
            }

            ShowSelection(recommended);
            ShowSelection(suggested, true);

            tabController.HideTab("ChooseRecommendedModsTabPage");

            if (installCanceled)
            {
                tabController.HideTab("WaitTabPage");
                tabController.ShowTab("ManageModsTabPage");
                e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                return;
            }

            // Now let's make all our changes.

            tabController.RenameTab("WaitTabPage", "Status log");
            tabController.ShowTab("WaitTabPage");
            tabController.SetTabLock(true);

            IDownloader downloader = new NetAsyncModulesDownloader(GUI.user);
            cancelCallback = () =>
            {
                downloader.CancelDownload();
                installCanceled = true;
            };

            resolvedAllProvidedMods = false;
            while (!resolvedAllProvidedMods)
            {
                try
                {
                    // used to check if all completed successfully despite user canceled the process
                    // if user clicks cancel before or during these steps, stepsDone != stepsToBeDone, so we know there were steps missing
                    // if user cancels afterwards, resolvedAllProvidedMods still evaluates to true
                    int stepsDone = 0;
                    int stepsToBeDone = 0;

                    e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                    if (toUninstall.Count > 0)
                    {
                        stepsToBeDone += 1;
                        if (!installCanceled)
                        {
                            installer.UninstallList(toUninstall);
                            stepsDone += 1;
                        }
                    }
                    if (toUpgrade.Count > 0)
                    {
                        stepsToBeDone += 1;
                        if(!installCanceled)
                        {
                            installer.Upgrade(toUpgrade, downloader);
                            stepsDone += 1;
                        }
                    }
                    if (toInstall.Count > 0)
                    {
                        stepsToBeDone += 1;
                        if (!installCanceled)
                        {
                            installer.InstallList(toInstall, opts.Value, downloader);
                            stepsDone += 1;
                        }
                    }
                    resolvedAllProvidedMods = (stepsDone == stepsToBeDone);
                    e.Result = new KeyValuePair<bool, ModChanges>(!installCanceled, opts.Key);
                    if (installCanceled)
                    {
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
            }
        }

        private void AddMod(
            IEnumerable<RelationshipDescriptor>  relations,
            Dictionary<CkanModule, List<string>> chooseAble,
            string                               identifier,
            IRegistryQuerier                     registry)
        {
            if (relations == null)
                return;
            foreach (RelationshipDescriptor rel in relations)
            {
                List<CkanModule> providers = registry.LatestAvailableWithProvides(
                    rel.name,
                    CurrentInstance.VersionCriteria(),
                    rel
                );
                foreach (CkanModule provider in providers)
                {
                    if (!registry.IsInstalled(provider.identifier)
                        && !toInstall.Any(m => m.identifier == provider.identifier))
                    {
                        // We want to show this mod to the user. Add it.
                        List<string> dependers;
                        if (chooseAble.TryGetValue(provider, out dependers))
                        {
                            // Add the dependent mod to the list of reasons this dependency is shown.
                            dependers.Add(identifier);
                        }
                        else
                        {
                            // Add a new entry if this provider isn't listed yet.
                            chooseAble.Add(provider, new List<string>() { identifier });
                        }
                    }
                }
            }
        }

        private void ShowSelection(Dictionary<CkanModule, List<string>> selectable, bool suggest = false)
        {
            if (installCanceled)
                return;

            // If we're going to install something anyway, then don't list it in the
            // recommended list, since they can't de-select it anyway.
            // The ToList dance is because we need to modify the dictionary while iterating over it,
            // and C# doesn't like that.
            foreach (CkanModule module in selectable.Keys.ToList())
            {
                if (toInstall.Any(m => m.identifier == module.identifier))
                {
                    selectable.Remove(module);
                }
            }

            Dictionary<CkanModule, string> mods = GetShowableMods(selectable);

            // If there are any mods that would be recommended, prompt the user to make
            // selections.
            if (mods.Any())
            {
                Util.Invoke(this, () => UpdateRecommendedDialog(mods, suggest));

                tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                tabController.SetTabLock(true);

                lock (this)
                {
                    Monitor.Wait(this);
                }

                tabController.SetTabLock(false);
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

            if (result.Key)
            {
                // install successful
                AddStatusMessage("Success!");
                HideWaitDialog(true);
                tabController.HideTab("ChangesetTabPage");
                ApplyToolButton.Enabled = false;
                RetryCurrentActionButton.Visible = false;
            }
            else if(installCanceled)
            {
                if (resolvedAllProvidedMods)
                {
                    // User cancelled the installation too late
                    FailWaitDialog("Success!", "Too late! All operations completed! Please undo your changes manually!", "Everything done!", resolvedAllProvidedMods);

                }
                else
                {
                    // User cancelled the installation before everytihng was completed
                    FailWaitDialog("Abort!", "User cancelled the (un)install/update manually!", "Installation cancelled!", resolvedAllProvidedMods);
                }
            }
            else
            {
                // There was an error
                FailWaitDialog("Error during installation!", "An unknown error occurred, please try again!", "Installation failed!", false);
            }
            // Rebuilds the list of GUIMods
            UpdateModsList(false, result.Value);

            if (modChangedCallback != null)
            {
                foreach (var mod in result.Value)
                {
                    modChangedCallback(mod.Mod, mod.ChangeType);
                }
            }
            UpdateChangesDialog(result.Value, installWorker);

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
                    Text = CurrentInstance.Cache.IsMaybeCachedZip(module)
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

        /// <summary>
        /// Tries to get every mod in the Dictionary, which can be installed
        /// It also transforms the Recommender list to a string
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        private Dictionary<CkanModule, string> GetShowableMods(Dictionary<CkanModule, List<string>> mods)
        {
            Dictionary<CkanModule, string> modules = new Dictionary<CkanModule, string>();

            var opts = new RelationshipResolverOptions
            {
                with_all_suggests              = false,
                with_recommends                = false,
                with_suggests                  = false,
                without_enforce_consistency    = false,
                without_toomanyprovides_kraken = true
            };

            foreach (var pair in mods)
            {
                try
                {
                    RelationshipResolver resolver = new RelationshipResolver(
                        new List<CkanModule> { pair.Key },
                        opts,
                        RegistryManager.Instance(manager.CurrentInstance).registry,
                        CurrentInstance.VersionCriteria()
                    );

                    if (resolver.ModList().Any())
                    {
                        // Resolver was able to find a way to install, so show it to the user
                        modules.Add(pair.Key, String.Join(",", pair.Value.ToArray()));
                    }
                }
                catch { }
            }
            return modules;
        }

        private void UpdateRecommendedDialog(Dictionary<CkanModule, string> mods, bool suggested = false)
        {
            if (!suggested)
            {
                RecommendedDialogLabel.Text =
                    "The following modules have been recommended by one or more of the chosen modules:";
                RecommendedModsListView.Columns[1].Text = "Recommended by:";
                RecommendedModsToggleCheckbox.Text = "(De-)select all recommended mods.";
                RecommendedModsToggleCheckbox.Checked=true;
                tabController.RenameTab("ChooseRecommendedModsTabPage", "Choose recommended mods");
            }
            else
            {
                RecommendedDialogLabel.Text =
                    "The following modules have been suggested by one or more of the chosen modules:";
                RecommendedModsListView.Columns[1].Text = "Suggested by:";
                RecommendedModsToggleCheckbox.Text = "(De-)select all suggested mods.";
                RecommendedModsToggleCheckbox.Checked=false;
                tabController.RenameTab("ChooseRecommendedModsTabPage", "Choose suggested mods");
            }

            RecommendedModsListView.Items.Clear();
            foreach (var pair in mods)
            {
                CkanModule module = pair.Key;
                ListViewItem item = new ListViewItem()
                {
                    Tag = module,
                    Checked = !suggested,
                    Text = CurrentInstance.Cache.IsMaybeCachedZip(pair.Key)
                        ? $"{pair.Key.name} {pair.Key.version} (cached)"
                        : $"{pair.Key.name} {pair.Key.version} ({pair.Key.download.Host ?? ""}, {CkanModule.FmtSize(pair.Key.download_size)})"
                };

                ListViewItem.ListViewSubItem recommendedBy = new ListViewItem.ListViewSubItem() { Text = pair.Value };

                item.SubItems.Add(recommendedBy);

                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem {Text = module.@abstract};

                item.SubItems.Add(description);

                RecommendedModsListView.Items.Add(item);
            }
        }

        private void RecommendedModsContinueButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked)
                {
                    toInstall.Add((CkanModule) item.Tag);
                }
            }

            lock (this)
            {
                Monitor.Pulse(this);
            }
        }

        private void RecommendedModsCancelButton_Click(object sender, EventArgs e)
        {
            installCanceled = true;

            lock (this)
            {
                Monitor.Pulse(this);
            }
        }
    }
}
