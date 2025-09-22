using System.Collections.Generic;

using Newtonsoft.Json;

using CKAN.Versioning;

namespace CKAN.Configuration
{
    [JsonConverter(typeof(CompatibleGameVersionsConverter))]
    public class CompatibleGameVersions
    {
        public GameVersion? GameVersionWhenWritten { get; set; }

        public List<GameVersion> Versions { get; set; } = new List<GameVersion>();
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
