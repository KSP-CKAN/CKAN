using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{
    public class GUIMod
    {
        private Module Mod { get; set; }

        public string Name
        {
            get { return Mod.name; }
        }

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
        public bool IsNew { get; set; }
        public bool IsCKAN { get; private set; }

        public string Version
        {
            get { return InstalledVersion ?? LatestVersion; }
        }

        public GUIMod(Module mod, IRegistryQuerier registry, KSPVersion current_ksp_version)
        {
            IsCKAN = mod is CkanModule;
            //Currently anything which could alter these causes a full reload of the modlist
            // If this is ever changed these could be moved into the properties
            Mod = mod;
            IsInstalled = registry.IsInstalled(mod.identifier, false);
            IsInstallChecked = IsInstalled;
            HasUpdate = registry.HasUpdate(mod.identifier, current_ksp_version);
            IsIncompatible = !mod.IsCompatibleKSP(current_ksp_version);
            IsAutodetected = registry.IsAutodetected(mod.identifier);
            Authors = mod.author == null ? "N/A" : String.Join(",", mod.author);

            var installed_version = registry.InstalledVersion(mod.identifier);
            Version latest_version = null;
            var ksp_version = mod.ksp_version;
            try
            {
                var latest_available = registry.LatestAvailable(mod.identifier, current_ksp_version);
                if (latest_available != null)
                    latest_version = latest_available.version;
            }
            catch (ModuleNotFoundKraken)
            {
                latest_version = installed_version;
            }


            InstalledVersion = installed_version != null ? installed_version.ToString() : "-";
            LatestVersion = latest_version != null ? latest_version.ToString() : "-";
            KSPversion = ksp_version != null ? ksp_version.ToString() : "-";

            Abstract = mod.@abstract;
            Homepage = mod.resources != null && mod.resources.homepage != null
                ? (object) mod.resources.homepage
                : "N/A";

            Identifier = mod.identifier;
        }

        public GUIMod(CkanModule mod, IRegistryQuerier registry, KSPVersion current_ksp_version)
            : this((Module) mod, registry, current_ksp_version)
        {
        }

        public CkanModule ToCkanModule()
        {
            if (!IsCKAN) throw new InvalidCastException("Method can not be called unless IsCKAN");
            var mod = Mod as CkanModule;
            return mod;
        }

        public Module ToModule()
        {
            return Mod;
        }

        public KeyValuePair<GUIMod, GUIModChangeType>? GetRequestedChange()
        {
            if (IsInstalled ^ IsInstallChecked)
            {
                var change_type = IsInstalled ? GUIModChangeType.Remove : GUIModChangeType.Install;
                return new KeyValuePair<GUIMod, GUIModChangeType>(this, change_type);
            }
            if (IsInstalled && (IsInstallChecked && HasUpdate && IsUpgradeChecked))
            {
                return new KeyValuePair<GUIMod, GUIModChangeType>(this, GUIModChangeType.Update);
            }
            return null;
        }

        public static implicit operator Module(GUIMod mod)
        {
            return mod.ToModule();
        }

        public void SetUpgradeChecked(DataGridViewRow row, bool? setvalueto = null)
        {
            //Contract.Requires<ArgumentException>(row.Cells[1] is DataGridViewCheckBoxCell);
            var update_cell = row.Cells[1] as DataGridViewCheckBoxCell;
            var old_value = (bool) update_cell.Value;

            bool value = (setvalueto.HasValue ? setvalueto.Value : old_value);
            IsUpgradeChecked = value;
            if (old_value != value) update_cell.Value = value;
        }

        public void SetInstallChecked(DataGridViewRow row)
        {
            //Contract.Requires<ArgumentException>(row.Cells[0] is DataGridViewCheckBoxCell);
            var install_cell = row.Cells[0] as DataGridViewCheckBoxCell;
            IsInstallChecked = (bool) install_cell.Value;
        }

        protected bool Equals(GUIMod other)
        {
            return Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GUIMod) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}