using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using CKAN.Versioning;

namespace CKAN
{
    using ModChanges = List<ModChange>;
    public partial class Main
    {

        private void ShowSelection(Dictionary<CkanModule, List<string>> selectable, HashSet<CkanModule> toInstall, bool suggest = false)
        {
            if (installCanceled)
                return;

            // If we're going to install something anyway, then don't list it in the
            // recommended list, since they can't de-select it anyway.
            // The ToList dance is because we need to modify the dictionary while iterating over it,
            // and C# doesn't like that.
            foreach (CkanModule module in selectable.Keys.ToList())
            {
                if (toInstall.Any(m => m.identifier == module.identifier))
                {
                    selectable.Remove(module);
                }
            }

            Dictionary<CkanModule, string> mods = GetShowableMods(
                selectable,
                RegistryManager.Instance(CurrentInstance).registry,
                CurrentInstance.VersionCriteria(),
                toInstall
            );

            // If there are any mods that would be recommended, prompt the user to make
            // selections.
            if (mods.Any())
            {
                Util.Invoke(this, () => UpdateRecommendedDialog(mods, suggest));

                tabController.ShowTab("ChooseRecommendedModsTabPage", 3);
                tabController.SetTabLock(true);

                lock (this)
                {
                    Monitor.Wait(this);
                }

                if (!installCanceled)
                {
                    toInstall.UnionWith(GetSelected());
                }
                tabController.SetTabLock(false);
            }
        }

        private void AddMod(
            IEnumerable<RelationshipDescriptor>  relations,
            Dictionary<CkanModule, List<string>> chooseAble,
            string                               identifier,
            IRegistryQuerier                     registry,
            HashSet<CkanModule>                  toInstall)
        {
            if (relations == null)
                return;
            foreach (RelationshipDescriptor rel in relations)
            {
                List<CkanModule> providers = registry.LatestAvailableWithProvides(
                    rel.name,
                    CurrentInstance.VersionCriteria(),
                    rel
                );
                foreach (CkanModule provider in providers)
                {
                    if (!registry.IsInstalled(provider.identifier)
                        && !toInstall.Any(m => m.identifier == provider.identifier))
                    {
                        // We want to show this mod to the user. Add it.
                        List<string> dependers;
                        if (chooseAble.TryGetValue(provider, out dependers))
                        {
                            // Add the dependent mod to the list of reasons this dependency is shown.
                            dependers.Add(identifier);
                        }
                        else
                        {
                            // Add a new entry if this provider isn't listed yet.
                            chooseAble.Add(provider, new List<string>() { identifier });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get every mod in the Dictionary, which can be installed
        /// It also transforms the Recommender list to a string
        /// </summary>
        /// <param name="mods">Map from recommendations to lists of recommenders</param>
        /// <param name="registry">Registry of current game instance</param>
        /// <param name="versionCriteria">Versions compatible with current instance</param>
        /// <param name="toInstall">Modules planned to be installed</param>
        /// <returns>Map from installable recommendations to string describing recommenders</returns>
        private Dictionary<CkanModule, string> GetShowableMods(
            Dictionary<CkanModule, List<string>> mods,
            IRegistryQuerier                     registry,
            KspVersionCriteria                   versionCriteria,
            HashSet<CkanModule>                  toInstall
        )
        {
            Dictionary<CkanModule, string> modules = new Dictionary<CkanModule, string>();

            var opts = new RelationshipResolverOptions
            {
                with_all_suggests              = false,
                with_recommends                = false,
                with_suggests                  = false,
                without_enforce_consistency    = false,
                without_toomanyprovides_kraken = true
            };

            foreach (var pair in mods)
            {
                try
                {
                    List<CkanModule> instPlusOne = toInstall.ToList();
                    instPlusOne.Add(pair.Key);
                    RelationshipResolver resolver = new RelationshipResolver(
                        instPlusOne,
                        null,
                        opts, registry, versionCriteria
                    );

                    if (resolver.ModList().Any())
                    {
                        // Resolver was able to find a way to install, so show it to the user
                        modules.Add(pair.Key, String.Join(",", pair.Value.ToArray()));
                    }
                }
                catch { }
            }
            return modules;
        }

        private void UpdateRecommendedDialog(Dictionary<CkanModule, string> mods, bool suggested = false)
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
                CkanModule module = pair.Key;
                ListViewItem item = new ListViewItem()
                {
                    Tag = module,
                    Checked = !suggested,
                    Text = Manager.Cache.IsMaybeCachedZip(pair.Key)
                        ? $"{pair.Key.name} {pair.Key.version} (cached)"
                        : $"{pair.Key.name} {pair.Key.version} ({pair.Key.download.Host ?? ""}, {CkanModule.FmtSize(pair.Key.download_size)})"
                };

                ListViewItem.ListViewSubItem recommendedBy = new ListViewItem.ListViewSubItem() { Text = pair.Value };

                item.SubItems.Add(recommendedBy);

                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem {Text = module.@abstract};

                item.SubItems.Add(description);

                RecommendedModsListView.Items.Add(item);
            }
        }

        private void RecommendedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectionModInfo(RecommendedModsListView.SelectedItems);
        }

        private void RecommendedModsToggleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var state = ((CheckBox)sender).Checked;
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked != state)
                    item.Checked = state;
            }

            RecommendedModsListView.Refresh();
        }

        private void RecommendedModsContinueButton_Click(object sender, EventArgs e)
        {
            lock (this)
            {
                Monitor.Pulse(this);
            }
        }

        private HashSet<CkanModule> GetSelected()
        {
            var toInstall = new HashSet<CkanModule>();
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked)
                {
                    toInstall.Add((CkanModule) item.Tag);
                }
            }
            return toInstall;
        }

        private void RecommendedModsCancelButton_Click(object sender, EventArgs e)
        {
            installCanceled = true;

            lock (this)
            {
                Monitor.Pulse(this);
            }
        }

        private void auditRecommendationsMenuItem_Click(object sender, EventArgs e)
        {
            // Run in a background task so GUI thread can react to user
            Task.Factory.StartNew(() => AuditRecommendations(
                RegistryManager.Instance(CurrentInstance).registry,
                CurrentInstance.VersionCriteria()
            ));
        }

        private void AuditRecommendations(IRegistryQuerier registry, KspVersionCriteria versionCriteria)
        {
            var recommended = new Dictionary<CkanModule, List<string>>();
            var suggested   = new Dictionary<CkanModule, List<string>>();
            var toInstall   = new HashSet<CkanModule>();

            // Find recommendations and suggestions
            foreach (var mod in registry.InstalledModules.Select(im => im.Module))
            {
                AddMod(mod.recommends, recommended, mod.identifier, registry, toInstall);
                AddMod(mod.suggests,   suggested,   mod.identifier, registry, toInstall);
            }

            if (!recommended.Any() && !suggested.Any())
            {
                GUI.user.RaiseError("No recommendations or suggestions found.");
                return;
            }

            // Prompt user to choose
            installCanceled = false;
            ShowSelection(recommended, toInstall);
            ShowSelection(suggested,   toInstall, true);
            tabController.HideTab("ChooseRecommendedModsTabPage");

            // Install
            if (!installCanceled && toInstall.Any())
            {
                installWorker.RunWorkerAsync(
                    new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                        toInstall.Select(mod => new ModChange(
                            new GUIMod(mod, registry, versionCriteria),
                            GUIModChangeType.Install,
                            null
                        )).ToList(),
                        RelationshipResolver.DefaultOpts()
                    )
                );
            }
        }

    }
}
