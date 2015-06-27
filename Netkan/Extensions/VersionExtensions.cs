using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Extensions
{
    internal static class VersionExtensions
    {
        public static JToken ToSpecVersionJson(this Version specVersion)
        {
            if (specVersion.IsEqualTo(new Version("v1.0")))
            {
                return 1;
            }
            else
            {
                return specVersion.ToString();
            }
        }
    }
}
