using System;
using System.Linq;
using System.Collections.Generic;

using Autofac;
using Newtonsoft.Json;

using CKAN.Configuration;
using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Represents a CKAN release on GitHub
    /// </summary>
    public class GitHubReleaseCkanUpdate : CkanUpdate
    {
        /// <summary>
        /// Initialize the Object
        /// </summary>
        /// <param name="json">JSON representation of release</param>
        public GitHubReleaseCkanUpdate(GitHubReleaseInfo? releaseJson = null)
        {
            if (releaseJson == null)
            {
                var coreConfig = ServiceLocator.Container.Resolve<IConfiguration>();
                var token = coreConfig.TryGetAuthToken(latestCKANReleaseApiUrl.Host, out string? t)
                    ? t : null;
                releaseJson = Net.DownloadText(latestCKANReleaseApiUrl, token) is string content
                              ? JsonConvert.DeserializeObject<GitHubReleaseInfo>(content)
                              : null;
            }
            if (releaseJson is null
                || releaseJson.tag_name is null
                || releaseJson.name is null
                || releaseJson.body is null
                || releaseJson.assets is null)
            {
                throw new Kraken(Properties.Resources.AutoUpdateNotFetched);
            }

            Version = new CkanModuleVersion(releaseJson.tag_name.ToString(),
                                            releaseJson.name.ToString());
            ReleaseNotes = ExtractReleaseNotes(releaseJson.body.ToString());

            var releaseAsset = releaseJson.assets.First(asset => asset.browser_download_url
                                                                      ?.ToString()
                                                                       .EndsWith("ckan.exe")
                                                                      ?? false);
            var updaterAsset = releaseJson.assets.First(asset => asset.browser_download_url
                                                                      ?.ToString()
                                                                       .EndsWith("AutoUpdater.exe")
                                                                      ?? false);
            if (releaseAsset.browser_download_url is null
                || updaterAsset.browser_download_url is null)
            {
                throw new Kraken(Properties.Resources.AutoUpdateNotFetched);
            }
            ReleaseDownload = releaseAsset.browser_download_url;
            ReleaseSize     = releaseAsset.size;
            UpdaterDownload = updaterAsset.browser_download_url;
            UpdaterSize     = updaterAsset.size;
        }

        public override IList<NetAsyncDownloader.DownloadTarget> Targets => new[]
        {
            new NetAsyncDownloader.DownloadTargetFile(
                UpdaterDownload, updaterFilename, UpdaterSize),
            new NetAsyncDownloader.DownloadTargetFile(
                ReleaseDownload, ckanFilename, ReleaseSize),
        };

        private Uri  ReleaseDownload { get; set; }
        private long ReleaseSize     { get; set; }
        private Uri  UpdaterDownload { get; set; }
        private long UpdaterSize     { get; set; }

        /// <summary>
        /// Extracts release notes from the body of text provided by the github API.
        /// By default this is everything after the first three dashes on a line by
        /// itself, but as a fallback we'll use the whole body if not found.
        /// </summary>
        /// <returns>The release notes.</returns>
        internal static string ExtractReleaseNotes(string releaseBody)
        {
            const string divider = "\r\n---\r\n";
            // Get at most two pieces, the first is the image, the second is the release notes
            string[] notesArray = releaseBody.Split(new string[] { divider }, 2, StringSplitOptions.None);
            return notesArray.Length > 1 ? notesArray[1] : notesArray[0];
        }

        private static readonly Uri latestCKANReleaseApiUrl =
            new Uri("https://api.github.com/repos/KSP-CKAN/CKAN/releases/latest");
    }
}
