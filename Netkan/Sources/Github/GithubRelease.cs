using System;
using System.Collections.Generic;
using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubRelease
    {
        public string        Author { get; }
        public ModuleVersion Tag    { get; }
        public List<GithubReleaseAsset> Assets { get; }

        public GithubRelease(string author, ModuleVersion tag, List<GithubReleaseAsset> assets)
        {
            Author       = author;
            Tag          = tag;
            Assets       = assets;
        }
    }
}
