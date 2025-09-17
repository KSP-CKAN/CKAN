using System;
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
        public SpacedockTransformer(ISpacedockApi api, IGithubApi githubApi)
        {
            _api       = api;
            _githubApi = githubApi;
        }

        public string Name => "spacedock";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "spacedock")
            {
                Log.InfoFormat("Executing SpaceDock transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                // Look up our mod on SD by its Id.
                var sdMod = _api.GetMod(Convert.ToInt32(metadata.Kref.Id));
                var versions = sdMod?.All();
                if (sdMod != null && versions != null)
                {
                    if (opts.SkipReleases != null)
                    {
                        versions = versions.Skip(opts.SkipReleases.Value);
                    }
                    if (opts.Releases != null)
                    {
                        versions = versions.Take(opts.Releases.Value);
                    }
                    bool returnedAny = false;
                    foreach (var vers in versions)
                    {
                        returnedAny = true;
                        yield return TransformOne(metadata, metadata.Json(), sdMod, vers);
                    }
                    if (!returnedAny)
                    {
                        Log.WarnFormat("No releases found for {0}", sdMod?.ToString());
                        yield return metadata;
                    }
                }
            }
            else
            {
                yield return metadata;
            }
        }

        private Metadata TransformOne(Metadata metadata, JObject json,
                                      SpacedockMod sdMod, SpacedockVersion latestVersion)
        {
            Log.InfoFormat("Found SpaceDock mod: {0} {1}", sdMod.name, latestVersion.friendly_version);

            // Only pre-fill version info if there's none already. GH #199
            if (json["ksp_version_min"] == null && json["ksp_version_max"] == null && json["ksp_version"] == null)
            {
                Log.DebugFormat("Writing ksp_version from SpaceDock: {0}", latestVersion.KSP_version);
                json["ksp_version"] = latestVersion.KSP_version?.WithoutBuild.ToString();
            }

            json.SafeAdd("name", sdMod.name);
            json.SafeAdd("abstract", sdMod.short_description);
            var ver = latestVersion.friendly_version?.ToString();
            json.SafeAdd("version", ver);
            json.Remove("$kref");
            json.SafeAdd("download", latestVersion.download_path?.OriginalString);
            json.SafeAdd("release_date", latestVersion.created);

            json.SafeAdd("author", () => GetAuthors(sdMod));

            // SD provides users with the following default selection of licenses. Let's convert them to CKAN
            // compatible license strings if possible.
            //
            // "MIT"   - OK
            // "BSD"   - Specific version is indeterminate
            // "GPLv2" - Becomes "GPL-2.0"
            // "GPLv3" - Becomes "GPL-3.0"
            // "LGPL"  - Specific version is indeterminate
            json.SafeAdd("license",
                         sdMod.license switch
                         {
                             "GPLv2"                        => "GPL-2.0",
                             "GPLv3"                        => "GPL-3.0",
                             "Other"                        => "unknown",
                             "ARR" or "All Rights Reserved"
                                   or "All rights reserved" => "restricted",
                             var sdLicense                  => sdLicense?.Trim().Replace(' ', '-'),
                         });

            // Make sure resources exist.
            if (json["resources"] == null)
            {
                json["resources"] = new JObject();
            }

            var resourcesJson = (JObject?)json["resources"];
            resourcesJson?.SafeAdd("spacedock", sdMod.GetPageUrl().OriginalString);
            TryAddResourceURL(metadata.Identifier, resourcesJson, "homepage",   sdMod.website);

            if (sdMod.background != null)
            {
                TryAddResourceURL(metadata.Identifier, resourcesJson, "x_screenshot", sdMod.background.ToString());
            }

            if (resourcesJson != null && !string.IsNullOrEmpty(sdMod.source_code))
            {
                try
                {
                    var uri = new Uri(sdMod.source_code);
                    if (uri.Host == "github.com"
                        && githubUrlPathPattern.Match(uri.AbsolutePath)
                           is Match { Success: true } match
                        && _githubApi.GetRepo(new GithubRef(match.Groups["owner"].Value,
                                                            match.Groups["repo"].Value))
                           is GithubRepo repoInfo)
                    {
                        GithubTransformer.SetRepoResources(repoInfo, resourcesJson);
                        if (repoInfo.Archived)
                        {
                            Log.Warn("Repo is archived, consider freezing");
                        }
                    }
                }
                catch (RequestThrottledKraken)
                {
                    // Treat rate limiting as a real error to avoid temporary metadata degradation
                    throw;
                }
                catch
                {
                    // Just give up, invalid URLs are fine
                }
            }
            TryAddResourceURL(metadata.Identifier, resourcesJson, "repository", sdMod.source_code);

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            return new Metadata(json);
        }

        private static void TryAddResourceURL(string identifier, JObject? resources, string key, string? rawURL)
        {
            if (rawURL != null && !string.IsNullOrEmpty(rawURL))
            {
                var normalized = Net.NormalizeUri(rawURL);
                if (!string.IsNullOrEmpty(normalized))
                {
                    resources?.SafeAdd(key, normalized);
                }
                else
                {
                    Log.WarnFormat("Could not normalize URL from {0}: {1}", identifier, rawURL);
                }
            }
        }

        private static JToken? GetAuthors(SpacedockMod mod)
            => Enumerable.Repeat(mod.author, 1)
                         .Concat(mod.shared_authors?.Select(i => i.Username)
                                                   ?? Enumerable.Empty<string>())
                         .ToJValueOrJArray();

        private static readonly Regex githubUrlPathPattern =
            new Regex("^/(?<owner>[^/]+)/(?<repo>[^/]+)",
                      RegexOptions.Compiled);

        private readonly ISpacedockApi _api;
        private readonly IGithubApi    _githubApi;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SpacedockTransformer));
    }
}
