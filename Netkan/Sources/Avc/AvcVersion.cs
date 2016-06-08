using CKAN.Versioning;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Avc
{
    public class AvcVersion
    {
        // Right now we only support KSP versioning info.

        [JsonProperty("URL")]
        public string Url;

        [JsonConverter(typeof(JsonAvcToVersion))]
        public Version version;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KspVersion ksp_version;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KspVersion ksp_version_min;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KspVersion ksp_version_max;
    }
}
