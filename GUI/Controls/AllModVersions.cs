using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using CKAN.Versioning;

namespace CKAN
{
    public partial class AllModVersions : UserControl
    {
        public AllModVersions()
        {
            InitializeComponent();
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
            return module.IsCompatibleKSP(Main.Instance.CurrentInstance.VersionCriteria())
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
                    string.Format(Properties.Resources.AllModVersionsInstallPrompt, module.ToString()),
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
                }
                VersionsListView.Items.Clear();
                if (value == null)
                {
                    return;
                }

                // Get all the data; can put this in bg if slow
                GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;
                var installer = new ModuleInstaller(
                    currentInstance,
                    Main.Instance.Manager.Cache,
                    Main.Instance.currentUser);
                Dictionary<CkanModule, bool> allAvailableVersions = null;
                try
                {
                    allAvailableVersions = registry.AvailableByIdentifier(value.Identifier)
                        .ToDictionary(m => m,
                            m => installable(installer, m, registry));
                }
                catch (ModuleNotFoundKraken)
                {
                    // Identifier unknown to registry, maybe installed from local .ckan
                    allAvailableVersions = new Dictionary<CkanModule, bool>();
                }

                // Take the module associated with GUIMod, if any, and append it to the list if it's not already there.
                var installedModule = value.InstalledMod?.Module;
                if (installedModule != null && !allAvailableVersions.ContainsKey(installedModule))
                {
                    allAvailableVersions.Add(installedModule, installable(installer, installedModule, registry));
                }

                if (!allAvailableVersions.Any())
                {
                    return;
                }

                ModuleVersion installedVersion = registry.InstalledVersion(value.Identifier);

                // Update UI; must be in fg
                ignoreItemCheck = true;
                bool latestCompatibleVersionAlreadyFound = false;

                // Only show checkboxes for non-DLC modules
                VersionsListView.CheckBoxes = !value.ToModule().IsDLC;

                VersionsListView.Items.AddRange(allAvailableVersions
                    .OrderByDescending(kvp => kvp.Key.version)
                    .Select(kvp =>
                {
                    CkanModule module = kvp.Key;
                    ModuleVersion minMod = null, maxMod = null;
                    GameVersion   minKsp = null, maxKsp = null;
                    Registry.GetMinMaxVersions(new List<CkanModule>() {module}, out minMod, out maxMod, out minKsp, out maxKsp);
                    ListViewItem toRet = new ListViewItem(new string[]
                        {
                            module.version.ToString(),
                            GameVersionRange.VersionSpan(currentInstance.game, minKsp, maxKsp),
                            module.release_date?.ToString("g") ?? ""
                        })
                    {
                        Tag  = module
                    };

                    if (kvp.Value)
                    {
                        if (!latestCompatibleVersionAlreadyFound)
                        {
                            latestCompatibleVersionAlreadyFound = true;
                            toRet.BackColor = Color.Green;
                            toRet.ForeColor = Color.White;
                        }
                        else
                        {
                            toRet.BackColor = Color.LightGreen;
                        }
                    }

                    if (installedVersion != null && installedVersion.IsEqualTo(module.version))
                    {
                        toRet.Font = new Font(toRet.Font, FontStyle.Bold);
                    }
                    if (module.Equals(value.SelectedMod))
                    {
                        toRet.Checked = true;
                    }
                    return toRet;
                }).ToArray());
                ignoreItemCheck = false;
            }
        }
    }
}
