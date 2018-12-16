using System.ComponentModel;
using System.Runtime.Serialization;
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
            get { return _assetMatch ?? Constants.DefaultAssetMatchPattern; }
            set { _assetMatch = value; }
        }
    }
}
