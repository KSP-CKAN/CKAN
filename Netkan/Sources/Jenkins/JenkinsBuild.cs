using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Jenkins
{
    public class JenkinsBuild
    {
        [JsonProperty("url")]
        public string Url;

        [JsonProperty("result")]
        public string Result;

        [JsonProperty("artifacts")]
        public JenkinsArtifact[] Artifacts;
    }
}
