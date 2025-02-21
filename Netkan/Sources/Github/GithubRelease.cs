using System;
using System.ComponentModel;

using Newtonsoft.Json;

using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubRelease
    {
        [JsonProperty("author")]
        public GithubUser? Author { get; set; }

        [JsonProperty("tag_name")]
        public ModuleVersion? Tag { get; set; }

        [JsonProperty("assets")]
        public GithubReleaseAsset[]? Assets { get; set; }

        [JsonProperty("zipball_url")]
        public Uri? SourceArchive { get; set; }

        [JsonProperty("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonProperty("prerelease")]
        [DefaultValue(false)]
        public bool PreRelease { get; set; } = false;
    }
}
