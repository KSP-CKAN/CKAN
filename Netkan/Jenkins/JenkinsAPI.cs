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

            try
            {
                return web.DownloadString(url);
            }
            catch(WebException webEx)
            {
                log.ErrorFormat ("WebException while accessing {0}: {1}", url, webEx);
                throw webEx;
            }
        }

        public static JenkinsBuild GetLatestBuild(string baseUri, string versionBase, bool stable = true)
        {
            JenkinsBuild result = null;
            if (!(baseUri.EndsWith ("/")))
            {
                baseUri = baseUri + "/";
            }

            // http://jenkins.mumech.com/job/MechJeb2/lastStableBuild/api/json
            string json = Call (baseUri + "lastStableBuild/api/json");
            JObject build = JObject.Parse (json);
            if (build != null)
            {
                string buildNumber = (string) build ["number"];

                JArray artifacts = (JArray) build ["artifacts"];
                log.DebugFormat("  Parsing artifacts from {0}", artifacts);
                foreach (JObject artifact in artifacts.Children())
                {
                    log.DebugFormat("    Parsing artifact from {0}", artifact);

                    string fileName = (string) artifact ["fileName"];

                    // TODO - filtering of artifacts, for now hardcoded for zip files.
                    if (fileName.EndsWith (".zip"))
                    {
                        result = new JenkinsBuild (artifact, versionBase, buildNumber, baseUri, stable);
                    }
                }
            }

            return result;
        }
    }
}