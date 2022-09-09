using System.Text.RegularExpressions;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Sources.Gitlab
{
    /// <summary>
    /// Represents a GitLab $kref
    /// </summary>
    internal sealed class GitlabRef : RemoteRef
    {
        /// <summary>
        /// Initialize the GitLab reference
        /// </summary>
        /// <param name="reference">The base $kref object from a netkan</param>
        public GitlabRef(RemoteRef reference)
            : base(reference)
        {
            var match = Pattern.Match(reference.Id);
            if (match.Success)
            {
                Account = match.Groups["account"].Value;
                Project = match.Groups["project"].Value;
            }
            else
            {
                throw new Kraken(string.Format(@"Could not parse reference: ""{0}""", reference));
            }
        }

        /// <summary>
        /// The first part of the "account/project" path from GitLab
        /// </summary>
        public readonly string Account;
        /// <summary>
        /// The second part of the "account/project" path from GitLab
        /// </summary>
        public readonly string Project;

        private static readonly Regex Pattern = new Regex(
            @"^(?<account>[^/]+)/(?<project>[^/]+)$",
            RegexOptions.Compiled);
    }
}
