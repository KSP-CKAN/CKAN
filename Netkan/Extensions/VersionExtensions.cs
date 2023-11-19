using CKAN.Versioning;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Extensions
{
    internal static class VersionExtensions
    {
        public static JToken ToSpecVersionJson(this ModuleVersion specVersion)
            => specVersion.IsEqualTo(new ModuleVersion("v1.0"))
                ? (JToken)1
                : (JToken)specVersion.ToString();
    }
}
