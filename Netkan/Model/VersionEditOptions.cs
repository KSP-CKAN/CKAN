using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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
                   var     tok => throw new Kraken(
                                      $"Unrecognized `x_netkan_version_edit` value: {tok}"),
               };

        [ExcludeFromCodeCoverage]
        public override bool CanConvert(Type objectType) => false;
        [ExcludeFromCodeCoverage]
        public override bool CanWrite => false;
        [ExcludeFromCodeCoverage]
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }
}
