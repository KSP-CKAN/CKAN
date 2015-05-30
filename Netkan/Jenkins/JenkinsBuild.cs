using System;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    public class JenkinsBuild : CkanInflator
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

        override public void InflateMetadata(JObject metadata, string filename, object context)
        {
            var repo = (string)context;

            // Check how big our file is
            long download_size = (new FileInfo (filename)).Length;

            // Make sure resources exist.
            if (metadata["resources"] == null)
            {
                metadata["resources"] = new JObject();
            }

            // Inflate(metadata, "author", author);
            Inflate(metadata, "version", version.ToString());
            Inflate(metadata, "download", Uri.EscapeUriString(download.ToString()));
            Inflate(metadata, "x_generated_by", "netkan");
            Inflate(metadata, "download_size", download_size);
            // Inflate((JObject) metadata["resources"], "repository", GithubPage(repo));
        }

        public string Download(string identifier, NetFileCache cache)
        {
            log.DebugFormat("Downloading {0}", download);

            string filename = ModuleInstaller.CachedOrDownload(identifier, version, download, cache);

            log.Debug("Downloaded.");

            return filename;
        }
    }
}

