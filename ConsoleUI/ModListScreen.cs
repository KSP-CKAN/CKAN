using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Autofac;

using CKAN.ConsoleUI.Toolkit;
using CKAN.Extensions;
using CKAN.Games;
using CKAN.Configuration;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen listing mods available for a given install
    /// </summary>
    public class ModListScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">Game instance manager object containing the current instance</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="regMgr">Registry manager for the current instance</param>
        /// <param name="game">The game of the current instance, used for getting known versions</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        /// <param name="regTheme">The theme to use for the registry update flow, if needed</param>
        public ModListScreen(GameInstanceManager mgr, RepositoryDataManager repoData, RegistryManager regMgr, IGame game, bool dbg, ConsoleTheme regTheme)
        {
            debug    = dbg;
            manager  = mgr;
            this.regMgr   = regMgr;
            registry = regMgr.registry;
            this.repoData = repoData;

            moduleList = new ConsoleListBox<CkanModule>(
                1, 4, -1, -2,
                GetAllMods(regTheme),
                new List<ConsoleListBoxColumn<CkanModule>>() {
                    new ConsoleListBoxColumn<CkanModule>() {
                        Header   = "",
                        Width    = 1,
                        Renderer = StatusSymbol
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = Properties.Resources.ModListNameHeader,
                        Width    = null,
                        Renderer = m => m.name ?? ""
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = Properties.Resources.ModListVersionHeader,
                        Width    = 10,
                        Renderer = m => ModuleInstaller.StripEpoch(m.version?.ToString() ?? ""),
                        Comparer = (a, b) => a.version.CompareTo(b.version)
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = Properties.Resources.ModListMaxGameVersionHeader,
                        Width    = 20,
                        Renderer = m => registry.LatestCompatibleGameVersion(game.KnownVersions, m.identifier)?.ToString() ?? "",
                        Comparer = (a, b) => registry.LatestCompatibleGameVersion(game.KnownVersions, a.identifier).CompareTo(registry.LatestCompatibleGameVersion(game.KnownVersions, b.identifier))
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = Properties.Resources.ModListDownloadsHeader,
                        Width    = 12,
                        Renderer = m => repoData.GetDownloadCount(registry.Repositories.Values, m.identifier)
                                                ?.ToString()
                                                ?? "",
                        Comparer = (a, b) => (repoData.GetDownloadCount(registry.Repositories.Values, a.identifier) ?? 0)
                                             .CompareTo(repoData.GetDownloadCount(registry.Repositories.Values, b.identifier) ?? 0),
                    }
                },
                1, 0, ListSortDirection.Descending,
                (CkanModule m, string filter) => {
                    // Search for author
                    if (filter.StartsWith("@")) {
                        string authorFilt = filter.Substring(1);
                        if (string.IsNullOrEmpty(authorFilt)) {
                            return true;
                        } else {
                            // Remove special characters from search term
                            authorFilt = CkanModule.nonAlphaNums.Replace(authorFilt, "");
                            return m.SearchableAuthors.Any((author) => author.IndexOf(authorFilt, StringComparison.CurrentCultureIgnoreCase) == 0);
                        }
                    // Other special search params: installed, updatable, new, conflicting and dependends
                    } else if (filter.StartsWith("~")) {
                        if (filter.Length <= 1) {
                            // Don't blank the list for just "~" by itself
                            return true;
                        } else
                        {
                            switch (filter.Substring(1, 1)) {
                            case "i":
                                return registry.IsInstalled(m.identifier, false);
                            case "u":
                                return upgradeableGroups?[true].Any(upg => upg.identifier == m.identifier) ?? false;
                            case "n":
                                // Filter new
                                return recent.Contains(m.identifier);
                            case "c":
                                if (m.conflicts != null) {
                                    string conflictsWith = filter.Substring(2);
                                    // Search for mods depending on a given mod
                                    foreach (var rel in m.conflicts) {
                                        if (rel.StartsWith(conflictsWith)) {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                            case "d":
                                if (m.depends != null) {
                                    string dependsOn = filter.Substring(2);
                                    // Search for mods depending on a given mod
                                    foreach (var rel in m.depends) {
                                        if (rel.StartsWith(dependsOn)) {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                        }
                        }

                        return false;
                    } else {
                        filter = CkanModule.nonAlphaNums.Replace(filter, "");

                        return m.SearchableIdentifier.IndexOf( filter, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || m.SearchableName.IndexOf(       filter, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || m.SearchableAbstract.IndexOf(   filter, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || m.SearchableDescription.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    }
                }
            );

            searchBox = new ConsoleField(-searchWidth, 2, -1) {
                GhostText = () => Focused() == searchBox
                    ? Properties.Resources.ModListSearchFocusedGhostText
                    : Properties.Resources.ModListSearchUnfocusedGhostText
            };
            searchBox.OnChange += (ConsoleField sender, string newValue) => {
                moduleList.FilterString = newValue;
            };

            AddObject(new ConsoleLabel(
                1, 2, -searchWidth - 2,
                () => string.Format(Properties.Resources.ModListCount, moduleList.VisibleRowCount())
            ));
            AddObject(searchBox);
            AddObject(moduleList);

            AddBinding(Keys.CtrlP, (object sender, ConsoleTheme theme) => PlayGame());
            AddBinding(Keys.CtrlQ, (object sender, ConsoleTheme theme) => false);
            AddBinding(Keys.AltX,  (object sender, ConsoleTheme theme) => false);
            AddBinding(Keys.F1,    (object sender, ConsoleTheme theme) => Help(theme));
            AddBinding(Keys.AltH,  (object sender, ConsoleTheme theme) => Help(theme));
            AddBinding(Keys.F5,    (object sender, ConsoleTheme theme) => UpdateRegistry(theme));
            AddBinding(Keys.CtrlR, (object sender, ConsoleTheme theme) => UpdateRegistry(theme));
            AddBinding(Keys.CtrlU, (object sender, ConsoleTheme theme) => UpgradeAll(theme));

            // Now a bunch of convenience shortcuts so you don't get stuck in the search box
            searchBox.AddBinding(Keys.PageUp, (object sender, ConsoleTheme theme) => {
                SetFocus(moduleList);
                return true;
            });
            searchBox.AddBinding(Keys.PageDown, (object sender, ConsoleTheme theme) => {
                SetFocus(moduleList);
                return true;
            });
            searchBox.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                SetFocus(moduleList);
                return true;
            });

            moduleList.AddBinding(Keys.CtrlF, (object sender, ConsoleTheme theme) => {
                SetFocus(searchBox);
                return true;
            });
            moduleList.AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => {
                searchBox.Clear();
                return true;
            });

            moduleList.AddTip(Properties.Resources.Enter, Properties.Resources.Details,
                () => moduleList.Selection != null
            );
            moduleList.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                if (moduleList.Selection != null) {
                    LaunchSubScreen(theme, new ModInfoScreen(manager, registry, plan, moduleList.Selection, debug));
                }
                return true;
            });

            // Conditionally show only one of these based on selected mod status

            moduleList.AddTip("+", Properties.Resources.ModListInstallTip,
                () => moduleList.Selection != null && !moduleList.Selection.IsDLC
                    && !registry.IsInstalled(moduleList.Selection.identifier, false)
            );
            moduleList.AddTip("+", Properties.Resources.ModListUpgradeTip,
                () => moduleList.Selection != null && !moduleList.Selection.IsDLC
                    && (upgradeableGroups?[true].Any(upg => upg.identifier == moduleList.Selection.identifier) ?? false)
            );
            moduleList.AddTip("+", Properties.Resources.ModListReplaceTip,
                () => moduleList.Selection != null
                    && registry.GetReplacement(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria()) != null
            );
            moduleList.AddBinding(Keys.Plus, (object sender, ConsoleTheme theme) => {
                if (moduleList.Selection != null && !moduleList.Selection.IsDLC) {
                    if (!registry.IsInstalled(moduleList.Selection.identifier, false)) {
                        plan.ToggleInstall(moduleList.Selection);
                    } else if (registry.IsInstalled(moduleList.Selection.identifier, false)
                            && (upgradeableGroups?[true].Any(upg => upg.identifier == moduleList.Selection.identifier) ?? false)) {
                        plan.ToggleUpgrade(moduleList.Selection);
                    } else if (registry.GetReplacement(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria()) != null) {
                        plan.ToggleReplace(moduleList.Selection.identifier);
                    }
                }
                return true;
            });

            moduleList.AddTip("-", Properties.Resources.ModListRemoveTip,
                () => moduleList.Selection != null && !moduleList.Selection.IsDLC
                    && registry.IsInstalled(moduleList.Selection.identifier, false)
                    && !registry.IsAutodetected(moduleList.Selection.identifier)
            );
            moduleList.AddBinding(Keys.Minus, (object sender, ConsoleTheme theme) => {
                if (moduleList.Selection != null && !moduleList.Selection.IsDLC
                    && registry.IsInstalled(moduleList.Selection.identifier, false)
                    && !registry.IsAutodetected(moduleList.Selection.identifier)) {
                    plan.ToggleRemove(moduleList.Selection);
                }
                return true;
            });

            moduleList.AddTip("F8", Properties.Resources.ModListAutoInstTip,
                () => moduleList.Selection != null && !moduleList.Selection.IsDLC
                    && (!registry.InstalledModule(moduleList.Selection.identifier)?.AutoInstalled ?? false)
            );
            moduleList.AddTip("F8", Properties.Resources.ModListUserSelectedTip,
                () => moduleList.Selection != null && !moduleList.Selection.IsDLC
                    && (registry.InstalledModule(moduleList.Selection.identifier)?.AutoInstalled ?? false)
            );
            moduleList.AddBinding(Keys.F8, (object sender, ConsoleTheme theme) => {
                InstalledModule im = registry.InstalledModule(moduleList.Selection.identifier);
                if (im != null && !moduleList.Selection.IsDLC) {
                    im.AutoInstalled = !im.AutoInstalled;
                    regMgr.Save(false);
                }
                return true;
            });

            AddTip("F9", Properties.Resources.ModListApplyChangesTip, plan.NonEmpty);
            AddBinding(Keys.F9, (object sender, ConsoleTheme theme) => {
                ApplyChanges(theme);
                return true;
            });

            // Show total download size of all installed mods
            AddObject(new ConsoleLabel(
                1, -1, searchWidth,
                () => string.Format(Properties.Resources.ModListSizeOnDisk, CkanModule.FmtSize(totalInstalledDownloadSize())),
                null,
                th => th.DimLabelFg
            ));

            AddObject(new ConsoleLabel(
                -searchWidth, -1, -2,
                () => {
                    var days = Math.Round(timeSinceUpdate.TotalDays);
                    return days <  1 ? ""
                        :  days == 1 ? string.Format(Properties.Resources.ModListUpdatedDayAgo,  days)
                        :              string.Format(Properties.Resources.ModListUpdatedDaysAgo, days);
                },
                null,
                (ConsoleTheme th) => {
                    return timeSinceUpdate < RepositoryDataManager.TimeTillStale     ? th.RegistryUpToDate
                        :  timeSinceUpdate < RepositoryDataManager.TimeTillVeryStale ? th.RegistryStale
                        :                                                              th.RegistryVeryStale;
                }
            ));

            List<ConsoleMenuOption> opts = new List<ConsoleMenuOption>() {
                new ConsoleMenuOption(Properties.Resources.ModListPlayMenu, "",
                                      Properties.Resources.ModListPlayMenuTip,
                                      true, null, null, null, true,
                                      () => new ConsolePopupMenu(
                                                manager.CurrentInstance
                                                       .game
                                                       .DefaultCommandLines(manager.SteamLibrary,
                                                                            new DirectoryInfo(manager.CurrentInstance.GameDir()))
                                                       .Select((cmd, i) => new ConsoleMenuOption(
                                                                               cmd,
                                                                               i == 0 ? $"{Properties.Resources.Ctrl}+P"
                                                                                      : "",
                                                                               cmd, true,
                                                                               th => PlayGame(cmd)))
                                                       .ToList())),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListSortMenu, "",
                    Properties.Resources.ModListSortMenuTip,
                    true, null, null, moduleList.SortMenu()),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListRefreshMenu, $"F5, {Properties.Resources.Ctrl}+R",
                    Properties.Resources.ModListRefreshMenuTip,
                    true, (ConsoleTheme th) => UpdateRegistry(th)),
                new ConsoleMenuOption(Properties.Resources.ModListUpgradeMenu, $"{Properties.Resources.Ctrl}+U",
                    Properties.Resources.ModListUpgradeMenuTip,
                    true, UpgradeAll, null, null, HasAnyUpgradeable()),
                new ConsoleMenuOption(Properties.Resources.ModListAuditRecsMenu, "",
                    Properties.Resources.ModListAuditRecsMenuTip,
                    true, ViewSuggestions),
                new ConsoleMenuOption(Properties.Resources.ModListImportMenu, "",
                    Properties.Resources.ModListImportMenuTip,
                    true, ImportDownloads),
                new ConsoleMenuOption(Properties.Resources.ModListExportMenu, "",
                    Properties.Resources.ModListExportMenuTip,
                    true, ExportInstalled),
                new ConsoleMenuOption(Properties.Resources.ModListInstallFromCkanMenu, "",
                    Properties.Resources.ModListInstallFromCkanMenuTip,
                    true, InstallFromCkan),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListInstanceSettingsMenu, "",
                    Properties.Resources.ModListInstanceSettingsMenuTip,
                    true, InstanceSettings),
                new ConsoleMenuOption(Properties.Resources.ModListSelectInstanceMenu, "",
                    Properties.Resources.ModListSelectInstanceMenuTip,
                    true, SelectInstall),
                new ConsoleMenuOption(Properties.Resources.ModListAuthTokenMenu, "",
                    Properties.Resources.ModListAuthTokenMenuTip,
                    true, EditAuthTokens),
                new ConsoleMenuOption(Properties.Resources.ModListFilterMenu, "",
                    Properties.Resources.ModListFilterMenuTip,
                    true, EditInstallFilters),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListHelpMenu, helpKey,
                    Properties.Resources.ModListHelpMenuTip,
                    true, Help),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListQuitMenu, $"{Properties.Resources.Ctrl}+Q",
                    Properties.Resources.ModListQuitMenuTip,
                    true, (ConsoleTheme th) => false)
            };
            if (debug) {
                opts.Add(null);
                opts.Add(new ConsoleMenuOption(Properties.Resources.ModListCaptureKeyMenu, "",
                    Properties.Resources.ModListCaptureKeyMenuTip,
                    true, CaptureKey));
            }
            mainMenu = new ConsolePopupMenu(opts);
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"{Meta.GetProductName()} {Meta.GetVersion()}";
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return $"{manager.CurrentInstance.game.ShortName} {manager.CurrentInstance.Version()} ({manager.CurrentInstance.Name})";
        }

        // Alt+H doesn't work on Mac, but F1 does, and we need
        // an option other than F1 for terminals that open their own help.
        private static readonly string helpKey = Platform.IsMac
            ? "F1"
            : $"F1, {Properties.Resources.Alt}+H";

        private bool ImportDownloads(ConsoleTheme theme)
        {
            DownloadImportDialog.ImportDownloads(theme, manager.CurrentInstance, repoData, manager.Cache, plan);
            RefreshList(theme);
            return true;
        }

        private bool CaptureKey(ConsoleTheme theme)
        {
            ConsoleKeyInfo k = default;
            ConsoleMessageDialog keyprompt = new ConsoleMessageDialog(Properties.Resources.ModListPressAKey, new List<string>());
            keyprompt.Run(theme, (ConsoleTheme th) => {
                k = Console.ReadKey(true);
            });
            ConsoleMessageDialog output = new ConsoleMessageDialog(
                $"Key: {k.Key,18}\nKeyChar:           0x{(int)k.KeyChar:x2}\nModifiers: {k.Modifiers,12}",
                new List<string> { Properties.Resources.OK }
            );
            output.Run(theme);
            return true;
        }

        private bool HasAnyUpgradeable()
        {
            return (upgradeableGroups?[true].Count ?? 0) > 0;
        }

        private bool UpgradeAll(ConsoleTheme theme)
        {
            plan.Upgrade.UnionWith(upgradeableGroups?[true].Select(m => m.identifier)
                                   ?? Enumerable.Empty<string>());
            return true;
        }

        private bool ViewSuggestions(ConsoleTheme theme)
        {
            ChangePlan reinstall = new ChangePlan();
            foreach (InstalledModule im in registry.InstalledModules) {
                // Only check mods that are still available
                try {
                    if (registry.LatestAvailable(im.identifier, manager.CurrentInstance.VersionCriteria()) != null) {
                        reinstall.Install.Add(im.Module);
                    }
                } catch {
                    // The registry object badly needs an IsAvailable check
                }
            }
            try {
                DependencyScreen ds = new DependencyScreen(manager, registry, reinstall, new HashSet<string>(), debug);
                if (ds.HaveOptions()) {
                    LaunchSubScreen(theme, ds);
                    bool needRefresh = false;
                    // Copy the right ones into our real plan
                    foreach (CkanModule mod in reinstall.Install) {
                        if (!registry.IsInstalled(mod.identifier, false)) {
                            plan.Install.Add(mod);
                            needRefresh = true;
                        }
                    }
                    if (needRefresh) {
                        RefreshList(theme);
                    }
                } else {
                    RaiseError(Properties.Resources.ModListAuditNotFound);
                }
            } catch (ModuleNotFoundKraken k) {
                RaiseError($"{k.module} {k.version}: {k.Message}");
            }
            return true;
        }

        private bool PlayGame()
            => PlayGame(manager.CurrentInstance
                               .game
                               .DefaultCommandLines(manager.SteamLibrary,
                                                    new DirectoryInfo(manager.CurrentInstance.GameDir()))
                               .FirstOrDefault());

        private bool PlayGame(string commandLine)
        {
            manager.CurrentInstance.PlayGame(commandLine);
            return true;
        }

        private bool UpdateRegistry(ConsoleTheme theme, bool showNewModsPrompt = true)
        {
            ProgressScreen ps = new ProgressScreen(
                Properties.Resources.ModListUpdateRegistryTitle,
                Properties.Resources.ModListUpdateRegistryMessage
            );
            LaunchSubScreen(theme, ps, (ConsoleTheme th) => {
                HashSet<string> availBefore = new HashSet<string>(
                    Array.ConvertAll(
                        registry.CompatibleModules(
                            manager.CurrentInstance.VersionCriteria()
                        ).ToArray(),
                        l => l.identifier
                    )
                );
                recent.Clear();
                try {
                    repoData.Update(registry.Repositories.Values.ToArray(),
                                    manager.CurrentInstance.game,
                                    false,
                                    new NetAsyncDownloader(ps),
                                    ps);
                } catch (Exception ex) {
                    // There can be errors while you re-install mods with changed metadata
                    ps.RaiseError(ex.Message + ex.StackTrace);
                }
                // Update recent with mods that were updated in this pass
                foreach (CkanModule mod in registry.CompatibleModules(
                        manager.CurrentInstance.VersionCriteria()
                    )) {
                    if (!availBefore.Contains(mod.identifier)) {
                        recent.Add(mod.identifier);
                    }
                }
            });
            if (showNewModsPrompt && recent.Count > 0 && RaiseYesNoDialog(newModPrompt(recent.Count))) {
                searchBox.Clear();
                moduleList.FilterString = searchBox.Value = "~n";
            }
            RefreshList(theme);
            return true;
        }

        private string newModPrompt(int howMany)
        {
            return howMany == 1
                ? string.Format(Properties.Resources.ModListNewMod,  howMany)
                : string.Format(Properties.Resources.ModListNewMods, howMany);
        }

        private bool ScanForMods()
        {
            try {
                regMgr.ScanUnmanagedFiles();
            } catch (InconsistentKraken ex) {
                // Warn about inconsistent state
                RaiseError(Properties.Resources.ModListScanBad, ex.Message);
            }
            return true;
        }

        private bool InstanceSettings(ConsoleTheme theme)
        {
            var prevRepos   = new SortedDictionary<string, Repository>(registry.Repositories);
            var prevVerCrit = manager.CurrentInstance.VersionCriteria();
            LaunchSubScreen(theme, new GameInstanceEditScreen(manager, repoData, manager.CurrentInstance));
            if (!registry.Repositories.DictionaryEquals(prevRepos)) {
                // Repos changed, need to fetch them
                UpdateRegistry(theme, false);
                RefreshList(theme);
            } else if (!manager.CurrentInstance.VersionCriteria().Equals(prevVerCrit)) {
                // VersionCriteria changed, need to re-check what is compatible
                RefreshList(theme);
            }
            return true;
        }

        private bool SelectInstall(ConsoleTheme theme)
        {
            GameInstance prevInst = manager.CurrentInstance;
            var prevRepos = new SortedDictionary<string, Repository>(registry.Repositories);
            var prevVerCrit = prevInst.VersionCriteria();
            LaunchSubScreen(theme, new GameInstanceListScreen(manager, repoData));
            if (!prevInst.Equals(manager.CurrentInstance)) {
                // Game instance changed, reset everything
                plan.Reset();
                regMgr = RegistryManager.Instance(manager.CurrentInstance, repoData);
                registry = regMgr.registry;
                RefreshList(theme);
            } else if (!registry.Repositories.DictionaryEquals(prevRepos)) {
                // Repos changed, need to fetch them
                UpdateRegistry(theme, false);
                RefreshList(theme);
            } else if (!manager.CurrentInstance.VersionCriteria().Equals(prevVerCrit)) {
                // VersionCriteria changed, need to re-check what is compatible
                RefreshList(theme);
            }
            return true;
        }

        private bool EditAuthTokens(ConsoleTheme theme)
        {
            LaunchSubScreen(theme, new AuthTokenScreen());
            return true;
        }

        private bool EditInstallFilters(ConsoleTheme theme)
        {
            LaunchSubScreen(theme, new InstallFiltersScreen(
                ServiceLocator.Container.Resolve<IConfiguration>(),
                manager.CurrentInstance
            ));
            return true;
        }

        private void RefreshList(ConsoleTheme theme)
        {
            // In the constructor this is called while moduleList is being populated, just do nothing in this case.
            // ModListScreen -> moduleList = (GetAllMods ...) -> UpdateRegistry -> RefreshList
            moduleList?.SetData(GetAllMods(theme, true));
        }

        private List<CkanModule> allMods = null;

        private List<CkanModule> GetAllMods(ConsoleTheme theme, bool force = false)
        {
            timeSinceUpdate = repoData.LastUpdate(registry.Repositories.Values);
            ScanForMods();
            if (allMods == null || force) {
                if (!registry?.HasAnyAvailable() ?? false)
                {
                    UpdateRegistry(theme, false);
                }
                var crit = manager.CurrentInstance.VersionCriteria();
                allMods = new List<CkanModule>(registry.CompatibleModules(crit));
                foreach (InstalledModule im in registry.InstalledModules) {
                    var m = Utilities.DefaultIfThrows(() => registry.LatestAvailable(im.identifier, crit));
                    if (m == null) {
                        // Add unavailable installed mods to the list
                        allMods.Add(im.Module);
                    }
                }
                upgradeableGroups = registry
                                    .CheckUpgradeable(manager.CurrentInstance, new HashSet<string>());
            }
            return allMods;
        }

        private bool ExportInstalled(ConsoleTheme theme)
        {
            try {
                // Save the mod list as "depends" without the installed versions.
                // Because that's supposed to work.
                regMgr.Save(true);
                string path = Path.Combine(
                    Platform.FormatPath(manager.CurrentInstance.CkanDir()),
                    $"{Properties.Resources.ModListExportPrefix}-{manager.CurrentInstance.Name}.ckan"
                );
                RaiseError(Properties.Resources.ModListExported, path);
            } catch (Exception ex) {
                RaiseError(Properties.Resources.ModListExportFailed, ex.Message);
            }
            return true;
        }

        private bool InstallFromCkan(ConsoleTheme theme)
        {
            var modules = InstallFromCkanDialog.ChooseCkanFiles(theme, manager.CurrentInstance);
            if (modules.Length > 0) {
                var crit = manager.CurrentInstance.VersionCriteria();
                var installed = regMgr.registry.InstalledModules.Select(inst => inst.Module).ToList();
                var cp = new ChangePlan();
                cp.Install.UnionWith(
                    modules.Concat(
                        modules.Where(m => m.IsMetapackage && m.depends != null)
                               .SelectMany(m => m.depends.Where(rel => !rel.MatchesAny(installed, null, null))
                                                         .Select(rel =>
                                                             // If there's a compatible match, return it
                                                             // Metapackages aren't intending to prompt users to choose providing mods
                                                             rel.ExactMatch(regMgr.registry, crit, installed, modules)
                                                             // Otherwise look for incompatible
                                                             ?? rel.ExactMatch(regMgr.registry, null, installed, modules))
                                                         .Where(mod => mod != null))));
                LaunchSubScreen(theme, new InstallScreen(manager, repoData, cp, debug));
                RefreshList(theme);
            }
            return true;
        }

        private bool Help(ConsoleTheme theme)
        {
            ModListHelpDialog hd = new ModListHelpDialog();
            hd.Run(theme);
            DrawBackground(theme);
            return true;
        }

        private bool ApplyChanges(ConsoleTheme theme)
        {
            if (plan.NonEmpty())
            {
                LaunchSubScreen(theme, new InstallScreen(manager, repoData, plan, debug));
                RefreshList(theme);
            }
            return true;
        }

        /// <summary>
        /// Return the symbol to use to represent a mod's StatusSymbol.
        /// This can't be static because the user's installation plans are part of the object.
        /// </summary>
        /// <param name="m">The mod</param>
        /// <returns>
        /// String containing symbol to use
        /// </returns>
        public string StatusSymbol(CkanModule m)
        {
            return StatusSymbol(plan.GetModStatus(manager, registry, m.identifier,
                                                  upgradeableGroups?[true] ?? new List<CkanModule>()));
        }

        /// <summary>
        /// Return the symbol to use to represent a mod's StatusSymbol.
        /// This can't be static because the user's installation plans are part of the object.
        /// </summary>
        /// <param name="st">Install status of a mod</param>
        /// <returns>
        /// String containing symbol to use
        /// </returns>
        public static string StatusSymbol(InstallStatus st)
        {
            switch (st) {
                case InstallStatus.Unavailable:   return unavailable;
                case InstallStatus.Removing:      return removing;
                case InstallStatus.Upgrading:     return upgrading;
                case InstallStatus.Upgradeable:   return upgradable;
                case InstallStatus.Installed:     return installed;
                case InstallStatus.AutoInstalled: return autoInstalled;
                case InstallStatus.Installing:    return installing;
                case InstallStatus.NotInstalled:  return notinstalled;
                case InstallStatus.AutoDetected:  return autodetected;
                case InstallStatus.Replaceable:   return replaceable;
                case InstallStatus.Replacing:     return replacing;
                default:                          return "";
            }
        }

        private long totalInstalledDownloadSize()
        {
            long total = 0;
            foreach (InstalledModule im in registry.InstalledModules) {
                total += im.Module.install_size > 0
                    ? im.Module.install_size
                    : im.Module.download_size;
            }
            return total;
        }

        private readonly GameInstanceManager                manager;
        private          RegistryManager                    regMgr;
        private          Registry                           registry;
        private readonly RepositoryDataManager              repoData;
        private          Dictionary<bool, List<CkanModule>> upgradeableGroups;
        private readonly bool                               debug;
        private          TimeSpan                           timeSinceUpdate = TimeSpan.Zero;

        private readonly ConsoleField               searchBox;
        private readonly ConsoleListBox<CkanModule> moduleList;

        private readonly ChangePlan      plan   = new ChangePlan();
        private readonly HashSet<string> recent = new HashSet<string>();

        private int searchWidth => Math.Max(30, Math.Max(
            Properties.Resources.ModListSearchFocusedGhostText.Length,
            Properties.Resources.ModListSearchUnfocusedGhostText.Length
        ));

        private static readonly string unavailable   = "!";
        private static readonly string notinstalled  = " ";
        private static readonly string installed     = Symbols.checkmark;
        private static readonly string autoInstalled = Symbols.feminineOrdinal;
        private static readonly string installing    = "+";
        private static readonly string upgradable    = Symbols.greaterEquals;
        private static readonly string upgrading     = "^";
        private static readonly string removing      = "-";
        private static readonly string autodetected  = Symbols.infinity;
        private static readonly string replaceable   = Symbols.doubleGreater;
        private static readonly string replacing     = Symbols.plusMinus;
    }

    /// <summary>
    /// Object representing sets of mods we plan to install, upgrade, and remove
    /// </summary>
    public class ChangePlan {

        /// <summary>
        /// Initialize the plan
        /// </summary>
        public ChangePlan() { }

        /// <summary>
        /// Add or remove a mod from the remove list
        /// </summary>
        /// <param name="mod">The mod to add or remove</param>
        public void ToggleRemove(CkanModule mod)
        {
            Install.Remove(mod);
            Upgrade.Remove(mod.identifier);
            toggleContains(Remove, mod.identifier);
        }

        /// <summary>
        /// Add or remove a mod from the install list
        /// </summary>
        /// <param name="mod">The mod to add or remove</param>
        public void ToggleInstall(CkanModule mod)
        {
            Upgrade.Remove(mod.identifier);
            Remove.Remove(mod.identifier);
            toggleContains(Install, mod);
        }

        /// <summary>
        /// Add or remove a mod from the upgrade list
        /// </summary>
        /// <param name="mod">The mod to add or remove</param>
        public void ToggleUpgrade(CkanModule mod)
        {
            Install.Remove(mod);
            Remove.Remove(mod.identifier);
            toggleContains(Upgrade, mod.identifier);
        }

        /// <summary>
        /// Add or remove a mod from the replace list
        /// </summary>
        /// <param name="identifier">The mod to Replace</param>
        public void ToggleReplace(string identifier)
        {
            Remove.Remove(identifier);
            toggleContains(Replace, identifier);
        }

        /// <summary>
        /// Return true if we are planning to make any changes, false otherwise
        /// </summary>
        public bool NonEmpty()
        {
             return Install.Count > 0 || Upgrade.Count > 0 || Remove.Count > 0 || Replace.Count > 0;
        }

        /// <summary>
        /// Reset the plan to no changes
        /// </summary>
        public void Reset()
        {
            Install.Clear();
            Upgrade.Clear();
            Remove.Clear();
        }

        /// <summary>
        /// Return a mod's current status
        /// This can't be static because the user's installation plans are part of the object.
        /// This function is extremely performance-sensitive because it's the default sort for
        /// the main mod list, so anything in here should be O(1) and fast.
        /// </summary>
        /// <param name="manager">Game instance manager containing the instances</param>
        /// <param name="registry">Registry of instance being displayed</param>
        /// <param name="identifier">The mod</param>
        /// <param name="upgradeable">List of modules that can be upgraded</param>
        /// <returns>
        /// Status of mod
        /// </returns>
        public InstallStatus GetModStatus(GameInstanceManager manager,
                                          IRegistryQuerier registry,
                                          string identifier,
                                          List<CkanModule> upgradeable)
        {
            if (registry.IsInstalled(identifier, false)) {
                if (Remove.Contains(identifier)) {
                    return InstallStatus.Removing;
                } else if (upgradeable.Any(m => m.identifier == identifier)) {
                    if (Upgrade.Contains(identifier)) {
                        return InstallStatus.Upgrading;
                    } else {
                        return InstallStatus.Upgradeable;
                    }
                } else if (registry.IsAutodetected(identifier)) {
                    return InstallStatus.AutoDetected;
                } else if (Replace.Contains(identifier)) {
                    return InstallStatus.Replacing;
                } else if (registry.GetReplacement(identifier, manager.CurrentInstance.VersionCriteria()) != null) {
                    return InstallStatus.Replaceable;
                } else if (!IsAnyAvailable(registry, identifier)) {
                    return InstallStatus.Unavailable;
                } else if (registry.InstalledModule(identifier)?.AutoInstalled ?? false) {
                    return InstallStatus.AutoInstalled;
                } else {
                    return InstallStatus.Installed;
                }
            } else {
                foreach (CkanModule m in Install)
                {
                    if (m.identifier == identifier)
                    {
                        return InstallStatus.Installing;
                    }
                }
                return InstallStatus.NotInstalled;
            }
        }

        /// <summary>
        /// Check whether an identifier is anywhere in the registry.
        /// </summary>
        /// <param name="registry">Reference to registry to query</param>
        /// <param name="identifier">Mod name to Find</param>
        /// <returns>
        /// True if there are any versions of this mod available, false otherwise.
        /// </returns>
        public static bool IsAnyAvailable(IRegistryQuerier registry, string identifier)
        {
            try {
                registry.LatestAvailable(identifier, null);
                return true;
            } catch (ModuleNotFoundKraken) {
                return false;
            }
        }

        /// <summary>
        /// Add or remove a value from a HashSet
        /// </summary>
        /// <param name="list">HashSet to manipulate</param>
        /// <param name="identifier">The value</param>
        public static void toggleContains(HashSet<string> list, string identifier)
        {
            if (list != null && !string.IsNullOrEmpty(identifier)) {
                if (list.Contains(identifier)) {
                    list.Remove(identifier);
                } else {
                    list.Add(identifier);
                }
            }
        }

        /// <summary>
        /// Add or remove a value from a HashSet
        /// </summary>
        /// <param name="list">HashSet to manipulate</param>
        /// <param name="mod">The value</param>
        public static void toggleContains(HashSet<CkanModule> list, CkanModule mod)
        {
            if (list != null && mod != null) {
                if (list.Contains(mod)) {
                    list.Remove(mod);
                } else {
                    list.Add(mod);
                }
            }
        }

        /// <summary>
        /// Mods we're planning to install
        /// </summary>
        public readonly HashSet<CkanModule> Install = new HashSet<CkanModule>();

        /// <summary>
        /// Mods we're planning to upgrade
        /// </summary>
        public readonly HashSet<string> Upgrade = new HashSet<string>();

        /// <summary>
        /// Mods we're planning to remove
        /// </summary>
        public readonly HashSet<string> Remove  = new HashSet<string>();

        /// <summary>
        /// Mods we're planning to replace with successor mods
        /// </summary>
        public readonly HashSet<string> Replace = new HashSet<string>();
    }

    /// <summary>
    /// Representation of the current status of a mod in an install
    /// </summary>
    public enum InstallStatus {

        /// <summary>
        /// This mod is not in the registry
        /// </summary>
        Unavailable,

        /// <summary>
        /// This mod is not installed
        /// </summary>
        NotInstalled,

        /// <summary>
        /// This mod is currently installed but planned to be removed
        /// </summary>
        Removing,

        /// <summary>
        /// This mod is installed and not upgradeable or planned to be removed
        /// </summary>
        Installed,

        /// <summary>
        /// Like Installed, but can be auto-removed if depending mods are removed
        /// </summary>
        AutoInstalled,

        /// <summary>
        /// This mod is not installed but we are planning to install it
        /// </summary>
        Installing,

        /// <summary>
        /// This mod is installed and there's an upgrade available for it
        /// </summary>
        Upgradeable,

        /// <summary>
        /// This mod is installed and we are planning to upgrade it
        /// </summary>
        Upgrading,

        /// <summary>
        /// This mod was installed manually
        /// </summary>
        AutoDetected,

        /// <summary>
        /// This mod is installed and can be replaced by a successor mod
        /// </summary>
        Replaceable,

        /// <summary>
        /// This mod is installed and we are planning to replace it
        /// </summary>
        Replacing,

    };
}
