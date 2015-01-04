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

        public JenkinsBuild (JObject parsed_json)
        {
            // version  = new Version( parsed_json["tag_name"].ToString() );
            version  = new Version( "2.4.2.0-382");
            download = new Uri ("http://jenkins.mumech.com/job/MechJeb2/lastSuccessfulBuild/artifact/jenkins-MechJeb2-382/MechJeb2-2.4.2.0-382.zip");
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

