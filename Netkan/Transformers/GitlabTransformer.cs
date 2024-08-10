using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using log4net;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Sources.Gitlab;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from GitLab.
    /// </summary>
    internal sealed class GitlabTransformer : ITransformer
    {
        /// <summary>
        /// Initialize the transformer
        /// </summary>
        /// <param name="api">Object to use for accessing the GitLab API</param>
        public GitlabTransformer(IGitlabApi api)
        {
            if (api == null)
            {
                throw new ArgumentNullException("api");
            }
            this.api = api;
        }

        /// <summary>
        /// Defines the name of this transformer
        /// </summary>
        public string Name => "gitlab";

        /// <summary>
        /// If input metadata has a GitLab kref, inflate it with whatever info we can get,
        /// otherwise return it unchanged
        /// </summary>
        /// <param name="metadata">Input netkan</param>
        /// <param name="opts">Inflation options from command line</param>
        /// <returns></returns>
        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref?.Source == Name)
            {
                var reference = new GitlabRef(metadata.Kref);
                var project   = api.GetProject(reference);
                var releases  = api.GetAllReleases(reference)
                    .Skip(opts.SkipReleases ?? 0)
                    .Take(opts.Releases     ?? 1)
                    .ToArray();
                if (releases.Length < 1)
                {
                    log.WarnFormat("No releases found for {0}", reference);
                    return Enumerable.Repeat(metadata, 1);
                }
                return releases.Select(release => TransformOne(
                    metadata.Json(), project, release));
            }
            else
            {
                // Passthrough for non-GitLab mods
                return Enumerable.Repeat(metadata, 1);
            }
        }

        private Metadata TransformOne(JObject json, GitlabProject project, GitlabRelease release)
        {
            var opts = (json["x_netkan_gitlab"] as JObject)?.ToObject<GitlabOptions>()
                ?? new GitlabOptions();
            if (!opts.UseSourceArchive)
            {
                throw new Exception("'x_netkan_gitlab.use_source_archive' missing or false; GitLab ONLY supports source archives!");
            }

            json.SafeAdd("name",     project.Name);
            json.SafeAdd("abstract", project.Description);
            json.SafeAdd("author",   release.Author.Name);
            json.SafeAdd("version",  release.TagName);
            json.SafeMerge("resources", JObject.FromObject(new Dictionary<string, string>()
            {
                { "repository", project.WebURL },
                { "bugtracker", project.IssuesEnabled ? $"{project.WebURL}/-/issues" : null },
                { "manual",     project.ReadMeURL },
            }));
            json.SafeAdd("download",
                release.Assets.Sources
                    .Where(src => src.Format == "zip")
                    .Select(src => src.URL)
                    .FirstOrDefault());
            json.SafeAdd(Metadata.UpdatedPropertyName, release.ReleasedAt);

            json.Remove("$kref");

            return new Metadata(json);
        }

        private static readonly ILog       log = LogManager.GetLogger(typeof(GitlabTransformer));
        private        readonly IGitlabApi api;
    }
}
