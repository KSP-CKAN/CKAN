using System;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    /// <summary>
    /// A simple class to pull the relevant details out of a Github release.
    /// For gh2ckan. :)
    /// </summary>
    public class GithubRelease : CkanInflator
    {
        public Version version;
        public Uri download;
        public long size;
        public string author;

        private static readonly ILog log = LogManager.GetLogger(typeof (GithubRelease));

        public GithubRelease (JObject parsed_json, JObject asset)
        {
            version  = new Version( parsed_json["tag_name"].ToString() );
            author   = parsed_json["author"]["login"].ToString();

            // GH #290, we need to look for the first asset which is a zip, otherwise we
            // end up picking up manuals, pictures of cats, and all sorts of other things.

            if (("application/x-zip-compressed".Equals (asset ["content_type"])) ||
                ("application/zip".Equals (asset ["content_type"])) ||
                (asset ["name"].ToString ().EndsWith (".zip", StringComparison.OrdinalIgnoreCase)))
            {
                size     = (long) asset["size"];
                download = new Uri( asset["browser_download_url"].ToString() );

                log.DebugFormat("Download {0} is {1} bytes", download, size);
            }
            else
            {
                // TODO: A proper kraken, please!
                throw new Kraken("Cannot find download");
            }
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

            Inflate(metadata, "author", author);
            Inflate(metadata, "version", version.ToString());
            Inflate(metadata, "download", Uri.EscapeUriString(download.ToString()));
            Inflate(metadata, "x_generated_by", "netkan");
            Inflate(metadata, "download_size", download_size);
            Inflate((JObject) metadata["resources"], "repository", GithubPage(repo));

        }

        public string Download(string identifier, NetFileCache cache)
        {

            log.DebugFormat("Downloading {0}", download);

            string filename = ModuleInstaller.CachedOrDownload(identifier, version, download, cache);

            log.Debug("Downloaded.");

            return filename;
        }

        public static string GithubPage(string repo)
        {
            var github_base = new Uri("https://github.com/");
            var url = new Uri(github_base, repo);
            return url.ToString();
        }


    }
}

