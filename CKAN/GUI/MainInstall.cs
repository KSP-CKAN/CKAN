using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CKAN
{

    public partial class Main : Form
    {

        private BackgroundWorker m_InstallWorker = null;

        private void InstallModsReportProgress(string message, int percent)
        {
            if (m_WaitDialog != null)
            {
                m_WaitDialog.SetDescription(message + " " + percent.ToString() + "%");
            }
        }

        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            m_WaitDialog.ClearLog();

            var opts = (KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>)e.Argument;

            ModuleInstaller installer = new ModuleInstaller();
            installer.onReportProgress += InstallModsReportProgress;

            // first we uninstall selected mods
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Remove)
                {
                    installer.Uninstall(change.Key.identifier);
                }
            }

            // install everything else
            HashSet<string> recommendedDialogShown = new HashSet<string>();
            HashSet<string> suggestedDialogShown = new HashSet<string>();

            List<string> toInstall = new List<string>();
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install)
                {
                    if (!recommendedDialogShown.Contains(change.Key.identifier))
                    {
                        List<string> recommended = new List<string>();
                        if (change.Key.recommends != null)
                        {
                            foreach (dynamic mod in change.Key.recommends)
                            {
                                if (RegistryManager.Instance().registry.LatestAvailable(mod.name.ToString(), KSP.Version()) != null)
                                {
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
                                foreach (var mod in recommendedToInstall)
                                {
                                    toInstall.Add(mod);
                                }
                            }

                            recommendedDialogShown.Add(change.Key.identifier);
                        }
                    }

                    if (!suggestedDialogShown.Contains(change.Key.identifier))
                    {
                        List<string> suggested = new List<string>();
                        if (change.Key.suggests != null)
                        {
                            foreach (dynamic mod in change.Key.suggests)
                            {
                                if (RegistryManager.Instance().registry.LatestAvailable(mod.name.ToString(), KSP.Version()) != null)
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
                                foreach (var mod in suggestedToInstall)
                                {
                                    toInstall.Add(mod);
                                }
                            }

                            suggestedDialogShown.Add(change.Key.identifier);
                        }
                    }
                }

                if (change.Value == GUIModChangeType.Install || change.Value == GUIModChangeType.Update)
                {
                    toInstall.Add(change.Key.identifier);
                }
            }

            if (toInstall.Any())
            {
                installer.InstallList(toInstall, opts.Value);
            }
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            UpdateModFilterList();
            m_WaitDialog.Close();
            Enabled = true;
        }

        private List<CkanModule> GetInstallDependencies(CkanModule module, RelationshipResolverOptions options)
        {
            List<string> tmp = new List<string>();
            tmp.Add(module.identifier);

            RelationshipResolver resolver = null;

            try
            {
                resolver = new RelationshipResolver(tmp, options);
            }
            catch (ModuleNotFoundException)
            {
                return null;
            }

            return resolver.ModList();
        }

    }

}
