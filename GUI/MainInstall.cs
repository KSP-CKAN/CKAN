using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    using ModChanges = List<ModChange>;
    public partial class Main
    {
        private BackgroundWorker installWorker;


        // used to signal the install worker that the user canceled the install process
        // this may happen on the recommended/suggested mods dialogs
        private volatile bool installCanceled;

        // this will be the final list of mods we want to install
        private HashSet<string> toInstall = new HashSet<string>();

        private void InstallMods(object sender, DoWorkEventArgs e) // this probably needs to be refactored
        {
            installCanceled = false;

            ClearLog();

            var opts =
                (KeyValuePair<ModChanges, RelationshipResolverOptions>) e.Argument;

            IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance).registry;
            ModuleInstaller installer = ModuleInstaller.GetInstance(CurrentInstance, GUI.user);
            // setup progress callback

            toInstall = new HashSet<string>();
            var toUninstall = new HashSet<string>();
            var toUpgrade = new HashSet<string>();

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
                        toInstall.Add(change.Mod.Identifier);
                        break;
                }
            }

            // Now work on satisifying dependencies.

            var recommended = new Dictionary<string, List<string>>();
            var suggested = new Dictionary<string, List<string>>();

            foreach (var change in opts.Key)
            {
                if (change.ChangeType == GUIModChangeType.Install)
                {
                    AddMod(change.Mod.ToModule().recommends, recommended, change.Mod.Identifier, registry);
                    AddMod(change.Mod.ToModule().suggests, suggested, change.Mod.Identifier, registry);
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


            var downloader = new NetAsyncModulesDownloader(GUI.user);
            cancelCallback = () =>
            {
                downloader.CancelDownload();
                installCanceled = true;
            };

            //Transaction is needed here to revert changes when an installation is cancelled
            //TODO: Cancellation should be handelt in the ModuleInstaller
            using (var transaction = CkanTransaction.CreateTransactionScope())
            {

                //Set the result to false/failed in case we return
                e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                SetDescription("Uninstalling selected mods");
                if (!WasSuccessful(() => installer.UninstallList(toUninstall)))
                    return;
                if (installCanceled) return;

                SetDescription("Updating selected mods");
                if (!WasSuccessful(() => installer.Upgrade(toUpgrade, downloader)))
                    return;
                if (installCanceled) return;

                // TODO: We should be able to resolve all our provisioning conflicts
                // before we start installing anything. CKAN.SanityChecker can be used to
                // pre-check if our changes are going to be consistent.

                bool resolvedAllProvidedMods = false;

                while (!resolvedAllProvidedMods)
                {
                    if (installCanceled)
                    {
                        e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                        return;
                    }
                    var ret = InstallList(toInstall, opts.Value, downloader);
                    if (!ret)
                    {
                        // install failed for some reason, error message is already displayed to the user
                        e.Result = new KeyValuePair<bool, ModChanges>(false, opts.Key);
                        return;
                    }
                    resolvedAllProvidedMods = true;
                }

                if (!installCanceled)
                {
                    transaction.Complete();
                }
            }

            e.Result = new KeyValuePair<bool, ModChanges>(!installCanceled, opts.Key);
        }

        private void AddMod(IEnumerable<RelationshipDescriptor> relations, Dictionary<string, List<string>> chooseAble, 
            string identifier, IRegistryQuerier registry)
        {
            if (relations == null)
                return;
            foreach (RelationshipDescriptor mod in relations)
            {
                try
                {
                    // if the mod is available for the current KSP version _and_
                    // the mod is not installed _and_
                    // the mod is not already in the install list
                    if (
                        registry.LatestAvailable(mod.name, CurrentInstance.Version()) != null &&
                        !registry.IsInstalled(mod.name) && !toInstall.Contains(mod.name))
                    {
                        // add it to the list of chooseAble mods we display to the user
                        if (!chooseAble.ContainsKey(mod.name))
                        {
                            chooseAble.Add(mod.name, new List<string>());
                        }
                        chooseAble[mod.name].Add(identifier);
                    }
                }
                // XXX - Don't ignore all krakens! Those things are important!
                catch (Kraken)
                {
                }
            }
        }

        private void ShowSelection(Dictionary<string, List<string>> selectable, bool suggest = false)
        {
            // If we're going to install something anyway, then don't list it in the
            // recommended list, since they can't de-select it anyway.
            foreach (var item in toInstall)
            {
                selectable.Remove(item);
            }

            // If there are any mods that would be recommended, prompt the user to make
            // selections.
            if (selectable.Any())
            {
                Util.Invoke(this, () => UpdateRecommendedDialog(selectable, suggest));

                tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                tabController.SetTabLock(true);

                lock (this)
                {
                    Monitor.Wait(this);
                }

                tabController.SetTabLock(false);
            }
        }

        /// <summary>
        /// Helper function to wrap around calls to ModuleInstaller.
        /// Handles some of the possible krakens and displays user friendly messages for them.
        /// </summary>
        private static bool WasSuccessful(Action action)
        {
            try
            {
                action();
            }
            catch (ModuleNotFoundKraken ex)
            {
                GUI.user.RaiseMessage(
                    "Module {0} required, but not listed in index, or not available for your version of KSP",
                    ex.module);
                return false;
            }
            catch (BadMetadataKraken ex)
            {
                GUI.user.RaiseMessage("Bad metadata detected for module {0}: {1}", ex.module, ex.Message);
                return false;
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
                        "https://github.com/KSP-CKAN/CKAN-meta/issues/new\n\n" +
                        "Please including the following information in your report:\r\n\r\n" +
                        "File           : {0}\r\n" +
                        "Installing Mod : {1}\r\n" +
                        "Owning Mod     : {2}\r\n" +
                        "CKAN Version   : {3}\r\n",
                        ex.filename, ex.installingModule, ex.owningModule,
                        Meta.Version()
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
                return false;
            }
            catch (InconsistentKraken ex)
            {
                // The prettiest Kraken formats itself for us.
                GUI.user.RaiseMessage(ex.InconsistenciesPretty);
                return false;
            }
            catch (CancelledActionKraken)
            {
                return false;
            }
            catch (MissingCertificateKraken kraken)
            {
                // Another very pretty kraken.
                GUI.user.RaiseMessage(kraken.ToString());
                return false;
            }
            catch (DownloadErrorsKraken)
            {
                // User notified in InstallList
                return false;
            }
            catch (DirectoryNotFoundKraken kraken)
            {
                GUI.user.RaiseMessage("\r\n{0}", kraken.Message);
                return false;
            }
            return true;
        }
        private bool InstallList(HashSet<string> toInstall, RelationshipResolverOptions options, IDownloader downloader)
        {
            if (toInstall.Any())
            {
                // actual magic happens here, we run the installer with our mod list
                var module_installer = ModuleInstaller.GetInstance(manager.CurrentInstance, GUI.user);
                module_installer.onReportModInstalled = OnModInstalled;
                return WasSuccessful(
                    () => module_installer.InstallList(toInstall.ToList(), options, downloader));
            }

            return true;
        }

        private void OnModInstalled(CkanModule mod)
        {
            AddStatusMessage("Module \"{0}\" successfully installed", mod.name);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            tabController.SetTabLock(false);

            var result = (KeyValuePair<bool, ModChanges>) e.Result;

            if (result.Key)
            {
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
                tabController.HideTab("ChangesetTabPage");
                ApplyToolButton.Enabled = false;
            }
            else
            {
                // there was an error
                // rollback user's choices but stay on the log dialog
                AddStatusMessage("Error!");
                SetDescription("An error occurred, check the log for information");
                Util.Invoke(DialogProgressBar, () => DialogProgressBar.Style = ProgressBarStyle.Continuous);
                Util.Invoke(DialogProgressBar, () => DialogProgressBar.Value = 0);

                var opts = result.Value;

                foreach (ModChange opt in opts)
                {
                    switch (opt.ChangeType)
                    {
                        case GUIModChangeType.Install:
                            MarkModForInstall(opt.Mod.Identifier);
                            break;
                        case GUIModChangeType.Update:
                            MarkModForUpdate(opt.Mod.Identifier);
                            break;
                        case GUIModChangeType.Remove:
                            MarkModForInstall(opt.Mod.Identifier, true);
                            break;
                    }
                }
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
                ListViewItem item = new ListViewItem {Tag = module, Checked = false, Text = module.name};


                ListViewItem.ListViewSubItem description =
                    new ListViewItem.ListViewSubItem {Text = module.@abstract};

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

        private void UpdateRecommendedDialog(Dictionary<string, List<string>> mods, bool suggested = false)
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
                CkanModule module;

                try
                {
                    var opts = new RelationshipResolverOptions
                    {
                        with_all_suggests = false,
                        with_recommends = false,
                        with_suggests = false,
                        without_enforce_consistency = false,
                        without_toomanyprovides_kraken = true
                    };

                    var resolver = new RelationshipResolver(new List<string> {pair.Key}, opts,
                        RegistryManager.Instance(manager.CurrentInstance).registry, CurrentInstance.Version());
                    if (!resolver.ModList().Any())
                    {
                        continue;
                    }

                    module = RegistryManager.Instance(manager.CurrentInstance)
                        .registry.LatestAvailable(pair.Key, CurrentInstance.Version());
                }
                catch
                {
                    continue;
                }

                if (module == null)
                {
                    continue;
                }

                ListViewItem item = new ListViewItem {Tag = module, Checked = !suggested, Text = pair.Key};


                ListViewItem.ListViewSubItem recommendedBy = new ListViewItem.ListViewSubItem();
                string recommendedByString = "";

                bool first = true;
                foreach (string mod in pair.Value)
                {
                    if (!first)
                    {
                        recommendedByString += ", ";
                    }
                    else
                    {
                        first = false;
                    }

                    recommendedByString += mod;
                }

                recommendedBy.Text = recommendedByString;

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
                    var identifier = ((CkanModule) item.Tag).identifier;
                    toInstall.Add(identifier);
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
