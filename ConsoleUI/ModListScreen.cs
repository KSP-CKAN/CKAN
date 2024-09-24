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
using CKAN.Versioning;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen listing mods available for a given install
    /// </summary>
    public class ModListScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="mgr">Game instance manager object containing the current instance</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        /// <param name="regMgr">Registry manager for the current instance</param>
        /// <param name="userAgent">HTTP useragent string to use</param>
        /// <param name="game">The game of the current instance, used for getting known versions</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public ModListScreen(ConsoleTheme          theme,
                             GameInstanceManager   mgr,
                             RepositoryDataManager repoData,
                             RegistryManager       regMgr,
                             string?               userAgent,
                             IGame                 game,
                             bool                  dbg)
            : base(theme)
        {
            debug    = dbg;
            manager  = mgr;
            this.regMgr = regMgr;
            registry = regMgr.registry;
            this.repoData = repoData;
            this.userAgent = userAgent;

            moduleList = new ConsoleListBox<CkanModule>(
                1, 4, -1, -2,
                GetAllMods(),
                new List<ConsoleListBoxColumn<CkanModule>>() {
                    new ConsoleListBoxColumn<CkanModule>(
                        "", StatusSymbol, null, 1),
                    new ConsoleListBoxColumn<CkanModule>(
                        Properties.Resources.ModListNameHeader,
                        m => m.name ?? "",
                        null, null),
                    new ConsoleListBoxColumn<CkanModule>(
                        Properties.Resources.ModListVersionHeader,
                        m => ModuleInstaller.StripEpoch(m.version?.ToString() ?? ""),
                        (a, b) => a.version.CompareTo(b.version),
                        10),
                    new ConsoleListBoxColumn<CkanModule>(
                        Properties.Resources.ModListMaxGameVersionHeader,
                        m => registry.LatestCompatibleGameVersion(game.KnownVersions, m.identifier)?.ToString() ?? "",
                        (a, b) => registry.LatestCompatibleGameVersion(game.KnownVersions, a.identifier) is GameVersion gvA
                               && registry.LatestCompatibleGameVersion(game.KnownVersions, b.identifier) is GameVersion gvB
                                   ? gvA.CompareTo(gvB)
                                   : 0,
                        20),
                    new ConsoleListBoxColumn<CkanModule>(
                        Properties.Resources.ModListDownloadsHeader,
                        m => repoData.GetDownloadCount(registry.Repositories.Values, m.identifier)
                                                ?.ToString()
                                                ?? "",
                        (a, b) => (repoData.GetDownloadCount(registry.Repositories.Values, a.identifier) ?? 0)
                                             .CompareTo(repoData.GetDownloadCount(registry.Repositories.Values, b.identifier) ?? 0),
                        12),
                },
                1, 0, ListSortDirection.Descending,
                (CkanModule m, string filter) => {
                    // Search for author
                    if (filter.StartsWith("@")) {
                        string authorFilt = filter[1..];
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
                        } else {
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
                                    string conflictsWith = filter[2..];
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
                                    string dependsOn = filter[2..];
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
            // Show total download size of all installed mods
            AddObject(new ConsoleLabel(
                1, 3, searchWidth,
                () => string.Format(Properties.Resources.ModListSizeOnDisk, CkanModule.FmtSize(totalInstalledDownloadSize())),
                null,
                th => th.DimLabelFg));

            AddObject(searchBox);
            AddObject(moduleList);

            AddBinding(Keys.CtrlP, (object sender) => PlayGame());
            AddBinding(Keys.CtrlQ, (object sender) => false);
            AddBinding(Keys.AltX,  (object sender) => false);
            AddBinding(Keys.F1,    (object sender) => Help());
            AddBinding(Keys.AltH,  (object sender) => Help());
            AddBinding(Keys.F5,    (object sender) => UpdateRegistry());
            AddBinding(Keys.CtrlR, (object sender) => UpdateRegistry());
            AddBinding(Keys.CtrlU, (object sender) => UpgradeAll());

            // Now a bunch of convenience shortcuts so you don't get stuck in the search box
            searchBox.AddBinding(Keys.PageUp, (object sender) => {
                SetFocus(moduleList);
                return true;
            });
            searchBox.AddBinding(Keys.PageDown, (object sender) => {
                SetFocus(moduleList);
                return true;
            });
            searchBox.AddBinding(Keys.Enter, (object sender) => {
                SetFocus(moduleList);
                return true;
            });

            moduleList.AddBinding(Keys.CtrlF, (object sender) => {
                SetFocus(searchBox);
                return true;
            });
            moduleList.AddBinding(Keys.Escape, (object sender) => {
                searchBox.Clear();
                return true;
            });

            moduleList.AddTip(Properties.Resources.Enter, Properties.Resources.Details,
                () => moduleList.Selection != null
            );
            moduleList.AddBinding(Keys.Enter, (object sender) => {
                if (moduleList.Selection != null) {
                    LaunchSubScreen(new ModInfoScreen(theme, manager, registry, userAgent,
                                                      plan, moduleList.Selection, upgradeableGroups?[true], debug));
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
                      && manager.CurrentInstance != null
                      && registry.GetReplacement(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria()) != null
            );
            moduleList.AddBinding(Keys.Plus, (object sender) => {
                if (moduleList.Selection != null && !moduleList.Selection.IsDLC && manager.CurrentInstance != null) {
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
            moduleList.AddBinding(Keys.Minus, (object sender) => {
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
            moduleList.AddBinding(Keys.F8, (object sender) => {
                if (moduleList.Selection is CkanModule m)
                {
                    var im = registry.InstalledModule(m.identifier);
                    if (im != null && !m.IsDLC) {
                        im.AutoInstalled = !im.AutoInstalled;
                        regMgr.Save(false);
                    }
                }
                return true;
            });

            AddTip("F9", Properties.Resources.ModListApplyChangesTip, plan.NonEmpty);
            AddBinding(Keys.F9, (object sender) => {
                ApplyChanges();
                return true;
            });

            // Abstract of currently selected mod under grid
            AddObject(new ConsoleLabel(1, -1, -1,
                                       () => moduleList.Selection?.@abstract.Trim() ?? "",
                                       null,
                                       th => th.DimLabelFg));

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

            var opts = new List<ConsoleMenuOption?>() {
                new ConsoleMenuOption(Properties.Resources.ModListPlayMenu, "",
                                      Properties.Resources.ModListPlayMenuTip,
                                      true, null, null, null, true,
                                      () => new ConsolePopupMenu(
                                                (manager.CurrentInstance
                                                       ?.game
                                                        .DefaultCommandLines(manager.SteamLibrary,
                                                                             new DirectoryInfo(manager.CurrentInstance.GameDir()))
                                                       ?? Enumerable.Empty<string>())
                                                        .Select((cmd, i) => new ConsoleMenuOption(
                                                                                cmd,
                                                                                i == 0 ? $"{Properties.Resources.Ctrl}+P"
                                                                                       : "",
                                                                                cmd, true,
                                                                                () => PlayGame(cmd)))
                                                        .OfType<ConsoleMenuOption?>()
                                                        .ToList())),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListSortMenu, "",
                    Properties.Resources.ModListSortMenuTip,
                    true, null, null, moduleList.SortMenu()),
                null,
                new ConsoleMenuOption(Properties.Resources.ModListRefreshMenu, $"F5, {Properties.Resources.Ctrl}+R",
                    Properties.Resources.ModListRefreshMenuTip,
                    true, () => UpdateRegistry()),
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
                    true, () => false)
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
            => $"{Meta.GetProductName()} {Meta.GetVersion()}";

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
            => $"{manager.CurrentInstance?.game.ShortName} {manager.CurrentInstance?.Version()} ({manager.CurrentInstance?.Name})";

        // Alt+H doesn't work on Mac, but F1 does, and we need
        // an option other than F1 for terminals that open their own help.
        private static readonly string helpKey = Platform.IsMac
            ? "F1"
            : $"F1, {Properties.Resources.Alt}+H";

        private bool ImportDownloads()
        {
            if (manager.CurrentInstance != null && manager.Cache != null)
            {
                DownloadImportDialog.ImportDownloads(theme, manager.CurrentInstance, repoData, manager.Cache, plan);
                RefreshList();
            }
            return true;
        }

        private bool CaptureKey()
        {
            ConsoleKeyInfo k = default;
            ConsoleMessageDialog keyprompt = new ConsoleMessageDialog(theme, Properties.Resources.ModListPressAKey, new List<string>());
            keyprompt.Run(() => {
                k = Console.ReadKey(true);
            });
            ConsoleMessageDialog output = new ConsoleMessageDialog(
                theme,
                $"Key: {k.Key,18}\nKeyChar:           0x{(int)k.KeyChar:x2}\nModifiers: {k.Modifiers,12}",
                new List<string> { Properties.Resources.OK }
            );
            output.Run();
            return true;
        }

        private bool HasAnyUpgradeable()
            => (upgradeableGroups?[true].Count ?? 0) > 0;

        private bool UpgradeAll()
        {
            plan.Upgrade.UnionWith(upgradeableGroups?[true].Select(m => m.identifier)
                                   ?? Enumerable.Empty<string>());
            return true;
        }

        private bool ViewSuggestions()
        {
            ChangePlan reinstall = new ChangePlan();
            foreach (InstalledModule im in registry.InstalledModules) {
                // Only check mods that are still available
                try {
                    if (registry.LatestAvailable(im.identifier, manager.CurrentInstance?.VersionCriteria()) != null) {
                        reinstall.Install.Add(im.Module);
                    }
                } catch {
                    // The registry object badly needs an IsAvailable check
                }
            }
            try {
                DependencyScreen ds = new DependencyScreen(theme, manager, registry, userAgent, reinstall, new HashSet<string>(), debug);
                if (ds.HaveOptions()) {
                    LaunchSubScreen(ds);
                    bool needRefresh = false;
                    // Copy the right ones into our real plan
                    foreach (CkanModule mod in reinstall.Install) {
                        if (!registry.IsInstalled(mod.identifier, false)) {
                            plan.Install.Add(mod);
                            needRefresh = true;
                        }
                    }
                    if (needRefresh) {
                        RefreshList();
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
            => manager.CurrentInstance != null
               && PlayGame(manager.CurrentInstance
                                  .game
                                  .DefaultCommandLines(manager.SteamLibrary,
                                                       new DirectoryInfo(manager.CurrentInstance.GameDir()))
                                  .First());

        private bool PlayGame(string commandLine)
        {
            manager.CurrentInstance?.PlayGame(commandLine);
            return true;
        }

        private bool UpdateRegistry(bool showNewModsPrompt = true)
        {
            ProgressScreen ps = new ProgressScreen(
                theme,
                Properties.Resources.ModListUpdateRegistryTitle,
                Properties.Resources.ModListUpdateRegistryMessage);
            LaunchSubScreen(ps, () => {
                if (manager.CurrentInstance != null)
                {
                    var availBefore = registry.CompatibleModules(manager.CurrentInstance.VersionCriteria())
                                              .Select(l => l.identifier)
                                              .ToHashSet();
                    recent.Clear();
                    try {
                        repoData.Update(registry.Repositories.Values.ToArray(),
                                        manager.CurrentInstance.game,
                                        false,
                                        new NetAsyncDownloader(ps, userAgent),
                                        ps,
                                        userAgent);
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
                }
            });
            if (showNewModsPrompt && recent.Count > 0 && RaiseYesNoDialog(newModPrompt(recent.Count))) {
                searchBox.Clear();
                moduleList.FilterString = searchBox.Value = "~n";
            }
            RefreshList();
            return true;
        }

        private static string newModPrompt(int howMany)
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

        private bool InstanceSettings()
        {
            if (manager.CurrentInstance != null)
            {
                var prevRepos   = new SortedDictionary<string, Repository>(registry.Repositories);
                var prevVerCrit = manager.CurrentInstance.VersionCriteria();
                LaunchSubScreen(new GameInstanceEditScreen(theme, manager, repoData, manager.CurrentInstance, userAgent));
                if (!registry.Repositories.DictionaryEquals(prevRepos)) {
                    // Repos changed, need to fetch them
                    UpdateRegistry(false);
                    RefreshList();
                } else if (!manager.CurrentInstance.VersionCriteria().Equals(prevVerCrit)) {
                    // VersionCriteria changed, need to re-check what is compatible
                    RefreshList();
                }
            }
            return true;
        }

        private bool SelectInstall()
        {
            if (manager.CurrentInstance != null)
            {
                var prevInst = manager.CurrentInstance;
                var prevRepos = new SortedDictionary<string, Repository>(registry.Repositories);
                var prevVerCrit = prevInst.VersionCriteria();
                LaunchSubScreen(new GameInstanceListScreen(theme, manager, repoData, userAgent));
                if (!prevInst.Equals(manager.CurrentInstance)) {
                    // Game instance changed, reset everything
                    plan.Reset();
                    regMgr = RegistryManager.Instance(manager.CurrentInstance, repoData);
                    registry = regMgr.registry;
                    RefreshList();
                } else if (!registry.Repositories.DictionaryEquals(prevRepos)) {
                    // Repos changed, need to fetch them
                    UpdateRegistry(false);
                    RefreshList();
                } else if (!manager.CurrentInstance.VersionCriteria().Equals(prevVerCrit)) {
                    // VersionCriteria changed, need to re-check what is compatible
                    RefreshList();
                }
            }
            return true;
        }

        private bool EditAuthTokens()
        {
            LaunchSubScreen(new AuthTokenScreen(theme));
            return true;
        }

        private bool EditInstallFilters()
        {
            if (manager.CurrentInstance != null)
            {
                LaunchSubScreen(new InstallFiltersScreen(
                    theme,
                    ServiceLocator.Container.Resolve<IConfiguration>(),
                    manager.CurrentInstance));
            }
            return true;
        }

        private void RefreshList()
        {
            // In the constructor this is called while moduleList is being populated, just do nothing in this case.
            // ModListScreen -> moduleList = (GetAllMods ...) -> UpdateRegistry -> RefreshList
            moduleList?.SetData(GetAllMods(true));
        }

        private List<CkanModule>? allMods = null;

        private List<CkanModule> GetAllMods(bool force = false)
        {
            if (manager.CurrentInstance != null)
            {
                timeSinceUpdate = repoData.LastUpdate(registry.Repositories.Values);
                ScanForMods();
                if (allMods == null || force) {
                    if (!registry.HasAnyAvailable())
                    {
                        UpdateRegistry(false);
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
            return new List<CkanModule>();
        }

        private bool ExportInstalled()
        {
            try {
                if (manager.CurrentInstance != null)
                {
                    // Save the mod list as "depends" without the installed versions.
                    // Because that's supposed to work.
                    regMgr.Save(true);
                    string path = Path.Combine(
                        Platform.FormatPath(manager.CurrentInstance.CkanDir()),
                        $"{Properties.Resources.ModListExportPrefix}-{manager.CurrentInstance.Name}.ckan");
                    RaiseError(Properties.Resources.ModListExported, path);
                }
            } catch (Exception ex) {
                RaiseError(Properties.Resources.ModListExportFailed, ex.Message);
            }
            return true;
        }

        private bool InstallFromCkan()
        {
            if (manager.CurrentInstance != null)
            {
                var modules = InstallFromCkanDialog.ChooseCkanFiles(theme, manager.CurrentInstance);
                if (modules.Length > 0) {
                    var crit = manager.CurrentInstance.VersionCriteria();
                    var installed = regMgr.registry.InstalledModules.Select(inst => inst.Module).ToList();
                    var cp = new ChangePlan();
                    cp.Install.UnionWith(
                        modules.Concat(
                            modules.Where(m => m.IsMetapackage && m.depends != null)
                                   .SelectMany(m => m.depends?.Where(rel => !rel.MatchesAny(installed, null, null))
                                                              .Select(rel =>
                                                                  // If there's a compatible match, return it
                                                                  // Metapackages aren't intending to prompt users to choose providing mods
                                                                  rel.ExactMatch(regMgr.registry, crit, installed, modules)
                                                                  // Otherwise look for incompatible
                                                                  ?? rel.ExactMatch(regMgr.registry, null, installed, modules))
                                                              .OfType<CkanModule>()
                                                             ?? Enumerable.Empty<CkanModule>())));
                    LaunchSubScreen(new InstallScreen(theme, manager, repoData, userAgent, cp, debug));
                    RefreshList();
                }
            }
            return true;
        }

        private bool Help()
        {
            ModListHelpDialog hd = new ModListHelpDialog(theme);
            hd.Run();
            DrawBackground();
            return true;
        }

        private bool ApplyChanges()
        {
            if (plan.NonEmpty())
            {
                LaunchSubScreen(new InstallScreen(theme, manager, repoData, userAgent, plan, debug));
                RefreshList();
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
            => StatusSymbol(plan.GetModStatus(manager, registry, m.identifier,
                                              upgradeableGroups?[true] ?? new List<CkanModule>()));

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

        private readonly GameInstanceManager                 manager;
        private          RegistryManager                     regMgr;
        private readonly string?                             userAgent;
        private          Registry                            registry;
        private readonly RepositoryDataManager               repoData;
        private          Dictionary<bool, List<CkanModule>>? upgradeableGroups;
        private readonly bool                                debug;
        private          TimeSpan                            timeSinceUpdate = TimeSpan.Zero;

        private readonly ConsoleField               searchBox;
        private readonly ConsoleListBox<CkanModule> moduleList;

        private readonly ChangePlan      plan   = new ChangePlan();
        private readonly HashSet<string> recent = new HashSet<string>();

        private static int searchWidth => Math.Max(30, Math.Max(
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
                } else if (manager.CurrentInstance != null
                           && registry.GetReplacement(identifier, manager.CurrentInstance.VersionCriteria()) != null) {
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
