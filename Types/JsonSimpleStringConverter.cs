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
            string value = (string) reader.Value;
            return Activator.CreateInstance (objectType, value);
        }

        public override bool CanConvert(Type objectType) {
            // TODO XXX: Magic type names! Can we just return true here, since
            // classes need to delcare their type-converters up-front anyway?
            return (objectType == typeof(Version) || objectType == typeof(KSPVersion));
        }
    }
}

