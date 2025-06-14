using System;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Spacedock
{
    public class SpacedockVersion
    {
        // These all get filled by JSON deserialisation.

        [JsonConverter(typeof(JsonConvertGameVersion))]
        [JsonProperty("game_version")]
        public GameVersion? KSP_version;

        public string? changelog;
        public DateTime? created;

        [JsonConverter(typeof(JsonConvertFromRelativeSdUri))]
        public Uri? download_path;

        public ModuleVersion? friendly_version;
        public int id;

        /// <summary>
        /// SpaceDock always trims trailing zeros from a three-part version
        /// (eg: 1.0.0 -> 1.0). This means we could potentially think some mods
        /// will work with more versions than they actually will. This converter
        /// puts the .0 back on when appropriate. GH #1156.
        /// </summary>
        internal class JsonConvertGameVersion : JsonConverter
        {
            public override object? ReadJson(JsonReader     reader,
                                             Type           objectType,
                                             object?        existingValue,
                                             JsonSerializer serializer)
                => reader.Value?.ToString() is string s
                       ? GameVersion.Parse(ExpandVersionIfNeeded(s))
                       : null;

            /// <summary>
            /// Actually expand the KSP version. It's way easier to test this than the override. :)
            /// </summary>
            public static string ExpandVersionIfNeeded(string version)
                => Regex.IsMatch(version, @"^\d+\.\d+$")
                       // Two part string, add our .0
                       ? version + ".0"
                       : version;

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
