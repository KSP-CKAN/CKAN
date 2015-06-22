using System;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN
{
    /// <summary>
    /// Internal class to read errors from KS.
    /// </summary>
    internal class KSError
    {
        #pragma warning disable 0649
        public string reason;
        public bool error;
        #pragma warning restore 0649
    }

    // KerbalStuff API
    public class KSAPI
    {
        private static readonly Uri kerbalstuff = new Uri("https://kerbalstuff.com/");
        private static readonly Uri kerbalstuff_api = new Uri(kerbalstuff, "/api/");
        private static readonly ILog log = LogManager.GetLogger(typeof (KSAPI));

        public static string Call(string path)
        {
            // TODO: There's got to be a better way than using regexps.
            // new Uri (kerbalstuff_api, path) doesn't work, it only uses the *base* of the first arg,
            // and hence drops the /api path.

            // Remove leading slashes.
            path = Regex.Replace(path, "^/+", "");

            string url = kerbalstuff_api + path;

            log.DebugFormat("Calling {0}", url);
            try
            {
                using (var web = new Web())
                {
                    return web.DownloadString(url);
                }
            }
            catch (DllNotFoundException)
            {
                //Curl is not installed. Curl is a workaround for a mono issue.
                //TODO Richard - Once repos are merged go and check all Platform calls to see if they are mono checks
                if (!Platform.IsWindows) throw;
                //On mircrosft.net so try native code.
                using (var web = new WebClient())
                {
                    try
                    {
                        return web.DownloadString(url);
                    }
                    catch (WebException web_ex)
                    {
                        log.ErrorFormat("WebException while accessing {0}: {1}", url, web_ex);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Given a mod id, returns a KSMod with its metadata from the network.
        /// </summary>
        public static KSMod Mod(int mod_id)
        {
            string json = Call("/mod/" + mod_id);

            // Check if the mod has been removed from KS.
            KSError error = JsonConvert.DeserializeObject<KSError>(json);

            if (error.error)
            {
                string error_message = String.Format("Could not get the mod from KS, reason: {0}.", error.reason);
                throw new Kraken(error_message);
            }

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
            Uri url_fixed = new Uri(kerbalstuff + route.TrimStart('/'));

            // Step 3: Profit!
            log.DebugFormat("Expanded URL is {0}", url_fixed.OriginalString);
            return url_fixed;
        }
    }
}