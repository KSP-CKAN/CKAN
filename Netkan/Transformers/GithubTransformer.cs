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

                // Find the release on github and download.
                var ghRelease = _api.GetLatestRelease(ghRef);

                if (ghRelease != null)
                {
                    json.SafeAdd("version", ghRelease.Version.ToString());
                    json.SafeAdd("author", ghRelease.Author);
                    json.SafeAdd("download", Uri.EscapeUriString(ghRelease.Download.ToString()));

                    // Make sure resources exist.
                    if (json["resources"] == null)
                    {
                        json["resources"] = new JObject();
                    }

                    var resourcesJson = (JObject)json["resources"];
                    resourcesJson.SafeAdd("repository", GithubPage(ghRef.Repository));

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

        private static string GithubPage(string repo)
        {
            return new Uri(new Uri("https://github.com/"), repo).ToString();
        }
    }
}
