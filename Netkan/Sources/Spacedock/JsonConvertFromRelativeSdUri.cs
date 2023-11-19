using System;
using System.Text.RegularExpressions;

using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Spacedock
{
    /// <summary>
    /// A simple helper class to prepend SpaceDock URLs.
    /// </summary>
    internal class JsonConvertFromRelativeSdUri : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                return ExpandPath(reader.Value.ToString());
            }

            return null;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns the route with the SpaceDock URI (not the API URI) pre-pended.
        /// </summary>
        private static Uri ExpandPath(string route)
        {
            Log.DebugFormat("Expanding {0} to full SpaceDock URL", route);

            // Alas, this isn't as simple as it may sound. For some reason
            // some—but not all—SD mods don't work the same way if the path provided
            // is escaped or un-escaped.

            // Update: The Uri class under mono doesn't un-escape everything when
            // .ToString() is called, even though the .NET documentation says that it
            // should. Rather than using it and going through escaping hell, we'll simply
            // concat our strings together and preserve escaping that way. If SD ever
            // start returning fully qualified URLs then we should see everyting break
            // pretty quickly, and we can rejoice because we won't need any of this code
            // again. -- PJF, KSP-CKAN/CKAN#816.

            // Step 1: Escape any spaces present. SD seems to escape everything else fine.
            route = Regex.Replace(route, " ", "%20");

            // Step 2: Trim leading slashes and prepend the SD host
            var urlFixed = new Uri(SpacedockApi.SpacedockBase + route.TrimStart('/'));

            // Step 3: Profit!
            Log.DebugFormat("Expanded URL is {0}", urlFixed.OriginalString);
            return urlFixed;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonConvertFromRelativeSdUri));
    }
}
