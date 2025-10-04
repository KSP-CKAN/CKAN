using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

using Autofac;

using CKAN.Configuration;
using CKAN.IO;
using CKAN.Versioning;
using CKAN.GUI.Attributes;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void ChooseRecommendedMods_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }

        private void ChooseRecommendedMods_OnConflictFound(string message)
        {
            StatusLabel.ToolTipText = StatusLabel.Text = message;
        }

        private void AuditRecommendationsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                // Run in a background task so GUI thread can react to user
                Task.Run(() => AuditRecommendations(
                    RegistryManager.Instance(CurrentInstance, repoData).registry,
                    CurrentInstance.VersionCriteria()));
            }
        }

        [ForbidGUICalls]
        private void AuditRecommendations(Registry registry, GameVersionCriteria versionCriteria)
        {
            if (CurrentInstance != null && Manager.Cache != null)
            {
                var installer = new ModuleInstaller(CurrentInstance, Manager.Cache,
                                                    ServiceLocator.Container.Resolve<IConfiguration>(),
                                                    currentUser);
                if (ModuleInstaller.FindRecommendations(
                        CurrentInstance,
                        registry.InstalledModules.Select(im => im.Module).ToHashSet(),
                        Array.Empty<CkanModule>(),
                        Array.Empty<CkanModule>(),
                        Array.Empty<CkanModule>(),
                        registry,
                        out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                        out Dictionary<CkanModule, List<string>> suggestions,
                        out Dictionary<CkanModule, HashSet<string>> supporters)
                    && configuration != null)
                {
                    tabController.ShowTab(ChooseRecommendedModsTabPage.Name, 3);
                    ChooseRecommendedMods.LoadRecommendations(
                        registry, Array.Empty<CkanModule>(), Array.Empty<CkanModule>(),
                        versionCriteria, Manager.Cache,
                        CurrentInstance.Game,
                        ModuleLabelList.ModuleLabels
                                       .LabelsFor(CurrentInstance.Name)
                                       .ToList(),
                        ServiceLocator.Container.Resolve<IConfiguration>(),
                        configuration,
                        recommendations, suggestions, supporters);
                    var result = ChooseRecommendedMods.Wait();
                    tabController.HideTab(ChooseRecommendedModsTabPage.Name);
                    if (result != null && result.Count != 0)
                    {
                        Wait.StartWaiting(InstallMods, PostInstallMods, true,
                            new InstallArgument(
                                result.Select(mod => new ModChange(mod, GUIModChangeType.Install,
                                                                   ServiceLocator.Container.Resolve<IConfiguration>()))
                                      .ToList(),
                                RelationshipResolverOptions.DependsOnlyOpts(CurrentInstance.StabilityToleranceConfig)));
                    }
                }
                else
                {
                    currentUser.RaiseError(Properties.Resources.MainRecommendationsNoneFound);
                }
            }
        }
    }
}
