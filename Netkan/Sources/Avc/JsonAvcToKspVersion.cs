using CKAN.Versioning;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CKAN.NetKAN.Sources.Avc
{
    /// <summary>
    /// Converts AVC style KSP versions into CKAN ones.
    /// </summary>
    public class JsonAvcToKspVersion : JsonConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonAvcToKspVersion));
        private const int AvcWildcard = -1;

        public override bool CanConvert(Type objectType)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var major = "0";
            var minor = "0";
            var patch = "0";

            var token = JToken.Load(reader);
            Log.DebugFormat("Read Token: {0}, {1}", new object[] { token.Type, token.ToString() });
            switch (token.Type)
            {
                case JTokenType.String:
                    var tokenArray = token.ToString().Split('.');

                    if (tokenArray.Length > 0)
                    {
                        major = tokenArray[0];
                    }

                    if (tokenArray.Length > 1)
                    {
                        minor = tokenArray[1];
                    }

                    if (tokenArray.Length > 2)
                    {
                        patch = tokenArray[2];
                    }
                    break;

                case JTokenType.Object:
                    major = (string)token["MAJOR"];
                    minor = (string)token["MINOR"];
                    patch = (string)token["PATCH"];
                    break;

                default:
                    throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            //AVC uses -1 to indicate a wildcard.
            int integer;
            string version;
            if (int.TryParse(major, out integer) && integer == AvcWildcard)
            {
                return KspVersion.Any;
            }
            else if (int.TryParse(minor, out integer) && integer == AvcWildcard)
            {
                version = major;
            }
            else if (int.TryParse(patch, out integer) && integer == AvcWildcard)
            {
                version = string.Join(".", major, minor);
            }
            else
            {
                version = string.Join(".", major, minor, patch);
            }

            Log.DebugFormat("  extracted version: {0}", version);
            var result = KspVersion.Parse(version);
            Log.DebugFormat("  generated result: {0}", result);
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonAvcToKspVersion));

        public override bool CanConvert(Type objectType)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var major = "0";
            var minor = "0";
            var patch = "0";
            string build = null;

            var token = JToken.Load(reader);
            Log.DebugFormat("Read Token: {0}, {1}", new Object[] { token.Type, token.ToString() });
            switch (token.Type)
            {
                case JTokenType.String:
                    var tokenArray = token.ToString().Split('.');

                    if (tokenArray.Length > 0)
                    {
                        major = tokenArray[0];
                    }

                    if (tokenArray.Length > 1)
                    {
                        minor = tokenArray[1];
                    }

                    if (tokenArray.Length > 2)
                    {
                        patch = tokenArray[2];
                    }

                    if (tokenArray.Length > 3)
                    {
                        build = tokenArray[3];
                    }

                    break;

                case JTokenType.Object:
                    major = (string)token["MAJOR"];
                    minor = (string)token["MINOR"];
                    patch = (string)token["PATCH"];
                    build = (string)token["BUILD"];
                    break;

                default:
                    throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            var components = new List<string>() { major, minor, patch };

            if (!string.IsNullOrWhiteSpace(build))
            {
                components.Add(build);
            }

            var version = string.Join(".", components);
            Log.DebugFormat("  extracted version: {0}", version);
            var result = new Version(version);
            Log.DebugFormat("  generated result: {0}", result);
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