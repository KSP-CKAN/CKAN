using System;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Sources.Github
{
    public class GithubRelease
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GithubRelease));

        public Version Version { get; private set; }
        public Uri Download { get; private set; }
        public long Size { get; private set; }
        public string Author { get; private set; }

        public GithubRelease(JObject release, JObject asset)
            : this(ParseArguments(release, asset)) {}

        public GithubRelease(Version version, Uri download, long size, string author)
            : this(new Arguments(version, download, size, author)) { }

        private GithubRelease(Arguments arguments)
        {
            Version = arguments.Version;
            Download = arguments.Download;
            Size = arguments.Size;
            Author = arguments.Author;
        }

        private static Arguments ParseArguments(JObject release, JObject asset)
        {
            var version = new Version((string)release["tag_name"]);
            var author = (string)release["author"]["login"];

            if (IsProbablyZip(asset))
            {
                var size = (long)asset["size"];
                var download = new Uri((string)asset["browser_download_url"]);

                Log.DebugFormat("Download {0} is {1} bytes", download, size);

                return new Arguments(version, download, size, author);
            }
            else
            {
                // TODO: A proper kraken, please!
                throw new Kraken("Cannot find download");
            }
        }

        private static bool IsProbablyZip(JObject asset)
        {
            // GH #290, we need to look for the first asset which is a zip, otherwise we
            // end up picking up manuals, pictures of cats, and all sorts of other things.

            var contentType = (string)asset["content_type"];
            var name = (string)asset["name"];

            return contentType == "application/x-zip-compressed" ||
                contentType == "application/zip" ||
                name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class Arguments
        {
            public Version Version { get; private set; }
            public Uri Download { get; private set; }
            public long Size { get; private set; }
            public string Author { get; private set; }

            public Arguments(Version version, Uri download, long size, string author)
            {
                Version = version;
                Download = download;
                Size = size;
                Author = author;
            }
        }
    }
}
