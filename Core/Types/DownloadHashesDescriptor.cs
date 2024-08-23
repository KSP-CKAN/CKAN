using Newtonsoft.Json;

namespace CKAN
{
    public class DownloadHashesDescriptor
    {
        [JsonProperty("sha1")]
        public string? sha1;

        [JsonProperty("sha256")]
        public string? sha256;
    }
}
