using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Services;

// We could use OctoKit for this, but since we're only pinging the
// release API, I'm happy enough without yet another dependency.

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GithubApi : IGithubApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GithubApi));
        private static readonly Uri ApiBase = new Uri("https://api.github.com/");

        private readonly IHttpService _http;
        private readonly string       _oauthToken;

        public GithubApi(IHttpService http, string oauthToken = null)
        {
            _http       = http;
            _oauthToken = oauthToken;
        }

        public GithubRepo GetRepo(GithubRef reference)
        {
            return JsonConvert.DeserializeObject<GithubRepo>(
                Call($"repos/{reference.Repository}")
            );
        }

        public GithubRelease GetLatestRelease(GithubRef reference)
        {
            return GetAllReleases(reference).FirstOrDefault();
        }

        public IEnumerable<GithubRelease> GetAllReleases(GithubRef reference)
        {
            var json = Call($"repos/{reference.Repository}/releases?per_page=100");
            Log.Debug("Parsing JSON...");
            var releases = JArray.Parse(json);

            // Finding the most recent *stable* release means filtering
            // out on pre-releases.

            foreach (var release in releases)
            {
                // First, check for prerelease status...
                if (reference.UsePrerelease == (bool)release["prerelease"])
                {
                    var version = new ModuleVersion((string)release["tag_name"]);
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
                        yield return new GithubRelease(author, version, download, updated);
                    }
                }
            }
        }

        private string Call(string path)
        {
            var url = new Uri(ApiBase, path);
            Log.DebugFormat("Calling {0}", url);

            try
            {
                return _http.DownloadText(url, _oauthToken);
            }
            catch (NativeAndCurlDownloadFailedKraken k)
            {
                if (k.responseStatus == 403 && k.responseHeader.Contains("X-RateLimit-Remaining: 0"))
                {
                    throw new Kraken("GitHub API rate limit exceeded.");
                }
                throw;
            }
        }
    }
}
