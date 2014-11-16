using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace CKAN
{
    /// <summary>
    /// Converter to allow old resources fields to be read in newer clients.
    /// </summary>
    public class JsonOldResourceUrlConverter : JsonConverter
    {
        public override bool CanConvert(Type object_type)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
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

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }
}

