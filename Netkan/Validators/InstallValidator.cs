using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class InstallValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            if (json.ContainsKey("install"))
            {
                foreach (JObject stanza in json["install"])
                {
                    string install_to = (string)stanza["install_to"];
                    if (metadata.SpecVersion < v1p2 && install_to.StartsWith("GameData/"))
                    {
                        throw new Kraken("spec_version v1.2+ required for GameData with path");
                    }
                    if (metadata.SpecVersion < v1p29 && install_to.StartsWith("Ships/Script"))
                    {
                        throw new Kraken("spec_version v1.29+ required to install to Ships/Script");
                    }
                    if (metadata.SpecVersion < v1p12 && install_to.StartsWith("Ships/"))
                    {
                        throw new Kraken("spec_version v1.12+ required to install to Ships/ with path");
                    }
                    if (metadata.SpecVersion < v1p16 && install_to.StartsWith("Ships/@thumbs"))
                    {
                        throw new Kraken("spec_version v1.16+ required to install to Ships/@thumbs with path");
                    }
                    if (metadata.SpecVersion < v1p4 && stanza.ContainsKey("find"))
                    {
                        throw new Kraken("spec_version v1.4+ required for install with 'find'");
                    }
                    if (metadata.SpecVersion < v1p10 && stanza.ContainsKey("find_regexp"))
                    {
                        throw new Kraken("spec_version v1.10+ required for install with 'find_regexp'");
                    }
                    if (metadata.SpecVersion < v1p16 && stanza.ContainsKey("find_matches_files"))
                    {
                        throw new Kraken("spec_version v1.16+ required for 'find_matches_files'");
                    }
                    if (metadata.SpecVersion < v1p18 && stanza.ContainsKey("as"))
                    {
                        throw new Kraken("spec_version v1.18+ required for 'as'");
                    }
                    // Check for normalized paths
                    foreach (string propName in pathProperties)
                    {
                        if (stanza.ContainsKey(propName))
                        {
                            string val  = (string)stanza[propName];
                            string norm = CKANPathUtils.NormalizePath(val);
                            if (val != norm)
                            {
                                throw new Kraken($"Path \"{val}\" in '{propName}' is not normalized, should be \"{norm}\"");
                            }
                        }
                    }
                }
            }
        }

        private static readonly ModuleVersion v1p2  = new ModuleVersion("v1.2");
        private static readonly ModuleVersion v1p4  = new ModuleVersion("v1.4");
        private static readonly ModuleVersion v1p10 = new ModuleVersion("v1.10");
        private static readonly ModuleVersion v1p12 = new ModuleVersion("v1.12");
        private static readonly ModuleVersion v1p16 = new ModuleVersion("v1.16");
        private static readonly ModuleVersion v1p18 = new ModuleVersion("v1.18");
        private static readonly ModuleVersion v1p29 = new ModuleVersion("v1.29");
        private static readonly string[] pathProperties = {"find", "file", "install_to"};
    }
}
