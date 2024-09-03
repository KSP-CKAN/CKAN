using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using CKAN.Versioning;

namespace CKAN.NetKAN.Sources.Github
{
    internal sealed class GithubRelease
    {
        public readonly string?                   Author;
        public readonly ModuleVersion?            Tag;
        public readonly List<GithubReleaseAsset>? Assets;
        public readonly bool                      PreRelease;
        public readonly DateTime?                 PublishedAt;

        public GithubRelease(GithubRef reference, JToken json)
        {
            PreRelease  = (bool?)json["prerelease"] ?? false;
            Tag         = (string?)json["tag_name"] is string s ? new ModuleVersion(s) : null;
            if (Tag == null)
            {
                throw new Kraken("GitHub release missing tag!");
            }
            Author      = (string?)json["author"]?["login"];
            PublishedAt = (DateTime?)json["published_at"];
            Assets      = reference.UseSourceArchive
                          && (string?)json["zipball_url"] is string sourceZIP
                ? new List<GithubReleaseAsset> {
                      new GithubReleaseAsset(Tag.ToString(), new Uri(sourceZIP), PublishedAt),
                  }
                : (JArray?)json["assets"] is JArray assets
                  && reference.Filter != null
                      ? assets.Select(asset => (string?)asset["name"] is string name
                                            && reference.Filter.IsMatch(name)
                                            && (string?)asset["browser_download_url"] is string url
                                            && (DateTime?)asset["updated_at"] is DateTime updated
                                                ? new GithubReleaseAsset(name, new Uri(url), updated)
                                                : null)
                              .OfType<GithubReleaseAsset>()
                              .ToList()
                      : new List<GithubReleaseAsset>();
        }

        public GithubRelease(string author, ModuleVersion tag, List<GithubReleaseAsset> assets)
        {
            Author = author;
            Tag    = tag;
            Assets = assets;
        }
    }
}
