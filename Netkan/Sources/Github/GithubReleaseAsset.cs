using System;

using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubReleaseAsset
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("browser_download_url")]
        public Uri? Download { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? Updated { get; set; }

        [JsonProperty("uploader")]
        public GithubUser? Uploader { get; set; }
    }
}
