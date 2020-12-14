using System;
using System.Net;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Services;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Spacedock
{
    internal sealed class SpacedockApi : ISpacedockApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpacedockApi));

        private static readonly Uri SpacedockBase = new Uri("https://spacedock.info/");
        private static readonly Uri SpacedockApiBase = new Uri(SpacedockBase, "/api/");

        private readonly IHttpService _http;

        public SpacedockApi(IHttpService http)
        {
            _http = http;
        }

        public SpacedockMod GetMod(int modId)
        {
            string json;
            try
            {
                json = Call("/mod/" + modId);
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    // Can't parse if no response
                    throw;
                }

                // SpaceDock returns a valid json with an error message in case of non 200 codes.
                json = new System.IO.StreamReader(e.Response.GetResponseStream(), System.Text.Encoding.UTF8).ReadToEnd();
                if (string.IsNullOrEmpty(json))
                {
                    // ... sometimes. Other times we get nothing.
                    throw;
                }
            }

            // Check if the mod has been removed from SD.
            var error = JsonConvert.DeserializeObject<SpacedockError>(json);

            if (error.error)
            {
                var errorMessage = $"Could not get the mod from SpaceDock, reason: {error.reason}";
                throw new Kraken(errorMessage);
            }

            return SpacedockMod.FromJson(json);
        }

        // TODO: DBB: Make this private
        /// <summary>
        ///     Returns the route with the SpaceDock URI (not the API URI) pre-pended.
        /// </summary>
        public static Uri ExpandPath(string route)
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
            var urlFixed = new Uri(SpacedockBase + route.TrimStart('/'));

            // Step 3: Profit!
            Log.DebugFormat("Expanded URL is {0}", urlFixed.OriginalString);
            return urlFixed;
        }

        private string Call(string path)
        {
            // TODO: There's got to be a better way than using regexps.
            // new Uri (spacedock_api, path) doesn't work, it only uses the *base* of the first arg,
            // and hence drops the /api path.

            // Remove leading slashes.
            path = Regex.Replace(path, "^/+", "");

            var url = SpacedockApiBase + path;

            Log.DebugFormat("Calling {0}", url);

            return _http.DownloadText(new Uri(url));
        }
    }
}
