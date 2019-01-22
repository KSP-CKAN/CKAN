using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubRepo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        [JsonProperty("license")]
        public GithubLicense License { get; set; }
    }

    public class GithubLicense
    {
        [JsonProperty("spdx_id")]
        public string Id;
    }
}
