using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{
    public class GUIMod
    {
        private CkanModule Mod { get; set; }
        public string Name { get { return Mod.name; } }
        public bool IsInstalled { get; private set; }
        public bool HasUpdate { get; private set; }
        public bool IsIncompatible { get; private set; }
        public bool IsAutodetected { get; private set; }
        public string Authors { get; private set; }
        public string InstalledVersion { get; private set; }
        public string LatestVersion { get; private set; }
        public string KSPversion { get; private set; }
        public string Abstract { get; private set; }
        public object Homepage { get; private set; }
        public string Identifier { get; private set; }
        public bool IsInstallChecked { get; set; }
        public bool IsUpgradeChecked { get; private set; }


        public GUIMod(CkanModule mod, Registry registry, KSPVersion ksp_version)
        {
            //Currently anything which could alter these causes a full reload of the modlist
            // If this is ever changed these could be moved into the properties
            Mod = mod;
            IsInstalled = registry.IsInstalled(mod.identifier);
            IsInstallChecked = IsInstalled;
            HasUpdate = registry.HasUpdate(mod.identifier);
            IsIncompatible = !mod.IsCompatibleKSP(ksp_version);
            IsAutodetected = registry.IsAutodetected(mod.identifier);
            Authors = mod.author == null ? "N/A" : String.Join(",", mod.author);

            var installedVersion = registry.InstalledVersion(mod.identifier);
            var latestVersion = mod.version;
            var kspVersion = mod.ksp_version;

            InstalledVersion = installedVersion != null ? installedVersion.ToString() : "-";
            LatestVersion = latestVersion != null ? latestVersion.ToString() : "-";
            KSPversion = kspVersion != null ? kspVersion.ToString() : "-";

            Abstract = mod.@abstract;
            Homepage = mod.resources != null && mod.resources.homepage != null
                ? (object)mod.resources.homepage
                : "N/A";

            Identifier = mod.identifier;
        }

        public CkanModule ToCkanModule()
        {
            return Mod;
        }

        public KeyValuePair<CkanModule, GUIModChangeType>? GetRequestedChange()
        {
            if (IsInstalled ^ IsInstallChecked)
            {
                var change_type = IsInstalled ? GUIModChangeType.Remove : GUIModChangeType.Install;
                return new KeyValuePair<CkanModule, GUIModChangeType>(Mod, change_type);
            }

            if (IsInstalled && (IsInstallChecked && HasUpdate && IsUpgradeChecked))
            {
                return new KeyValuePair<CkanModule, GUIModChangeType>(Mod, GUIModChangeType.Update);
            }
            return null;
        }

        public static implicit operator CkanModule(GUIMod mod)
        {
            return mod.ToCkanModule();
        }

        public void SetUpgradeChecked(DataGridViewRow row, bool? setvalueto = null)
        {
            //Contract.Requires<ArgumentException>(row.Cells[1] is DataGridViewCheckBoxCell);

            var update_cell = row.Cells[1] as DataGridViewCheckBoxCell;

            bool value = (setvalueto.HasValue ? setvalueto.Value : (bool)update_cell.Value);
            IsUpgradeChecked = value;
            update_cell.Value = value;
        }

        public void SetInstallChecked(DataGridViewRow row)
        {
            //Contract.Requires<ArgumentException>(row.Cells[0] is DataGridViewCheckBoxCell);
            var install_cell = row.Cells[0] as DataGridViewCheckBoxCell;
            IsInstallChecked = (bool)install_cell.Value;
        }
    }
}
