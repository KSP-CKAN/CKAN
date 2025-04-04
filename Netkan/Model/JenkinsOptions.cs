using System.ComponentModel;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace CKAN.NetKAN.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class JenkinsOptions
    {
        [JsonProperty("build", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                               NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(JenkinsBuildType.stable)]
        public readonly JenkinsBuildType BuildType = JenkinsBuildType.stable;

        [JsonProperty("use_filename_version", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                              NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(false)]
        public readonly bool UseFilenameVersion = false;

        [JsonProperty("asset_match", NullValueHandling = NullValueHandling.Ignore)]
        public readonly Regex AssetMatchPattern = Constants.DefaultAssetMatchPattern;
    }

    public enum JenkinsBuildType
    {
        any,
        completed,
        failed,
        stable,
        successful,
        unstable,
        unsuccessful,
    }
}
