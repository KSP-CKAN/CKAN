using System.Collections.Generic;

namespace CKAN.NetKAN.Sources.Gitlab
{
    /// <summary>
    /// Interface for classes providing access to the GitLab API
    /// Allows mocking up in tests
    /// </summary>
    internal interface IGitlabApi
    {
        /// <summary>
        /// Retrieve info about a GitLab project from the API
        /// </summary>
        /// <param name="reference">Specification of which project to retrieve</param>
        /// <returns>A project object</returns>
        GitlabProject GetProject(GitlabRef reference);

        /// <summary>
        /// Retrieve info about a GitLab project's releases from the API
        /// </summary>
        /// <param name="reference">Specification of which project's releases to retrieve</param>
        /// <returns>Sequence of release objects from the API</returns>
        IEnumerable<GitlabRelease> GetAllReleases(GitlabRef reference);
    }
}
