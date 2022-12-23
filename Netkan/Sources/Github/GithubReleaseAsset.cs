using System;
using System.Collections.Generic;
using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubReleaseAsset
    {
        public String Name { get; }
        public Uri Download { get; }
        public DateTime? Updated { get; }

        public GithubReleaseAsset(string name, Uri download, DateTime? updated)
        {
            Name = name;
            Download = download;
            Updated = updated;
        }
    }
}
