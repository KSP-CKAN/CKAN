using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Avc
{
    public class AvcVersion
    {
        // Right now we only support KSP versioning info.

        [JsonConverter(typeof(JsonAvcToVersion))]
        public Version version;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version_min;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version_max;
    }
}
