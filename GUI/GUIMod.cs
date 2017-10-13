using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using CKAN.Versioning;

namespace CKAN
{
    public sealed class GUIMod
    {
        private CkanModule Mod { get; set; }

        public string Name
        {
            get { return Mod.name.Trim(); }
        }

        public bool IsInstalled { get; private set; }
        public bool HasUpdate { get; private set; }
        public bool IsIncompatible { get; private set; }
        public bool IsAutodetected { get; private set; }
        public string Authors { get; private set; }
        public string InstalledVersion { get; private set; }
        public string LatestVersion { get; private set; }
        public string DownloadSize { get; private set; }
        public bool IsCached { get; private set; }

        // These indicate the maximum KSP version that the maximum available
        // version of this mod can handle. The "Long" version also indicates
        // to the user if a mod upgrade would be required. (#1270)
        public string KSPCompatibility { get; private set; }
        public string KSPCompatibilityLong { get; private set; }

        public string KSPversion { get; private set; }
        public string Abstract { get; private set; }
        public string Homepage { get; private set; }
        public string Identifier { get; private set; }
        public bool IsInstallChecked { get; set; }
        public bool IsUpgradeChecked { get; private set; }
        public bool IsNew { get; set; }
        public bool IsCKAN { get; private set; }
        public string Abbrevation { get; private set; }

        public string Version => IsInstalled ? InstalledVersion : LatestVersion;

        public GUIMod(CkanModule mod, IRegistryQuerier registry, KspVersionCriteria currentKspVersion)
        {
            IsCKAN = mod is CkanModule;
            //Currently anything which could alter these causes a full reload of the modlist
            // If this is ever changed these could be moved into the properties
            Mod = mod;
            IsInstalled = registry.IsInstalled(mod.identifier, false);
            IsInstallChecked = IsInstalled;
            HasUpdate = registry.HasUpdate(mod.identifier, currentKspVersion);
            IsIncompatible = !mod.IsCompatibleKSP(currentKspVersion);
            IsAutodetected = registry.IsAutodetected(mod.identifier);
            Authors = mod.author == null ? "N/A" : String.Join(",", mod.author);

            var installedVersion = registry.InstalledVersion(mod.identifier);
            Version latestVersion = null;
            var kspVersion = mod.ksp_version;

            try
            {
                var latestAvailable = registry.LatestAvailable(mod.identifier, currentKspVersion);
                if (latestAvailable != null)
                    latestVersion = latestAvailable.version;
            }
            catch (ModuleNotFoundKraken)
            {
                latestVersion = installedVersion;
            }

            InstalledVersion = installedVersion != null ? installedVersion.ToString() : "-";

            // Let's try to find the compatibility for this mod. If it's not in the registry at
            // all (because it's a DarkKAN mod) then this might fail.

            CkanModule latestAvailableForAnyVersion = null;

            try
            {
                latestAvailableForAnyVersion = registry.LatestAvailable(mod.identifier, null);
            }
            catch
            {
                // If we can't find the mod in the CKAN, but we've a CkanModule installed, then
                // use that.
                if (IsCKAN)
                    latestAvailableForAnyVersion = (CkanModule) mod;
            }

            // If there's known information for this mod in any form, calculate the highest compatible
            // KSP.
            if (latestAvailableForAnyVersion != null)
            {
                KSPCompatibility = KSPCompatibilityLong = registry.LatestCompatibleKSP(mod.identifier)?.ToString() ?? "";

                // If the mod we have installed is *not* the mod we have installed, or we don't know
                // what we have installed, indicate that an upgrade would be needed.
                if (installedVersion == null || !latestAvailableForAnyVersion.version.IsEqualTo(installedVersion))
                {
                    KSPCompatibilityLong = string.Format("{0} (using mod version {1})",
                        KSPCompatibility, latestAvailableForAnyVersion.version);
                }
            }
            else
            {
                // No idea what this mod is, sorry!
                KSPCompatibility = KSPCompatibilityLong = "unknown";
            }

            if (latestVersion != null)
            {
                LatestVersion = latestVersion.ToString();
            }
            else if (latestAvailableForAnyVersion != null)
            {
                LatestVersion = latestAvailableForAnyVersion.version.ToString();
            }
            else
            {
                LatestVersion = "-";
            }

            KSPversion = kspVersion != null ? kspVersion.ToString() : "-";

            Abstract = mod.@abstract;

            // If we have a homepage provided, use that; otherwise use the spacedock page, curse page or the github repo so that users have somewhere to get more info than just the abstract.

            Homepage = "N/A";
            if (mod.resources != null)
            {
                if (mod.resources.homepage != null)
                {
                    Homepage = mod.resources.homepage.ToString();
                }
                else if (mod.resources.spacedock != null)
                {
                    Homepage = mod.resources.spacedock.ToString();
                }
                else if (mod.resources.curse != null)
                {
                    Homepage = mod.resources.curse.ToString();
                }
                else if (mod.resources.repository != null)
                {
                    Homepage = mod.resources.repository.ToString();
                }
            }

            Identifier = mod.identifier;

            if (mod.download_size == 0)
                DownloadSize = "N/A";
            else if (mod.download_size / 1024.0 < 1)
                DownloadSize = "1<KB";
            else
                DownloadSize = mod.download_size / 1024+"";

            Abbrevation = new string(mod.name.Split(' ').
                Where(s => s.Length > 0).Select(s => s[0]).ToArray());

            UpdateIsCached();
        }

        public void UpdateIsCached()
        {
            if (Main.Instance?.CurrentInstance?.Cache == null || Mod?.download == null)
            {
                return;
            }

            IsCached = Main.Instance.CurrentInstance.Cache.IsMaybeCachedZip(Mod.download);
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
                var changeType = IsInstalled ? GUIModChangeType.Remove : GUIModChangeType.Install;
                return new KeyValuePair<GUIMod, GUIModChangeType>(this, changeType);
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

        public void SetUpgradeChecked(DataGridViewRow row, bool? newValue = null)
        {
            var updateCell = row.Cells[1] as DataGridViewCheckBoxCell;
            if (updateCell == null)
            {
                throw new Kraken("Expected second column to contain a checkbox.");
            }
            var oldValue = (bool) updateCell.Value;

            var value = newValue ?? oldValue;
            IsUpgradeChecked = value;
            if (oldValue != value) updateCell.Value = value;
        }

        public void SetInstallChecked(DataGridViewRow row, bool? newValue = null)
        {
            var installCell = row.Cells[0] as DataGridViewCheckBoxCell;
            if (installCell == null)
            {
                throw new Kraken("Expected first column to contain a checkbox.");
            }

            var changeTo = newValue ?? (bool)installCell.Value;
            //Need to do this check here to prevent an infinite loop
            //which is at least happening on Linux
            //TODO: Elimate the cause
            if (changeTo == IsInstallChecked) return;
            IsInstallChecked = changeTo;
            installCell.Value = IsInstallChecked;
        }

        private bool Equals(GUIMod other)
        {
            return Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GUIMod) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
