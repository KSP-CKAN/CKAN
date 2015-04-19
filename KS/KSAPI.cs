using System;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;
using CurlSharp;

namespace CKAN.NetKAN
{
    // KerbalStuff API
    public class KSAPI
    {
        private static readonly Uri kerbalstuff = new Uri("https://kerbalstuff.com/");
        private static readonly Uri kerbalstuff_api = new Uri(kerbalstuff, "/api/");
        private static readonly ILog log = LogManager.GetLogger(typeof (KSAPI));

        public KSAPI()
        {
        }

        public static string Call(string path)
        {
            // TODO: There's got to be a better way than using regexps.
            // new Uri (kerbalstuff_api, path) doesn't work, it only uses the *base* of the first arg,
            // and hence drops the /api path.

            // Remove leading slashes. 
            path = Regex.Replace(path, "^/+", "");

            string url = kerbalstuff_api + path;

            log.DebugFormat("Calling {0}", url);

			using(var web = new Web())
			{
				return web.DownloadString(url);
			}
        }

        /// <summary>
        /// Given a mod id, returns a KSMod with its metadata from the network.
        /// </summary>
        public static KSMod Mod(int mod_id)
        {
            string json = Call("/mod/" + mod_id);
            return Mod(json);
        }

        /// <summary>
        /// Given a JSON string, inflates and returns a KSMod.
        /// </summary>
        public static KSMod Mod(string json)
        {
            return JsonConvert.DeserializeObject<KSMod>(json);
        }

        /// <summary>
        ///     Returns the route with the KerbalStuff URI (not the API URI) pre-pended.
        /// </summary>
        public static Uri ExpandPath(string route)
        {
            log.DebugFormat("Expanding {0} to full KS URL", route);

            // Alas, this isn't as simple as it may sound. For some reason
            // some—but not all—KS mods don't work the same way if the path provided
            // is escaped or un-escaped. Since our curl implementation preserves the
            // "original" string used to download a mod, we need to jump through some
            // hoops to make sure this is escaped.

            var url = new Uri (kerbalstuff,route);
            var url_fixed = new Uri (Uri.EscapeUriString(url.ToString()));

            log.DebugFormat ("Expanded URL is {0}", url_fixed.OriginalString);
            return url_fixed;
        }
    }
}
