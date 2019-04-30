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
        // Build up the list of who recommends what
        private Dictionary<CkanModule, List<string>> getDependersIndex(
            IEnumerable<CkanModule> sourceModules,
            IRegistryQuerier        registry,
            HashSet<CkanModule>     toExclude
        )
        {
            Dictionary<CkanModule, List<string>> dependersIndex = new Dictionary<CkanModule, List<string>>();
            foreach (CkanModule mod in sourceModules)
            {
                foreach (List<RelationshipDescriptor> relations in new List<List<RelationshipDescriptor>>() { mod.recommends, mod.suggests })
                {
                    if (relations != null)
                    {
                        foreach (RelationshipDescriptor rel in relations)
                        {
                            List<CkanModule> providers = rel.LatestAvailableWithProvides(
                                registry,
                                CurrentInstance.VersionCriteria()
                            );
                            foreach (CkanModule provider in providers)
                            {
                                if (!registry.IsInstalled(provider.identifier)
                                    && !toExclude.Any(m => m.identifier == provider.identifier))
                                {
                                    List<string> dependers;
                                    if (dependersIndex.TryGetValue(provider, out dependers))
                                    {
                                        // Add the dependent mod to the list of reasons this dependency is shown.
                                        dependers.Add(mod.identifier);
                                    }
                                    else
                                    {
                                        // Add a new entry if this provider isn't listed yet.
                                        dependersIndex.Add(provider, new List<string>() { mod.identifier });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dependersIndex;
        }

        private IEnumerable<ListViewItem> getRecSugRows(
            IEnumerable<CkanModule> sourceModules,
            IRegistryQuerier        registry,
            HashSet<CkanModule>     toInstall
        )
        {
            Dictionary<CkanModule, List<string>> dependersIndex = getDependersIndex(sourceModules, registry, toInstall);
            foreach (CkanModule mod in sourceModules.Where(m => m.recommends != null))
            {
                foreach (RelationshipDescriptor rel in mod.recommends)
                {
                    List<CkanModule> providers = rel.LatestAvailableWithProvides(
                        registry,
                        CurrentInstance.VersionCriteria()
                    );
                    foreach (CkanModule provider in providers)
                    {
                        List<string> dependers;
                        if (!registry.IsInstalled(provider.identifier)
                            && !toInstall.Any(m => m.identifier == provider.identifier)
                            && dependersIndex.TryGetValue(provider, out dependers)
                            && CanInstall(registry, CurrentInstance.VersionCriteria(),
                                RelationshipResolver.DependsOnlyOpts(),
                                toInstall.ToList().Concat(new List<CkanModule>() { provider }).ToList()))
                        {
                            dependersIndex.Remove(provider);
                            yield return getRecSugItem(
                                provider,
                                string.Join(", ", dependers),
                                RecommendationsGroup,
                                providers.Count <= 1 || provider.identifier == (rel as ModuleRelationshipDescriptor)?.name
                            );
                        }
                    }
                }
            }
            foreach (CkanModule mod in sourceModules.Where(m => m.suggests != null))
            {
                foreach (RelationshipDescriptor rel in mod.suggests)
                {
                    List<CkanModule> providers = rel.LatestAvailableWithProvides(
                        registry,
                        CurrentInstance.VersionCriteria()
                    );
                    foreach (CkanModule provider in providers)
                    {
                        List<string> dependers;
                        if (!registry.IsInstalled(provider.identifier)
                            && !toInstall.Any(m => m.identifier == provider.identifier)
                            && dependersIndex.TryGetValue(provider, out dependers)
                            && CanInstall(registry, CurrentInstance.VersionCriteria(),
                                RelationshipResolver.DependsOnlyOpts(),
                                toInstall.ToList().Concat(new List<CkanModule>() { provider }).ToList()))
                        {
                            dependersIndex.Remove(provider);
                            yield return getRecSugItem(
                                provider,
                                string.Join(", ", dependers),
                                SuggestionsGroup,
                                false
                            );
                        }
                    }
                }
            }
        }

        private void ShowRecSugDialog(IEnumerable<ListViewItem> rows, HashSet<CkanModule> toInstall)
        {
            Util.Invoke(this, () =>
            {
                RecommendedModsToggleCheckbox.Checked = true;
                tabController.RenameTab("ChooseRecommendedModsTabPage", Properties.Resources.MainRecommendationsTitle);

                RecommendedModsListView.Items.Clear();
                RecommendedModsListView.Items.AddRange(rows.ToArray());
            });

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

        private ListViewItem getRecSugItem(CkanModule module, string descrip, ListViewGroup group, bool check)
        {
            ListViewItem item = new ListViewItem()
            {
                Tag     = module,
                Checked = check,
                Group   = group,
                Text    = Manager.Cache.IsMaybeCachedZip(module)
                    ? string.Format(Properties.Resources.MainChangesetCached, module.name, module.version)
                    : string.Format(Properties.Resources.MainChangesetHostSize, module.name, module.version, module.download.Host ?? "", CkanModule.FmtSize(module.download_size))
            };

            item.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = descrip          });
            item.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = module.@abstract });
            return item;
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
            var toInstall   = new HashSet<CkanModule>();
            var rows = getRecSugRows(registry.InstalledModules.Select(im => im.Module), registry, toInstall);
            if (!rows.Any())
            {
                GUI.user.RaiseError(Properties.Resources.MainRecommendationsNoneFound);
                return;
            }

            // Prompt user to choose
            installCanceled = false;
            ShowRecSugDialog(rows, toInstall);
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
