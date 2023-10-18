using System.Text.RegularExpressions;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GithubRef : RemoteRef
    {
        private static readonly Regex Pattern = new Regex(
            @"^(?<account>[^/]+)/(?<project>[^/]+)(?:(/asset_match/(?<filter>.+))|(/version_from_asset/(?<versionFromAsset>.+)))?$",
            RegexOptions.Compiled
        );

        public string Account { get; private set; }
        public string Project { get; private set; }
        public string Repository { get; private set; }
        public Regex Filter { get; private set; }
        public Regex VersionFromAsset { get; private set; }
        public bool UseSourceArchive { get; private set; }
        public bool UsePrerelease { get; private set; }

        public GithubRef(string remoteRefToken, bool useSourceArchive, bool usePrerelease)
            : this(new RemoteRef(remoteRefToken), useSourceArchive, usePrerelease) { }

        public GithubRef(RemoteRef remoteRef, bool useSourceArchive, bool usePrerelease)
            : base(remoteRef)
        {
            var match = Pattern.Match(remoteRef.Id);

            if (match.Success)
            {
                Account = match.Groups["account"].Value;
                Project = match.Groups["project"].Value;
                Repository = string.Format("{0}/{1}", Account, Project);

                Filter = match.Groups["filter"].Success ?
                    new Regex(match.Groups["filter"].Value, RegexOptions.Compiled) :
                    Constants.DefaultAssetMatchPattern;

                VersionFromAsset = match.Groups["versionFromAsset"].Success ?
                    new Regex(match.Groups["versionFromAsset"].Value, RegexOptions.Compiled) :
                    null;

                UseSourceArchive = useSourceArchive;
                UsePrerelease = usePrerelease;
            }
            else
            {
                throw new Kraken(string.Format(@"Could not parse reference: ""{0}""", remoteRef));
            }
        }
    }
}
