using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        /// <param name="mgr">KSP manager containing game instances</param>
        /// <param name="cp">Plan of other mods to be added or removed</param>
        /// <param name="m">The module to display</param>
        public ModInfoScreen(KSPManager mgr, ChangePlan cp, CkanModule m)
        {
            mod      = m;
            manager  = mgr;
            plan     = cp;
            registry = RegistryManager.Instance(manager.CurrentInstance).registry;

            int midL = Console.WindowWidth / 2 - 1;

            AddObject(new ConsoleLabel(
                1, 1, -1,
                () => mod.name == mod.identifier ? mod.name : $"{mod.name} ({mod.identifier})",
                () => ConsoleTheme.Current.ActiveFrameFg
            ));
            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => $"By {string.Join(", ", mod.author)}"
            ));

            AddObject(new ConsoleFrame(
                1, 3, midL, 7,
                () => "",
                () => ConsoleTheme.Current.NormalFrameFg,
                false
            ));
            AddObject(new ConsoleLabel(
                3, 4, 11,
                () => "License:",
                () => ConsoleTheme.Current.DimLabelFg
            ));
            AddObject(new ConsoleLabel(
                13, 4, midL - 2,
                () => string.Join(", ", Array.ConvertAll<License, string>(
                        mod.license.ToArray(), (l => l.ToString())))
            ));
            AddObject(new ConsoleLabel(
                3, 5, 12,
                () => $"Download:",
                () => ConsoleTheme.Current.DimLabelFg
            ));
            AddObject(new ConsoleLabel(
                13, 5, midL - 2,
                () => ModUtils.FmtSize(mod.download_size)
            ));
            AddObject(new ConsoleLabel(
                3, 6, midL - 2,
                HostedOn
            ));

            int depsBot = addDependencies();
            int versBot = addVersionDisplay();

            AddObject(new ConsoleFrame(
                1, Math.Max(depsBot, versBot) + 1, -1, -1,
                () => "Description",
                () => ConsoleTheme.Current.NormalFrameFg,
                false
            ));
            ConsoleTextBox tb = new ConsoleTextBox(
                3, Math.Max(depsBot, versBot) + 2, -3, -2, false,
                TextAlign.Left,
                () => ConsoleTheme.Current.MainBg,
                () => ConsoleTheme.Current.LabelFg
            );
            tb.AddLine(mod.@abstract);
            if (!string.IsNullOrEmpty(mod.description)
                    && mod.description != mod.@abstract) {
                tb.AddLine(mod.description);
            }
            AddObject(tb);
            if (!ChangePlan.IsAnyAvailable(registry, mod.identifier)) {
                tb.AddLine("\r\nNOTE: This mod is installed but no longer available.");
                tb.AddLine("If you uninstall it, CKAN will not be able to re-install it.");
            }

            AddTip("Esc", "Back");
            AddBinding(Keys.Escape, (object sender) => false);

            AddTip(
                "Alt+D", "Download",
                () => !manager.CurrentInstance.Cache.IsMaybeCachedZip(mod.download)
            );
            AddBinding(Keys.AltD, (object sender) => {
                Download();
                return true;
            });

            if (mod.resources != null) {
                List<ConsoleMenuOption> opts = new List<ConsoleMenuOption>();

                if (mod.resources.homepage != null) {
                    opts.Add(new ConsoleMenuOption(
                        "Home page",  "", "Open the home page URL in a browser",
                        true,
                        () => LaunchURL(mod.resources.homepage)
                    ));
                }
                if (mod.resources.repository != null) {
                    opts.Add(new ConsoleMenuOption(
                        "Repository", "", "Open the repository URL in a browser",
                        true,
                        () => LaunchURL(mod.resources.repository)
                    ));
                }
                if (mod.resources.bugtracker != null) {
                    opts.Add(new ConsoleMenuOption(
                        "Bugtracker", "", "Open the bug tracker URL in a browser",
                        true,
                        () => LaunchURL(mod.resources.bugtracker)
                    ));
                }
                if (mod.resources.spacedock != null) {
                    opts.Add(new ConsoleMenuOption(
                        "SpaceDock",  "", "Open the SpaceDock URL in a browser",
                        true,
                        () => LaunchURL(mod.resources.spacedock)
                    ));
                }
                if (mod.resources.curse != null) {
                    opts.Add(new ConsoleMenuOption(
                        "Curse",      "", "Open the Curse URL in a browser",
                        true,
                        () => LaunchURL(mod.resources.curse)
                    ));
                }

                if (opts.Count > 0) {
                    mainMenu = new ConsolePopupMenu(opts);
                }
            }

            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => "Mod Details";
        }

        /// <summary>
        /// Return the menu of resource URLs
        /// </summary>
        /// <returns>
        /// Main menu object created in the constructor,
        /// or null if the mod defines no URLs
        /// </returns>
        protected override ConsolePopupMenu GetMainMenu()
        {
            return mainMenu;
        }

        private bool LaunchURL(Uri u)
        {
            // I'm getting error output on Linux, because this runs xdg-open which
            // calls chromium-browser which prints a bunch of stuff about plugins that
            // no one cares about.  Which corrupts the screen.
            // But redirecting stdout requires UseShellExecute=false, which doesn't
            // support launching URLs!  .NET's API design has painted us into a corner.
            // So instead we display a popup dialog for the garbage to print all over,
            // then wait 1.5 seconds and refresh the screen when it closes.
            ConsoleMessageDialog d = new ConsoleMessageDialog("Launching...", new List<string>());
            d.Run(() => {
                Process.Start(new ProcessStartInfo() {
                    UseShellExecute = true,
                    FileName        = u.ToString()
                });
                System.Threading.Thread.Sleep(1500);
            });
            return true;
        }

        private int addDependencies(int top = 8)
        {
            int numDeps  = mod.depends?.Count   ?? 0;
            int numConfs = mod.conflicts?.Count ?? 0;

            if (numDeps + numConfs > 0) {
                int midL = Console.WindowWidth / 2 - 1;
                int h    = Math.Min(11, numDeps + numConfs + 2);
                const int lblW = 16;
                int nameW = midL - 2 - lblW - 2;

                AddObject(new ConsoleFrame(
                    1, top, midL, top + h - 1,
                    () => "Dependencies",
                    () => ConsoleTheme.Current.NormalFrameFg,
                    false
                ));
                if (numDeps > 0) {
                    AddObject(new ConsoleLabel(
                        3, top + 1, 3 + lblW - 1,
                        () => $"Required ({numDeps}):",
                        () => ConsoleTheme.Current.DimLabelFg
                    ));
                    ConsoleTextBox tb = new ConsoleTextBox(
                        3 + lblW, top + 1, midL - 2, top + 1 + numDeps - 1, false,
                        TextAlign.Left,
                        () => ConsoleTheme.Current.MainBg,
                        () => ConsoleTheme.Current.LabelFg
                    );
                    AddObject(tb);
                    foreach (RelationshipDescriptor rd in mod.depends) {
                        tb.AddLine(ScreenObject.FormatExactWidth(
                            // Show install status
                            ModListScreen.StatusSymbol(plan.GetModStatus(manager, registry, rd.name))
                                + rd.name,
                            nameW
                        ));
                    }
                }
                if (numConfs > 0) {
                    AddObject(new ConsoleLabel(
                        3, top + 1 + numDeps, 3 + lblW - 1,
                        () => $"Conflicts ({numConfs}):",
                        () => ConsoleTheme.Current.DimLabelFg
                    ));
                    ConsoleTextBox tb = new ConsoleTextBox(
                        3 + lblW, top + 1 + numDeps, midL - 2, top + h - 2, false,
                        TextAlign.Left,
                        () => ConsoleTheme.Current.MainBg,
                        () => ConsoleTheme.Current.LabelFg
                    );
                    AddObject(tb);
                    // FUTURE: Find mods that conflict with this one
                    //         See GUI/MainModList.cs::ComputeConflictsFromModList
                    foreach (RelationshipDescriptor rd in mod.conflicts) {
                        tb.AddLine(ScreenObject.FormatExactWidth(
                            // Show install status
                            ModListScreen.StatusSymbol(plan.GetModStatus(manager, registry, rd.name))
                            + rd.name,
                            nameW
                        ));
                    }
                }
                return top + h - 1;
            }
            return top - 1;
        }

        private DateTime InstalledOn(string identifier)
        {
            return registry.InstalledModule(identifier).InstallTime;
        }

        private int addVersionDisplay()
        {
            int       boxLeft  = Console.WindowWidth / 2 + 1,
                      boxTop   = 3;
            const int boxRight = -1,
                      boxH     = 5;

            if (ChangePlan.IsAnyAvailable(registry, mod.identifier)) {

                List<CkanModule> avail              = registry.AllAvailable(       mod.identifier);
                CkanModule       inst               = registry.GetInstalledVersion(mod.identifier);
                CkanModule       latest             = registry.LatestAvailable(    mod.identifier, null);
                bool             installed          = registry.IsInstalled(mod.identifier, false);
                bool             latestIsInstalled  = inst?.Equals(latest) ?? false;
                List<CkanModule> others             = avail;

                others.Remove(inst);
                others.Remove(latest);

                if (installed) {

                    DateTime instTime = InstalledOn(mod.identifier);

                    if (latestIsInstalled) {

                        addVersionBox(
                            boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                            () => $"Latest/Installed {instTime.ToString("d")}",
                            () => ConsoleTheme.Current.ActiveFrameFg,
                            true,
                            new List<CkanModule>() {inst}
                        );
                        boxTop += boxH;

                    } else {

                        addVersionBox(
                            boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                            () => "Latest Version",
                            () => ConsoleTheme.Current.AlertFrameFg,
                            false,
                            new List<CkanModule>() {latest}
                        );
                        boxTop += boxH;

                        addVersionBox(
                            boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                            () => $"Installed {instTime.ToString("d")}",
                            () => ConsoleTheme.Current.ActiveFrameFg,
                            true,
                            new List<CkanModule>() {inst}
                        );
                        boxTop += boxH;

                    }
                } else {

                    addVersionBox(
                        boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                        () => "Latest Version",
                        () => ConsoleTheme.Current.NormalFrameFg,
                        false,
                        new List<CkanModule>() {latest}
                    );
                    boxTop += boxH;

                }

                if (others.Count > 0) {

                    addVersionBox(
                        boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                        () => "Other Versions",
                        () => ConsoleTheme.Current.NormalFrameFg,
                        false,
                        others
                    );
                    boxTop += boxH;

                }

            } else {

                DateTime instTime = InstalledOn(mod.identifier);
                // Mod is no longer indexed, but we can generate a display
                // of the old info about it from when we installed it
                addVersionBox(
                    boxLeft, boxTop, boxRight, boxTop + boxH - 1,
                    () => $"UNAVAILABLE/Installed {instTime.ToString("d")}",
                    () => ConsoleTheme.Current.AlertFrameFg,
                    true,
                    new List<CkanModule>() {mod}
                );
                boxTop += boxH;

            }

            return boxTop - 1;
        }

        private void addVersionBox(int l, int t, int r, int b, Func<string> title, Func<ConsoleColor> color, bool doubleLine, List<CkanModule> releases)
        {
            AddObject(new ConsoleFrame(
                l, t, r, b,
                title,
                color,
                doubleLine
            ));

            if (releases != null && releases.Count > 0) {

                Version    minMod = null, maxMod = null;
                KspVersion minKsp = null, maxKsp = null;

                foreach (CkanModule rel in releases) {
                    if (minMod == null || minMod > rel.version) {
                        minMod = rel.version;
                    }
                    if (maxMod == null || maxMod < rel.version) {
                        maxMod = rel.version;
                    }
                    KspVersion relMin = rel.EarliestCompatibleKSP();
                    KspVersion relMax = rel.LatestCompatibleKSP();
                    if (minKsp == null || !minKsp.IsAny && (minKsp > relMin || relMin.IsAny)) {
                        minKsp = relMin;
                    }
                    if (maxKsp == null || !maxKsp.IsAny && (maxKsp < relMax || relMax.IsAny)) {
                        maxKsp = relMax;
                    }
                }

                AddObject(new ConsoleLabel(
                    l + 2, t + 1, r - 2,
                    () => minMod == maxMod
                        ? $"{ModUtils.WithAndWithoutEpoch(minMod.ToString())}"
                        : $"{ModUtils.WithAndWithoutEpoch(minMod.ToString())} - {ModUtils.WithAndWithoutEpoch(maxMod.ToString())}",
                    color
                ));
                AddObject(new ConsoleLabel(
                    l + 2, t + 2, r - 2,
                    () => "Compatible with:",
                    () => ConsoleTheme.Current.DimLabelFg
                ));
                AddObject(new ConsoleLabel(
                    l + 4, t + 3, r - 2,
                    () => VersionSpan(minKsp, maxKsp),
                    color
                ));

            }
        }

        private static string SameVersionString(KspVersion v)
        {
            return v.IsAny ? "all versions" : v.ToString();
        }

        private static string VersionSpan(KspVersion minKsp, KspVersion maxKsp)
        {
            return minKsp == maxKsp ? $"KSP {SameVersionString(minKsp)}"
                :  minKsp.IsAny     ? $"KSP {maxKsp} and earlier"
                :  maxKsp.IsAny     ? $"KSP {minKsp} and later"
                :                     $"KSP {minKsp} - {maxKsp}";
        }

        private string HostedOn()
        {
            string dl = mod.download.ToString();
            foreach (var kvp in hostDomains) {
                if (dl.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                    return $"Hosted on {kvp.Value}";
                }
            }
            if (mod.resources != null) {
                if (mod.resources.bugtracker != null) {
                    string bt = mod.resources.bugtracker.ToString();
                    foreach (var kvp in hostDomains) {
                        if (bt.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            return $"Report bugs on {kvp.Value}";
                        }
                    }
                }
                if (mod.resources.repository != null) {
                    string rep = mod.resources.repository.ToString();
                    foreach (var kvp in hostDomains) {
                        if (rep.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            return $"Repository on {kvp.Value}";
                        }
                    }
                }
                if (mod.resources.homepage != null) {
                    string hp = mod.resources.homepage.ToString();
                    foreach (var kvp in hostDomains) {
                        if (hp.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase) >= 0) {
                            return $"Home page on {kvp.Value}";
                        }
                    }
                }
            }
            return mod.download.Host;
        }

        private void Download()
        {
            ProgressScreen            ps   = new ProgressScreen($"Downloading {mod.identifier}");
            NetAsyncModulesDownloader dl   = new NetAsyncModulesDownloader(ps);
            ModuleInstaller           inst = ModuleInstaller.GetInstance(manager.CurrentInstance, ps);
            LaunchSubScreen(
                ps,
                () => dl.DownloadModules(inst.Cache, new List<CkanModule> {mod})
            );
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

        private KSPManager       manager;
        private IRegistryQuerier registry;
        private ChangePlan       plan;
        private CkanModule       mod;
        private ConsolePopupMenu mainMenu;
    }

    /// <summary>
    /// Functions for formatting mod info
    /// </summary>
    public static class ModUtils {

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string StripEpoch(string version)
        {
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            return epochMatch.IsMatch(version)
                ? epochReplace.Replace(version, @"$2")
                : version;
        }

        /// <summary>
        /// As above, but includes the original in parentheses
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string WithAndWithoutEpoch(string version)
        {
            return epochMatch.IsMatch(version)
                ? $"{epochReplace.Replace(version, @"$2")} ({version})"
                : version;
        }

        /// <summary>
        /// Format a byte count into readable file size
        /// </summary>
        /// <param name="bytes">Number of bytes in a file</param>
        /// <returns>
        /// ### bytes or ### KB or ### MB or ### GB
        /// </returns>
        public static string FmtSize(long bytes)
        {
            const double K = 1024;
            if (bytes < K) {
                return $"{bytes} bytes";
            } else if (bytes < K * K) {
                return $"{bytes / K:N1} KB";
            } else if (bytes < K * K * K) {
                return $"{bytes / K / K:N1} MB";
            } else {
                return $"{bytes / K / K / K:N1} GB";
            }
        }

        private static readonly Regex epochMatch   = new Regex(@"^[0-9][0-9]*:[^:]+$");
        private static readonly Regex epochReplace = new Regex(@"^([^:]+):([^:]+)$");
    }

}
