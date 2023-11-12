using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    /// <summary>
    /// Converter to allow old resources fields to be read in newer clients.
    /// </summary>
    public class JsonOldResourceUrlConverter : JsonConverter
    {
        public override bool CanConvert(Type object_type)
        {
            // We *only* want to be triggered for types that have explicitly
            // set an attribute in their class saying they can be converted.
            // By returning false here, we declare we're not interested in participating
            // in any other conversions.
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            // If we've got an object, extract the URL, since that's all we use now.
            if (token.Type == JTokenType.Object)
            {
                return token["url"].ToObject<Uri>();
            }

            // Otherwise just return whatever we found, which we hope converts okay. :)
            return token.ToObject<Uri>();
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }
}
