using Newtonsoft.Json;

using CKAN.Versioning;

namespace CKAN.Avc
{
    public class AvcVersion
    {
        [JsonProperty("NAME")]
        public string? Name;

        [JsonProperty("DOWNLOAD")]
        public string? Download;

        [JsonProperty("GITHUB")]
        public AvcVersionGithubRef? Github;

        [JsonProperty("URL")]
        public string? Url;

        [JsonProperty("VERSION")]
        [JsonConverter(typeof(JsonAvcToVersion))]
        public ModuleVersion? version;

        [JsonProperty("KSP_VERSION")]
        [JsonConverter(typeof(JsonAvcToGameVersion))]
        public GameVersion? ksp_version;

        [JsonProperty("KSP_VERSION_MIN")]
        [JsonConverter(typeof(JsonAvcToGameVersion))]
        public GameVersion? ksp_version_min;

        [JsonProperty("KSP_VERSION_MAX")]
        [JsonConverter(typeof(JsonAvcToGameVersion))]
        public GameVersion? ksp_version_max;

    }

    public class AvcVersionGithubRef
    {
        [JsonProperty("USERNAME")]
        public string? Username;

        [JsonProperty("REPOSITORY")]
        public string? Repository;
    }
}
