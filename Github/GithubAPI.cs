using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json.Linq;

// We could use OctoKit for this, but since we're only pinging the
// release API, I'm happy enough without yet another dependency.

namespace CKAN.NetKAN
{
    public static class GithubAPI
    {
        private static readonly string asset_match = "/asset_match/";
        private static readonly Uri api_base = new Uri("https://api.github.com/");
        private static readonly ILog log = LogManager.GetLogger(typeof (KSAPI));
        private static readonly WebClient web = new WebClient();
        private static bool done_init;

        internal static void Init() {
            if (done_init)
            {
                return;
            }

            // Github requires a user-agent. How about that?
            web.Headers.Add("user-agent", Net.UserAgentString);

            done_init = true;
        }

        public static void SetCredentials(string oauth_token)
        {
            web.Headers.Add("Authorization", String.Format("token {0}", oauth_token));
        }

        public static string Call(string path)
        {
            Init();

            Uri url = new Uri (api_base, path);
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
        /// Repository is in the form "Author/Repo". Eg: "pjf/DogeCoinFlag".
        /// </summary>
        public static GithubRelease GetLatestRelease( string repository, bool prerelease )
        {
            string assetFilter = ".zip";

            int asset_match_index = repository.IndexOf (asset_match);
            if (asset_match_index > -1) {
                assetFilter = repository.Substring (asset_match_index + asset_match.Length);
                repository = repository.Substring (0, asset_match_index);
                log.DebugFormat ("Asset Filter: '{0}'", assetFilter);
            }

            string json = Call ("repos/" + repository + "/releases");
            log.Debug("Parsing JSON...");
            JArray releases = JArray.Parse(json);
            string releaseType = prerelease ? "pre-" : "stable";
            log.Debug("Parsed, finding most recent " + releaseType + " release");

            // Finding the most recent *stable* release means filtering
            // out on pre-releases.
            GithubRelease result = null;

            foreach (JObject release in releases)
            {
                // First, check for prerelease status...
                if (prerelease == (bool)release ["prerelease"])
                {
                    JArray assets = (JArray) release ["assets"];
                    foreach (JObject asset in assets)
                    {
                        // Then, check against the regex, which might default to ".zip"
                        if (Regex.IsMatch ((string) asset ["name"], assetFilter, RegexOptions.IgnoreCase))
                        {
                            log.DebugFormat ("Hit on {0}", asset.ToString ());
                            result = new GithubRelease (release, asset);
                            return result;
                        }
                    }
                }
            }

            return result;
        }
    }
}

