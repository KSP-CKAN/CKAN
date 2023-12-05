using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    /// <summary>
    /// A converter that ensures an object is always serialized and deserialized as empty,
    /// for backwards compatibility with a client that assumes it will never be null
    /// </summary>
    public class JsonAlwaysEmptyObjectConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Read and discard this field's object (without this, loading stops!)
            _ = JToken.Load(reader);
            return Activator.CreateInstance(objectType);
        }

        public override bool CanWrite => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JObject());
        }

        // Only convert when we're an explicit attribute
        public override bool CanConvert(Type object_type) => false;
    }
}
