using System;

namespace CKAN.NetKAN.Sources.Github
{
    public sealed class GithubRelease
    {
        public string Author { get; private set; }
        public Version Version { get; private set; }
        public Uri Download { get; private set; }

        public GithubRelease(string author, Version version, Uri download)
        {
            Author = author;
            Version = version;
            Download = download;
        }
    }
}
