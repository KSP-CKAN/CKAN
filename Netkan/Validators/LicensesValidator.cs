using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class LicensesValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            JArray licenses = !json.ContainsKey("license") ? null
                : json["license"] is JArray array
                    ? array
                    : new JArray() { json["license"] };
            if (licenses != null)
            {
                if (metadata.SpecVersion < v1p8 && json["license"] is JArray)
                {
                    throw new Kraken("spec_version v1.8+ required for license as array");
                }
                foreach (var lic in licenses)
                {
                    if (metadata.SpecVersion < v1p2 && (string)lic == "WTFPL")
                    {
                        throw new Kraken("spec_version v1.2+ required for license 'WTFPL'");
                    }
                    if (metadata.SpecVersion < v1p18 && (string)lic == "Unlicense")
                    {
                        throw new Kraken("spec_version v1.18+ required for license 'Unlicense'");
                    }
                    if (metadata.SpecVersion < v1p30 && (string)lic == "MPL-2.0")
                    {
                        throw new Kraken("spec_version v1.30+ required for license 'MPL-2.0'");
                    }
                }
            }
            var kref = (string)json["$kref"] ?? "";
            if (!metanetkan.IsMatch(kref) && !json.ContainsKey("x_netkan_license_ok"))
            {
                if (licenses == null || licenses.Count < 1)
                {
                    throw new Kraken("License should match spec");
                }
                else
                {
                    foreach (var lic in licenses)
                    {
                        try
                        {
                            // This will throw BadMetadataKraken if the license isn't known
                            new License((string)lic);
                        }
                        catch
                        {
                            throw new Kraken($"License {lic} should match spec");
                        }
                    }
                }
            }
        }

        private static readonly Regex metanetkan = new Regex(
            @"^#/ckan/netkan/",
            RegexOptions.Compiled
        );
        private static readonly ModuleVersion v1p2  = new ModuleVersion("v1.2");
        private static readonly ModuleVersion v1p8  = new ModuleVersion("v1.8");
        private static readonly ModuleVersion v1p18 = new ModuleVersion("v1.18");
        private static readonly ModuleVersion v1p30 = new ModuleVersion("v1.30");
    }
}
