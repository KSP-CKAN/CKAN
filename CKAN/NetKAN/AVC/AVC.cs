using Newtonsoft.Json;

namespace CKAN.NetKAN
{

    public class AVC
    {

        // Right now we only support KSP versioning info.

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version_min;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version_max;

    }
}

