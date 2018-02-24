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
                Version installedVersion = registry.InstalledVersion(value.Identifier);

                bool latestCompatibleVersionAlreadyFound = false;
                VersionsListView.Items.AddRange(allAvailableVersions.Select(module =>
                {
                    ListViewItem toRet = new ListViewItem();
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

                    toRet.Text = module.version.ToString();
                    toRet.SubItems.Add(module.HighestCompatibleKSP());
                    return toRet;

                }).ToArray());
            }
        }
    }
}
