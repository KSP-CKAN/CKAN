using System;
using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubRelease
    {
        public string           Author       { get; }
        public ModuleVersion    Version      { get; }
        public Uri              Download     { get; }
        public DateTime?        AssetUpdated { get; }

        public GithubRelease(string author, ModuleVersion version, Uri download, DateTime? updated)
        {
            Author       = author;
            Version      = version;
            Download     = download;
            AssetUpdated = updated;
        }
    }
}
