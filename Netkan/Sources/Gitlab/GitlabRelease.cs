using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Gitlab
{
    /// <summary>
    /// Represents a release from the GitLab API
    /// </summary>
    internal sealed class GitlabRelease
    {
        [JsonProperty("tag_name")]
        public readonly string TagName;

        [JsonProperty("author")]
        public readonly GitlabReleaseAuthor Author = new GitlabReleaseAuthor();

        [JsonProperty("released_at")]
        public readonly DateTime ReleasedAt;

        [JsonProperty("assets")]
        public readonly GitlabReleaseAssets Assets = new GitlabReleaseAssets();
    }

    /// <summary>
    /// Represents an author from the GitLab API
    /// </summary>
    internal sealed class GitlabReleaseAuthor
    {
        [JsonProperty("name")]
        public readonly string Name;
    }

    /// <summary>
    /// Represents an assets object from the GitLab API
    /// </summary>
    internal sealed class GitlabReleaseAssets
    {
        [JsonProperty("sources")]
        public readonly List<GitlabReleaseAssetSource> Sources = new List<GitlabReleaseAssetSource>();
    }

    /// <summary>
    /// Represents an assets source object from the GitLab API
    /// </summary>
    internal sealed class GitlabReleaseAssetSource
    {
        [JsonProperty("format")]
        public readonly string Format;

        [JsonProperty("url")]
        public readonly string URL;
    }
}
