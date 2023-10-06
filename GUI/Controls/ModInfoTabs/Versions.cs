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

        public GUIMod SelectedModule
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

        private GameInstance        currentInstance => Main.Instance.CurrentInstance;
        private GameInstanceManager manager         => Main.Instance.Manager;
        private IUser               user            => Main.Instance.currentUser;

        private readonly RepositoryDataManager repoData;
        private GUIMod                  visibleGuiModule;
        private bool                    ignoreItemCheck;
        private CancellationTokenSource cancelTokenSrc;

        private void VersionsListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!ignoreItemCheck && e.CurrentValue != e.NewValue
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

        [ForbidGUICalls]
        private bool installable(ModuleInstaller     installer,
                                 CkanModule          module,
                                 IRegistryQuerier    registry)
            => installable(installer, module, registry, currentInstance.VersionCriteria());

        [ForbidGUICalls]
        private static bool installable(ModuleInstaller     installer,
                                        CkanModule          module,
                                        IRegistryQuerier    registry,
                                        GameVersionCriteria crit)
            => module.IsCompatible(crit)
                && installer.CanInstall(new List<CkanModule>() { module },
                                        RelationshipResolverOptions.DependsOnlyOpts(),
                                        registry, crit);

        private bool allowInstall(CkanModule module)
        {
            IRegistryQuerier registry = RegistryManager.Instance(currentInstance, repoData).registry;
            var installer = new ModuleInstaller(currentInstance, manager.Cache, user);

            return installable(installer, module, registry)
                || Main.Instance.YesNoDialog(
                    string.Format(Properties.Resources.AllModVersionsInstallPrompt,
                        module.ToString(),
                        currentInstance.VersionCriteria().ToSummaryString(currentInstance.game)),
                    Properties.Resources.AllModVersionsInstallYes,
                    Properties.Resources.AllModVersionsInstallNo);
        }

        private void visibleGuiModule_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedMod":
                    UpdateSelection();
                    break;
            }
        }

        private void UpdateSelection()
        {
            bool prevIgnore = ignoreItemCheck;
            ignoreItemCheck = true;
            foreach (ListViewItem item in VersionsListView.Items)
            {
                CkanModule module = item.Tag as CkanModule;
                item.Checked = module.Equals(visibleGuiModule.SelectedMod);
            }
            ignoreItemCheck = prevIgnore;
        }

        private List<CkanModule> getVersions(GUIMod gmod)
        {
            IRegistryQuerier registry = RegistryManager.Instance(currentInstance, repoData).registry;

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

        private ListViewItem[] getItems(GUIMod gmod, List<CkanModule> versions)
        {
            IRegistryQuerier registry         = RegistryManager.Instance(currentInstance, repoData).registry;
            ModuleVersion    installedVersion = registry.InstalledVersion(gmod.Identifier);

            var items = versions.OrderByDescending(module => module.version)
                .Select(module =>
            {
                CkanModule.GetMinMaxVersions(
                    new List<CkanModule>() {module},
                    out ModuleVersion minMod, out ModuleVersion maxMod,
                    out GameVersion minKsp,   out GameVersion maxKsp);
                ListViewItem toRet = new ListViewItem(new string[]
                {
                    module.version.ToString(),
                    GameVersionRange.VersionSpan(currentInstance.game, minKsp, maxKsp),
                    module.release_date?.ToString("g") ?? ""
                })
                {
                    Tag = module
                };
                if (installedVersion != null && installedVersion.IsEqualTo(module.version))
                {
                    toRet.Font = new Font(toRet.Font, FontStyle.Bold);
                }
                if (module.Equals(gmod.SelectedMod))
                {
                    toRet.Checked = true;
                }
                return toRet;
            }).ToArray();

            return items;
        }

        [ForbidGUICalls]
        private void checkInstallable(ListViewItem[] items)
        {
            var registry  = RegistryManager.Instance(currentInstance, repoData).registry;
            var installer = new ModuleInstaller(currentInstance, manager.Cache, user);
            ListViewItem latestCompatible = null;
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
                       .Where(item => installable(installer, item.Tag as CkanModule, registry))
                       // Jump back to GUI thread for the updates for each compatible item
                       .ForAll(item => Util.Invoke(this, () =>
                       {
                           if (latestCompatible == null || item.Index < latestCompatible.Index)
                           {
                               VersionsListView.BeginUpdate();
                               if (latestCompatible != null)
                               {
                                   // Revert color of previous best guess
                                   latestCompatible.BackColor = Color.LightGreen;
                                   latestCompatible.ForeColor = SystemColors.ControlText;
                               }
                               latestCompatible = item;
                               item.BackColor = Color.Green;
                               item.ForeColor = Color.White;
                               VersionsListView.EndUpdate();
                           }
                           else
                           {
                               item.BackColor = Color.LightGreen;
                           }
                       }));
            Util.Invoke(this, () => UseWaitCursor = false);
        }

        private void Refresh(GUIMod gmod)
        {
            // checkInstallable needs this to stop background threads on switch to another mod
            cancelTokenSrc     = new CancellationTokenSource();
            var startingModule = gmod;
            var items          = getItems(gmod, getVersions(gmod));
            Util.AsyncInvoke(this, () =>
            {
                VersionsListView.BeginUpdate();
                VersionsListView.Items.Clear();
                // Make sure user hasn't switched to another mod while we were loading
                if (startingModule.Equals(visibleGuiModule))
                {
                    // Only show checkboxes for non-DLC modules
                    VersionsListView.CheckBoxes = !gmod.ToModule().IsDLC;
                    ignoreItemCheck = true;
                    VersionsListView.Items.AddRange(items);
                    ignoreItemCheck = false;
                    VersionsListView.EndUpdate();
                    // Check installability in the background because it's slow
                    UseWaitCursor = true;
                    Task.Factory.StartNew(() => checkInstallable(items));
                }
            });
        }
    }
}
