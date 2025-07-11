using System.Text.RegularExpressions;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GithubRef : RemoteRef
    {
        public GithubRef(RemoteRef remoteRef, bool useSourceArchive)
            : base(remoteRef)
        {
            if (remoteRef.Id != null
                && Pattern.Match(remoteRef.Id) is Match { Success: true } match)
            {
                Account = match.Groups["account"].Value;
                Project = match.Groups["project"].Value;
                Repository = $"{Account}/{Project}";

                AssetMatch       = RegexFrom(match, "assetMatch")
                                   ?? Constants.DefaultAssetMatchPattern;
                VersionFromAsset = RegexFrom(match, "versionFromAsset");
                UseSourceArchive = useSourceArchive;
            }
            else
            {
                throw new Kraken(string.Format(@"Could not parse reference: ""{0}""", remoteRef));
            }
        }

        private static Regex? RegexFrom(Match match, string groupKey)
            => match.Groups[groupKey] is Group { Success: true } group
                   ? new Regex(group.Value, RegexOptions.Compiled)
                   : null;

        public GithubRef(string owner, string project)
            : base($"#/ckan/github/{owner}/{project}")
        {
            Account          = owner;
            Project          = project;
            Repository       = $"{owner}/{project}";
            AssetMatch       = Constants.DefaultAssetMatchPattern;
            VersionFromAsset = null;
        }

        public string Account          { get; private set; }
        public string Project          { get; private set; }
        public string Repository       { get; private set; }
        public Regex  AssetMatch       { get; private set; }
        public Regex? VersionFromAsset { get; private set; }
        public bool   UseSourceArchive { get; private set; }

        public bool FilterMatches(GithubReleaseAsset asset)
            => asset.Name is string name && (VersionFromAsset?.IsMatch(name)
                                                             ?? AssetMatch.IsMatch(name));

        private static readonly Regex Pattern = new Regex(
            @"^(?<account>[^/]+)/(?<project>[^/]+)(?:(/asset_match/(?<assetMatch>.+))|(/version_from_asset/(?<versionFromAsset>.+)))?$",
            RegexOptions.Compiled);
    }
}
