using System.ComponentModel;

using Newtonsoft.Json;

namespace CKAN.NetKAN.Model
{
    /// <summary>
    /// Represents the x_netkan_gitlab object from a netkan
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class GitlabOptions
    {
        /// <summary>
        /// True to use source ZIP for a release.
        /// Note that this MUST be true because GitLab only provides source ZIPs!
        /// If they add other assets in the future, this requirement can be relaxed.
        /// </summary>
        [JsonProperty("use_source_archive", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                            NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool UseSourceArchive = false;
    }
}
