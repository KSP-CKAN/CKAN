using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using log4net;
using Newtonsoft.Json;

using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Sources.Spacedock
{
    internal sealed class SpacedockApi : ISpacedockApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpacedockApi));

        public static readonly Uri SpacedockBase = new Uri("https://spacedock.info/");
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
                json = new StreamReader(e.Response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
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
