using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from GitHub.
    /// </summary>
    internal sealed class GithubTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GithubTransformer));

        private readonly IGithubApi _api;
        private readonly bool       _matchPreleases;

        public string Name { get { return "github"; } }

        public GithubTransformer(IGithubApi api, bool matchPreleases)
        {
            if (api == null)
                throw new ArgumentNullException("api");

            _api            = api;
            _matchPreleases = matchPreleases;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "github")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing GitHub transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var useSourceAchive = false;

                var githubMetadata = (JObject)json["x_netkan_github"];
                if (githubMetadata != null)
                {
                    var githubUseSourceArchive = (bool?)githubMetadata["use_source_archive"];

                    if (githubUseSourceArchive != null)
                    {
                        useSourceAchive = githubUseSourceArchive.Value;
                    }
                }

                var ghRef = new GithubRef(metadata.Kref, useSourceAchive, _matchPreleases);

                // Get the GitHub repository
                var ghRepo = _api.GetRepo(ghRef);
                var versions = _api.GetAllReleases(ghRef);
                if (opts.SkipReleases.HasValue)
                {
                    versions = versions.Skip(opts.SkipReleases.Value);
                }
                if (opts.Releases.HasValue)
                {
                    versions = versions.Take(opts.Releases.Value);
                }
                bool returnedAny = false;
                foreach (GithubRelease rel in versions)
                {
                    returnedAny = true;
                    yield return TransformOne(metadata, metadata.Json(), ghRef, ghRepo, rel);
                }
                if (!returnedAny)
                {
                    Log.WarnFormat("No releases found for {0}", ghRef.Repository);
                    yield return metadata;
                }
            }
            else
            {
                yield return metadata;
            }
        }

        private Metadata TransformOne(Metadata metadata, JObject json, GithubRef ghRef, GithubRepo ghRepo, GithubRelease ghRelease)
        {
            if (!string.IsNullOrWhiteSpace(ghRepo.Description))
                json.SafeAdd("abstract", ghRepo.Description);

            // GitHub says NOASSERTION if it can't figure out the repo's license
            if (!string.IsNullOrWhiteSpace(ghRepo.License?.Id)
                && ghRepo.License.Id != "NOASSERTION")
                json.SafeAdd("license", ghRepo.License.Id);

            // Make sure resources exist.
            if (json["resources"] == null)
                json["resources"] = new JObject();

            var resourcesJson = (JObject)json["resources"];

            if (!string.IsNullOrWhiteSpace(ghRepo.Homepage))
                resourcesJson.SafeAdd("homepage", ghRepo.Homepage);

            resourcesJson.SafeAdd("repository", ghRepo.HtmlUrl);

            if (ghRelease != null)
            {
                json.SafeAdd("version",  ghRelease.Version.ToString());
                json.SafeAdd("author",   () => getAuthors(ghRepo, ghRelease));
                json.SafeAdd("download", ghRelease.Download.ToString());
                json.SafeAdd(Metadata.UpdatedPropertyName, ghRelease.AssetUpdated);

                if (ghRef.Project.Contains("_"))
                {
                    json.SafeAdd("name", ghRef.Project.Replace("_", " "));
                }
                else if (ghRef.Project.Contains("-"))
                {
                    json.SafeAdd("name", ghRef.Project.Replace("-", " "));
                }
                else if (ghRef.Project.Contains("."))
                {
                    json.SafeAdd("name", ghRef.Project.Replace(".", " "));
                }
                else
                {
                    var repoName = ghRef.Project;
                    for (var i = 1; i < repoName.Length - 1; ++i)
                    {
                        if (char.IsLower(repoName[i - 1]) && char.IsUpper(repoName[i]) || repoName[i - 1] != ' ' && char.IsUpper(repoName[i]) && char.IsLower(repoName[i + 1]))
                        {
                            repoName = repoName.Insert(i, " ");
                        }
                    }

                    json.SafeAdd("name", repoName);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }
            else
            {
                Log.WarnFormat("No releases found for {0}", ghRef.Repository);
                return metadata;
            }
        }

        private JToken getAuthors(GithubRepo repo, GithubRelease release)
        {
            // Start with the user that published the release
            var authors = new List<string>() { release.Author };
            for (GithubRepo r = repo; r != null;)
            {
                switch (r.Owner?.Type)
                {
                    case userType:
                        // Prepend repo owner
                        if (!authors.Contains(r.Owner.Login))
                            authors.Insert(0, r.Owner.Login);
                        break;
                    case orgType:
                        // Prepend org members
                        authors.InsertRange(0,
                            _api.getOrgMembers(r.Owner)
                                .Where(u => !authors.Contains(u.Login))
                                .Select(u => u.Login)
                        );
                        break;
                }
                // Check parent repos
                r = r.ParentRepo == null
                    ? null
                    : _api.GetRepo(new GithubRef($"#/ckan/github/{r.ParentRepo.FullName}", false, _matchPreleases));
            }
            // Return a string if just one author, else an array
            return authors.Count == 1 ? (JToken)authors.First() : new JArray(authors);
        }

        private const string userType = "User";
        private const string orgType  = "Organization";
    }
}
