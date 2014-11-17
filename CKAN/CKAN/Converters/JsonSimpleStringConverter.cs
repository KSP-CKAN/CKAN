namespace CKAN {

    using Newtonsoft.Json;
    using System;
    using log4net;

    // A lovely class for serialising things that can be converted
    // to simple strings and back.

    public class JsonSimpleStringConverter : JsonConverter {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue (value.ToString ());
        }
    
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            // If we find a null, then that might be okay, so we pass it down to our
            // activator. Otherwise we convert to string, since that's our job.
            string value = reader.Value == null ? null : reader.Value.ToString();
            return Activator.CreateInstance (objectType, value);
        }

        public override bool CanConvert(Type objectType) {
            // We *only* want to be triggered for types that have explicitly
            // set an attribute in their class saying they can be converted.
            // By returning false here, we declare we're not interested in participating
            // in any other conversions.
            return false;
        }
    }
}

