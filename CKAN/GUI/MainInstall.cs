using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

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
        private bool installCanceled = false;

        // this will be the final list of mods we want to install 
        private HashSet<string> toInstall = new HashSet<string>();

        private void InstallMods(object sender, DoWorkEventArgs e) // this probably needs to be refactored
        {
            installCanceled = false;

            ClearLog();

            var opts =
                (KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>) e.Argument;

            var installer = ModuleInstaller.Instance;
            // setup progress callback
            installer.onReportProgress += InstallModsReportProgress;

            // first we uninstall whatever the user wanted to plus the mods we want to update
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Remove)
                {
                    SetDescription(String.Format("Uninstalling mod \"{0}\"", change.Key.name));
                    installer.UninstallList(change.Key.identifier);
                }
                else if (change.Value == GUIModChangeType.Update)
                {
                    // TODO: Proper upgrades when ckan.dll supports them.
                    installer.UninstallList(change.Key.identifier);
                }
            }

            toInstall = new HashSet<string>();

            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install)
                {
                    toInstall.Add(change.Key.identifier);
                }
            }

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
                            catch (Kraken)
                            {
                            }
                        }
                    }
                }
                else if (change.Value == GUIModChangeType.Update)
                {
                    // any mods for update we just put in the install list
                    toInstall.Add(change.Key.identifier);
                }
            }

            foreach(var item in toInstall)
            {
                recommended.Remove(item);
            }

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

            m_TabController.RenameTab("WaitTabPage", "Installing mods");
            m_TabController.ShowTab("WaitTabPage");
            m_TabController.SetTabLock(true);

            bool resolvedAllProvidedMods = false;

            while(!resolvedAllProvidedMods)
            {
                try
                {
                    InstallList(toInstall, opts.Value);
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
        }

        private void InstallList(HashSet<string> toInstall, RelationshipResolverOptions options)
        {
            if (toInstall.Any())
            {
                var downloader = new NetAsyncDownloader();

                // actual magic happens here, we run the installer with our mod list
                ModuleInstaller.Instance.onReportModInstalled = OnModInstalled;
                cancelCallback = downloader.CancelDownload;

                try
                {
                    ModuleInstaller.Instance.InstallList(toInstall.ToList(), options, downloader);
                }
                catch (CancelledActionKraken)
                {
                    // User cancelled, no action needed.
                }
                catch (InconsistentKraken inconsistency)
                {
                    string message = "";
                    bool first = true;
                    foreach(var msg in inconsistency.inconsistencies)
                    {
                        if (!first)
                        {
                            message += ", ";
                        }
                        else
                        {
                            first = false;
                        }

                        message += msg;
                    }

                    User.Error("Inconsistency detected - {0}", message);
                }

                // TODO: Handle our other krakens here, we want the user to know
                // when things have gone wrong!

            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            AddStatusMessage("Module \"{0}\" successfully installed", mod.name);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            UpdateModFilterList();
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
                        first = true;
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