using System;
using System.ComponentModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class VersionEditOptions
    {
        [JsonProperty("find", NullValueHandling = NullValueHandling.Ignore)]
        public readonly string? Find;

        [JsonProperty("replace", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                 NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue("${version}")]
        public readonly string Replace = "${version}";

        [JsonProperty("strict", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                                NullValueHandling    = NullValueHandling.Ignore)]
        [DefaultValue(true)]
        public readonly bool Strict = true;

        public VersionEditOptions(string find)
        {
            Find = find;
        }
    }

    internal class JsonVersionEditConverter : JsonConverter
    {
        public override object? ReadJson(JsonReader     reader,
                                         Type           objectType,
                                         object?        existingValue,
                                         JsonSerializer serializer)
            => JToken.Load(reader) switch
               {
                   JValue  val => new VersionEditOptions(val.ToString()),
                   JObject obj => obj.ToObject<VersionEditOptions>(),
                   JToken  tok => throw new Kraken(string.Format(
                                           "Unrecognized `x_netkan_version_edit` value: {0}",
                                           tok)),
                   _           => throw new Kraken("Unrecognized `x_netkan_version_edit` value"),
               };

        public override bool CanConvert(Type objectType) => false;
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }
}
