using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private void InstallMods(object sender, DoWorkEventArgs e) // this probably needs to be refactored
        {
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

            // these keep the history of dialogs asking the user which recommendations/suggestions to install
            var recommendedDialogShown = new HashSet<string>();
            var suggestedDialogShown = new HashSet<string>();

            // this will be the final list of mods we want to install 
            var toInstall = new HashSet<string>();

            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install)
                {
                    toInstall.Add(change.Key.identifier);
                }
            }

            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install)
                {
                    // check if we haven't already displayed the recommended dialog for this mod
                    if (!recommendedDialogShown.Contains(change.Key.identifier))
                    {
                        var recommended = new List<string>();
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
                                        recommended.Add(mod.name.ToString());
                                    }
                                }
                                catch (Kraken)
                                {
                                }
                            }
                        }

                        if (recommended.Any())
                        {
                            List<string> recommendedToInstall = m_RecommendsDialog.ShowRecommendsDialog
                                (
                                    String.Format("{0} recommends the following mods:", change.Key.name),
                                    recommended
                                );

                            if (recommendedToInstall != null)
                            {
                                foreach (string mod in recommendedToInstall)
                                {
                                    toInstall.Add(mod); 
                                }
                            }

                            recommendedDialogShown.Add(change.Key.identifier);
                        }
                    }

                    if (!suggestedDialogShown.Contains(change.Key.identifier))
                    {
                        var suggested = new List<string>();
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
                                        suggested.Add(mod.name);
                                    }
                                }
                                catch (Kraken)
                                {
                                }
                            }
                        }

                        if (suggested.Any())
                        {
                            List<string> suggestedToInstall = m_RecommendsDialog.ShowRecommendsDialog
                                (
                                    String.Format("{0} suggests the following mods:", change.Key.name),
                                    suggested
                                );

                            if (suggestedToInstall != null)
                            {
                                foreach (string mod in suggestedToInstall)
                                {
                                    toInstall.Add(mod);
                                }
                            }

                            suggestedDialogShown.Add(change.Key.identifier);
                        }
                    }
                }
                else if (change.Value == GUIModChangeType.Update)
                {
                    // any mods for update we just put in the install list
                    toInstall.Add(change.Key.identifier);
                }
            }

            InstallList(toInstall, opts.Value);
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

           AddStatusMessage("");
            HideWaitDialog(true);
            Util.Invoke(this, () => Enabled = true);
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