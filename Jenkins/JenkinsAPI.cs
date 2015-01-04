using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    // Jenkins API
    public class JenkinsAPI
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (JenkinsAPI));
        private static readonly WebClient web = new WebClient();

        public JenkinsAPI()
        {
            web.Headers.Add("user-agent", Net.UserAgentString);
        }

        public static string Call(string path)
        {
            Uri url = new Uri (path);
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

        public static JenkinsBuild GetLatestBuild(string baseUri, bool stable = true)
        {
            JenkinsBuild result = null;

            // http://jenkins.mumech.com/job/MechJeb2/lastStableBuild/api/json
            string json = Call (baseUri + "lastStableBuild/api/json");
            log.DebugFormat("Parsing JSON from {0}", json);
            JObject build = JObject.Parse (json);
            if (build != null) 
            {
                JArray artifacts = (JArray) build ["artifacts"];
                log.DebugFormat("  Parsing artifacts from {0}", artifacts);
                foreach (JObject artifact in artifacts.Children())
                {
                    log.DebugFormat("    Parsing artifact from {0}", artifact);

                    string fileName = (string) artifact ["fileName"];
                    string relativePath = (string) artifact ["relativePath"];

                    if (fileName.EndsWith (".zip"))
                    {
                        result = new JenkinsBuild (artifact);
                    }

                    // new GithubRelease(final_releases.Cast<JObject>().First());
                }
                // string releaseType = prerelease ? "pre-" : "stable";
                // log.Debug("Parsed, finding most recent " + releaseType + " release");

                // Finding the most recent *stable* release means filtering
                // out on pre-releases.

                // var final_releases = releases.Where(x => (bool) x["prerelease"] == prerelease);

                // return !final_releases.Any() ? null : new GithubRelease(final_releases.Cast<JObject>().First());
            }

            return result;
        }
    }
}