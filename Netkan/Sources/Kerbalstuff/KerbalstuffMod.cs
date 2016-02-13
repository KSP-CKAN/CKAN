using System;
using System.Linq;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Kerbalstuff
{
    internal class KerbalstuffMod
    {
        [JsonProperty] public int id; // KSID
        [JsonProperty] public string license;
        [JsonProperty] public string name;
        [JsonProperty] public string short_description;
        [JsonProperty] public string author;
        [JsonProperty] public KSVersion[] versions;
        [JsonProperty] public string website;
        [JsonProperty] public string source_code;
        [JsonProperty] public int default_version_id;
        [JsonConverter(typeof(KSVersion.JsonConvertFromRelativeKsUri))]
        public Uri background;

        public KSVersion Latest()
        {
            // The version we want is specified by `default_version_id`, it's not just
            // the latest. See GH #214. Thanks to @Starstrider42 for spotting this.

            var latest =
                from release in versions
                where release.id == default_version_id
                select release
            ;

            // There should only ever be one.
            return latest.First();
        }
        
        /// <summary>
        /// Returns the path to the mod's home on KerbalStuff
        /// </summary>
        /// <returns>The home.</returns>
        public Uri GetPageUrl()
        {
            return KerbalstuffApi.ExpandPath(string.Format("/mod/{0}/{1}", id, name));
        }

        public override string ToString()
        {
            return string.Format("{0}", name);
        }

        public static KerbalstuffMod FromJson(string json)
        {
            return JsonConvert.DeserializeObject<KerbalstuffMod>(json);
        }
    }
}
