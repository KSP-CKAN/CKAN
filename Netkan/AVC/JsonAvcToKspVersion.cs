using System;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    /// <summary>
    /// Converts AVC style KSP versions into CKAN ones.
    /// </summary>
    public class JsonAvcToKspVersion : JsonConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (JsonAvcToKspVersion));
        private const int AVC_WILDCARD = -1;

        public override bool CanConvert(Type object_type)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            string major = "0";
            string minor = "0";
            string patch = "0";

            JToken token = JToken.Load(reader);
            log.DebugFormat("Read Token: {0}, {1}", new Object[] {token.Type, token.ToString()});
            switch (token.Type)
            {
                case JTokenType.String:
                    var token_array = token.ToString().Split('.');

                    if (token_array.Length >= 0)
                    {
                        major = token_array[0];
                    }

                    if (token_array.Length >= 1)
                    {
                        minor = token_array[1];
                    }

                    if (token_array.Length >= 2)
                    {
                        patch = token_array[2];
                    }
                    break;
                case JTokenType.Object:
                    major = (string) token["MAJOR"];
                    minor = (string) token["MINOR"];
                    patch = (string) token["PATCH"];
                    break;
                default:
                    throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            //AVC uses -1 to indicate a wildcard.
            int integer;
            string version;
            if (int.TryParse(major, out integer) && integer == AVC_WILDCARD)
            {
                version = null;
            }
            else if (int.TryParse(minor, out integer) && integer == AVC_WILDCARD)
            {
                version = major;
            }
            else if (int.TryParse(patch, out integer) && integer == AVC_WILDCARD)
            {
                version = string.Join(".", major, minor);
            }
            else
            {
                version = string.Join(".", major, minor, patch);
            }

            log.DebugFormat("  extracted version: {0}", version);
            KSPVersion result = new KSPVersion(version);
            log.DebugFormat("  generated result: {0}", result.ToString());
            return result;
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

    /// <summary>
    /// Converts AVC style versions into CKAN ones.
    /// </summary>
    public class JsonAvcToVersion : JsonConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (JsonAvcToKspVersion));

        public override bool CanConvert(Type object_type)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
        }

        public override object ReadJson(JsonReader reader, Type object_type, object existing_value,
            JsonSerializer serializer)
        {
            string major = "0";
            string minor = "0";
            string patch = "0";

            JToken token = JToken.Load(reader);
            log.DebugFormat("Read Token: {0}, {1}", new Object[] {token.Type, token.ToString()});
            switch (token.Type)
            {
                case JTokenType.String:
                    var token_array = token.ToString().Split('.');

                    if (token_array.Length >= 0)
                    {
                        major = token_array[0];
                    }

                    if (token_array.Length >= 1)
                    {
                        minor = token_array[1];
                    }

                    if (token_array.Length >= 2)
                    {
                        patch = token_array[2];
                    }
                    break;
                case JTokenType.Object:
                    major = (string) token["MAJOR"];
                    minor = (string) token["MINOR"];
                    patch = (string) token["PATCH"];
                    break;
                default:
                    throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            string version = string.Join(".", major, minor, patch);
            log.DebugFormat("  extracted version: {0}", version);
            Version result = new Version(version);
            log.DebugFormat("  generated result: {0}", result.ToString());
            return result;
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