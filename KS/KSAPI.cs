using System;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN
{
    // KerbalStuff API
    public class KSAPI
    {
        private static readonly Uri kerbalstuff = new Uri("https://kerbalstuff.com/");
        private static readonly Uri kerbalstuff_api = new Uri(kerbalstuff, "/api/");
        private static readonly ILog log = LogManager.GetLogger(typeof (KSAPI));
        private static readonly WebClient web = new WebClient();

        public KSAPI()
        {
            web.Headers.Add("user-agent", Net.UserAgentString);
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

            string result = "";
            try
            {
                result = web.DownloadString(url);
            }
            catch(WebException webEx)
            {
                log.ErrorFormat ("WebException while accessing {0}: {1}", url, webEx);
                throw webEx;
            }

            return result;
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
        /// <returns>The path.</returns>
        /// <param name="route">Route.</param>
        public static Uri ExpandPath(string route)
        {
            return new Uri(kerbalstuff, route);
        }
    }
}