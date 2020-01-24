using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private bool allowInstall(CkanModule module)
        {
            return module.IsCompatibleKSP(Main.Instance.Manager.CurrentInstance.VersionCriteria())
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

                KSP currentInstance = Main.Instance.Manager.CurrentInstance;
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

                KspVersionCriteria kspVersionCriteria = currentInstance.VersionCriteria();
                ModuleVersion installedVersion = registry.InstalledVersion(value.Identifier);

                bool latestCompatibleVersionAlreadyFound = false;
                VersionsListView.Items.AddRange(allAvailableVersions.Select(module =>
                {
                    ModuleVersion minMod = null, maxMod = null;
                    KspVersion    minKsp = null, maxKsp = null;
                    Registry.GetMinMaxVersions(new List<CkanModule>() {module}, out minMod, out maxMod, out minKsp, out maxKsp);
                    ListViewItem toRet = new ListViewItem(new string[]
                        {
                            module.version.ToString(),
                            KspVersionRange.VersionSpan(minKsp, maxKsp).ToString()
                        })
                    {
                        Tag  = module
                    };

                    if (module.IsCompatibleKSP(kspVersionCriteria))
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
