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
            if (m_WaitDialog != null)
            {
                m_WaitDialog.SetDescription(message + " - " + percent + "%");
                m_WaitDialog.SetProgress(percent);
                //AddStatusMessage(message + " - " + percent.ToString() + "%");
            }
        }

        private void InstallMods(object sender, DoWorkEventArgs e) // this probably needs to be refactored
        {
            m_WaitDialog.ClearLog();

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
                    m_WaitDialog.SetDescription(String.Format("Uninstalling mod \"{0}\"", change.Key.name));
                    installer.Uninstall(change.Key.identifier, true);
                }
                else if (change.Value == GUIModChangeType.Update)
                {
                    installer.Uninstall(change.Key.identifier, false);
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
                    // check if we haven't already displayed the recommended dialog for this mod
                    if (!recommendedDialogShown.Contains(change.Key.identifier))
                    {
                        var recommended = new List<string>();
                        if (change.Key.recommends != null)
                        {
                            foreach (RelationshipDescriptor mod in change.Key.recommends)
                            {
                                // if the mod is available for the current KSP version _and_
                                // the mod is not installed _and_
                                // the mod is not already in the install list
                                if (
                                    RegistryManager.Instance()
                                        .registry.LatestAvailable(mod.name.ToString(), KSP.Version()) != null &&
                                    !RegistryManager.Instance().registry.IsInstalled(mod.name.ToString()) &&
                                    !toInstall.Contains(mod.name.ToString()))
                                {
                                    // add it to the list of recommended mods we display to the user
                                    recommended.Add(mod.name.ToString());
                                }
                            }
                        }

                        if (recommended.Count() > 0)
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
                                if (
                                    RegistryManager.Instance()
                                        .registry.LatestAvailable(mod.name.ToString(), KSP.Version()) != null &&
                                    !RegistryManager.Instance().registry.IsInstalled(mod.name.ToString()) &&
                                    !toInstall.Contains(mod.name.ToString()))
                                {
                                    suggested.Add(mod.name);
                                }
                            }
                        }

                        if (suggested.Count() > 0)
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

                    // finally add the mod itself to the install list
                    toInstall.Add(change.Key.identifier);
                }
                else if (change.Value == GUIModChangeType.Update)
                {
                    // any mods for update we just put in the install list
                    toInstall.Add(change.Key.identifier);
                }
            }

            if (toInstall.Any())
            {
                // actual magic happens here, we run the installer with our mod list
                installer.onReportModInstalled = OnModInstalled;
                installer.InstallList(toInstall.ToList(), opts.Value);
            }
        }

        private void OnModInstalled(CkanModule mod)
        {
            AddStatusMessage("Module \"{0}\" successfully installed", mod.name);
            UpdateModsList();
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            UpdateModFilterList();
            AddStatusMessage("");
            m_WaitDialog.Close();

            Util.Invoke(this, () => RecreateDialogs());
            Util.Invoke(this, () => Enabled = true);
        }

        private List<CkanModule> GetInstallDependencies(CkanModule module, RelationshipResolverOptions options)
        {
            var tmp = new List<string>();
            tmp.Add(module.identifier);

            RelationshipResolver resolver = null;

            try
            {
                resolver = new RelationshipResolver(tmp, options);
            }
            catch (ModuleNotFoundKraken)
            {
                return null;
            }

            return resolver.ModList();
        }
    }
}