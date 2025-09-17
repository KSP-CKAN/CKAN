using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class OverrideOptions
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public readonly List<string>? VersionConstraints;

        [JsonProperty("before", NullValueHandling = NullValueHandling.Ignore)]
        public readonly string? BeforeStep;

        [JsonProperty("after", NullValueHandling = NullValueHandling.Ignore)]
        public readonly string? AfterStep;

        [JsonProperty("override", NullValueHandling = NullValueHandling.Ignore)]
        public readonly Dictionary<string, JToken>? Override;

        [JsonProperty("delete", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public readonly List<string>? Delete;
    }
}
