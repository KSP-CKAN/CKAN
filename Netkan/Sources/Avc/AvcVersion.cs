using CKAN.Versioning;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Avc
{
    public class AvcVersion
    {
        [JsonProperty("NAME")]
        public string Name;

        [JsonProperty("DOWNLOAD")]
        public string Download;

        [JsonProperty("GITHUB")]
        public AvcVersionGithubRef Github;

        [JsonProperty("URL")]
        public string Url;

        [JsonConverter(typeof(JsonAvcToVersion))]
        public ModuleVersion version;

        [JsonConverter(typeof(JsonAvcToGameVersion))]
        public GameVersion ksp_version;

        [JsonConverter(typeof(JsonAvcToGameVersion))]
        public GameVersion ksp_version_min;

        [JsonConverter(typeof(JsonAvcToGameVersion))]
        public GameVersion ksp_version_max;

    }

    public class AvcVersionGithubRef
    {
        [JsonProperty("USERNAME")]
        public string Username;

        [JsonProperty("REPOSITORY")]
        public string Repository;
    }
}
