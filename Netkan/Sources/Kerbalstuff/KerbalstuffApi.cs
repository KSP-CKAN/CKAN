using System;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Services;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Kerbalstuff
{
    internal sealed class KerbalstuffApi : IKerbalstuffApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KerbalstuffApi));

        private static readonly Uri KerbalstuffBase = new Uri("https://kerbalstuff.com/");
        private static readonly Uri KerbalstuffApiBase = new Uri(KerbalstuffBase, "/api/");

        private readonly IHttpService _http;

        public KerbalstuffApi(IHttpService http)
        {
            _http = http;
        }

        public KerbalstuffMod GetMod(int modId)
        {
            var json = Call("/mod/" + modId);

            // Check if the mod has been removed from KS.
            var error = JsonConvert.DeserializeObject<KerbalstuffError>(json);

            if (error.error)
            {
                var errorMessage = string.Format("Could not get the mod from KS, reason: {0}.", error.reason);
                throw new Kraken(errorMessage);
            }

            return KerbalstuffMod.FromJson(json);
        }

        // TODO: DBB: Make this private
        /// <summary>
        ///     Returns the route with the KerbalStuff URI (not the API URI) pre-pended.
        /// </summary>
        public static Uri ExpandPath(string route)
        {
            Log.DebugFormat("Expanding {0} to full KS URL", route);

            // Alas, this isn't as simple as it may sound. For some reason
            // some—but not all—KS mods don't work the same way if the path provided
            // is escaped or un-escaped. Since our curl implementation preserves the
            // "original" string used to download a mod, we need to jump through some
            // hoops to make sure this is escaped.

            // Update: The Uri class under mono doesn't un-escape everything when
            // .ToString() is called, even though the .NET documentation says that it
            // should. Rather than using it and going through escaping hell, we'll simply
            // concat our strings together and preserve escaping that way. If KS ever
            // start returning fully qualified URLs then we should see everyting break
            // pretty quickly, and we can rejoice because we won't need any of this code
            // again. -- PJF, KSP-CKAN/CKAN#816.

            // Step 1: Escape any spaces present. KS seems to escape everything else fine.
            route = Regex.Replace(route, " ", "%20");

            // Step 2: Trim leading slashes and prepend the KS host
            var urlFixed = new Uri(KerbalstuffBase + route.TrimStart('/'));

            // Step 3: Profit!
            Log.DebugFormat("Expanded URL is {0}", urlFixed.OriginalString);
            return urlFixed;
        }

        private string Call(string path)
        {
            // TODO: There's got to be a better way than using regexps.
            // new Uri (kerbalstuff_api, path) doesn't work, it only uses the *base* of the first arg,
            // and hence drops the /api path.

            // Remove leading slashes.
            path = Regex.Replace(path, "^/+", "");

            var url = KerbalstuffApiBase + path;

            Log.DebugFormat("Calling {0}", url);

            return _http.DownloadText(new Uri(url));
        }
    }
}