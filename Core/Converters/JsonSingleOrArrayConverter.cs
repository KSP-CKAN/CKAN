using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    /// <summary>
    /// With thanks to 
    /// https://stackoverflow.com/questions/18994685/how-to-handle-both-a-single-item-and-an-array-for-the-same-property-using-json-n
    /// </summary>
    public class JsonSingleOrArrayConverter<T> : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }

            // If the object is null, we'll return null. Otherwise end up with a list of null.
            return token.ToObject<T>() == null ? null : new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as List<T>;
            serializer.Serialize(writer, list?.Count == 1 ? list[0] : value);
        }

        /// <summary>
        /// We *only* want to be triggered for types that have explicitly
        /// set an attribute in their class saying they can be converted.
        /// By returning false here, we declare we're not interested in participating
        /// in any other conversions.
        /// </summary>
        /// <returns>
        /// false
        /// </returns>
        public override bool CanConvert(Type object_type) => false;
    }
}
