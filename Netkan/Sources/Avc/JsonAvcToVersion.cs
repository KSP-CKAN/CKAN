using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Avc
{
    /// <summary>
    /// Converts AVC style versions into CKAN ones.
    /// </summary>
    public class JsonAvcToVersion : JsonConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (JsonAvcToGameVersion));

        public override bool CanConvert(Type objectType)
        {
            // We trust you only to call this on things we can convert, okay?
            return true;
        }

        public override object? ReadJson(JsonReader      reader,
                                         Type?           objectType,
                                         object?         existingValue,
                                         JsonSerializer? serializer)
        {
            var major = "0";
            var minor = "0";
            var patch = "0";
            string? build = null;

            var token = JToken.Load(reader);
            Log.DebugFormat("Read Token: {0}, {1}", new object[] {token.Type, token.ToString()});
            switch (token.Type)
            {
                case JTokenType.String:
                    var tokenArray = token.ToString().Split('.');
                    if (//tokenArray is [var majorToken, ..]
                        tokenArray.Length > 0
                        && tokenArray[0] is var majorToken)
                    {
                        major = majorToken;
                        if (//tokenArray is [_, var minorToken, ..]
                            tokenArray.Length > 1
                            && tokenArray[1] is var minorToken)
                        {
                            minor = minorToken;
                            if (//tokenArray is [_, _, var patchToken, ..]
                                tokenArray.Length > 2
                                && tokenArray[2] is var patchToken)
                            {
                                patch = patchToken;
                                if (//tokenArray is [_, _, _, var buildToken, ..]
                                    tokenArray.Length > 3
                                    && tokenArray[3] is var buildToken)
                                {
                                    build = buildToken;
                                }
                            }
                        }
                    }
                    break;
                case JTokenType.Object:
                    major = token.Value<string>("MAJOR") ?? major;
                    minor = token.Value<string>("MINOR") ?? minor;
                    patch = token.Value<string>("PATCH") ?? patch;
                    build = token.Value<string>("BUILD") ?? build;
                    break;
                default:
                    throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            var components = new List<string>() { major, minor, patch };

            if (build != null && !string.IsNullOrWhiteSpace(build))
            {
                components.Add(build);
            }

            var version = string.Join(".", components);
            Log.DebugFormat("  extracted version: {0}", version);
            var result = new ModuleVersion(version);
            Log.DebugFormat("  generated result: {0}", result);
            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
