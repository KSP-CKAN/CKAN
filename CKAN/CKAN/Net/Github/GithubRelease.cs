using System;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    /// <summary>
    /// A simple class to pull the relevant details out of a Github release.
    /// For gh2ckan. :)
    /// </summary>
    public class GithubRelease
    {
        public Version version;
        public Uri download;
        public long size;
        public string author;

        private static readonly ILog log = LogManager.GetLogger(typeof (GithubRelease));

        public GithubRelease (JObject parsed_json)
        {
            version  = new Version( parsed_json["tag_name"].ToString() );
            author   = parsed_json["author"]["login"].ToString();

            // GH #290, we need to look for the first asset which is a zip, otherwise we
            // end up picking up manuals, pictures of cats, and all sorts of other things.

            JToken asset = parsed_json["assets"]
                .Children()
                .Where(asset_info => 
                    asset_info["content_type"].ToString() == "application/x-zip-compressed" ||
                    asset_info["content_type"].ToString() == "application/zip" ||
                    asset_info["name"].ToString().EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                )
                .FirstOrDefault();

            if (asset == null)
            {
                // TODO: A proper kraken, please!
                throw new Kraken("Cannot find download");
            }

            size     = (long) asset["size"];
            download = new Uri( asset["browser_download_url"].ToString() );

            log.DebugFormat("Download {0} is {1} bytes", download, size);
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

