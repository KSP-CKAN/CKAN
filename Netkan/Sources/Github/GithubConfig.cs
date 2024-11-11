using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GitHubConfig
    {
        [JsonProperty("use_source_archive")]
        public bool UseSourceArchive { get; set; } = false;

        [JsonProperty("prereleases")]
        public bool? Prereleases { get; set; } = null;
    }
}
