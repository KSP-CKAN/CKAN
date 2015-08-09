using System;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Sources.Jenkins
{
    public class JenkinsBuild
    {
        public Version version;
        public Uri download;

        private static readonly ILog log = LogManager.GetLogger(typeof (JenkinsBuild));

        public JenkinsBuild (JObject parsed_json, string versionBase, string buildNumber, string baseUri, bool stable = true)
        {
            log.DebugFormat ("Parsing build from {0}, {1}, {2}", versionBase, buildNumber, baseUri);
            log.DebugFormat ("Artifact JSON: {0}", parsed_json);
            // We don't have a good way of obtaining version information (yet)
            // This might be improved via AVC support on addons which contain that...
            version  = new Version( versionBase + "-" + buildNumber );
            string relativePath = (string) parsed_json ["relativePath"];
            string downloadUri = baseUri + "lastStableBuild/artifact/" + relativePath;
            log.DebugFormat ("Download URI: {0}", downloadUri);
            download = new Uri (downloadUri);
        }
    }
}

