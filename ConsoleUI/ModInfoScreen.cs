using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CKAN.Versioning;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen showing details about a mod
    /// </summary>
    public class ModInfoScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="mgr">Game instance manager containing game instances</param>
        /// <param name="registry">Registry of the current instance for finding mods</param>
        /// <param name="userAgent">HTTP useragent string to use</param>
        /// <param name="cp">Plan of other mods to be added or removed</param>
        /// <param name="m">The module to display</param>
        /// <param name="dbg">True if debug options should be available, false otherwise</param>
        public ModInfoScreen(ConsoleTheme        theme,
                             GameInstanceManager mgr,
                             Registry            registry,
                             string?             userAgent,
                             ChangePlan          cp,
                             CkanModule          m,
                             bool                dbg)
            : base(theme)
        {
            debug    = dbg;
            mod      = m;
            manager  = mgr;
            plan     = cp;
            this.registry = registry;
            this.userAgent = userAgent;

            int midL = (Console.WindowWidth / 2) - 1;

            AddObject(new ConsoleLabel(
                1, 1, -1,
                () => mod.name == mod.identifier ? mod.name : $"{mod.name} ({mod.identifier})",
                null,
                th => th.ActiveFrameFg
            ));
            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => string.Format(Properties.Resources.ModInfoAuthors, string.Join(", ", mod.author))
            ));

            AddObject(new ConsoleFrame(
                1, 3, midL, 7,
                () => "",
                th => th.NormalFrameFg,
                false
            ));
            AddObject(new ConsoleLabel(
                3, 4, 11,
                () => Properties.Resources.ModInfoLicence,
                null,
                th => th.DimLabelFg
            ));
            AddObject(new ConsoleLabel(
                13, 4, midL - 2,
                () => string.Join(", ", Array.ConvertAll(
                        mod.license.ToArray(), l => l.ToString()))
            ));
            AddObject(new ConsoleLabel(
                3, 5, 12,
                () => Properties.Resources.ModInfoDownload,
                null,
                th => th.DimLabelFg
            ));
            AddObject(new ConsoleLabel(
                13, 5, midL / 2,
                () => CkanModule.FmtSize(mod.download_size)
            ));
            if (mod.install_size > 0)
            {
                AddObject(new ConsoleLabel(
                    midL / 2, 5, (midL / 2) + 9,
                    () => "Install:",
                    null,
                    th => th.DimLabelFg
                ));
                AddObject(new ConsoleLabel(
                    (midL / 2) + 9, 5, midL - 2,
                    () => CkanModule.FmtSize(mod.install_size)
                ));
            }
            AddObject(new ConsoleLabel(
                3, 6, midL - 2,
                HostedOn
            ));

            int depsBot = addDependencies();
            int versBot = addVersionDisplay();

            AddObject(new ConsoleFrame(
                1, Math.Max(depsBot, versBot) + 1, -1, -1,
                () => Properties.Resources.ModInfoDescriptionFrame,
                th => th.NormalFrameFg,
                false
            ));
            ConsoleTextBox tb = new ConsoleTextBox(
                3, Math.Max(depsBot, versBot) + 2, -3, -2, false,
                TextAlign.Left,
                th => th.MainBg,
                th => th.LabelFg
            );
            tb.AddLine(mod.@abstract);
            if (mod.description != null && !string.IsNullOrEmpty(mod.description)
                    && mod.description != mod.@abstract) {
                tb.AddLine(mod.description);
            }
            AddObject(tb);
            if (!ChangePlan.IsAnyAvailable(registry, mod.identifier)) {
                tb.AddLine(Properties.Resources.ModInfoUnavailableWarning);
            }
            tb.AddScrollBindings(this, theme);

            AddTip(Properties.Resources.Esc, Properties.Resources.Back);
            AddBinding(Keys.Escape, (object sender) => false);

            AddTip($"{Properties.Resources.Ctrl}+D", Properties.Resources.ModInfoDownloadToCache,
                () => manager.Cache != null && !manager.Cache.IsMaybeCachedZip(mod) && !mod.IsDLC
            );
            AddBinding(Keys.CtrlD, (object sender) => {
                if (!mod.IsDLC) {
                    Download();
                }
                return true;
            });

            if (mod.resources != null) {
                var opts = new List<ConsoleMenuOption?>();

                if (mod.resources.homepage != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoHomePage,  "", Properties.Resources.ModInfoHomePageTip,
                        true,
                        () => LaunchURL(theme, mod.resources.homepage)));
                }
                if (mod.resources.repository != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoRepository, "", Properties.Resources.ModInfoRepositoryTip,
                        true,
                        () => LaunchURL(theme, mod.resources.repository)));
                }
                if (mod.resources.bugtracker != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoBugtracker, "", Properties.Resources.ModInfoBugtrackerTip,
                        true,
                        () => LaunchURL(theme, mod.resources.bugtracker)));
                }
                if (mod.resources.discussions != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoDiscussions, "", Properties.Resources.ModInfoDiscussionsTip,
                        true,
                        () => LaunchURL(theme, mod.resources.discussions)));
                }
                if (mod.resources.spacedock != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoSpaceDock,  "", Properties.Resources.ModInfoSpaceDockTip,
                        true,
                        () => LaunchURL(theme, mod.resources.spacedock)));
                }
                if (mod.resources.curse != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoCurse,      "", Properties.Resources.ModInfoCurseTip,
                        true,
                        () => LaunchURL(theme, mod.resources.curse)));
                }
                if (mod.resources.store != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoStore,      "", Properties.Resources.ModInfoStoreTip,
                        true,
                        () => LaunchURL(theme, mod.resources.store)));
                }
                if (mod.resources.steamstore != null) {
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoSteamStore, "", Properties.Resources.ModInfoSteamStoreTip,
                        true,
                        () => LaunchURL(theme, mod.resources.steamstore)));
                }
                if (debug) {
                    opts.Add(null);
                    opts.Add(new ConsoleMenuOption(
                        Properties.Resources.ModInfoViewMetadata, "", Properties.Resources.ModInfoViewMetadataTip,
                        true,
                        ViewMetadata));
                }

                if (opts.Count > 0) {
                    mainMenu = new ConsolePopupMenu(opts);
                }
            }
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
            => Properties.Resources.ModInfoTitle;

        /// <summary>
        /// Label menu as Links
        /// </summary>
        protected override string MenuTip()
            => Properties.Resources.ModInfoMenuTip;

        private bool ViewMetadata()
        {
            var md = new ConsoleMessageDialog(
                theme,
                $"\"{mod.identifier}\": {registry.GetAvailableMetadata(mod.identifier)}",
                new List<string> { Properties.Resources.OK },
                () => string.Format(Properties.Resources.ModInfoViewMetadataTitle, mod.name),
                TextAlign.Left);
            md.Run();
            DrawBackground();
            return true;
        }

        /// <summary>
        /// Launch a URL in the system browser.
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="u">URL to launch</param>
        /// <returns>
        /// True.
        /// </returns>
        public static bool LaunchURL(ConsoleTheme theme, Uri u)
        {
            // I'm getting error output on Linux, because this runs xdg-open which
            // calls chromium-browser which prints a bunch of stuff about plugins that
            // no one cares about.  Which corrupts the screen.
            // But redirecting stdout requires UseShellExecute=false, which doesn't
            // support launching URLs!  .NET's API design has painted us into a corner.
            // So instead we display a popup dialog for the garbage to print all over,
            // then wait 1.5 seconds and refresh the screen when it closes.
            var d = new ConsoleMessageDialog(theme, Properties.Resources.ModInfoURLLaunching, new List<string>());
            d.Run(() => {
                Utilities.ProcessStartURL(u.ToString());
                Thread.Sleep(1500);
            });
            return true;
        }

        private int addDependencies(int top = 8)
        {
            if (manager.CurrentInstance != null)
            {
                const int lblW = 16;
                int midL = (Console.WindowWidth / 2) - 1;
                int nameW = midL - 2 - lblW - 2 - 1;
                var upgradeable = registry.CheckUpgradeable(manager.CurrentInstance,
                                                            new HashSet<string>())
                                          [true];
                var depends = (mod.depends?.Select(dep => RelationshipString(dep, upgradeable, nameW))
                                          ?? Enumerable.Empty<string>())
                                  .ToArray();
                var conflicts = (mod.conflicts?.Select(con => RelationshipString(con, upgradeable, nameW))
                                              ?? Enumerable.Empty<string>())
                                    .ToArray();

                if (depends.Length + conflicts.Length > 0) {
                    int h    = Math.Min(11, depends.Length + conflicts.Length + 2);
                    int depsH = (h - 2) * depends.Length / (depends.Length + conflicts.Length);

                    AddObject(new ConsoleFrame(
                        1, top, midL, top + h - 1,
                        () => Properties.Resources.ModInfoDependenciesFrame,
                        th => th.NormalFrameFg,
                        false));
                    if (depends.Length > 0) {
                        AddObject(new ConsoleLabel(
                            3, top + 1, 3 + lblW - 1,
                            () => string.Format(Properties.Resources.ModInfoRequiredLabel, depends.Length),
                            null,
                            th => th.DimLabelFg));
                        var tb = new ConsoleTextBox(
                            3 + lblW, top + 1, midL - 2, top + 1 + depsH - 1, false,
                            TextAlign.Left,
                            th => th.MainBg,
                            th => th.LabelFg);
                        AddObject(tb);
                        foreach (var d in depends) {
                            tb.AddLine(d);
                        }
                    }
                    if (conflicts.Length > 0) {
                        AddObject(new ConsoleLabel(
                            3, top + 1 + depsH, 3 + lblW - 1,
                            () => string.Format(Properties.Resources.ModInfoConflictsLabel, conflicts.Length),
                            null,
                            th => th.DimLabelFg));
                        var tb = new ConsoleTextBox(
                            3 + lblW, top + 1 + depsH, midL - 2, top + h - 2, false,
                            TextAlign.Left,
                            th => th.MainBg,
                            th => th.LabelFg);
                        AddObject(tb);
                        // FUTURE: Find mods that conflict with this one
                        //         See GUI/MainModList.cs::ComputeConflictsFromModList
                        foreach (var c in conflicts) {
                            tb.AddLine(c);
                        }
                    }
                    return top + h - 1;
                }
            }
            return top - 1;
        }

        private string RelationshipString(RelationshipDescriptor rel,
                                          List<CkanModule>       upgradeable,
                                          int                    width)
            => ScreenObject.TruncateLength((ModListScreen.StatusSymbol(
                                                rel is ModuleRelationshipDescriptor mrd
                                                    // Show install status
                                                    ? plan.GetModStatus(manager, registry,
                                                                        mrd.name, upgradeable)
                                                    : InstallStatus.NotInstalled))
                                           + rel.ToString(),
                                           width);

        private DateTime? InstalledOn(string identifier)
            // This can be null for manually installed mods
            => registry.InstalledModule(identifier)?.InstallTime;

        private int addVersionDisplay()
        {
            int       boxLeft  = (Console.WindowWidth / 2) + 1,
                      boxTop   = 3;
            const int boxRight = -1,
                      boxH     = 5;

            if (manager.CurrentInstance != null
                && ChangePlan.IsAnyAvailable(registry, mod.identifier)) {

                var  inst              = registry.GetInstalledVersion(mod.identifier);
                var  latest            = registry.LatestAvailable(mod.identifier, null);
                var  others            = registry.AvailableByIdentifier(mod.identifier)
                                                 .Except(new[] { inst, latest }.OfType<CkanModule>())
                                                 .OfType<CkanModule?>()
                                                 .ToList();

                bool installed         = registry.IsInstalled(mod.identifier, false);
                bool latestIsInstalled = inst?.Equals(latest) ?? false;

                if (installed) {

                    DateTime? instTime = InstalledOn(mod.identifier);

                    if (latestIsInstalled) {

                        if (registry.GetReplacement(mod.identifier,
                                                    manager.CurrentInstance.VersionCriteria())
                                is ModuleReplacement mr) {

                            // Show replaced_by
                            addVersionBox(
                                boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                                () => string.Format(Properties.Resources.ModInfoReplacedBy, mr.ReplaceWith.identifier),
                                th => th.AlertFrameFg,
                                false,
                                new List<CkanModule?>() { mr.ReplaceWith });
                            boxTop += boxH;

                            addVersionBox(
                                boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                                () => instTime.HasValue
                                    ? string.Format(Properties.Resources.ModInfoInstalledOn,
                                                    instTime.Value.ToString("d"))
                                    : Properties.Resources.ModInfoInstalledManually,
                                th => th.ActiveFrameFg,
                                true,
                                new List<CkanModule?>() { inst });
                            boxTop += boxH;

                        } else {

                            addVersionBox(
                                boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                                () => instTime.HasValue
                                    ? string.Format(Properties.Resources.ModInfoLatestInstalledOn,
                                                    instTime.Value.ToString("d"))
                                    : Properties.Resources.ModInfoLatestInstalledManually,
                                th => th.ActiveFrameFg,
                                true,
                                new List<CkanModule?>() { inst });
                            boxTop += boxH;

                        }

                    } else {

                        addVersionBox(
                            boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                            () => Properties.Resources.ModInfoLatestVersion,
                            th => th.AlertFrameFg,
                            false,
                            new List<CkanModule?>() { latest });
                        boxTop += boxH;

                        addVersionBox(
                            boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                            () => instTime.HasValue
                                ? string.Format(Properties.Resources.ModInfoInstalledOn,
                                                instTime.Value.ToString("d"))
                                : Properties.Resources.ModInfoInstalledManually,
                            th => th.ActiveFrameFg,
                            true,
                            new List<CkanModule?>() { inst });
                        boxTop += boxH;

                    }
                } else {

                    addVersionBox(
                        boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                        () => Properties.Resources.ModInfoLatestVersion,
                        th => th.NormalFrameFg,
                        false,
                        new List<CkanModule?>() { latest });
                    boxTop += boxH;

                }

                if (others.Count > 0) {

                    addVersionBox(
                        boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                        () => Properties.Resources.ModInfoOtherVersions,
                        th => th.NormalFrameFg,
                        false,
                        others);
                    boxTop += boxH;

                }

            } else {

                DateTime? instTime = InstalledOn(mod.identifier);
                // Mod is no longer indexed, but we can generate a display
                // of the old info about it from when we installed it
                addVersionBox(
                    boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                    () => instTime.HasValue
                        ? string.Format(Properties.Resources.ModInfoUnavailableInstalledOn, instTime.Value.ToString("d"))
                        : Properties.Resources.ModInfoUnavailableInstalledManually,
                    th => th.AlertFrameFg,
                    true,
                    new List<CkanModule?>() { mod });
                boxTop += boxH;

            }
            return boxTop - 1;
        }

        private void addVersionBox(int l, int t, int r, int b,
                                   Func<string> title,
                                   Func<ConsoleTheme, ConsoleColor> color,
                                   bool doubleLine,
                                   List<CkanModule?> releases)
        {
            AddObject(new ConsoleFrame(
                l, t, r, b,
                title,
                color,
                doubleLine
            ));

            if (releases != null && releases.Count > 0) {

                CkanModule.GetMinMaxVersions(releases,
                                             out ModuleVersion? minMod, out ModuleVersion? maxMod,
                                             out GameVersion? minKsp,   out GameVersion? maxKsp);
                AddObject(new ConsoleLabel(
                    l + 2, t + 1, r - 2,
                    () => minMod == maxMod
                        ? $"{ModuleInstaller.WithAndWithoutEpoch(minMod?.ToString() ?? "???")}"
                        : $"{ModuleInstaller.WithAndWithoutEpoch(minMod?.ToString() ?? "???")} - {ModuleInstaller.WithAndWithoutEpoch(maxMod?.ToString() ?? "???")}",
                    null,
                    color
                ));
                AddObject(new ConsoleLabel(
                    l + 2, t + 2, r - 2,
                    () => Properties.Resources.ModInfoCompatibleWith,
                    null,
                    th => th.DimLabelFg
                ));
                AddObject(new ConsoleLabel(
                    l + 4, t + 3, r - 2,
                    () => manager.CurrentInstance == null
                        ? ""
                        : GameVersionRange.VersionSpan(manager.CurrentInstance.game,
                                                       minKsp ?? GameVersion.Any,
                                                       maxKsp ?? GameVersion.Any),
                    null,
                    color
                ));

            }
        }

        private string HostedOn()
        {
            if (mod.download != null && mod.download.Count > 0)
            {
                var downloadHosts = mod.download
                    .Select(dlUri => dlUri.Host)
                    .Select(host =>
                        hostDomains.TryGetValue(host, out string? name)
                            ? name
                            : host);
                return string.Format(Properties.Resources.ModInfoHostedOn,
                                     string.Join(", ", downloadHosts));
            }
            if (mod.resources != null) {
                if (mod.resources.bugtracker != null) {
                    string bt = mod.resources.bugtracker.ToString();
                    foreach (var kvp in hostDomains) {
                        if (bt.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            return string.Format(Properties.Resources.ModInfoReportBugsOn, kvp.Value);
                        }
                    }
                }
                if (mod.resources.repository != null) {
                    string rep = mod.resources.repository.ToString();
                    foreach (var kvp in hostDomains) {
                        if (rep.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            return string.Format(Properties.Resources.ModInfoRepositoryOn, kvp.Value);
                        }
                    }
                }
                if (mod.resources.homepage != null) {
                    string hp = mod.resources.homepage.ToString();
                    foreach (var kvp in hostDomains) {
                        if (hp.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            return string.Format(Properties.Resources.ModInfoHomePageOn, kvp.Value);
                        }
                    }
                }

                if (mod.resources.store != null || mod.resources.steamstore != null) {
                    return mod.resources.steamstore == null ? Properties.Resources.ModInfoBuyFromKSPStore
                        :  mod.resources.store      == null ? Properties.Resources.ModInfoBuyFromSteamStore
                        :                                     Properties.Resources.ModInfoBuyFromKSPStoreOrSteamStore;
                }
            }
            return "";
        }

        private void Download()
        {
            if (manager.CurrentInstance != null && manager.Cache != null)
            {
                var ps   = new ProgressScreen(theme, string.Format(Properties.Resources.ModInfoDownloading, mod.identifier));
                var dl   = new NetAsyncModulesDownloader(ps, manager.Cache, userAgent);
                var inst = new ModuleInstaller(manager.CurrentInstance, manager.Cache, ps, userAgent);
                LaunchSubScreen(
                    ps,
                    () => {
                        try {
                            dl.DownloadModules(new List<CkanModule> {mod});
                            if (!manager.Cache.IsMaybeCachedZip(mod)) {
                                ps.RaiseError(Properties.Resources.ModInfoDownloadFailed);
                            }
                        } catch (Exception ex) {
                            ps.RaiseError(Properties.Resources.ModInfoDownloadFailed, ex);
                        }
                    }
                );
                // Don't let the installer re-use old screen references
                inst.User = new NullUser();
            }
        }

        private static readonly Dictionary<string, string> hostDomains = new Dictionary<string, string>() {
            { "github.com",                   "GitHub"           },
            { "spacedock.info",               "SpaceDock"        },
            { "archive.org",                  "Internet Archive" },
            { "cursecdn.com",                 "CurseForge"       },
            { "kerbalstuff.com",              "KerbalStuff"      },
            { "dropbox.com",                  "DropBox"          },
            { "forum.kerbalspaceprogram.com", "KSP Forums"       }
        };

        private readonly GameInstanceManager manager;
        private readonly IRegistryQuerier    registry;
        private readonly string?             userAgent;
        private readonly ChangePlan          plan;
        private readonly CkanModule          mod;
        private readonly bool                debug;
    }

}
