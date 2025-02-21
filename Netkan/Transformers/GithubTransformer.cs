using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;
using CKAN.Extensions;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from GitHub.
    /// </summary>
    internal sealed class GithubTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GithubTransformer));

        private readonly IGithubApi _api;
        private readonly bool?      _matchPreleases;

        public string Name => "github";

        public GithubTransformer(IGithubApi api, bool? matchPreleases)
        {
            _api            = api;
            _matchPreleases = matchPreleases;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions? opts)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "github" && opts != null)
            {
                var json = metadata.Json();

                // Tell downstream translators that this host's API is unreliable
                opts.FlakyAPI = true;

                Log.InfoFormat("Executing GitHub transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var conf = (json.TryGetValue("x_netkan_github", out JToken? jtok)
                            && jtok is JObject jobj
                                ? jobj.ToObject<GitHubConfig>() : null)
                           ?? new GitHubConfig();

                var ghRef = new GithubRef(metadata.Kref, conf.UseSourceArchive);

                // Get the GitHub repository
                var ghRepo = _api.GetRepo(ghRef);
                if (ghRepo == null)
                {
                    throw new Kraken("Failed to get GitHub repo info!");
                }
                if (ghRepo.Archived)
                {
                    Log.Warn("Repo is archived, consider freezing");
                }
                var releases = _api.GetAllReleases(ghRef, _matchPreleases ?? conf.Prereleases);
                if (opts.SkipReleases.HasValue)
                {
                    releases = releases.Skip(opts.SkipReleases.Value);
                }
                if (opts.Releases.HasValue)
                {
                    releases = releases.Take(opts.Releases.Value);
                }
                bool returnedAny = false;
                foreach (var rel in releases)
                {
                    if (ghRef.UseSourceArchive && rel.Tag != null)
                    {
                        returnedAny = true;
                        yield return TransformOne(metadata, metadata.Json(), ghRef, ghRepo, rel,
                                                  new GithubReleaseAsset()
                                                  {
                                                      Name     = rel.Tag.ToString(),
                                                      Download = rel.SourceArchive,
                                                      Updated  = rel.PublishedAt,
                                                      Uploader = rel.Author,
                                                  },
                                                  rel.Tag.ToString());
                    }
                    else if (ghRef.VersionFromAsset != null && rel.Assets != null)
                    {
                        Log.DebugFormat("Found version_from_asset regex, inflating all assets");
                        foreach (var asset in rel.Assets)
                        {
                            var match = ghRef.VersionFromAsset.Match(asset.Name ?? "");
                            if (!match.Success)
                            {
                                continue;
                            }

                            var extractedVersion = match.Groups["version"];
                            if (!extractedVersion.Success)
                            {
                                throw new Exception("version_from_asset contains no 'version' capturing group");
                            }

                            returnedAny = true;
                            yield return TransformOne(metadata, metadata.Json(), ghRef, ghRepo, rel, asset, extractedVersion.Value);
                        }
                    }
                    else if (rel.Assets != null && rel.Tag != null)
                    {
                        var assets = rel.Assets.Where(ghRef.FilterMatches).ToArray();
                        if (assets.Length > 1)
                        {
                            Log.WarnFormat("Multiple assets found for {0} {1} without `version_from_asset`",
                                metadata.Identifier, rel.Tag);
                        }
                        returnedAny = true;
                        yield return TransformOne(metadata, metadata.Json(), ghRef, ghRepo, rel,
                                                  assets.First(), rel.Tag.ToString());
                    }
                }
                if (!returnedAny)
                {
                    if (ghRef.Filter != Constants.DefaultAssetMatchPattern)
                    {
                        Log.WarnFormat("No releases found for {0} with asset_match {1}", ghRef.Repository, ghRef.Filter);
                    }
                    else if (ghRef.VersionFromAsset != null)
                    {
                        Log.WarnFormat("No releases found for {0} with version_from_asset {1}", ghRef.Repository, ghRef.VersionFromAsset);
                    }
                    else
                    {
                        Log.WarnFormat("No releases found for {0}", ghRef.Repository);
                    }
                    yield return metadata;
                }
            }
            else
            {
                yield return metadata;
            }
        }

        private Metadata TransformOne(Metadata           metadata,
                                      JObject            json,
                                      GithubRef          ghRef,
                                      GithubRepo         ghRepo,
                                      GithubRelease      ghRelease,
                                      GithubReleaseAsset ghAsset,
                                      string             version)
        {
            if (!string.IsNullOrWhiteSpace(ghRepo.Description))
            {
                json.SafeAdd("abstract", ghRepo.Description);
            }

            // GitHub says NOASSERTION if it can't figure out the repo's license
            if (ghRepo.License?.Id != null && !string.IsNullOrWhiteSpace(ghRepo.License.Id)
                && ghRepo.License.Id != "NOASSERTION")
            {
                json.SafeAdd("license", ghRepo.License.Id);
            }

            // Make sure resources exist.
            if (json["resources"] == null)
            {
                json["resources"] = new JObject();
            }
            if (json["resources"] is JObject resourcesJson)
            {
                SetRepoResources(ghRepo, resourcesJson);
            }

            if (ghRelease?.Tag != null)
            {
                json.SafeAdd("version",  version);
                json.SafeAdd("author",   () => GetAuthors(ghRepo, ghRelease));
                json.Remove("$kref");
                if (ghAsset.Download is Uri url)
                {
                    json.SafeAdd("download", url.ToString());
                }
                if (ghRef.UseSourceArchive)
                {
                    // https://docs.github.com/en/rest/repos/contents#download-a-repository-archive-zip
                    // GitHub requires clients to specify JSON format
                    // to download a source ZIP (!!!)
                    json.SafeAdd("download_content_type",
                                 "application/vnd.github+json");
                }
                json.SafeAdd(Metadata.UpdatedPropertyName, ghAsset.Updated);

                if (ghRef.Project.Contains('_'))
                {
                    json.SafeAdd("name", ghRef.Project.Replace("_", " "));
                }
                else if (ghRef.Project.Contains('-'))
                {
                    json.SafeAdd("name", ghRef.Project.Replace("-", " "));
                }
                else if (ghRef.Project.Contains('.'))
                {
                    json.SafeAdd("name", ghRef.Project.Replace(".", " "));
                }
                else
                {
                    var repoName = ghRef.Project;
                    for (var i = 1; i < repoName.Length - 1; ++i)
                    {
                        if ((char.IsLower(repoName[i - 1]) && char.IsUpper(repoName[i]))
                            || (repoName[i - 1] != ' ' && char.IsUpper(repoName[i]) && char.IsLower(repoName[i + 1])))
                        {
                            repoName = repoName.Insert(i, " ");
                        }
                    }

                    json.SafeAdd("name", repoName);
                }

                if (ghRelease.PreRelease)
                {
                    json.SafeAdd("release_status", "testing");
                }

                json.SafeMerge(
                    "x_netkan_version_pieces",
                    JObject.FromObject(new Dictionary<string, string>{ {"tag", ghRelease.Tag.ToString()} }));

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }
            else
            {
                Log.WarnFormat("No releases found for {0}", ghRef.Repository);
                return metadata;
            }
        }

        public static void SetRepoResources(GithubRepo repo, JObject resources)
        {
            resources.SafeAdd("repository", repo.HtmlUrl);
            if (!string.IsNullOrWhiteSpace(repo.Homepage))
            {
                resources.SafeAdd("homepage", repo.Homepage);
            }
            if (repo.HasIssues)
            {
                resources.SafeAdd("bugtracker", $"{repo.HtmlUrl}/issues");
            }
            if (repo.HasDiscussions)
            {
                resources.SafeAdd("discussions", $"{repo.HtmlUrl}/discussions");
            }
        }

        private JToken? GetAuthors(GithubRepo    repo,
                                   GithubRelease release)
            => repo.TraverseNodes(r => r.ParentRepo == null
                                           ? null
                                           : _api.GetRepo(new GithubRef($"#/ckan/github/{r.ParentRepo.FullName}",
                                                          false)))
                   .Reverse()
                   .Select(r => r.Owner)
                   .Append(release.Author)
                   .OfType<GithubUser>()
                   .SelectMany(u => u.Type switch
                                    {
                                        GithubUserType.User => Enumerable.Repeat(u.Login, 1),
                                        GithubUserType.Organization => _api.getOrgMembers(u)
                                                                           .Select(u => u.Login),
                                        GithubUserType.Bot or _ => Enumerable.Empty<string>(),
                                    })
                   .ToJValueOrJArray();
    }
}
