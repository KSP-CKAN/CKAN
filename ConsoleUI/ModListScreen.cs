using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen listing mods available for a given install
    /// </summary>
    public class ModListScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">KSP manager object containing the current instance</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public ModListScreen(KSPManager mgr, bool dbg)
        {
            debug    = dbg;
            manager  = mgr;
            registry = RegistryManager.Instance(manager.CurrentInstance).registry;

            moduleList = new ConsoleListBox<CkanModule>(
                1, 4, -1, -2,
                GetAllMods(),
                new List<ConsoleListBoxColumn<CkanModule>>() {
                    new ConsoleListBoxColumn<CkanModule>() {
                        Header   = "",
                        Width    = 1,
                        Renderer = StatusSymbol
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = "Name",
                        Width    = 44,
                        Renderer = m => m.name ?? ""
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = "Version",
                        Width    = 10,
                        Renderer = m => ModuleInstaller.StripEpoch(m.version?.ToString() ?? ""),
                        Comparer = (a, b) => a.version.CompareTo(b.version)
                    }, new ConsoleListBoxColumn<CkanModule>() {
                        Header   = "Max KSP version",
                        Width    = 17,
                        Renderer = m => registry.LatestCompatibleKSP(m.identifier)?.ToString() ?? "",
                        Comparer = (a, b) => registry.LatestCompatibleKSP(a.identifier).CompareTo(registry.LatestCompatibleKSP(b.identifier))
                    }
                },
                1, 0, ListSortDirection.Descending,
                (CkanModule m, string filter) => {
                    if (filter.StartsWith("@")) {
                        string authorFilt = filter.Substring(1);
                        if (string.IsNullOrEmpty(authorFilt)) {
                            return true;
                        } else if (m.author != null) {
                            foreach (string auth in m.author) {
                                if (auth.IndexOf(authorFilt, StringComparison.CurrentCultureIgnoreCase) == 0) {
                                    return true;
                                }
                            }
                        }
                        return false;
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
                                        if (rel.name.IndexOf(conflictsWith, StringComparison.CurrentCultureIgnoreCase) == 0) {
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
                                        if (rel.name.IndexOf(dependsOn, StringComparison.CurrentCultureIgnoreCase) == 0) {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                        }
                        return false;
                    } else {
                        return m.identifier.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || m.name.IndexOf(      filter, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || m.@abstract.IndexOf( filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    }
                }
            );

            searchBox = new ConsoleField(-searchWidth, 2, -1) {
                GhostText = () => Focused() == searchBox
                    ? "<Type to search>"
                    : "<Ctrl+F to search>"
            };
            searchBox.OnChange += (ConsoleField sender, string newValue) => {
                moduleList.FilterString = newValue;
            };

            AddObject(new ConsoleLabel(
                1, 2, -searchWidth - 2,
                () => $"{moduleList.VisibleRowCount()} mods"
            ));
            AddObject(searchBox);
            AddObject(moduleList);

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

            moduleList.AddTip("Enter", "Details",
                () => moduleList.Selection != null
            );
            moduleList.AddBinding(Keys.Enter, (object sender) => {
                if (moduleList.Selection != null) {
                    LaunchSubScreen(new ModInfoScreen(manager, plan, moduleList.Selection, debug));
                }
                return true;
            });

            // Conditionally show only one of these based on selected mod status

            moduleList.AddTip("+", "Install",
                () => moduleList.Selection != null
                    && !registry.IsInstalled(moduleList.Selection.identifier, false)
            );
            moduleList.AddTip("+", "Upgrade",
                () => moduleList.Selection != null
                    && registry.HasUpdate(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria())
            );
            moduleList.AddBinding(Keys.Plus, (object sender) => {
                if (moduleList.Selection != null) {
                    if (!registry.IsInstalled(moduleList.Selection.identifier, false)) {
                        plan.ToggleInstall(moduleList.Selection.identifier);
                    } else if (registry.IsInstalled(moduleList.Selection.identifier, false)
                            && registry.HasUpdate(moduleList.Selection.identifier, manager.CurrentInstance.VersionCriteria())) {
                        plan.ToggleUpgrade(moduleList.Selection.identifier);
                    }
                }
                return true;
            });

            moduleList.AddTip("-", "Remove",
                () => moduleList.Selection != null
                    && (registry.IsInstalled(moduleList.Selection.identifier, false))
            );
            moduleList.AddBinding(Keys.Minus, (object sender) => {
                if (moduleList.Selection != null && registry.IsInstalled(moduleList.Selection.identifier, false)) {
                    plan.ToggleRemove(moduleList.Selection.identifier);
                }
                return true;
            });

            AddTip("F9", "Apply changes", plan.NonEmpty);
            AddBinding(Keys.F9, (object sender) => {
                ApplyChanges();
                return true;
            });

            // Show total download size of all installed mods
            AddObject(new ConsoleLabel(
                1, -1, searchWidth,
                () => $"{CkanModule.FmtSize(totalInstalledDownloadSize())} installed",
                null,
                () => ConsoleTheme.Current.DimLabelFg
            ));

            AddObject(new ConsoleLabel(
                -searchWidth, -1, -2,
                () => {
                    int days = daysSinceUpdated(registryFilePath());
                    return days <  1 ? ""
                        :  days == 1 ? $"Updated at least {days} day ago"
                        :              $"Updated at least {days} days ago";
                },
                null,
                () => {
                    int daysSince = daysSinceUpdated(registryFilePath());
                    if (daysSince < daysTillStale) {
                        return ConsoleTheme.Current.RegistryUpToDate;
                    } else if (daysSince < daystillVeryStale) {
                        return ConsoleTheme.Current.RegistryStale;
                    } else {
                        return ConsoleTheme.Current.RegistryVeryStale;
                    }
                }
            ));

            List<ConsoleMenuOption> opts = new List<ConsoleMenuOption>() {
                new ConsoleMenuOption("Sort...",                    "",
                    "Change the sorting of the list of mods",
                    true, null, null, moduleList.SortMenu()),
                null,
                new ConsoleMenuOption("Refresh mod list", "F5, Ctrl+R",
                    "Refresh the list of mods",
                    true, UpdateRegistry),
                new ConsoleMenuOption("Upgrade all",          "Ctrl+U",
                    "Mark all available updates for installation",
                    true, UpgradeAll),
                new ConsoleMenuOption("Audit recommendations",      "",
                    "List mods suggested and recommended by installed mods",
                    true, ViewSuggestions),
                new ConsoleMenuOption("Scan KSP dir",               "",
                    "Check for manually installed mods",
                    true, ScanForMods),
                new ConsoleMenuOption("Import downloads...",        "",
                    "Select manually downloaded mods to import into CKAN",
                    true, ImportDownloads),
                new ConsoleMenuOption("Export installed...",        "",
                    "Save your mod list",
                    true, ExportInstalled),
                null,
                new ConsoleMenuOption("Select KSP install...",      "",
                    "Switch to a different game instance",
                    true, SelectInstall),
                new ConsoleMenuOption("Authentication tokens...",     "",
                    "Edit authentication tokens sent to download servers",
                    true, EditAuthTokens),
                null,
                new ConsoleMenuOption("Help",                  helpKey,
                    "Tips & tricks",
                    true, Help),
                null,
                new ConsoleMenuOption("Quit",                 "Ctrl+Q",
                    "Exit to DOS",
                    true, () => false)
            };
            if (debug) {
                opts.Add(null);
                opts.Add(new ConsoleMenuOption("DEBUG: Capture key...", "",
                    "Print details of how your system reports a keystroke for debugging",
                    true, CaptureKey));
            }
            mainMenu = new ConsolePopupMenu(opts);

            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => $"KSP {manager.CurrentInstance.Version()} ({manager.CurrentInstance.Name})";
        }

        // Alt+H doesn't work on Mac, but F1 does, and we need
        // an option other than F1 for terminals that open their own help.
        private static readonly string helpKey = Platform.IsMac
            ? "F1"
            : "F1, Alt+H";

        private bool ImportDownloads()
        {
            DownloadImportDialog.ImportDownloads(manager.CurrentInstance, plan);
            RefreshList();
            return true;
        }

        private bool CaptureKey()
        {
            ConsoleKeyInfo k = default(ConsoleKeyInfo);
            ConsoleMessageDialog keyprompt = new ConsoleMessageDialog("Press a key", new List<string>());
            keyprompt.Run(() => {
                k = Console.ReadKey(true);
            });
            ConsoleMessageDialog output = new ConsoleMessageDialog(
                $"Key: {k.Key,18}\nKeyChar:           0x{(int)k.KeyChar:x2}\nModifiers: {k.Modifiers,12}",
                new List<string> {"OK"}
            );
            output.Run();
            return true;
        }

        private bool UpgradeAll()
        {
            foreach (InstalledModule im in registry.InstalledModules) {
                if (registry.HasUpdate(im.identifier, manager.CurrentInstance.VersionCriteria())) {
                    plan.Upgrade.Add(im.identifier);
                }
            }
            return true;
        }

        private bool ViewSuggestions()
        {
            ChangePlan reinstall = new ChangePlan();
            foreach (InstalledModule im in registry.InstalledModules) {
                // Only check mods that are still available
                try {
                    if (registry.LatestAvailable(im.identifier, manager.CurrentInstance.VersionCriteria()) != null) {
                        reinstall.Install.Add(im.identifier);
                    }
                } catch {
                    // The registry object badly needs an IsAvailable check
                }
            }
            try {
                DependencyScreen ds = new DependencyScreen(manager, reinstall, new HashSet<string>(), debug);
                if (ds.HaveOptions()) {
                    LaunchSubScreen(ds);
                    bool needRefresh = false;
                    // Copy the right ones into our real plan
                    foreach (string mod in reinstall.Install) {
                        if (!registry.IsInstalled(mod, false)) {
                            plan.Install.Add(mod);
                            needRefresh = true;
                        }
                    }
                    if (needRefresh) {
                        RefreshList();
                    }
                } else {
                    RaiseError("Installed mods have no unsatisfied recommendations or suggestions.");
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

        private bool UpdateRegistry()
        {
            ConsoleMessageDialog d = new ConsoleMessageDialog(
                "Updating registry...",
                new List<string>()
            );
            d.Run(() => {
                HashSet<string> availBefore = new HashSet<string>(
                    Array.ConvertAll<CkanModule, string>(
                        registry.Available(
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
                        this
                    );
                } catch (Exception ex) {
                    // There can be errors while you re-install mods with changed metadata
                    RaiseError(ex.Message + ex.StackTrace);
                }
                // Update recent with mods that were updated in this pass
                foreach (CkanModule mod in registry.Available(
                        manager.CurrentInstance.VersionCriteria()
                    )) {
                    if (!availBefore.Contains(mod.identifier)) {
                        recent.Add(mod.identifier);
                    }
                }
            });
            if (recent.Count > 0 && RaiseYesNoDialog(newModPrompt(recent.Count))) {
                searchBox.Clear();
                moduleList.FilterString = searchBox.Value = "~n";
            }
            RefreshList();
            return true;
        }

        private string newModPrompt(int howMany)
        {
            return howMany == 1
                ? $"{howMany} new mod available since last update. Show it?"
                : $"{howMany} new mods available since last update. Show them?";
        }

        private bool ScanForMods()
        {
            try {
                manager.CurrentInstance.ScanGameData();
            } catch (InconsistentKraken ex) {
                // Warn about inconsistent state
                RaiseError(ex.InconsistenciesPretty + " The repo has not been saved.");
            }
            return true;
        }

        private bool SelectInstall()
        {
            KSP prevInst = manager.CurrentInstance;
            LaunchSubScreen(new KSPListScreen(manager));
            // Abort if same instance as before
            if (!prevInst.Equals(manager.CurrentInstance)) {
                plan.Reset();
                registry = RegistryManager.Instance(manager.CurrentInstance).registry;
                RefreshList();
            }
            return true;
        }

        private bool EditAuthTokens()
        {
            LaunchSubScreen(new AuthTokenScreen());
            return true;
        }

        private void RefreshList()
        {
            moduleList.SetData(GetAllMods(true));
        }

        private List<CkanModule> allMods = null;

        private List<CkanModule> GetAllMods(bool force = false)
        {
            if (allMods == null || force) {
                allMods = new List<CkanModule>(registry.Available(manager.CurrentInstance.VersionCriteria()));
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

        private bool ExportInstalled()
        {
            try {
                // Save the mod list as "depends" without the installed versions.
                // Beacause that's supposed to work.
                RegistryManager.Instance(manager.CurrentInstance).Save(true);
                string path = Path.Combine(
                    manager.CurrentInstance.CkanDir(),
                    $"installed-{manager.CurrentInstance.Name}.ckan"
                );
                RaiseError($"Mod list exported to {path}");
            } catch (Exception ex) {
                RaiseError($"Export failed: {ex.Message}");
            }
            return true;
        }

        private bool Help()
        {
            ModListHelpDialog hd = new ModListHelpDialog();
            hd.Run();
            DrawBackground();
            return true;
        }

        private bool ApplyChanges()
        {
            LaunchSubScreen(new InstallScreen(manager, plan, debug));
            RefreshList();
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
                case InstallStatus.Unavailable:  return unavailable;
                case InstallStatus.Removing:     return removing;
                case InstallStatus.Upgrading:    return upgrading;
                case InstallStatus.Upgradeable:  return upgradable;
                case InstallStatus.Installed:    return installed;
                case InstallStatus.Installing:   return installing;
                case InstallStatus.NotInstalled: return notinstalled;
                default:                         return "";
            }
        }

        private long totalInstalledDownloadSize()
        {
            long total = 0;
            foreach (InstalledModule im in registry.InstalledModules) {
                total += im.Module.download_size;
            }
            return total;
        }

        private KSPManager       manager;
        private IRegistryQuerier registry;
        private bool             debug;

        private ConsoleField               searchBox;
        private ConsoleListBox<CkanModule> moduleList;

        private ChangePlan      plan   = new ChangePlan();
        private HashSet<string> recent = new HashSet<string>();

        private const int searchWidth       = 30;
        private const int daysTillStale     = 7;
        private const int daystillVeryStale = 30;

        private static readonly string unavailable  = "!";
        private static readonly string notinstalled = " ";
        private static readonly string installed    = Symbols.checkmark;
        private static readonly string installing   = "+";
        private static readonly string upgradable   = Symbols.greaterEquals;
        private static readonly string upgrading    = "^";
        private static readonly string removing     = "-";
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
        /// <param name="identifier">The mod to add or remove</param>
        public void ToggleRemove(string identifier)
        {
            Install.Remove(identifier);
            Upgrade.Remove(identifier);
            toggleContains(Remove, identifier);
        }

        /// <summary>
        /// Add or remove a mod from the install list
        /// </summary>
        /// <param name="identifier">The mod to add or remove</param>
        public void ToggleInstall(string identifier)
        {
            Upgrade.Remove(identifier);
            Remove.Remove(identifier);
            toggleContains(Install, identifier);
        }

        /// <summary>
        /// Add or remove a mod from the upgrade list
        /// </summary>
        /// <param name="identifier">The mod to add or remove</param>
        public void ToggleUpgrade(string identifier)
        {
            Install.Remove(identifier);
            Remove.Remove(identifier);
            toggleContains(Upgrade, identifier);
        }

        /// <summary>
        /// Return true if we are planning to make any changes, false otherwise
        /// </summary>
        public bool NonEmpty()
        {
             return Install.Count > 0 || Upgrade.Count > 0 || Remove.Count > 0;
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
        /// <param name="manager">KSP manager containing the instances</param>
        /// <param name="registry">Registry of instance being displayed</param>
        /// <param name="identifier">The mod</param>
        /// <returns>
        /// Status of mod
        /// </returns>
        public InstallStatus GetModStatus(KSPManager manager, IRegistryQuerier registry, string identifier)
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
                } else if (!IsAnyAvailable(registry, identifier)) {
                    return InstallStatus.Unavailable;
                } else {
                    return InstallStatus.Installed;
                }
            } else {
                if (Install.Contains(identifier)) {
                    return InstallStatus.Installing;
                } else {
                    return InstallStatus.NotInstalled;
                }
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
        /// Mods we're planning to install
        /// </summary>
        public readonly HashSet<string> Install = new HashSet<string>();

        /// <summary>
        /// Mods we're planning to upgrade
        /// </summary>
        public readonly HashSet<string> Upgrade = new HashSet<string>();

        /// <summary>
        /// Mods we're planning to remove
        /// </summary>
        public readonly HashSet<string> Remove  = new HashSet<string>();
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
    };
}
