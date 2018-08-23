using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using CKAN.Versioning;
using CKAN.GameVersionProviders;

namespace CKAN
{
    public sealed class GUIMod
    {
        private CkanModule Mod { get; set; }

        public string Name { get; private set; }
        public bool IsInstalled { get; private set; }
        public bool HasUpdate { get; private set; }
        public bool IsIncompatible { get; private set; }
        public bool IsAutodetected { get; private set; }
        public string Authors { get; private set; }
        public string InstalledVersion { get; private set; }
        public DateTime? InstallDate { get; private set; }
        public string LatestVersion { get; private set; }
        public string DownloadSize { get; private set; }
        public bool IsCached { get; private set; }

        // These indicate the maximum KSP version that the maximum available
        // version of this mod can handle. The "Long" version also indicates
        // to the user if a mod upgrade would be required. (#1270)
        public string KSPCompatibility { get; private set; }
        public string KSPCompatibilityLong { get; private set; }

        public string Abstract { get; private set; }
        public string Homepage { get; private set; }
        public string Identifier { get; private set; }
        public bool IsInstallChecked { get; set; }
        public bool IsUpgradeChecked { get; private set; }
        public bool IsNew { get; set; }
        public bool IsCKAN { get; private set; }
        public string Abbrevation { get; private set; }

        /// <summary>
        /// Return whether this mod is installable.
        /// Used for determining whether to show a checkbox in the leftmost column.
        /// </summary>
        /// <returns>
        /// False if the mod is auto detected,
        /// otherwise true if it's compatible or already installed,
        /// otherwise false.
        /// </returns>
        public bool IsInstallable()
        {
            // Auto detected mods are never installable
            if (IsAutodetected)
                return false;
            // Compatible mods are installable, but so are mods that are already installed
            return !IsIncompatible || IsInstalled;
        }

        public string Version
        {
            get { return IsInstalled ? InstalledVersion : LatestVersion; }
        }

        /// <summary>
        /// Initialize a GUIMod based on an InstalledModule
        /// </summary>
        /// <param name="instMod">The installed module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_ksp_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(InstalledModule instMod, IRegistryQuerier registry, KspVersionCriteria current_ksp_version, bool incompatible = false)
            : this(instMod.Module, registry, current_ksp_version, incompatible)
        {
            IsInstalled      = true;
            IsInstallChecked = true;
            InstallDate      = instMod.InstallTime;
            InstalledVersion = instMod.Module.version.ToString();
            if (LatestVersion == null || LatestVersion.Equals("-"))
            {
                LatestVersion = InstalledVersion;
            }
        }

        /// <summary>
        /// Initialize a GUIMod based on a CkanModule
        /// </summary>
        /// <param name="mod">The module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_ksp_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(CkanModule mod, IRegistryQuerier registry, KspVersionCriteria current_ksp_version, bool incompatible = false)
            : this(mod.identifier, registry, current_ksp_version, incompatible)
        {
            Mod           = mod;
            IsCKAN        = mod is CkanModule;

            Name          = mod.name.Trim();
            Abstract      = mod.@abstract.Trim();
            Abbrevation   = new string(Name.Split(' ').Where(s => s.Length > 0).Select(s => s[0]).ToArray());
            Authors       = mod.author == null ? "N/A" : String.Join(",", mod.author);

            HasUpdate      = registry.HasUpdate(mod.identifier, current_ksp_version);
            DownloadSize   = mod.download_size == 0 ? "N/A" : CkanModule.FmtSize(mod.download_size);
            IsIncompatible = IsIncompatible || !mod.IsCompatibleKSP(current_ksp_version);

            if (mod.resources != null)
            {
                Homepage = mod.resources.homepage?.ToString()
                        ?? mod.resources.spacedock?.ToString()
                        ?? mod.resources.curse?.ToString()
                        ?? mod.resources.repository?.ToString()
                        ?? "N/A";
            }

            UpdateIsCached();
        }

        /// <summary>
        /// Initialize a GUIMod based on just an identifier
        /// </summary>
        /// <param name="identifier">The id of the module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_ksp_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(string identifier, IRegistryQuerier registry, KspVersionCriteria current_ksp_version, bool incompatible = false)
        {
            Identifier     = identifier;
            IsIncompatible = incompatible;
            IsAutodetected = registry.IsAutodetected(identifier);

            ModuleVersion latest_version = null;
            try
            {
                latest_version = registry.LatestAvailable(identifier, current_ksp_version)?.version;
            }
            catch (ModuleNotFoundKraken)
            {
            }

            // Let's try to find the compatibility for this mod. If it's not in the registry at
            // all (because it's a DarkKAN mod) then this might fail.

            CkanModule latest_available_for_any_ksp = null;

            try
            {
                latest_available_for_any_ksp = registry.LatestAvailable(identifier, null);
            }
            catch
            { }

            // If there's known information for this mod in any form, calculate the highest compatible
            // KSP.
            if (latest_available_for_any_ksp != null)
            {
                KSPCompatibility = registry.LatestCompatibleKSP(identifier)?.ToYalovString()
                    ?? "Unknown";
                KSPCompatibilityLong = $"{KSPCompatibility} (using mod version {latest_available_for_any_ksp.version})";
            }
            else
            {
                // No idea what this mod is, sorry!
                KSPCompatibility = KSPCompatibilityLong = "unknown";
            }

            if (latest_version != null)
            {
                LatestVersion = latest_version.ToString();
            }
            else if (latest_available_for_any_ksp != null)
            {
                LatestVersion = latest_available_for_any_ksp.version.ToString();
            }
            else
            {
                LatestVersion = "-";
            }

            // If we have a homepage provided, use that; otherwise use the spacedock page, curse page or the github repo so that users have somewhere to get more info than just the abstract.

            Homepage = "N/A";
        }

        public void UpdateIsCached()
        {
            if (Main.Instance?.CurrentInstance?.Cache == null || Mod?.download == null)
            {
                return;
            }

            IsCached = Main.Instance.CurrentInstance.Cache.IsMaybeCachedZip(Mod);
        }

        public CkanModule ToCkanModule()
        {
            if (!IsCKAN) throw new InvalidCastException("Method can not be called unless IsCKAN");
            var mod = Mod as CkanModule;
            return mod;
        }

        public CkanModule ToModule()
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

        public static implicit operator CkanModule(GUIMod mod)
        {
            return mod.ToModule();
        }

        public void SetUpgradeChecked(DataGridViewRow row, bool? set_value_to = null)
        {
            //Contract.Requires<ArgumentException>(row.Cells[1] is DataGridViewCheckBoxCell);
            var update_cell = row.Cells[1] as DataGridViewCheckBoxCell;
            var old_value = (bool) update_cell.Value;

            bool value = set_value_to ?? old_value;
            IsUpgradeChecked = value;
            if (old_value != value) update_cell.Value = value;
        }

        public void SetInstallChecked(DataGridViewRow row, bool? set_value_to = null)
        {
            //Contract.Requires<ArgumentException>(row.Cells[0] is DataGridViewCheckBoxCell);
            var install_cell = row.Cells[0] as DataGridViewCheckBoxCell;
            if (install_cell != null)
            {
                bool changeTo = set_value_to ?? (bool)install_cell.Value;
                if (IsInstallChecked != changeTo)
                {
                    IsInstallChecked = changeTo;
                }
                // Setting this property causes ModList_CellValueChanged to be called,
                // which calls SetInstallChecked again. Treat it conservatively.
                if ((bool)install_cell.Value != IsInstallChecked)
                {
                    install_cell.Value = IsInstallChecked;
                    // These calls are needed to force the UI to update,
                    // otherwise the checkbox will look checked when it's unchecked or vice versa
                    row.DataGridView.RefreshEdit();
                    row.DataGridView.Refresh();
                }
            }
        }

        private bool Equals(GUIMod other)
        {
            return Equals(Identifier, other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GUIMod) obj);
        }

        public override int GetHashCode()
        {
            return Identifier?.GetHashCode() ?? 0;
        }

    }
}
