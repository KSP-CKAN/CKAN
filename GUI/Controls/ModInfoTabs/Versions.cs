using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

using CKAN.Versioning;

namespace CKAN.GUI
{
    public partial class Versions : UserControl
    {
        public Versions()
        {
            InitializeComponent();
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

        private GUIMod visibleGuiModule = null;
        private bool   ignoreItemCheck  = false;

        private void VersionsListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (ignoreItemCheck || e.CurrentValue == e.NewValue)
            {
                return;
            }
            ListViewItem item   = VersionsListView.Items[e.Index];
            CkanModule   module = item.Tag as CkanModule;
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

        private bool installable(ModuleInstaller installer, CkanModule module, IRegistryQuerier registry)
        {
            return module.IsCompatible(Main.Instance.CurrentInstance.VersionCriteria())
                && installer.CanInstall(
                    RelationshipResolver.DependsOnlyOpts(),
                    new List<CkanModule>() { module },
                    registry);
        }

        private bool allowInstall(CkanModule module)
        {
            GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
            IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;
            var installer = new ModuleInstaller(
                currentInstance,
                Main.Instance.Manager.Cache,
                Main.Instance.currentUser);

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
            GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
            IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;

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
            GameInstance     currentInstance  = Main.Instance.Manager.CurrentInstance;
            IRegistryQuerier registry         = RegistryManager.Instance(currentInstance).registry;
            ModuleVersion    installedVersion = registry.InstalledVersion(gmod.Identifier);

            var items = versions.OrderByDescending(module => module.version)
                .Select(module =>
            {
                Registry.GetMinMaxVersions(
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

        private void checkInstallable(ListViewItem[] items)
        {
            GameInstance     currentInstance = Main.Instance.Manager.CurrentInstance;
            IRegistryQuerier registry        = RegistryManager.Instance(currentInstance).registry;
            bool latestCompatibleVersionAlreadyFound = false;
            var installer = new ModuleInstaller(
                currentInstance,
                Main.Instance.Manager.Cache,
                Main.Instance.currentUser);
            foreach (ListViewItem item in items)
            {
                if (item.ListView == null)
                {
                    // User switched to another mod, quit
                    break;
                }
                if (installable(installer, item.Tag as CkanModule, registry))
                {
                    if (!latestCompatibleVersionAlreadyFound)
                    {
                        latestCompatibleVersionAlreadyFound = true;
                        Util.Invoke(this, () =>
                        {
                            item.BackColor = Color.Green;
                            item.ForeColor = Color.White;
                        });
                    }
                    else
                    {
                        Util.Invoke(this, () =>
                        {
                            item.BackColor = Color.LightGreen;
                        });
                    }
                }
            }
            Util.Invoke(this, () =>
            {
                UseWaitCursor = false;
            });
        }

        private void Refresh(GUIMod gmod)
        {
            Util.Invoke(this, () => VersionsListView.Items.Clear());
            var startingModule = gmod;
            var items          = getItems(gmod, getVersions(gmod));
            // Make sure user hasn't switched to another mod while we were loading
            if (startingModule.Equals(visibleGuiModule))
            {
                // Only show checkboxes for non-DLC modules
                VersionsListView.CheckBoxes = !gmod.ToModule().IsDLC;
                ignoreItemCheck = true;
                VersionsListView.Items.AddRange(items);
                ignoreItemCheck = false;
                // Check installability in the background because it's slow
                UseWaitCursor = true;
                Task.Factory.StartNew(() => checkInstallable(items));
            }
        }
    }
}
