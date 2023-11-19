using System.Collections.Generic;

using Newtonsoft.Json;

namespace CKAN
{
    [JsonConverter(typeof(CompatibleGameVersionsConverter))]
    public class CompatibleGameVersions
    {
        public string GameVersionWhenWritten { get; set; }

        public List<string> Versions { get; set; } = new List<string>();
    }

    public class CompatibleGameVersionsConverter : JsonPropertyNamesChangedConverter
    {
        protected override Dictionary<string, string> mapping
            => new Dictionary<string, string>
            {
                { "VersionOfKspWhenWritten", "GameVersionWhenWritten" },
                { "CompatibleKspVersions",   "Versions" }
            };
    }
}
