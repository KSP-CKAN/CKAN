using System;
using System.Collections.Generic;

namespace CKAN
{
    public class GitHubReleaseInfo
    {
        public string      tag_name;
        public string      name;
        public string      body;
        public List<Asset> assets;

        public sealed class Asset
        {
            public Uri  browser_download_url;
            public long size;
        }
    }
}
