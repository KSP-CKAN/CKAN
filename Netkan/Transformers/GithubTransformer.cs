using System;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from GitHub.
    /// </summary>
    internal sealed class GithubTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GithubTransformer));

        private readonly IGithubApi _api;
        private readonly bool _matchPreleases;

        public string Name { get { return "github"; } }

        public GithubTransformer(IGithubApi api, bool matchPreleases)
        {
            if (api == null)
                throw new ArgumentNullException("api");

            _api = api;
            _matchPreleases = matchPreleases;
        }

        public Metadata Transform(Metadata metadata)
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
                // Get the GitHub release
                var ghRelease = _api.GetLatestRelease(ghRef);

                // Make sure resources exist.
                if (json["resources"] == null)
                    json["resources"] = new JObject();

                var resourcesJson = (JObject)json["resources"];

                if (!string.IsNullOrWhiteSpace(ghRepo.Description))
                    json.SafeAdd("abstract", ghRepo.Description);

                if (!string.IsNullOrWhiteSpace(ghRepo.Homepage))
                    resourcesJson.SafeAdd("homepage", ghRepo.Homepage);

                resourcesJson.SafeAdd("repository", ghRepo.HtmlUrl);

                if (ghRelease != null)
                {
                    json.SafeAdd("version",  ghRelease.Version.ToString());
                    json.SafeAdd("author",   ghRelease.Author);
                    json.SafeAdd("download", Uri.EscapeUriString(ghRelease.Download.ToString()));
                    json.SafeAdd(Model.Metadata.UpdatedPropertyName, ghRelease.AssetUpdated);

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
                }
            }

            return metadata;
        }
    }
}
