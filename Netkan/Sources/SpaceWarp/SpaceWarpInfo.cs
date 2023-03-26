using System;

using Newtonsoft.Json;

// https://github.com/SpaceWarpDev/SpaceWarp/blob/main/example_mod_info.json

namespace CKAN.NetKAN.Sources.SpaceWarp
{
    public class SpaceWarpInfo
    {
        public string mod_id;
        public string name;
        public string author;
        public string description;
        public string source;
        public string version;
        public Uri version_check;
        public Dependency[] dependencies;
        public VersionMinMax ksp2_version;
    }

    public class Dependency
    {
        public string id;
        public VersionMinMax version;
    }

    public class VersionMinMax
    {
        [JsonConverter(typeof(SpaceWarpGameVersionConverter))]
        public string min;
        [JsonConverter(typeof(SpaceWarpGameVersionConverter))]
        public string max;
    }

    public class SpaceWarpGameVersionConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => (reader.Value as string)?.Replace("*", "any");

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
