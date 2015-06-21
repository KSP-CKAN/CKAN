using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    using ModWithChangeType = KeyValuePair<CkanModule, GUIModChangeType>;

    internal class MainInstall
    {
        private readonly List<ModWithChangeType> mods_with_changes;

        private readonly KSPVersion version;
        private readonly Registry registry;
        private readonly ModuleInstaller installer;

        private readonly HashSet<string> to_install = new HashSet<string>();
        private readonly HashSet<string> to_upgrade = new HashSet<string>();
        private readonly HashSet<string> to_uninstall = new HashSet<string>();

        public readonly Dictionary<string, List<string>> recommended = new Dictionary<string, List<string>>();
        public readonly Dictionary<string, List<string>> suggested = new Dictionary<string, List<string>>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="mods_with_changes"></param>
        /// <param name="installer">Assumes that the User properly uses the invoke methods</param>
        /// <param name="registry"></param>
        /// <param name="version"></param>
        public MainInstall(
            List<ModWithChangeType> mods_with_changes,
            ModuleInstaller installer, Registry registry, KSPVersion version)
        {
            this.registry = registry;
            this.version = version;
            this.mods_with_changes = mods_with_changes;
            this.installer = installer;
        }

        public void Start()
        {
            // First compose sets of what the user wants installed, upgraded, and removed.
            foreach (var change in mods_with_changes)
            {
                switch (change.Value)
                {
                    case GUIModChangeType.Remove:
                        to_uninstall.Add(change.Key.identifier);
                        break;
                    case GUIModChangeType.Update:
                        to_upgrade.Add(change.Key.identifier);
                        break;
                    case GUIModChangeType.Install:
                        to_install.Add(change.Key.identifier);
                        break;
                }
            }

            //Construct the lists of recommended and suggested mods.
            foreach (
                var ckan_module in
                    mods_with_changes.Where(change => change.Value == GUIModChangeType.Install)
                        .Select(change => change.Key))
            {
                AddInstallableModsToDict(version, recommended, ckan_module.recommends, ckan_module.identifier);
                AddInstallableModsToDict(version, suggested, ckan_module.suggests, ckan_module.identifier);
            }

            // If we're going to install something anyway, then don't list it in the
            // recommended list, since they can't de-select it anyway.
            foreach (var item in to_install)
            {
                recommended.Remove(item);
                suggested.Remove(item);
            }
        }

        private void AddInstallableModsToDict(KSPVersion ksp_version, Dictionary<string, List<string>> dictionary,
            List<RelationshipDescriptor> candidates, string endorser)
        {
            if (candidates == null) return;

            foreach (var mod in candidates)
            {
                try
                {
                    // if the mod is available for the current KSP version _and_
                    // the mod is not installed _and_
                    // the mod is not already in the install list
                    if (registry.LatestAvailable(mod.name, ksp_version) != null &&
                        !registry.IsInstalled(mod.name) &&
                        !to_install.Contains(mod.name))
                    {
                        // add it to the list of mods we display to the user
                        dictionary.AppendItemOrAddNewList(mod.name, endorser);
                    }
                }
                catch (ModuleNotFoundKraken)
                {
                }
            }
        }

        public void UninstallList()
        {
            installer.UninstallList(to_uninstall);
        }

        public void Upgrade(NetAsyncDownloader downloader)
        {
            installer.Upgrade(to_upgrade, downloader);
        }

        public bool InstallList(RelationshipResolverOptions options, NetAsyncDownloader downloader, GUIUser user,
            ModuleInstaller module_installer)
        {
            if (!to_install.Any()) return true;
            try
            {
                module_installer.InstallList(to_install.ToList(), options, downloader);
            }
            catch (ModuleNotFoundKraken ex)
            {
                user.RaiseMessage(
                    "Module {0} required, but not listed in index, or not available for your version of KSP", ex.module);
                return false;
            }
            catch (BadMetadataKraken ex)
            {
                user.RaiseMessage("Bad metadata detected for module {0}: {1}", ex.module, ex.Message);
                return false;
            }
            catch (FileExistsKraken ex)
            {
                if (ex.owning_module != null)
                {
                    user.RaiseMessage(
                        "\nOh no! We tried to overwrite a file owned by another mod!\n" +
                        "Please try a `ckan update` and try again.\n\n" +
                        "If this problem re-occurs, then it maybe a packaging bug.\n" +
                        "Please report it at:\n\n" +
                        "https://github.com/KSP-CKAN/CKAN-meta/issues/new\n\n" +
                        "Please including the following information in your report:\n\n" +
                        "File           : {0}\n" +
                        "Installing Mod : {1}\n" +
                        "Owning Mod     : {2}\n" +
                        "CKAN Version   : {3}\n",
                        ex.filename, ex.installing_module, ex.owning_module,
                        Meta.Version()
                        );
                }
                else
                {
                    user.RaiseMessage(
                        "\n\nOh no!\n\n" +
                        "It looks like you're trying to install a mod which is already installed,\n" +
                        "or which conflicts with another mod which is already installed.\n\n" +
                        "As a safety feature, the CKAN will *never* overwrite or alter a file\n" +
                        "that it did not install itself.\n\n" +
                        "If you wish to install {0} via the CKAN,\n" +
                        "then please manually uninstall the mod which owns:\n\n" +
                        "{1}\n\n" + "and try again.\n",
                        ex.installing_module, ex.filename
                        );
                }

                user.RaiseMessage("Your GameData has been returned to its original state.\n");
                return false;
            }
            catch (InconsistentKraken ex)
            {
                // The prettiest Kraken formats itself for us.
                user.RaiseMessage(ex.InconsistenciesPretty);
                return false;
            }
            catch (CancelledActionKraken)
            {
                return false;
            }
            catch (MissingCertificateKraken kraken)
            {
                // Another very pretty kraken.
                user.RaiseMessage(kraken.ToString());
                return false;
            }
            catch (DownloadErrorsKraken)
            {
                // User notified in InstallList
                return false;
            }
            catch (DirectoryNotFoundKraken kraken)
            {
                user.RaiseMessage("\n{0}", kraken.Message);
                return false;
            }
            return true;
        }

        public void Add(string identifier)
        {
            to_install.Add(identifier);
        }
    }


    public class MainInstallGUI
    {
        private delegate void MainCancelCallback();

        private MainCancelCallback cancel_callback;
        private readonly Main main;
        private readonly KSP ksp_instance;
        internal BackgroundWorker install_worker;


        // used to signal the install worker that the user canceled the install process
        // this may happen on the recommended/suggested mods dialogs
        private volatile bool install_canceled;

        // this will be the final list of mods we want to install
        private MainInstall main_install;
        private readonly TabController tab_controller;

        public MainInstallGUI(KSP ksp_instance, Main main, TabController tab_controller)
        {
            this.main = main;
            this.ksp_instance = ksp_instance;
            this.tab_controller = tab_controller;

            install_worker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};
            install_worker.DoWork += InstallMods;

            // TODO. The remaining usages of main are as follows.
            // main.ClearLog();
            // main.SetDescription()
            // main.AddStatusMessage()
            // main.CancelCurrentActionButton1.Enabled = false;
            // main.HideWaitDialog(true);
            //
            // main.ChooseProvidedModsLabel.Text
            // main.ChooseProvidedModsListView.Items.Clear()
            // main.ChooseProvidedModsListView.ItemChecked
            //
            // main.RecommendedDialogLabel.Text
            // main.RecommendedModsListView.Columns[1].Text
            // main.RecommendedModsListView.Items.Clear()
            // main.RecommendedModsListView.Items.Add(item)
            // Consider either a move of these into this class or grouping them into interfaces which are passed in.
        }


        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            var opts = (KeyValuePair<List<ModWithChangeType>, RelationshipResolverOptions>) e.Argument;

            main.ClearLog();
            main_install = new MainInstall(opts.Key, ModuleInstaller.GetInstance(ksp_instance, GUI.user),
                RegistryManager.Instance(ksp_instance).registry, ksp_instance.Version());
            main_install.Start();

            DisplayModSelectionTab(main_install.recommended, "Choose recommended mods");
            DisplayModSelectionTab(main_install.suggested, "Choose suggested mods");

            if (install_canceled)
            {
                tab_controller.HideTab("WaitTabPage");
                tab_controller.ShowTab("ManageModsTabPage");
                e.Result = new KeyValuePair<bool, List<ModWithChangeType>>(false, opts.Key);
                return;
            }

            // Now let's make all our changes.

            tab_controller.RenameTab("WaitTabPage", "Installing mods");
            tab_controller.ShowTab("WaitTabPage");
            tab_controller.SetTabLock(true);


            var downloader = new NetAsyncDownloader(GUI.user);
            cancel_callback = () =>
            {
                downloader.CancelDownload();
                install_canceled = true;
            };


            main.SetDescription("Uninstalling selected mods");
            main_install.UninstallList();
            if (install_canceled) return;

            main.SetDescription("Updating selected mods");
            main_install.Upgrade(downloader);


            // TODO: We should be able to resolve all our provisioning conflicts
            // before we start installing anything. CKAN.SanityChecker can be used to
            // pre-check if our changes are going to be consistent.

            bool resolved_all_provided_mods = false;

            while (!resolved_all_provided_mods)
            {
                if (install_canceled)
                {
                    e.Result = new KeyValuePair<bool, List<ModWithChangeType>>(false,
                        opts.Key);
                    return;
                }
                var ret = main_install.InstallList(opts.Value, downloader, GUI.user,
                    ModuleInstaller.GetInstance(ksp_instance, GUI.user));
                if (!ret)
                {
                    // install failed for some reason, error message is already displayed to the user
                    e.Result = new KeyValuePair<bool, List<ModWithChangeType>>(false,
                        opts.Key);
                    return;
                }
                resolved_all_provided_mods = true;
            }
            e.Result = new KeyValuePair<bool, List<KeyValuePair<CkanModule, GUIModChangeType>>>(true, opts.Key);
        }

        private void DisplayModSelectionTab(Dictionary<string, List<string>> modlist, string tab_name)
        {
            // If there are any mods that would be recommended, prompt the user to make
            // selections.
            if (modlist.Any())
            {
                Util.Invoke(main, () => UpdateRecommendedDialog(modlist));

                tab_controller.ShowTab("ChooseRecommendedModsTabPage", 3);
                tab_controller.RenameTab("ChooseRecommendedModsTabPage", tab_name);
                tab_controller.SetTabLock(true);

                LockAndWait();

                tab_controller.SetTabLock(false);
            }

            tab_controller.HideTab("ChooseRecommendedModsTabPage");
        }


        private TaskCompletionSource<CkanModule> toomany_source;

        public void UpdateProvidedModsDialog(TooManyModsProvideKraken too_many_provides,
            TaskCompletionSource<CkanModule> task)
        {
            toomany_source = task;

            Util.Invoke(main.ChooseProvidedModsLabel, () =>
            {
                main.ChooseProvidedModsLabel.Text = String.Format(
                    "Module {0} is provided by more than one available module, please choose one of the following mods:",
                    too_many_provides.requested);
                main.ChooseProvidedModsListView.Items.Clear();
            });

            main.ChooseProvidedModsListView.ItemChecked += ChooseProvidedModsListView_ItemChecked;

            var items = new List<ListViewItem>(too_many_provides.modules.Count);
            foreach (var module in too_many_provides.modules)
            {
                var item = new ListViewItem {Tag = module, Checked = true, Text = module.name};
                var description = new ListViewItem.ListViewSubItem {Text = module.@abstract};

                item.SubItems.Add(description);
                items.Add(item);
            }

            Util.Invoke(main.ChooseProvidedModsListView,
                () => { main.ChooseProvidedModsListView.Items.AddRange(items.ToArray()); }
                );
        }


        private void ChooseProvidedModsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var any_item_selected = main.ChooseProvidedModsListView.Items.Cast<ListViewItem>().Any(item => item.Checked);
            main.ChooseProvidedModsContinueButton.Enabled = any_item_selected;
            if (!e.Item.Checked)
            {
                return;
            }

            foreach (ListViewItem item in main.ChooseProvidedModsListView.Items.Cast<ListViewItem>()
                .Where(item => item != e.Item && item.Checked))
            {
                item.Checked = false;
            }
        }

        internal void ChooseProvidedModsCancelButton_Click(object sender, EventArgs e)
        {
            toomany_source.SetResult(null);
        }

        internal void ChooseProvidedModsContinueButton_Click(object sender, EventArgs e)
        {
            toomany_source.SetResult(
                (CkanModule) main.ChooseProvidedModsListView.Items.Cast<ListViewItem>()
                    .First(item => item.Checked).Tag);
        }


        private void UpdateRecommendedDialog(Dictionary<string, List<string>> mods, bool suggested = false)
        {
            if (!suggested)
            {
                main.RecommendedDialogLabel.Text =
                    "The following modules have been recommended by one or more of the chosen modules:";

                main.RecommendedModsListView.Columns[
                    1].
                    Text
                    = "Recommended by:";
            }
            else
            {
                main.RecommendedDialogLabel.Text
                    =
                    "The following modules have been suggested by one or more of the chosen modules:";

                main.RecommendedModsListView.Columns[
                    1].
                    Text
                    = "Suggested by:";
            }

            main.RecommendedModsListView.Items.Clear();

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

                    var registry = RegistryManager.Instance(ksp_instance).registry;
                    var resolver = new RelationshipResolver(new[] {pair.Key}, opts, registry, ksp_instance.Version());
                    if (!resolver.ModList().Any())
                    {
                        continue;
                    }

                    module = registry.LatestAvailable(pair.Key, ksp_instance.Version());
                }
                catch
                {
                    continue
                        ;
                }
                if (module == null)

                {
                    continue
                        ;
                }

                var item = new ListViewItem
                {
                    Tag = module,
                    Checked = !suggested,
                    Text = pair.Key,
                    SubItems =
                    {
                        new ListViewItem.ListViewSubItem
                        {
                            Text = string.Join(", ", pair.Value)
                        },
                        new ListViewItem.ListViewSubItem {Text = module.@abstract}
                    }
                };
                main.RecommendedModsListView.Items.Add(item);
            }
        }

        internal void RecommendedModsContinueButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in main.RecommendedModsListView.Items)
            {
                if (item.Checked)
                {
                    var identifier = ((CkanModule) item.Tag).identifier;
                    main_install.Add(identifier);
                }
            }

            LockAndPulse();
        }

        internal void RecommendedModsCancelButton_Click(object sender, EventArgs e)
        {
            install_canceled = true;

            LockAndPulse();
        }

        public void CancelCurrentActionButton_Click(object sender, EventArgs e)
        {
            if (cancel_callback != null)
            {
                cancel_callback();
                main.CancelCurrentActionButton1.Enabled = false;
                main.HideWaitDialog(true);
            }
        }

        private void LockAndWait()
        {
            lock (this)
            {
                Monitor.Wait(this);
            }
        }

        private void LockAndPulse()
        {
            lock (this)
            {
                Monitor.Pulse(this);
            }
        }
    }
}