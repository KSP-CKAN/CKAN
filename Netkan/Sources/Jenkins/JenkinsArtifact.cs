using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Jenkins
{
    public class JenkinsArtifact
    {
        [JsonProperty("fileName")]
        public string FileName;

        [JsonProperty("relativePath")]
        public string RelativePath;
    }
}
