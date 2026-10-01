using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;

using CKAN.IO;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Main
    {
        private static readonly ILog urlLog = LogManager.GetLogger("URL");

        private void WireProtocolRouter()
        {
            ProtocolRouter.OnFocus += HandleProtocolFocus;
            ProtocolRouter.OnSearch += HandleProtocolSearch;
            ProtocolRouter.OnInstall += HandleProtocolInstall;
            ProtocolRouter.OnError += HandleProtocolError;
        }

        private void UnwireProtocolRouter()
        {
            ProtocolRouter.OnFocus -= HandleProtocolFocus;
            ProtocolRouter.OnSearch -= HandleProtocolSearch;
            ProtocolRouter.OnInstall -= HandleProtocolInstall;
            ProtocolRouter.OnError -= HandleProtocolError;
        }

        private static bool ModalDialogOpen()
            => Application.OpenForms.OfType<Form>().Any(f => f.Modal);

        [ForbidGUICalls]
        private void InvokeIfReady(Action action)
        {
            // Invoke async because we don't want the URL pipe to be held up by UI rebuilding in the install case.
            Util.AsyncInvoke(this, () =>
            {
                if (Waiting || ManageMods.MainModList == null || tabController.TabLocked || ModalDialogOpen())
                {
                    urlLog.Warn("Ignoring URL because CKAN is busy");
                    RaiseToForeground();
                }
                else
                {
                    action();
                }
            });
        }

        // Bring CKAN to the foreground after handling a ckan:// URL.
        // The url handler stub grants us its foreground rights first. Without them Windows demotes this to a taskbar flash.
        private void RaiseToForeground()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            Activate();
        }

        [ForbidGUICalls]
        private void HandleProtocolError(UrlError error)
        {
            urlLog.WarnFormat("Cannot handle URL: {0}", error);

            InvokeIfReady(() =>
            {
                // Foreground before the modal, or it opens behind the window.
                RaiseToForeground();
                ErrorDialog("{0}", ProtocolRouter.ErrorMessage(error));
            });
        }

        [ForbidGUICalls]
        private void HandleProtocolFocus(string identifier)
        {
            urlLog.DebugFormat("Focus requested: {0}", identifier);

            InvokeIfReady(() =>
            {
                tabController.ShowTab(ManageModsTabPage.Name);

                // Clear any search that might be hiding the mod.
                ManageMods.SetSearches(new List<ModSearch>());

                ManageMods.FocusMod(identifier, exactMatch: true, showAsFirst: true);
                RaiseToForeground();
            });
        }

        [ForbidGUICalls]
        private void HandleProtocolSearch(string query)
        {
            urlLog.DebugFormat("Search requested: {0}", query);

            InvokeIfReady(() =>
            {
                if (CurrentInstance == null)
                {
                    urlLog.Warn("Instance not loaded. Cannot search");
                    return;
                }

                tabController.ShowTab(ManageModsTabPage.Name);
                RaiseToForeground();

                var search = ModSearch.Parse(ModuleLabelList.ModuleLabels, CurrentInstance, query);
                if (search == null)
                {
                    urlLog.WarnFormat("Could not parse search: {0}", query);
                    return;
                }

                ManageMods.SetSearches(new List<ModSearch> { search });
            });
        }

        [ForbidGUICalls]
        private void HandleProtocolInstall(List<(string Mod, string? Version)> mods)
        {
            urlLog.DebugFormat("Install requested: {0}", string.Join(", ", mods.Select(m => m.Mod)));

            InvokeIfReady(() =>
            {
                if (CurrentInstance == null)
                {
                    urlLog.Warn("Instance not loaded. Cannot mark anything for install");
                    return;
                }

                var rows = ManageMods.MainModList?.full_list_of_mod_rows;
                if (rows == null)
                {
                    urlLog.Warn("Mod list not loaded. Cannot mark anything for install");
                    return;
                }

                RaiseToForeground();

                // Resolve mods from the URL and have the user confirm or decide any issues.

                var inst = CurrentInstance;
                var reg = RegistryManager.Instance(inst, repoData).registry;
                var crit = inst.VersionCriteria();

                List<ResolvedMod> resolved = InstallResolver.Resolve(mods, reg, inst.StabilityToleranceConfig, crit);
                InstallUrlPlan plan = InstallUrlPrompt.Confirm(resolved, currentUser, inst.Game.ShortName, crit.ToSummaryString(inst.Game));

                // Freeze so the changeset recomputes once at the end instead of once per mod.
                ManageMods.WithFrozenChangeset(() =>
                {
                    foreach (var module in plan.Install)
                    {
                        // Setting SelectedMod is the same thing as when the user ticks an install checkbox.
                        // Therefore URL clicks accumulate with anything already marked and the user can remove mods normally.
                        if (rows.TryGetValue(module.identifier, out var row)
                            && row.Tag is GUIMod gmod)
                        {
                            gmod.SelectedMod = module;
                        }
                        else
                        {
                            urlLog.WarnFormat("No mod list row for {0}. Cannot mark for install", module.identifier);
                        }
                    }

                    ManageMods.MarkModsForReinstall(plan.Reinstall);
                });

                // Take user to changeset screen.
                if (plan.Any)
                {
                    tabController.ShowTab(ChangesetTabPage.Name, 1);
                }
            });
        }
    }
}