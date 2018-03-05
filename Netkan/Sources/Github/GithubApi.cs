using System;
using System.Linq;
using System.Net;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// We could use OctoKit for this, but since we're only pinging the
// release API, I'm happy enough without yet another dependency.

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GithubApi : IGithubApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GithubApi));
        private static readonly Uri ApiBase = new Uri("https://api.github.com/");

        private readonly string _oauthToken;

        public GithubApi(string oauthToken = null)
        {
            _oauthToken = oauthToken;
        }

        public GithubRepo GetRepo(GithubRef reference)
        {
            return JsonConvert.DeserializeObject<GithubRepo>(Call(string.Format("repos/{0}", reference.Repository)));
        }

        public GithubRelease GetLatestRelease(GithubRef reference)
        {
            var json = Call(string.Format("repos/{0}/releases", reference.Repository));
            Log.Debug("Parsing JSON...");
            var releases = JArray.Parse(json);

            // Finding the most recent *stable* release means filtering
            // out on pre-releases.

            foreach (var release in releases)
            {
                // First, check for prerelease status...
                if (reference.UsePrerelease == (bool)release["prerelease"])
                {
                    var version = new Version((string)release["tag_name"]);
                    var author = (string)release["author"]["login"];

                    Uri       download = null;
                    DateTime? updated  = null;
                    DateTime  parsed;

                    if (reference.UseSourceArchive)
                    {
                        Log.Debug("Using GitHub source archive");
                        download = new Uri((string)release["zipball_url"]);
                        if (DateTime.TryParse(release["published_at"].ToString(), out parsed))
                        {
                            updated = parsed;
                        }
                    }
                    else
                    {
                        var assets = (JArray)release["assets"];

                        foreach (var asset in assets.Where(asset => reference.Filter.IsMatch((string)asset["name"])))
                        {
                            Log.DebugFormat("Using GitHub asset: {0}", asset["name"]);
                            download = new Uri((string)asset["browser_download_url"]);
                            if (DateTime.TryParse(asset["updated_at"].ToString(), out parsed))
                            {
                                updated = parsed;
                            }
                            break;
                        }
                    }

                    if (download != null)
                    {
                        return new GithubRelease(author, version, download, updated);
                    }
                }
            }

            return null;
        }

        private string Call(string path)
        {
            var web = new WebClient();
            web.Headers.Add("User-Agent", Net.UserAgentString);

            if (_oauthToken != null)
            {
                web.Headers.Add("Authorization", string.Format("token {0}", _oauthToken));
            }

            var url = new Uri(ApiBase, path);
            Log.DebugFormat("Calling {0}", url);

            try
            {
                return web.DownloadString(url);
            }
            catch (WebException webEx)
            {
                Log.ErrorFormat("WebException while accessing {0}: {1}", url, webEx);
                throw;
            }
        }
    }
}
