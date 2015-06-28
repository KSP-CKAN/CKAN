using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Reads a URL string, and returns a Uri object. Returns null if the string
    /// doesn't parse.
    /// </summary>
    public class JsonIgnoreBadUrlConverter : JsonConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonIgnoreBadUrlConverter));

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = reader.Value == null ? null : reader.Value.ToString();

            if (value == null)
                return null;

            Uri url;

            try
            {
                url = new Uri(value);
                return url;
            }
            catch
            {
                log.WarnFormat("{0} is not a valid URL, ignoring", value);
                return null;
            }
        }

        /// <summary>
        /// Opt out of converting any types, except those we've been specifically applied to.
        /// </summary>
        public override bool CanConvert(Type object_type)
        {
            return false;
        }

        /// <summary>
        /// Opt out of writing anything, otherwise things go horribly wrong when we try
        /// to write to the registry.
        /// </summary>
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

