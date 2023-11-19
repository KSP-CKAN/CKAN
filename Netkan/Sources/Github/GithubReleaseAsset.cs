using System;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubReleaseAsset
    {
        public string    Name     { get; }
        public Uri       Download { get; }
        public DateTime? Updated  { get; }

        public GithubReleaseAsset(string name, Uri download, DateTime? updated)
        {
            Name     = name;
            Download = download;
            Updated  = updated;
        }
    }
}
