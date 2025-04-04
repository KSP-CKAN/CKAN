using System.ComponentModel;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class GithubOptions
    {
        [JsonProperty("use_source_archive", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                            NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool UseSourceArchive = false;

        [JsonProperty("prereleases", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                     NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(null)]
        public readonly bool? Prereleases;
    }
}
