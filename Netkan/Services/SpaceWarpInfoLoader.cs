using System;

using Newtonsoft.Json;

using CKAN.SpaceWarp;
using CKAN.NetKAN.Sources.Github;

namespace CKAN.NetKAN.Services
{
    internal sealed class SpaceWarpInfoLoader : ISpaceWarpInfoLoader
    {
        public SpaceWarpInfoLoader(IHttpService http, IGithubApi github)
        {
            this.http   = http;
            this.github = github;
        }

        public SpaceWarpInfo? Load(string spaceWarpInfo)
            => ParseSpaceWarpJson(spaceWarpInfo) is SpaceWarpInfo swinfo
                   ? CheckRemote(swinfo)
                   : null;

        private SpaceWarpInfo CheckRemote(SpaceWarpInfo swinfo)
        {
            try
            {
                return swinfo.version_check is Uri url
                       && Uri.IsWellFormedUriString(url.OriginalString, UriKind.Absolute)
                       && ParseSpaceWarpJson(github.DownloadText(url)
                                             ?? http.DownloadText(url))
                          is SpaceWarpInfo remoteInfo
                       && swinfo.version == remoteInfo.version
                           ? remoteInfo
                           : swinfo;
            }
            catch (Exception exc)
            {
                throw new Kraken($"Error fetching remote swinfo {swinfo.version_check}: {exc.Message}");
            }
        }

        private static SpaceWarpInfo? ParseSpaceWarpJson(string? json)
            => json == null ? null
                            : JsonConvert.DeserializeObject<SpaceWarpInfo>(json, ignoreJsonErrors);

        private readonly IHttpService http;
        private readonly IGithubApi   github;

        private static readonly JsonSerializerSettings ignoreJsonErrors = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Error = (sender, e) => e.ErrorContext.Handled = true
        };
    }
}
