using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

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

        private void auditRecommendationsMenuItem_Click(object sender, EventArgs e)
        {
            // Run in a background task so GUI thread can react to user
            Task.Factory.StartNew(() => AuditRecommendations(
                RegistryManager.Instance(CurrentInstance, repoData).registry,
                CurrentInstance.VersionCriteria()
            ));
        }

        [ForbidGUICalls]
        private void AuditRecommendations(IRegistryQuerier registry, GameVersionCriteria versionCriteria)
        {
            var installer = new ModuleInstaller(CurrentInstance, Manager.Cache, currentUser);
            if (installer.FindRecommendations(
                registry.InstalledModules.Select(im => im.Module).ToHashSet(),
                new List<CkanModule>(),
                registry as Registry,
                out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                out Dictionary<CkanModule, List<string>> suggestions,
                out Dictionary<CkanModule, HashSet<string>> supporters
            ))
            {
                tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                ChooseRecommendedMods.LoadRecommendations(
                    registry, new List<CkanModule>(), new HashSet<CkanModule>(),
                    versionCriteria, Manager.Cache,
                    CurrentInstance.game,
                    ManageMods.mainModList.ModuleLabels
                                          .LabelsFor(CurrentInstance.Name)
                                          .ToList(),
                    configuration,
                    recommendations, suggestions, supporters);
                var result = ChooseRecommendedMods.Wait();
                tabController.HideTab("ChooseRecommendedModsTabPage");
                if (result != null && result.Any())
                {
                    Wait.StartWaiting(InstallMods, PostInstallMods, true,
                        new InstallArgument(
                            result.Select(mod => new ModChange(mod, GUIModChangeType.Install))
                                  .ToList(),
                            RelationshipResolverOptions.DependsOnlyOpts()));
                }
            }
            else
            {
                currentUser.RaiseError(Properties.Resources.MainRecommendationsNoneFound);
            }
        }
    }
}
