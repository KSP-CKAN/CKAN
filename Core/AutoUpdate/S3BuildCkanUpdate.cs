using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using CKAN.Versioning;

namespace CKAN
{
    public class S3BuildCkanUpdate : CkanUpdate
    {
        public S3BuildCkanUpdate(S3BuildVersionInfo? versionJson = null, string? userAgent = null)
        {
            versionJson ??= Net.DownloadText(new Uri(S3BaseUrl, VersionJsonUrlPiece), userAgent) is string content
                                ? JsonConvert.DeserializeObject<S3BuildVersionInfo>(content)
                                : null;
            if (versionJson is null || versionJson.version is null)
            {
                throw new Kraken(Properties.Resources.AutoUpdateNotFetched);
            }
            Version         = new CkanModuleVersion(versionJson.version.ToString(), "dev");
            ReleaseNotes    = versionJson.changelog;
            UpdaterDownload = new Uri(S3BaseUrl, AutoUpdaterUrlPiece);
            ReleaseDownload = new Uri(S3BaseUrl, CkanUrlPiece);
        }

        public override IReadOnlyCollection<NetAsyncDownloader.DownloadTarget> Targets => new[]
        {
            new NetAsyncDownloader.DownloadTargetFile(UpdaterDownload, updaterFilename),
            new NetAsyncDownloader.DownloadTargetFile(ReleaseDownload, ckanFilename),
        };

        private Uri ReleaseDownload { get; set; }
        private Uri UpdaterDownload { get; set; }

        private static readonly Uri S3BaseUrl =
            new Uri("https://ksp-ckan.s3-us-west-2.amazonaws.com/");
        private const           string VersionJsonUrlPiece = "version.json";
        private const           string AutoUpdaterUrlPiece = "AutoUpdater.exe";
        private const           string CkanUrlPiece        = "ckan.exe";
    }
}
