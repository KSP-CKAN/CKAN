using System;

namespace CKAN
{
    public class GUIMod
    {
        private readonly Registry registry;
        private CkanModule Mod { get; set; }
        public string Name { get { return Mod.name; } }

        public bool IsInstalled
        {
            get
            {
                return Mod.IsInstalled;
            }
        }

        public bool HasUpdate
        {
            get
            {
                return Mod.HasUpdate;
            }
        }

        public bool IsIncompatible
        {
            get
            {
                return Mod.IsIncompatible;
            }
        }

        public bool IsAutodetected
        {
            get
            {
                return Mod.IsAutodetected;
            }
        }

        public string Authors
        {
            get
            {
                return Mod.author == null ? "N/A" : String.Join(",", Mod.author);
            }
        }

        public string InstalledVersion { get; private set; }
        public string LatestVersion { get; private set; }
        public string KSPversion { get; private set; }

        public string Abstract
        {
            get { return Mod.@abstract; }
        }

        public object Homepage { get; private set; }

        public string Identifier
        {
            get { return Mod.identifier; }
        }

        public bool IsInstallChecked { get; set; }
        public bool IsUpgradeChecked { get; set; }


        public GUIMod(CkanModule mod, Registry registry)
        {
            this.registry = registry;
            //Currently anything which could alter these causes a full reload of the modlist
            // If this is ever changed these could be moved into the properties
            Mod = mod;
            IsInstallChecked = IsInstalled;

            var installed_version = registry.InstalledVersion(mod.identifier);
            var latest_version = mod.version;
            var ksp_version = mod.ksp_version;

            InstalledVersion = installed_version != null ? installed_version.ToString() : "-";
            LatestVersion = latest_version != null ? latest_version.ToString() : "-";
            KSPversion = ksp_version != null ? ksp_version.ToString() : "-";

            Homepage = mod.resources != null && mod.resources.homepage != null
                ? (object)mod.resources.homepage
                : "N/A";
        }

        public CkanModule ToCkanModule()
        {
            return Mod;
        }
    }
}