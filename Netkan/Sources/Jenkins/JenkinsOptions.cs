using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Jenkins
{
    public class JenkinsOptions
    {
        [JsonProperty("build", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("stable")]
        public string BuildType = "stable";

        [JsonProperty("use_filename_version")]
        [DefaultValue(false)]
        public bool UseFilenameVersion = false;

        private Regex _assetMatch;
        [JsonProperty("asset_match")]
        public Regex AssetMatchPattern
        {
            get => _assetMatch ?? Constants.DefaultAssetMatchPattern;
            #pragma warning disable IDE0027
            set { _assetMatch = value; }
            #pragma warning restore IDE0027
        }
    }
}
