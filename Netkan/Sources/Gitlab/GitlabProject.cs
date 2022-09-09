using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Gitlab
{
    /// <summary>
    /// Represents a project from the GitLab API
    /// </summary>
    public sealed class GitlabProject
    {
        [JsonProperty("name")]
        public readonly string Name;

        [JsonProperty("description")]
        public readonly string Description;

        [JsonProperty("web_url")]
        public readonly string WebURL;

        [JsonProperty("issues_enabled")]
        public readonly bool IssuesEnabled;

        [JsonProperty("readme_url")]
        public readonly string ReadMeURL;
    }
}
