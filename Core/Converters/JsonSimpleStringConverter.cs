using System;

using Newtonsoft.Json;

namespace CKAN
{
    /// <summary>
    /// Serialises things that can be converted
    /// to simple strings and back.
    /// </summary>
    public class JsonSimpleStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // If we find a null, then that might be okay, so we pass it down to our
            // activator. Otherwise we convert to string, since that's our job.
            return Activator.CreateInstance(objectType, reader.Value?.ToString());
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
        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }
}
