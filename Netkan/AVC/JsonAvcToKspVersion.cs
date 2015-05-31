using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace CKAN.NetKAN
{
    /// <summary>
    /// Converts AVC style KSP versions into CKAN ones.
    /// </summary>
    public class JsonAvcToKspVersion : JsonConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (JsonAvcToKspVersion));

        public override bool CanConvert(Type object_type)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string major = "0";
            string minor = "0";
            string patch = "0";

            JToken token = JToken.Load(reader);
            log.DebugFormat ("Read Token: {0}, {1}", new Object[] { token.Type, token.ToString() });
            if (token.Type == JTokenType.String)
            {
                string[] tokenArray = token.ToString ().Split ('.');

                if (tokenArray.Length >= 0)
                {
                    major = tokenArray [0];
                }

                if (tokenArray.Length >= 1)
                {
                    minor = tokenArray [1];
                }

                if (tokenArray.Length >= 2)
                {
                    patch = tokenArray [2];
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                major = (string) token ["MAJOR"];
                minor = (string) token ["MINOR"];
                patch = (string) token ["PATCH"];
            }
            else
            {
                throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            string version = string.Join(".", major, minor, patch);
            log.DebugFormat ("  extracted version: {0}", version);
            KSPVersion result = new KSPVersion(version);
            log.DebugFormat ("  generated result: {0}", result.ToString());
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string major = "0";
            string minor = "0";
            string patch = "0";

            JToken token = JToken.Load(reader);
            log.DebugFormat ("Read Token: {0}, {1}", new Object[] { token.Type, token.ToString() });
            if (token.Type == JTokenType.String)
            {
                string[] tokenArray = token.ToString ().Split ('.');

                if (tokenArray.Length >= 0)
                {
                    major = tokenArray [0];
                }

                if (tokenArray.Length >= 1)
                {
                    minor = tokenArray [1];
                }

                if (tokenArray.Length >= 2)
                {
                    patch = tokenArray [2];
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                major = (string) token ["MAJOR"];
                minor = (string) token ["MINOR"];
                patch = (string) token ["PATCH"];
            }
            else
            {
                throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            string version = string.Join(".", major, minor, patch);
            log.DebugFormat ("  extracted version: {0}", version);
            Version result = new Version(version);
            log.DebugFormat ("  generated result: {0}", result.ToString());
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

