using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CKAN.Versioning;

namespace CKAN
{
    public partial class MainAllModVersions : UserControl
    {
        public MainAllModVersions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// React to double click of a version by prompting to install
        /// </summary>
        /// <param name="sender">The version list view</param>
        /// <param name="e">The mouse click event</param>
        public void VersionsListView_DoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info   = ((ListView)sender).HitTest(e.X, e.Y);
            ListViewItem        item   = info.Item;
            CkanModule          module = item.Tag as CkanModule;

            if (module.IsCompatibleKSP(Main.Instance.Manager.CurrentInstance.VersionCriteria())
                && Main.Instance.YesNoDialog($"Install {module}?"))
            {
                Main.Instance.InstallModuleDriver(
                    RegistryManager.Instance(Main.Instance.Manager.CurrentInstance).registry,
                    module
                );
            }
        }

        public GUIMod SelectedModule
        {
            set
            {
                VersionsListView.Items.Clear();

                KSP currentInstance = Main.Instance.Manager.CurrentInstance;
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;

                List<CkanModule> allAvailableVersions;
                try
                {
                    allAvailableVersions = registry.AllAvailable(value.Identifier)
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
                    return toRet;
                }).ToArray());
            }
        }
    }
}
