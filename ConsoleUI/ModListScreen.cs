using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Autofac;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen listing mods available for a given install
    /// </summary>
    public class ModListScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">Game instance manager object containing the current instance</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        /// <param name="regTheme">The theme to use for the registry update flow, if needed</param>
        public ModListScreen(GameInstanceManager mgr, bool dbg, ConsoleTheme regTheme)
        {
            debug    = dbg;
            manager  = mgr;
            registry = RegistryManager.Instance(manager.CurrentInstance).registry;

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
                        Width    = 44,
                        Renderer = m => m.name ?? ""
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = Properties.Resources.ModListVersionHeader,
                        Width    = 10,
                        Renderer = m => ModuleInstaller.StripEpoch(m.version?.ToString() ?? ""),
                        Comparer = (a, b) => a.version.CompareTo(b.version)
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = Properties.Resources.ModListMaxGameVersionHeader,
                        Width    = 20,
                        Renderer = m => registry.LatestCompatibleKSP(m.identifier)?.ToString() ?? "",
                        Comparer = (a, b) => registry.LatestCompatibleKSP(a.identifier).CompareTo(registry.LatestCompatibleKSP(b.identifier))
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
                        } else switch (filter.Substring(1, 1)) {
                            case "i":
                                return registry.IsInstalled(m.identifier, false);
                            case "u":
                                return registry.HasUpdate(m.identifier, manager.CurrentInstance.VersionCriteria());
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

            moduleList.AddTip(Properties.Resources.F2, Properties.Resources.LaunchKSP);
            moduleList.AddBinding(Keys.F2, (object sender, ConsoleTheme theme) => {
                manager.CurrentInstance.LaunchGame(this, launchAnyWay);
                return true;
            });

            moduleList.AddTip(Properties.Resources.Enter, Properties.Resources.Details,
                () => moduleList.Selection != null
            );
            moduleList.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                if (moduleList.Selection != null) {
                    LaunchSubScreen(theme, new ModInfoScreen(manager, plan, moduleList.Selection, debug));
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
                    && registry.HasUpdate(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria())
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
                            && registry.HasUpdate(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria())) {
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
                    RegistryManager.Instance(manager.CurrentInstance).Save(false);
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
                    int days = daysSinceUpdated(registryFilePath());
                    return days <  1 ? ""
                        :  days == 1 ? string.Format(Properties.Resources.ModListUpdatedDayAgo,  days)
                        :              string.Format(Properties.Resources.ModListUpdatedDaysAgo, days);
                },
                null,
                (ConsoleTheme th) => {
                    int daysSince = daysSinceUpdated(registryFilePath());
                    return daysSince < daysTillStale     ? th.RegistryUpToDate
                        :  daysSince < daystillVeryStale ? th.RegistryStale
                        :                                  th.RegistryVeryStale;
                }
            ));

            List<ConsoleMenuOption> opts = new List<ConsoleMenuOption>() {
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
            return $"CKAN {Meta.GetVersion()}";
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
            DownloadImportDialog.ImportDownloads(theme, manager.CurrentInstance, manager.Cache, plan);
            RefreshList(theme);
            return true;
        }

        private bool CaptureKey(ConsoleTheme theme)
        {
            ConsoleKeyInfo k = default(ConsoleKeyInfo);
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
            foreach (string identifier in registry.Installed(true).Select(kvp => kvp.Key)) {
                if (registry.HasUpdate(identifier, manager.CurrentInstance.VersionCriteria())) {
                    return true;
                }
            }
            return false;
        }

        private bool UpgradeAll(ConsoleTheme theme)
        {
            foreach (string identifier in registry.Installed(true).Select(kvp => kvp.Key)) {
                if (registry.HasUpdate(identifier, manager.CurrentInstance.VersionCriteria())) {
                    plan.Upgrade.Add(identifier);
                }
            }
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
                DependencyScreen ds = new DependencyScreen(manager, reinstall, new HashSet<string>(), debug);
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

        private string registryFilePath()
        {
            return Path.Combine(manager.CurrentInstance.CkanDir(), "registry.json");
        }

        private int daysSinceUpdated(string filename)
        {
            return (DateTime.Now - File.GetLastWriteTime(filename)).Days;
        }

        private bool UpdateRegistry(ConsoleTheme theme, bool showNewModsPrompt = true)
        {
            ProgressScreen ps = new ProgressScreen(
                Properties.Resources.ModListUpdateRegistryTitle,
                Properties.Resources.ModListUpdateRegistryMessage
            );
            LaunchSubScreen(theme, ps, (ConsoleTheme th) => {
                HashSet<string> availBefore = new HashSet<string>(
                    Array.ConvertAll<CkanModule, string>(
                        registry.CompatibleModules(
                            manager.CurrentInstance.VersionCriteria()
                        ).ToArray(),
                        (l => l.identifier)
                    )
                );
                recent.Clear();
                try {
                    Repo.UpdateAllRepositories(
                        RegistryManager.Instance(manager.CurrentInstance),
                        manager.CurrentInstance,
                        manager.Cache,
                        ps
                    );
                } catch (ReinstallModuleKraken rmk) {
                    ChangePlan reinstPlan = new ChangePlan();
                    foreach (CkanModule m in rmk.Modules) {
                        reinstPlan.ToggleUpgrade(m);
                    }
                    LaunchSubScreen(theme, new InstallScreen(manager, reinstPlan, debug));
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
                manager.CurrentInstance.Scan();
            } catch (InconsistentKraken ex) {
                // Warn about inconsistent state
                RaiseError(Properties.Resources.ModListScanBad, ex.InconsistenciesPretty);
            }
            return true;
        }

        private bool InstanceSettings(ConsoleTheme theme)
        {
            var prevRepos   = new SortedDictionary<string, Repository>(registry.Repositories);
            var prevVerCrit = manager.CurrentInstance.VersionCriteria();
            LaunchSubScreen(theme, new GameInstanceEditScreen(manager, manager.CurrentInstance));
            if (!SortedDictionaryEquals(registry.Repositories, prevRepos)) {
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
            LaunchSubScreen(theme, new GameInstanceListScreen(manager));
            if (!prevInst.Equals(manager.CurrentInstance)) {
                // Game instance changed, reset everything
                plan.Reset();
                registry = RegistryManager.Instance(manager.CurrentInstance).registry;
                RefreshList(theme);
            } else if (!SortedDictionaryEquals(registry.Repositories, prevRepos)) {
                // Repos changed, need to fetch them
                UpdateRegistry(theme, false);
                RefreshList(theme);
            } else if (!manager.CurrentInstance.VersionCriteria().Equals(prevVerCrit)) {
                // VersionCriteria changed, need to re-check what is compatible
                RefreshList(theme);
            }
            return true;
        }

        private bool SortedDictionaryEquals<K, V>(SortedDictionary<K, V> a, SortedDictionary<K, V> b)
        {
            return a == null ? b == null
                 : b == null ? false
                 : a.Count == b.Count
                    && a.Keys.All(k => b.ContainsKey(k))
                    && b.Keys.All(k => a.ContainsKey(k) && a[k].Equals(b[k]));
        }

        private bool EditAuthTokens(ConsoleTheme theme)
        {
            LaunchSubScreen(theme, new AuthTokenScreen());
            return true;
        }

        private bool EditInstallFilters(ConsoleTheme theme)
        {
            LaunchSubScreen(theme, new InstallFiltersScreen(
                ServiceLocator.Container.Resolve<Configuration.IConfiguration>(),
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
            ScanForMods();
            if (allMods == null || force) {
                if (!registry?.HasAnyAvailable() ?? false)
                {
                    UpdateRegistry(theme, false);
                }
                allMods = new List<CkanModule>(registry.CompatibleModules(manager.CurrentInstance.VersionCriteria()));
                foreach (InstalledModule im in registry.InstalledModules) {
                    CkanModule m = null;
                    try {
                        m = registry.LatestAvailable(im.identifier, manager.CurrentInstance.VersionCriteria());
                    } catch (ModuleNotFoundKraken) { }
                    if (m == null) {
                        // Add unavailable installed mods to the list
                        allMods.Add(im.Module);
                    }
                }
            }
            return allMods;
        }

        private bool ExportInstalled(ConsoleTheme theme)
        {
            try {
                // Save the mod list as "depends" without the installed versions.
                // Beacause that's supposed to work.
                RegistryManager.Instance(manager.CurrentInstance).Save(true);
                string path = Path.Combine(
                    manager.CurrentInstance.CkanDir(),
                    $"{Properties.Resources.ModListExportPrefix}-{manager.CurrentInstance.Name}.ckan"
                );
                RaiseError(Properties.Resources.ModListExported, path);
            } catch (Exception ex) {
                RaiseError(Properties.Resources.ModListExportFailed, ex.Message);
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
            LaunchSubScreen(theme, new InstallScreen(manager, plan, debug));
            RefreshList(theme);
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
            return StatusSymbol(plan.GetModStatus(manager, registry, m.identifier));
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

        /// <summary>
        /// Asks the user is they want to continue with launching the game.
        /// </summary>
        public Tuple<bool, bool> launchAnyWay(string text, string suppressText)
        {
            int result = RaiseSelectionDialog(text, "Cancel", "Launch", "Launch and Don't Ask Again");
            if (result == 0) {
                return new Tuple<bool, bool>(false, false);
            } else if (result == 2) {
                return new Tuple<bool, bool>(true, true);
            } else {
                return new Tuple<bool, bool>(true, false);
            }
        }

        private GameInstanceManager manager;
        private Registry            registry;
        private bool                debug;

        private ConsoleField               searchBox;
        private ConsoleListBox<CkanModule> moduleList;

        private ChangePlan      plan   = new ChangePlan();
        private HashSet<string> recent = new HashSet<string>();

        private int searchWidth => Math.Max(30, Math.Max(
            Properties.Resources.ModListSearchFocusedGhostText.Length,
            Properties.Resources.ModListSearchUnfocusedGhostText.Length
        ));
        private const int daysTillStale     = 7;
        private const int daystillVeryStale = 30;

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
        /// <returns>
        /// Status of mod
        /// </returns>
        public InstallStatus GetModStatus(GameInstanceManager manager, IRegistryQuerier registry, string identifier)
        {
            if (registry.IsInstalled(identifier, false)) {
                if (Remove.Contains(identifier)) {
                    return InstallStatus.Removing;
                } else if (registry.HasUpdate(identifier, manager.CurrentInstance.VersionCriteria())) {
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
