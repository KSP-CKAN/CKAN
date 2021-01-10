using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
    using ModChanges = List<ModChange>;

    public partial class Main
    {
        private void ChooseRecommendedMods_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }

        private void ChooseRecommendedMods_OnConflictFound(string message)
        {
            AddStatusMessage(message);
        }

        private void auditRecommendationsMenuItem_Click(object sender, EventArgs e)
        {
            // Run in a background task so GUI thread can react to user
            Task.Factory.StartNew(() => AuditRecommendations(
                RegistryManager.Instance(CurrentInstance).registry,
                CurrentInstance.VersionCriteria()
            ));
        }

        private void AuditRecommendations(IRegistryQuerier registry, GameVersionCriteria versionCriteria)
        {
            var installer = ModuleInstaller.GetInstance(CurrentInstance, Manager.Cache, currentUser);
            if (installer.FindRecommendations(
                registry.InstalledModules.Select(im => im.Module).ToHashSet(),
                new HashSet<CkanModule>(),
                registry as Registry,
                out Dictionary<CkanModule, Tuple<bool, List<string>>> recommendations,
                out Dictionary<CkanModule, List<string>> suggestions,
                out Dictionary<CkanModule, HashSet<string>> supporters
            ))
            {
                tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                ChooseRecommendedMods.LoadRecommendations(
                    registry, versionCriteria,
                    Manager.Cache, recommendations, suggestions, supporters);
                var result = ChooseRecommendedMods.Wait();
                tabController.HideTab("ChooseRecommendedModsTabPage");
                if (result != null && result.Any())
                {
                    installWorker.RunWorkerAsync(
                        new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                            result.Select(mod => new ModChange(
                                mod,
                                GUIModChangeType.Install,
                                null
                            )).ToList(),
                            RelationshipResolver.DependsOnlyOpts()
                        )
                    );
                }
            }
            else
            {
                currentUser.RaiseError(Properties.Resources.MainRecommendationsNoneFound);
            }
        }
    }
}
