using System;

namespace CKAN
{
    public class GUIMod
    {
        private CkanModule Mod { get; set; }
        public string Name{ get { return Mod.name; }}
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
        public bool IsUpgradeChecked { get; set; }


        public GUIMod(CkanModule mod, Registry registry)
        {
            Mod = mod;
            IsInstalled = registry.IsInstalled(mod.identifier);
            IsInstallChecked = IsInstalled;
            HasUpdate = IsInstalled && Mod.version.IsGreaterThan(registry.InstalledVersion(mod.identifier));
            IsIncompatible = !registry.IsCompatible(mod.identifier);
            //TODO Remove magic values
            IsAutodetected = IsInstalled && registry.InstalledVersion(mod.identifier).ToString().Equals("autodetected dll");
            Authors = mod.author == null ? "N/A" : String.Join(",", mod.author);
            
            var installedVersion = registry.InstalledVersion(mod.identifier);
            var latestVersion = mod.version;
            var kspVersion = mod.ksp_version;

            InstalledVersion = installedVersion != null ? installedVersion.ToString() : "-";            
            LatestVersion = latestVersion != null ? latestVersion.ToString() : "-";            
            KSPversion = kspVersion != null ? kspVersion.ToString() : "-";

            Abstract = mod.@abstract;
            Homepage = mod.resources != null && mod.resources.homepage != null
                ? (object) mod.resources.homepage
                : "N/A";

            Identifier = mod.identifier;
        }

        public CkanModule ToCkanModule()
        {
            return Mod;
        }
    }
}