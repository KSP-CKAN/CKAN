using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Sources.Gitlab
{
    /// <summary>
    /// Provides convenient access to the GitLab API
    /// https://docs.gitlab.com/ee/api/
    /// </summary>
    internal sealed class GitlabApi : IGitlabApi
    {
        /// <summary>
        /// Initialize the API object
        /// </summary>
        /// <param name="http">HTTP service for getting URLs</param>
        /// <param name="token">GitLab API token</param>
        public GitlabApi(IHttpService http, string token = null)
        {
            this.http  = http;
            this.token = token;
        }

        /// <summary>
        /// Retrieve info about a GitLab project from the API
        /// </summary>
        /// <param name="reference">Specification of which project to retrieve</param>
        /// <returns>A project object</returns>
        public GitlabProject GetProject(GitlabRef reference)
        {
            // https://docs.gitlab.com/ee/api/projects
            return JsonConvert.DeserializeObject<GitlabProject>(
                http.DownloadText(
                    new Uri(apiBase, $"{reference.Account}%2F{reference.Project}"),
                    token, null));
        }

        /// <summary>
        /// Retrieve info about a GitLab project's releases from the API
        /// </summary>
        /// <param name="reference">Specification of which project's releases to retrieve</param>
        /// <returns>Sequence of release objects from the API</returns>
        public IEnumerable<GitlabRelease> GetAllReleases(GitlabRef reference)
        {
            // https://docs.gitlab.com/ee/api/releases/
            return JArray.Parse(
                http.DownloadText(
                    new Uri(apiBase, $"{reference.Account}%2F{reference.Project}/releases"),
                        token, null))
                .Select(releaseJson => releaseJson.ToObject<GitlabRelease>());
        }

        private readonly IHttpService http;
        private readonly string       token;

        private static readonly Uri apiBase = new Uri("https://gitlab.com/api/v4/projects/");
    }
}
