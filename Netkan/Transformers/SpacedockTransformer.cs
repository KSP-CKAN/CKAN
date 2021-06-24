﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Spacedock;
using CKAN.NetKAN.Sources.Github;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from SpaceDock.
    /// </summary>
    internal sealed class SpacedockTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpacedockTransformer));

        private readonly ISpacedockApi _api;
        private readonly IGithubApi    _githubApi;

        public string Name { get { return "spacedock"; } }

        public SpacedockTransformer(ISpacedockApi api, IGithubApi githubApi)
        {
            _api       = api;
            _githubApi = githubApi;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "spacedock")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing SpaceDock transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // Look up our mod on SD by its Id.
                var sdMod = _api.GetMod(Convert.ToInt32(metadata.Kref.Id));
                var versions = sdMod.All();
                if (opts.SkipReleases.HasValue)
                {
                    versions = versions.Skip(opts.SkipReleases.Value);
                }
                if (opts.Releases.HasValue)
                {
                    versions = versions.Take(opts.Releases.Value);
                }
                bool returnedAny = false;
                foreach (SDVersion vers in versions)
                {
                    returnedAny = true;
                    yield return TransformOne(metadata, metadata.Json(), sdMod, vers);
                }
                if (!returnedAny)
                {
                    Log.WarnFormat("No releases found for {0}", sdMod.ToString());
                    yield return metadata;
                }
            }
            else
            {
                yield return metadata;
            }
        }

        private Metadata TransformOne(Metadata metadata, JObject json, SpacedockMod sdMod, SDVersion latestVersion)
        {
            Log.InfoFormat("Found SpaceDock mod: {0} {1}", sdMod.name, latestVersion.friendly_version);

            // Only pre-fill version info if there's none already. GH #199
            if (json["ksp_version_min"] == null && json["ksp_version_max"] == null && json["ksp_version"] == null)
            {
                Log.DebugFormat("Writing ksp_version from SpaceDock: {0}", latestVersion.KSP_version);
                json["ksp_version"] = latestVersion.KSP_version.ToString();
            }

            json.SafeAdd("name", sdMod.name);
            json.SafeAdd("abstract", sdMod.short_description);
            json.SafeAdd("version", latestVersion.friendly_version.ToString());
            json.Remove("$kref");
            json.SafeAdd("download", latestVersion.download_path.OriginalString);
            json.SafeAdd(Model.Metadata.UpdatedPropertyName, latestVersion.created);

            var authors = GetAuthors(sdMod);

            if (authors.Count == 1)
                json.SafeAdd("author", sdMod.author);
            else if (authors.Count > 1)
                json.SafeAdd("author", new JArray(authors));

            // SD provides users with the following default selection of licenses. Let's convert them to CKAN
            // compatible license strings if possible.
            //
            // "MIT" - OK
            // "BSD" - Specific version is indeterminate
            // "GPLv2" - Becomes "GPL-2.0"
            // "GPLv3" - Becomes "GPL-3.0"
            // "LGPL" - Specific version is indeterminate

            var sdLicense = sdMod.license.Trim();

            switch (sdLicense)
            {
                case "GPLv2":
                    json.SafeAdd("license", "GPL-2.0");
                    break;
                case "GPLv3":
                    json.SafeAdd("license", "GPL-3.0");
                    break;
                default:
                    json.SafeAdd("license", sdLicense);
                    break;
            }

            // Make sure resources exist.
            if (json["resources"] == null)
            {
                json["resources"] = new JObject();
            }

            var resourcesJson = (JObject)json["resources"];
            resourcesJson.SafeAdd("spacedock", sdMod.GetPageUrl().OriginalString);
            TryAddResourceURL(metadata.Identifier, resourcesJson, "homepage",   sdMod.website);

            if (sdMod.background != null)
            {
                TryAddResourceURL(metadata.Identifier, resourcesJson, "x_screenshot", sdMod.background.ToString());
            }

            if (!string.IsNullOrEmpty(sdMod.source_code))
            {
                try
                {
                    var uri = new Uri(sdMod.source_code);
                    if (uri.Host == "github.com")
                    {
                        var match = githubUrlPathPattern.Match(uri.AbsolutePath);
                        if (match.Success)
                        {
                            var owner = match.Groups["owner"].Value;
                            var repo  = match.Groups["repo"].Value;
                            var repoInfo = _githubApi.GetRepo(new GithubRef(
                                $"#/ckan/github/{owner}/{repo}", false, false
                            ));

                            if (sdMod.source_code != repoInfo.HtmlUrl)
                            {
                                TryAddResourceURL(metadata.Identifier, resourcesJson, "repository", repoInfo.HtmlUrl);
                            }
                            // Fall back to homepage from GitHub
                            TryAddResourceURL(metadata.Identifier, resourcesJson, "homepage", repoInfo.Homepage);
                            if (repoInfo.HasIssues)
                            {
                                // Set bugtracker if repo has issues list
                                TryAddResourceURL(metadata.Identifier, resourcesJson, "bugtracker", $"{repoInfo.HtmlUrl}/issues");
                            }
                            if (repoInfo.Archived)
                            {
                                Log.Warn("Repo is archived, consider freezing");
                            }
                        }
                    }
                }
                catch
                {
                    // Just give up, it's fine
                }
            }
            TryAddResourceURL(metadata.Identifier, resourcesJson, "repository", sdMod.source_code);

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            return new Metadata(json);
        }

        private void TryAddResourceURL(string identifier, JObject resources, string key, string rawURL)
        {
            if (!string.IsNullOrEmpty(rawURL))
            {
                string normalized = Normalize(rawURL);
                if (!string.IsNullOrEmpty(normalized))
                {
                    resources.SafeAdd(key, normalized);
                }
                else
                {
                    Log.WarnFormat("Could not normalize URL from {0}: {1}", identifier, rawURL);
                }
            }
        }

        /// <summary>
        /// Provide an escaped version of the given Uri string, including converting
        /// square brackets to their escaped forms.
        /// </summary>
        /// <returns>
        /// <c>null</c> if the string is not a valid <see cref="Uri"/>, otherwise its normlized form.
        /// </returns>
        private static string Normalize(string uri)
        {
            Log.DebugFormat("Escaping {0}", uri);

            var escaped = Uri.EscapeUriString(uri);

            // Square brackets are "reserved characters" that should not appear
            // in strings to begin with, so C# doesn't try to escape them in case
            // they're being used in a special way. They're not; some mod authors
            // just have crazy ideas as to what should be in a URL, and SD doesn't
            // escape them in its API. There's probably more in RFC 3986.

            escaped = escaped.Replace("[", Uri.HexEscape('['));
            escaped = escaped.Replace("]", Uri.HexEscape(']'));

            // Make sure we have a "http://" or "https://" start.
            if (!Regex.IsMatch(escaped, "(?i)^(http|https)://"))
            {
                // Prepend "http://", as we do not know if the site supports https.
                escaped = "http://" + escaped;
            }

            if (Uri.IsWellFormedUriString(escaped, UriKind.Absolute))
            {
                Log.DebugFormat("Escaped to {0}", escaped);
                return escaped;
            }
            else
            {
                return null;
            }
        }

        private static List<string> GetAuthors(SpacedockMod mod)
        {
            var result = new List<string> { mod.author };

            if (mod.shared_authors != null)
                result.AddRange(mod.shared_authors.Select(i => i.Username));

            return result;
        }

        private static readonly Regex githubUrlPathPattern = new Regex(
            "^/(?<owner>[^/]+)/(?<repo>[^/]+)",
            RegexOptions.Compiled
        );

    }
}
