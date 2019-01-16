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
                List<CkanModule> providers = rel.LatestAvailableWithProvides(
                    registry,
                    CurrentInstance.VersionCriteria()
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
            return mods.Where(kvp => CanInstall(
                registry, versionCriteria,
                RelationshipResolver.DependsOnlyOpts(),
                toInstall.ToList().Concat(new List<CkanModule>() { kvp.Key }).ToList()
            )).ToDictionary(
                kvp => kvp.Key,
                kvp => string.Join(", ", kvp.Value.ToArray())
            );
        }

        /// <summary>
        /// Determine whether there is any way to install the given set of mods.
        /// Handles virtual dependencies, including recursively.
        /// </summary>
        /// <param name="registry">Registry of instance into which we want to install</param>
        /// <param name="versionCriteria">Compatible versions of instance</param>
        /// <param name="opts">Installer options</param>
        /// <param name="toInstall">Mods we want to install</param>
        /// <returns>
        /// True if it's possible to install these mods, false otherwise
        /// </returns>
        private bool CanInstall(
            IRegistryQuerier            registry,
            KspVersionCriteria          versionCriteria,
            RelationshipResolverOptions opts,
            List<CkanModule>            toInstall
        )
        {
            string request = toInstall.Select(m => m.identifier).Aggregate((a, b) => $"{a}, {b}");
            try
            {
                RelationshipResolver resolver = new RelationshipResolver(
                    toInstall,
                    null,
                    opts, registry, versionCriteria
                );

                if (resolver.ModList().Count() >= toInstall.Count(m => !m.IsMetapackage))
                {
                    // We can install with no further dependencies
                    string recipe = resolver.ModList()
                        .Select(m => m.identifier)
                        .Aggregate((a, b) => $"{a}, {b}");
                    log.Debug($"Installable: {request}: {recipe}");
                    return true;
                }
                else
                {
                    string problems = resolver.ConflictList.Values
                        .Aggregate((a, b) => $"{a}, {b}");
                    log.Debug($"Can't install {request}: {problems}");
                    return false;
                }
            }
            catch (TooManyModsProvideKraken k)
            {
                // One of the dependencies is virtual
                foreach (CkanModule mod in k.modules)
                {
                    // Try each option recursively to see if any are successful
                    if (CanInstall(registry, versionCriteria, opts, toInstall.Concat(new List<CkanModule>() { mod }).ToList()))
                    {
                        // Child call will emit debug output, so we don't need to here
                        return true;
                    }
                }
                log.Debug($"Can't install {request}: Can't install provider of {k.requested}");
            }
            catch (InconsistentKraken k)
            {
                log.Debug($"Can't install {request}: {k.ShortDescription}");
            }
            catch (Exception ex)
            {
                log.Debug($"Can't install {request}: {ex.Message}");
            }
            return false;
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
                        RelationshipResolver.DependsOnlyOpts()
                    )
                );
            }
        }

    }
}
