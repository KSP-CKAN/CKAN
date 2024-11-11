using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Games;
using CKAN.Extensions;
using CKAN.Versioning;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Versions : UserControl
    {
        public Versions()
        {
            InitializeComponent();
            repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();
        }

        /// <summary>
        /// Make the ListView redraw itself.
        /// Works around a problem where the headers aren't drawn when this tab activates.
        /// </summary>
        public void ForceRedraw()
        {
            VersionsListView.EndUpdate();
        }

        public GUIMod? SelectedModule
        {
            set
            {
                if (!(visibleGuiModule?.Equals(value) ?? value?.Equals(visibleGuiModule) ?? true))
                {
                    // Stop background loading of row colors
                    cancelTokenSrc?.Cancel();
                    // Listen for property changes (we only care about GUIMod.SelectedMod)
                    if (visibleGuiModule != null)
                    {
                        visibleGuiModule.PropertyChanged -= visibleGuiModule_PropertyChanged;
                    }
                    visibleGuiModule = value;
                    if (visibleGuiModule != null)
                    {
                        visibleGuiModule.PropertyChanged += visibleGuiModule_PropertyChanged;
                    }

                    if (value != null)
                    {
                        Refresh(value);
                    }
                }
            }
        }

        private static GameInstance?        currentInstance => Main.Instance?.CurrentInstance;
        private static GameInstanceManager? manager         => Main.Instance?.Manager;
        private static IUser?               user            => Main.Instance?.currentUser;

        private readonly RepositoryDataManager    repoData;
        private          GUIMod?                  visibleGuiModule;
        private          bool                     ignoreItemCheck;
        private          CancellationTokenSource? cancelTokenSrc;

        private void VersionsListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!ignoreItemCheck && e.CurrentValue != e.NewValue && visibleGuiModule != null
                && VersionsListView.Items[e.Index].Tag is CkanModule module)
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        if (allowInstall(module))
                        {
                            // Add this version to the change set
                            visibleGuiModule.SelectedMod = module;
                        }
                        else
                        {
                            // Abort! Abort!
                            e.NewValue = CheckState.Unchecked;
                        }
                        break;

                    case CheckState.Unchecked:
                        // Remove or cancel installation
                        visibleGuiModule.SelectedMod = null;
                        break;
                }
            }
        }

        private void VersionsListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs? e)
        {
            if (e is {Item: {Selected: true}})
            {
                e.Item.Selected = false;
                e.Item.Focused  = false;
            }
        }

        [ForbidGUICalls]
        private static bool installable(CkanModule       module,
                                        IRegistryQuerier registry,
                                        ReleaseStatus    stabilityTolerance)
            => currentInstance != null
                && module.release_status <= stabilityTolerance
                && installable(module, registry,
                               currentInstance.game,
                               currentInstance.VersionCriteria());

        [ForbidGUICalls]
        private static bool installable(CkanModule          module,
                                        IRegistryQuerier    registry,
                                        IGame               game,
                                        GameVersionCriteria crit)
            => module.IsCompatible(crit)
               && currentInstance != null
               && ModuleInstaller.CanInstall(new List<CkanModule>() { module },
                                             RelationshipResolverOptions.DependsOnlyOpts(currentInstance.StabilityToleranceConfig),
                                             registry, game, crit);

        private bool allowInstall(CkanModule module)
        {
            if (currentInstance == null || manager?.Cache == null || user == null)
            {
                return false;
            }
            var stabilityTolerance = currentInstance.StabilityToleranceConfig.ModStabilityTolerance(module.identifier)
                                     ?? currentInstance.StabilityToleranceConfig.OverallStabilityTolerance;
            IRegistryQuerier registry = RegistryManager.Instance(currentInstance, repoData).registry;

            return installable(module, registry, stabilityTolerance)
                || (Main.Instance?.YesNoDialog(
                    module.release_status > stabilityTolerance
                        ? string.Format(Properties.Resources.AllModVersionsPrereleasePrompt,
                                        module,
                                        module.release_status.LocalizeName(),
                                        stabilityTolerance.LocalizeName())
                        : string.Format(Properties.Resources.AllModVersionsInstallPrompt,
                                        module,
                                        currentInstance.VersionCriteria()
                                                       .ToSummaryString(currentInstance.game)),
                    Properties.Resources.AllModVersionsInstallYes,
                    Properties.Resources.AllModVersionsInstallNo) ?? false);
        }

        private void visibleGuiModule_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
        {
            switch (e?.PropertyName)
            {
                case nameof(GUIMod.SelectedMod):
                    UpdateSelection();
                    break;
            }
        }

        private void UpdateSelection()
        {
            if (visibleGuiModule != null)
            {
                bool prevIgnore = ignoreItemCheck;
                ignoreItemCheck = true;
                foreach (var item in VersionsListView.Items.OfType<ListViewItem>())
                {
                    var module = item.Tag as CkanModule;
                    item.Checked = module?.Equals(visibleGuiModule.SelectedMod) ?? false;
                }
                ignoreItemCheck = prevIgnore;
            }
        }

        private List<CkanModule> getVersions(GUIMod gmod)
        {
            if (currentInstance != null)
            {
                var registry = RegistryManager.Instance(currentInstance, repoData).registry;

                // Can't be functional because AvailableByIdentifier throws exceptions
                var versions = new List<CkanModule>();
                try
                {
                    versions = registry.AvailableByIdentifier(gmod.Identifier).ToList();
                }
                catch (ModuleNotFoundKraken)
                {
                    // Identifier unknown to registry, maybe installed from local .ckan
                }

                // Take the module associated with GUIMod, if any, and append it to the list if it's not already there.
                var installedModule = gmod.InstalledMod?.Module;
                if (installedModule != null && !versions.Contains(installedModule))
                {
                    versions.Add(installedModule);
                }

                return versions;
            }
            return new List<CkanModule>();
        }

        private ListViewItem[] getItems(GUIMod gmod, List<CkanModule> versions)
        {
            if (currentInstance != null)
            {
                var registry         = RegistryManager.Instance(currentInstance, repoData).registry;
                var installedVersion = registry.InstalledVersion(gmod.Identifier);
                var stabilityTolerance = currentInstance.StabilityToleranceConfig.ModStabilityTolerance(gmod.Identifier)
                                         ?? currentInstance.StabilityToleranceConfig.OverallStabilityTolerance;

                var items = versions.OrderByDescending(module => module.version)
                    .Select(module =>
                {
                    CkanModule.GetMinMaxVersions(
                        new List<CkanModule>() {module},
                        out ModuleVersion? minMod, out ModuleVersion? maxMod,
                        out GameVersion? minKsp,   out GameVersion? maxKsp);
                    ListViewItem toRet = new ListViewItem(new string[]
                    {
                        module.version.ToString(),
                        GameVersionRange.VersionSpan(currentInstance.game,
                                                     minKsp ?? GameVersion.Any,
                                                     maxKsp ?? GameVersion.Any),
                        module.release_date?.ToString("g") ?? ""
                    })
                    {
                        Tag = module,
                    };
                    if (installedVersion != null && installedVersion.IsEqualTo(module.version))
                    {
                        toRet.Font = new Font(toRet.Font,
                                              module.release_status <= stabilityTolerance
                                                  ? InstalledLabel.Font.Style
                                                  : InstalledLabel.Font.Style
                                                    | PrereleaseLabel.Font.Style);
                    }
                    else if (module.release_status > stabilityTolerance)
                    {
                        toRet.Font = new Font(toRet.Font, PrereleaseLabel.Font.Style);
                    }
                    if (module.release_status > stabilityTolerance)
                    {
                        toRet.BackColor = PrereleaseLabel.BackColor;
                    }
                    if (module.Equals(gmod.SelectedMod))
                    {
                        toRet.Checked = true;
                    }
                    return toRet;
                }).ToArray();

                return items;
            }
            return Array.Empty<ListViewItem>();
        }

        [ForbidGUICalls]
        private void checkInstallable(ListViewItem[] items)
        {
            if (currentInstance != null && manager?.Cache != null
                && user != null && cancelTokenSrc != null && visibleGuiModule != null)
            {
                var stabilityTolerance = currentInstance.StabilityToleranceConfig.ModStabilityTolerance(visibleGuiModule.Identifier)
                                         ?? currentInstance.StabilityToleranceConfig.OverallStabilityTolerance;
                var registry = RegistryManager.Instance(currentInstance, repoData).registry;
                ListViewItem? latestCompatible = null;
                // Load balance the items so they're processed roughly in-order instead of blocks
                Partitioner.Create(items, true)
                           // Distribute across cores
                           .AsParallel()
                           // Return them as they're processed
                           .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                           // Abort when they switch to another mod
                           .WithCancellation(cancelTokenSrc.Token)
                           // Check the important ones first
                           .OrderBy(item => (item.Tag as CkanModule) != visibleGuiModule.InstalledMod?.Module
                                            && (item.Tag as CkanModule) != visibleGuiModule.SelectedMod)
                           // Slow step to be performed across multiple cores
                           .Where(item => item.Tag is CkanModule m
                                          && installable(m, registry, stabilityTolerance))
                           // Jump back to GUI thread for the updates for each compatible item
                           .ForAll(item => Util.Invoke(this, () =>
                           {
                               if (latestCompatible == null || item.Index < latestCompatible.Index)
                               {
                                   VersionsListView.BeginUpdate();
                                   if (latestCompatible != null)
                                   {
                                       // Revert color of previous best guess
                                       latestCompatible.BackColor = CompatibleLabel.BackColor;
                                       latestCompatible.ForeColor = CompatibleLabel.ForeColor;
                                   }
                                   latestCompatible = item;
                                   item.BackColor = LatestCompatibleLabel.BackColor;
                                   item.ForeColor = LatestCompatibleLabel.ForeColor;
                                   VersionsListView.EndUpdate();
                               }
                               else
                               {
                                   item.BackColor = CompatibleLabel.BackColor;
                               }
                           }));
                Util.Invoke(this, () => UseWaitCursor = false);
            }
        }

        private void Refresh(GUIMod gmod)
        {
            if (currentInstance != null)
            {
                // checkInstallable needs this to stop background threads on switch to another mod
                cancelTokenSrc     = new CancellationTokenSource();
                var startingModule = gmod;
                var versions       = getVersions(gmod);
                var items          = getItems(gmod, versions);
                var stabilityTolerance = currentInstance.StabilityToleranceConfig.ModStabilityTolerance(gmod.Identifier)
                                         ?? currentInstance.StabilityToleranceConfig.OverallStabilityTolerance;
                Util.AsyncInvoke(this, () =>
                {
                    UpdateStabilityToleranceComboBox(gmod);
                    LabelTable.SuspendLayout();
                    InstalledLabel.Visible = gmod.IsInstalled;
                    PrereleaseLabel.Visible = versions.Any(m => m.release_status > stabilityTolerance);
                    LabelTable.ResumeLayout();
                    VersionsListView.BeginUpdate();
                    VersionsListView.Items.Clear();
                    // Make sure user hasn't switched to another mod while we were loading
                    if (startingModule.Equals(visibleGuiModule))
                    {
                        // Only show checkboxes for non-DLC modules
                        VersionsListView.CheckBoxes = !gmod.ToModule().IsDLC;
                        ignoreItemCheck = true;
                        VersionsListView.Items.AddRange(items);
                        VersionsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        VersionsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        ignoreItemCheck = false;
                        VersionsListView.EndUpdate();
                        // Check installability in the background because it's slow
                        UseWaitCursor = true;
                        Task.Factory.StartNew(() => checkInstallable(items));
                    }
                });
            }
        }

        private void UpdateStabilityToleranceComboBox(GUIMod gmod)
        {
            if (currentInstance != null)
            {
                StabilityToleranceComboBox.Items.Clear();
                StabilityToleranceComboBox.Items.AddRange(
                    Enumerable.Repeat((ReleaseStatus?)null, 1)
                              .Concat(Enum.GetValues(typeof(ReleaseStatus))
                                          .OfType<ReleaseStatus>()
                                          .OrderBy(relStat => (int)relStat)
                                          .OfType<ReleaseStatus?>())
                              .Select(relStat => new ReleaseStatusItem(relStat))
                              .ToArray());
                StabilityToleranceComboBox.SelectedIndex =
                    StabilityToleranceComboBox.Items
                                              .OfType<ReleaseStatusItem>()
                                              .Select(item => item.Value)
                                              .ToList()
                                              .IndexOf(currentInstance.StabilityToleranceConfig
                                                                      .ModStabilityTolerance(gmod.Identifier));
            }
        }

        private void StabilityToleranceComboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            // Don't change values on scroll
            if (e is HandledMouseEventArgs me)
            {
                me.Handled = true;
            }
        }

        private void StabilityToleranceComboBox_SelectionChanged(object? sender, EventArgs? e)
        {
            if (currentInstance != null && visibleGuiModule != null
                && StabilityToleranceComboBox.SelectedItem is ReleaseStatusItem item)
            {
                var ident = visibleGuiModule.Identifier;
                currentInstance.StabilityToleranceConfig.SetModStabilityTolerance(
                    ident, item.Value);
            }
        }
    }
}
