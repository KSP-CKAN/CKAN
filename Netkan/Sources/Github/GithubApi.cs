using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

using log4net;
using Newtonsoft.Json;

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
        private readonly string?      _oauthToken;

        private const string rawMediaType = "application/vnd.github.v3.raw";

        // https://github.com/<OWNER>/<REPO>/blob/<BRANCH>/<PATH>
        // https://github.com/<OWNER>/<REPO>/tree/<BRANCH>/<PATH>
        // https://github.com/<OWNER>/<REPO>/raw/<BRANCH>/<PATH>
        private static readonly Regex githubUrlRegex = new Regex(
            @"^/(?<owner>[^/]+)/(?<repo>[^/]+)/(?<type>[^/]+)/(?<branch>[^/]+)/(?<path>.+)$",
            RegexOptions.Compiled);

        private static readonly HashSet<string> urlTypes = new HashSet<string>()
        {
            "blob", "tree", "raw"
        };

        // https://raw.githubusercontent.com/<OWNER>/<REPO>/<BRANCH>/<PATH>
        private static readonly Regex githubUserContentUrlRegex =
            new Regex(@"^/(?<owner>[^/]+)/(?<repo>[^/]+)/(refs/heads/)?(?<branch>[^/]+)/(?<path>.+)$",
                      RegexOptions.Compiled);

        public GithubApi(IHttpService http, string? oauthToken = null)
        {
            _http       = http;
            _oauthToken = oauthToken;
        }

        public GithubRepo? GetRepo(GithubRef reference)
            => Call($"repos/{reference.Repository}") is string s
                   ? JsonConvert.DeserializeObject<GithubRepo>(s)
                   : null;

        public GithubRelease? GetLatestRelease(GithubRef reference, bool? usePrerelease)
            => GetAllReleases(reference, usePrerelease).FirstOrDefault();

        private static bool ReleaseTypeMatches(bool? UsePrerelease, bool isPreRelease)
            => !UsePrerelease.HasValue
               || UsePrerelease.Value == isPreRelease;

        public IEnumerable<GithubRelease> GetAllReleases(GithubRef reference, bool? usePrerelease)
        {
            const int perPage = 10;
            for (int page = 1; true; ++page)
            {
                var json = Call($"repos/{reference.Repository}/releases?per_page={perPage}&page={page}");
                if (json == null)
                {
                    break;
                }
                Log.Debug("Parsing JSON...");
                var releases = JsonConvert.DeserializeObject<GithubRelease[]>(json)
                               ?? Array.Empty<GithubRelease>();
                if (releases.Length < 1)
                {
                    // That's all folks!
                    break;
                }

                foreach (var ghRel in releases.Where(r => ReleaseTypeMatches(usePrerelease, r.PreRelease)
                                                          // Skip releases without assets
                                                          && (reference.UseSourceArchive
                                                              || (r.Assets != null
                                                                  && r.Assets.Any(reference.FilterMatches))))
                                              // Insurance against GitHub returning them in the wrong order
                                              .OrderByDescending(r => r.PublishedAt))
                {
                    yield return ghRel;
                }
            }
        }

        public List<GithubUser> getOrgMembers(GithubUser organization)
            => (Call($"orgs/{organization.Login}/public_members") is string s
                    ? JsonConvert.DeserializeObject<List<GithubUser>>(s)
                    : null)
               ?? new List<GithubUser>();

        /// <summary>
        /// Download a URL via the GitHubAPI.
        /// Will use a token if we have one.
        /// </summary>
        /// <param name="url">The URL to download</param>
        /// <returns>
        /// null if the URL isn't on GitHub, otherwise the contents of the download
        /// </returns>
        public string? DownloadText(Uri url)
        {
            if (TryGetGitHubPath(url, out string? ghOwner,
                                      out string? ghRepo,
                                      out string? ghBranch,
                                      out string? ghPath))
            {
                Log.InfoFormat("Found GitHub URL, retrieving with API: {0} {1} {2} {3}",
                               ghOwner, ghRepo, ghBranch, ghPath);
                return Call(
                    $"repos/{ghOwner}/{ghRepo}/contents/{ghPath}?ref={ghBranch}",
                    rawMediaType);
            }
            else
            {
                Log.DebugFormat("Not a GitHub URL: {0}", url.ToString());
                return null;
            }
        }

        private static bool TryGetGitHubPath(Uri url,
                                             [NotNullWhen(true)] out string? owner,
                                             [NotNullWhen(true)] out string? repo,
                                             [NotNullWhen(true)] out string? branch,
                                             [NotNullWhen(true)] out string? path)
        {
            switch (url.Host)
            {
                case "github.com":
                    Log.DebugFormat("Found standard GitHub host, checking format");
                    Match ghMatch = githubUrlRegex.Match(url.AbsolutePath);
                    if (ghMatch.Success && urlTypes.Contains(ghMatch.Groups["type"].Value))
                    {
                        Log.DebugFormat("Matched standard GitHub format");
                        owner  = ghMatch.Groups["owner"].Value;
                        repo   = ghMatch.Groups["repo"].Value;
                        branch = ghMatch.Groups["branch"].Value;
                        path   = ghMatch.Groups["path"].Value;
                        return true;
                    }
                    break;

                case "raw.githubusercontent.com":
                    Log.DebugFormat("Found raw GitHub host, checking format");
                    Match rawMatch = githubUserContentUrlRegex.Match(url.AbsolutePath);
                    if (rawMatch.Success)
                    {
                        Log.DebugFormat("Matched raw GitHub format");
                        owner  = rawMatch.Groups["owner"].Value;
                        repo   = rawMatch.Groups["repo"].Value;
                        branch = rawMatch.Groups["branch"].Value;
                        path   = rawMatch.Groups["path"].Value;
                        return true;
                    }
                    break;
            }
            owner = repo = branch = path = null;
            return false;
        }

        private string? Call(string path, string? mimeType = null)
        {
            var url = new Uri(ApiBase, path);
            Log.DebugFormat("Calling {0}", url);

            try
            {
                return _http.DownloadText(url, _oauthToken, mimeType);
            }
            catch (WebException k)
            {
                if (((HttpWebResponse?)k.Response)?.StatusCode is HttpStatusCode.Forbidden
                                                               #if NETFRAMEWORK
                                                               or (HttpStatusCode)429
                                                               #else
                                                               or HttpStatusCode.TooManyRequests
                                                               #endif
                                                               or HttpStatusCode.ServiceUnavailable
                    && k.Response.Headers["X-RateLimit-Remaining"] == "0"
                    && Net.ThrottledHosts.TryGetValue(url.Host, out Uri? infoUrl)
                    && infoUrl is not null)
                {
                    throw new RequestThrottledKraken(url, infoUrl, k,
                                                     $"GitHub API rate limit exceeded: {path}");
                }
                throw;
            }
        }
    }
}
