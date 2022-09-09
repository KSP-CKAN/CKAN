using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Gitlab
{
    /// <summary>
    /// Represents the x_netkan_gitlab object from a netkan
    /// </summary>
    internal sealed class GitlabOptions
    {
        /// <summary>
        /// True to use source ZIP for a release.
        /// Note that this MUST be true because GitLab only provides source ZIPs!
        /// If they add other assets in the future, this requirement can be relaxed.
        /// </summary>
        [JsonProperty("use_source_archive")]
        public readonly bool UseSourceArchive = false;
    }
}
