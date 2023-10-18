using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GithubRelease
    {
        public readonly string                   Author;
        public readonly ModuleVersion            Tag;
        public readonly List<GithubReleaseAsset> Assets;
        public readonly bool                     PreRelease;
        public readonly DateTime?                PublishedAt;

        public GithubRelease(GithubRef reference, JToken json)
        {
            PreRelease  = (bool)json["prerelease"];
            Tag         = new ModuleVersion((string)json["tag_name"]);
            Author      = (string)json["author"]["login"];
            PublishedAt = (DateTime?)json["published_at"];
            Assets      = reference.UseSourceArchive
                ? new List<GithubReleaseAsset> {
                    new GithubReleaseAsset(
                        Tag.ToString(),
                        new Uri((string)json["zipball_url"]),
                        PublishedAt)
                } : ((JArray)json["assets"])
                    .Where(asset => reference.Filter.IsMatch((string)asset["name"]))
                    .Select(asset => new GithubReleaseAsset(
                        (string)asset["name"],
                        new Uri((string)asset["browser_download_url"]),
                        (DateTime?)asset["updated_at"]))
                    .ToList();
        }

        public GithubRelease(string author, ModuleVersion tag, List<GithubReleaseAsset> assets)
        {
            Author = author;
            Tag    = tag;
            Assets = assets;
        }
    }
}
