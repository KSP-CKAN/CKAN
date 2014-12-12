
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {
        private BackgroundWorker m_InstallWorker;

        private void InstallModsReportProgress(string message, int percent)
        {
            SetDescription(message + " - " + percent + "%");
            SetProgress(percent);
        }

        // used to signal the install worker that the user canceled the install process
        // this may happen on the recommended/suggested mods dialogs
        private volatile bool installCanceled = false;

        // this will be the final list of mods we want to install 
        private HashSet<string> toInstall = new HashSet<string>();

        private void InstallMods(object sender, DoWorkEventArgs e) // this probably needs to be refactored
        {
            installCanceled = false;

            ClearLog();

            var opts =
                (KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>) e.Argument;

            ModuleInstaller installer = ModuleInstaller.Instance;
            // setup progress callback
            installer.onReportProgress += InstallModsReportProgress;

            toInstall = new HashSet<string>();
            var toUninstall = new HashSet<string>();
            var toUpgrade = new HashSet<string>();

            // First compose sets of what the user wants installed, upgraded, and removed.
            foreach (KeyValuePair<CkanModule,GUIModChangeType> change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Remove)
                {
                    toUninstall.Add(change.Key.identifier);
                }
                else if (change.Value == GUIModChangeType.Update)
                {
                    toUpgrade.Add(change.Key.identifier);
                }
                else if (change.Value == GUIModChangeType.Install)
                {
                    toInstall.Add(change.Key.identifier);
                }
            }

            // Now work on satisifying dependencies.

            var recommended = new Dictionary<string, List<string>>();
            var suggested = new Dictionary<string, List<string>>();

            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install)
                {
                    if (change.Key.recommends != null)
                    {
                        foreach (RelationshipDescriptor mod in change.Key.recommends)
                        {
                            try
                            {
                                // if the mod is available for the current KSP version _and_
                                // the mod is not installed _and_
                                // the mod is not already in the install list
                                if (
                                    RegistryManager.Instance(KSPManager.CurrentInstance)
                                        .registry.LatestAvailable(mod.name.ToString(), KSPManager.CurrentInstance.Version()) != null &&
                                    !RegistryManager.Instance(KSPManager.CurrentInstance).registry.IsInstalled(mod.name.ToString()) &&
                                    !toInstall.Contains(mod.name.ToString()))
                                {
                                    // add it to the list of recommended mods we display to the user
                                    if (recommended.ContainsKey(mod.name))
                                    {
                                        recommended[mod.name].Add(change.Key.identifier);
                                    }
                                    else
                                    {
                                        recommended.Add(mod.name, new List<string> { change.Key.identifier });
                                    }
                                }
                            }
                            // XXX - Don't ignore all krakens! Those things are important!
                            catch (Kraken)
                            {
                            }
                        }
                    }

                    if (change.Key.suggests != null)
                    {
                        foreach (RelationshipDescriptor mod in change.Key.suggests)
                        {
                            try
                            {
                                if (
                                RegistryManager.Instance(KSPManager.CurrentInstance)
                                    .registry.LatestAvailable(mod.name.ToString(), KSPManager.CurrentInstance.Version()) != null &&
                                !RegistryManager.Instance(KSPManager.CurrentInstance).registry.IsInstalled(mod.name.ToString()) &&
                                !toInstall.Contains(mod.name.ToString()))
                                {
                                    if (suggested.ContainsKey(mod.name))
                                    {
                                        suggested[mod.name].Add(change.Key.identifier);
                                    }
                                    else
                                    {
                                        suggested.Add(mod.name, new List<string> { change.Key.identifier });
                                    }
                                }
                            }
                            // XXX - Don't ignore all krakens! Those things are important!
                            catch (Kraken)
                            {
                            }
                        }
                    }
                }
            }

            // If we're going to install something anyway, then don't list it in the
            // recommended list, since they can't de-select it anyway.
            foreach(var item in toInstall)
            {
                recommended.Remove(item);
            }

            // If there are any mods that would be recommended, prompt the user to make
            // selections.
            if(recommended.Any())
            {
                Util.Invoke(this, () => UpdateRecommendedDialog(recommended));

                m_TabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                m_TabController.RenameTab("ChooseRecommendedModsTabPage", "Choose recommended mods");
                m_TabController.SetTabLock(true);

                lock(this)
                {
                    Monitor.Wait(this);
                }

                m_TabController.SetTabLock(false);
            }

            m_TabController.HideTab("ChooseRecommendedModsTabPage");

            // And now on to suggestions. Again, we don't show anything that's scheduled to
            // be installed on our suggest list.
            foreach(var item in toInstall)
            {
                suggested.Remove(item);
            }

            if (suggested.Any())
            {
                Util.Invoke(this, () => UpdateRecommendedDialog(suggested, true));

                m_TabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                m_TabController.RenameTab("ChooseRecommendedModsTabPage", "Choose suggested mods");
                m_TabController.SetTabLock(true);

                lock (this)
                {
                    Monitor.Wait(this);
                }

                m_TabController.SetTabLock(false);
            }

            m_TabController.HideTab("ChooseRecommendedModsTabPage");

            if (installCanceled)
            {
                m_TabController.HideTab("WaitTabPage");
                m_TabController.ShowTab("ManageModsTabPage");
                return;
            }

            // Now let's make all our changes.

            m_TabController.RenameTab("WaitTabPage", "Installing mods");
            m_TabController.ShowTab("WaitTabPage");
            m_TabController.SetTabLock(true);
            using (var transaction = new CkanTransaction())
            {
                var downloader = new NetAsyncDownloader();
                cancelCallback = () =>
                {
                    downloader.CancelDownload();
                    installCanceled = true;
                };

            
                SetDescription("Uninstalling selected mods");
                installer.UninstallList(toUninstall);
                if (installCanceled) return;
                SetDescription("Updating selected mods");
                try
                {
                    installer.Upgrade(toUpgrade, downloader);
                }
                catch (ModuleNotFoundKraken ex)
                {
                    User.WriteLine("Module {0} required, but not listed in index, or not available for your version of KSP", ex.module);
                    return;
                }


                // TODO: We should be able to resolve all our provisioning conflicts
                // before we start installing anything. CKAN.SanityChecker can be used to
                // pre-check if our changes are going to be consistent.

                bool resolvedAllProvidedMods = false;
                
                while (!resolvedAllProvidedMods)
                {
                    if (installCanceled) return;
                    try
                    {

                        InstallList(toInstall, opts.Value, downloader);
                        resolvedAllProvidedMods = true;
                    }
                    catch (TooManyModsProvideKraken tooManyProvides)
                    {
                        Util.Invoke(this, () => UpdateProvidedModsDialog(tooManyProvides));

                        m_TabController.ShowTab("ChooseProvidedModsTabPage", 3);
                        m_TabController.SetTabLock(true);

                        lock (this)
                        {
                            Monitor.Wait(this);
                        }

                        m_TabController.SetTabLock(false);

                        m_TabController.HideTab("ChooseProvidedModsTabPage");

                        if (installCanceled)
                        {
                            m_TabController.HideTab("WaitTabPage");
                            m_TabController.ShowTab("ManageModsTabPage");
                            return;
                        }

                        m_TabController.ShowTab("WaitTabPage");
                    }
                }
                if (!installCanceled)
                {
                    transaction.Complete();
                }
                    
            }
            
        }

        private void InstallList(HashSet<string> toInstall, RelationshipResolverOptions options, NetAsyncDownloader downloader)
        {
            if (toInstall.Any())
            {
                // actual magic happens here, we run the installer with our mod list
                ModuleInstaller.Instance.onReportModInstalled = OnModInstalled;                

                try
                {
                    ModuleInstaller.Instance.InstallList(toInstall.ToList(), options, downloader);
                }
                catch (ModuleNotFoundKraken ex)
                {
                    User.WriteLine("Module {0} required, but not listed in index, or not available for your version of KSP", ex.module);
                    return;
                }
                catch (BadMetadataKraken ex)
                {
                    User.WriteLine("Bad metadata detected for module {0}: {1}", ex.module, ex.Message);
                    return;
                }
                catch (FileExistsKraken ex)
                {
                    if (ex.owning_module != null)
                    {
                        User.WriteLine(
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
                        User.WriteLine(
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

                    User.WriteLine("Your GameData has been returned to its original state.\n");
                    return;
                }
                catch (InconsistentKraken ex)
                {
                    // The prettiest Kraken formats itself for us.
                    User.WriteLine(ex.InconsistenciesPretty);
                    return;
                }
                catch (CancelledActionKraken)
                {
                    return;
                }
                catch (MissingCertificateKraken kraken)
                {
                    // Another very pretty kraken.
                    Console.WriteLine(kraken);
                    return;
                }
                catch (DownloadErrorsKraken e)
                {
                    return false;
                }
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            AddStatusMessage("Module \"{0}\" successfully installed", mod.name);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            m_TabController.SetTabLock(false);

            AddStatusMessage("");
            HideWaitDialog(true);
            Util.Invoke(this, () => Enabled = true);
            Util.Invoke(menuStrip1, () => menuStrip1.Enabled = true);
        }

        private void UpdateProvidedModsDialog(TooManyModsProvideKraken tooManyProvides)
        {
            ChooseProvidedModsLabel.Text = String.Format("Module {0} is provided by more than one available module, please choose one of the following mods:", tooManyProvides.requested); ;

            ChooseProvidedModsListView.Items.Clear();

            ChooseProvidedModsListView.ItemChecked += ChooseProvidedModsListView_ItemChecked;

            foreach(CkanModule module in tooManyProvides.modules)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = module;
                item.Checked = true;

                item.Text = module.name;

                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem();
                description.Text = module.@abstract;

                item.SubItems.Add(description);
                ChooseProvidedModsListView.Items.Add(item);
            }
        }

        void ChooseProvidedModsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if(!e.Item.Checked)
            {
                return;
            }

            foreach(ListViewItem item in ChooseProvidedModsListView.Items)
            {
                if(item != e.Item && item.Checked)
                {
                    item.Checked = false;
                }
            }
        }

        private void ChooseProvidedModsCancelButton_Click(object sender, EventArgs e)
        {
            installCanceled = true;

            lock (this)
            {
                Monitor.Pulse(this);
            }
        }

        private void ChooseProvidedModsContinueButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in ChooseProvidedModsListView.Items)
            {
                if (item.Checked)
                {
                    var identifier = ((CkanModule)item.Tag).identifier;
                    toInstall.Add(identifier);
                    break;
                }
            }

            lock (this)
            {
                Monitor.Pulse(this);
            }
        }

        private void UpdateRecommendedDialog(Dictionary<string, List<string>> mods, bool suggested = false)
        {
            if(!suggested)
            {
                RecommendedDialogLabel.Text = "The following modules have been recommended by one or more of the chosen modules:";
                RecommendedModsListView.Columns[1].Text = "Recommended by:";
            }
            else
            {
                RecommendedDialogLabel.Text = "The following modules have been suggested by one or more of the chosen modules:";
                RecommendedModsListView.Columns[1].Text = "Suggested by:";
            }


            RecommendedModsListView.Items.Clear();

            foreach(var pair in mods)
            {
                CkanModule module = null;

                try
                {
                    module = RegistryManager.Instance(KSPManager.CurrentInstance).registry.LatestAvailable(pair.Key);
                }
                catch
                {
                    continue;
                }

                if(module == null)
                {
                    continue;
                }

                ListViewItem item = new ListViewItem();
                item.Tag = module;
                item.Checked = !suggested;

                item.Text = pair.Key;

                ListViewItem.ListViewSubItem recommendedBy = new ListViewItem.ListViewSubItem();
                string recommendedByString = "";

                bool first = true;
                foreach(string mod in pair.Value)
                {
                    if(!first)
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

                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem();
                description.Text = module.@abstract;

                item.SubItems.Add(description);

                RecommendedModsListView.Items.Add(item);
            }
        }

        private void RecommendedModsContinueButton_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem item in RecommendedModsListView.Items)
            {
                if(item.Checked)
                {
                    var identifier = ((CkanModule)item.Tag).identifier;
                    toInstall.Add(identifier);
                }
            }

            lock(this)
            {
                Monitor.Pulse(this);
            }
        }

        private void RecommendedModsCancelButton_Click(object sender, EventArgs e)
        {
            installCanceled = true;

            lock(this)
            {
                Monitor.Pulse(this);
            }
        }

        /// <summary>
        /// Returns mods that we require to install the selected module.
        /// This returns null if we can't compute these without user input,
        /// or if the mods conflict.
        /// </summary>
        private List<CkanModule> GetInstallDependencies(CkanModule module, RelationshipResolverOptions options)
        {
            var tmp = new List<string>();
            tmp.Add(module.identifier);

            RelationshipResolver resolver = null;

            try
            {
                resolver = new RelationshipResolver(tmp, options, RegistryManager.Instance(KSPManager.CurrentInstance).registry);
            }
            catch (Kraken kraken)
            {
                // TODO: Both of these krakens contain extra information; either a list of
                // mods the user can choose from, or a list of inconsistencies that are blocking
                // this selection. We *should* display those to the user. See GH #345.
                if (kraken is TooManyModsProvideKraken || kraken is InconsistentKraken)
                {
                    // Expected krakens.
                    return null;
                }
                else if (kraken is ModuleNotFoundKraken)
                {
                    var not_found = (ModuleNotFoundKraken)kraken;
                    log.ErrorFormat(
                        "Can't find {0}, but {1} depends on it",
                        not_found.module, module
                    );
                    return null;
                }
                 
                log.ErrorFormat("Unexpected Kraken in GetInstallDeps: {0}", kraken.GetType());
                return null;
            }

            return resolver.ModList();
        }
    }
}