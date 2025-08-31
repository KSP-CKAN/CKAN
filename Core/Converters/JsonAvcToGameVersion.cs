using System;
using System.Diagnostics.CodeAnalysis;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.Versioning;

namespace CKAN.Avc
{
    /// <summary>
    /// Converts AVC style KSP versions into CKAN ones.
    /// </summary>
    public class JsonAvcToGameVersion : JsonConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (JsonAvcToGameVersion));
        private const int AvcWildcard = -1;

        // We trust you only to call this on things we can convert, okay?
        [ExcludeFromCodeCoverage]
        public override bool CanConvert(Type objectType) => true;

        public override object? ReadJson(JsonReader      reader,
                                         Type?           objectType,
                                         object?         existingValue,
                                         JsonSerializer? serializer)
        {
            string? major = null;
            string? minor = null;
            string? patch = null;

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
                            }
                        }
                    }
                    break;
                case JTokenType.Object:
                    major = token.Value<string>("MAJOR");
                    minor = token.Value<string>("MINOR");
                    patch = token.Value<string>("PATCH");
                    break;
                default:
                    throw new InvalidCastException("Trying to convert non-JSON object to Version object");
            }

            //AVC uses -1 to indicate a wildcard.
            string version;
            if (major == null || (int.TryParse(major, out int integer) && integer == AvcWildcard))
            {
                return GameVersion.Any;
            }
            else if (minor == null || (int.TryParse(minor, out integer) && integer == AvcWildcard))
            {
                version = major;
            }
            else if (patch == null || (int.TryParse(patch, out integer) && integer == AvcWildcard))
            {
                version = string.Join(".", major, minor);
            }
            else
            {
                version = string.Join(".", major, minor, patch);
            }

            Log.DebugFormat("  extracted version: {0}", version);
            var result = GameVersion.Parse(version);
            Log.DebugFormat("  generated result: {0}", result);
            return result;
        }

        [ExcludeFromCodeCoverage]
        public override bool CanWrite => false;

        [ExcludeFromCodeCoverage]
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
