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

        private bool installable(CkanModule module)
        {
            var currentInstance = Main.Instance.Manager.CurrentInstance;
            var version = currentInstance.VersionCriteria();
            if (!module.IsCompatibleKSP(version))
            {
                return false;
            }
            try
            {
                RelationshipResolver resolver = new RelationshipResolver(
                    new CkanModule[] { module },
                    null,
                    new RelationshipResolverOptions()
                    {
                        with_recommends = false,
                        without_toomanyprovides_kraken = true,
                    },
                    RegistryManager.Instance(currentInstance).registry,
                    version
                );
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool allowInstall(CkanModule module)
        {
            return installable(module)
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
                ignoreItemCheck = true;
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
                // Only show checkboxes for non-DLC modules
                VersionsListView.CheckBoxes = !value.ToModule().IsDLC;

                GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;

                List<CkanModule> allAvailableVersions;
                try
                {
                    allAvailableVersions = registry.AvailableByIdentifier(value.Identifier)
                        .OrderByDescending(m => m.version)
                        .ToList();
                }
                catch (ModuleNotFoundKraken)
                {
                    // No versions to be shown, abort and hope an auto refresh happens
                    return;
                }

                ModuleVersion installedVersion = registry.InstalledVersion(value.Identifier);

                bool latestCompatibleVersionAlreadyFound = false;
                VersionsListView.Items.AddRange(allAvailableVersions.Select(module =>
                {
                    ModuleVersion minMod = null, maxMod = null;
                    GameVersion   minKsp = null, maxKsp = null;
                    Registry.GetMinMaxVersions(new List<CkanModule>() {module}, out minMod, out maxMod, out minKsp, out maxKsp);
                    ListViewItem toRet = new ListViewItem(new string[]
                        {
                            module.version.ToString(),
                            GameVersionRange.VersionSpan(currentInstance.game, minKsp, maxKsp)
                        })
                    {
                        Tag  = module
                    };

                    if (installable(module))
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
