using System;
using System.IO;
using System.Collections.Generic;

using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Object representing a CKAN release
    /// </summary>
    public abstract class CkanUpdate
    {
        public CkanModuleVersion Version         { get; protected set; }
        public Uri               ReleaseDownload { get; protected set; }
        public long              ReleaseSize     { get; protected set; }
        public Uri               UpdaterDownload { get; protected set; }
        public long              UpdaterSize     { get; protected set; }
        public string            ReleaseNotes    { get; protected set; }

        public string updaterFilename = $"{Path.GetTempPath()}{Guid.NewGuid()}.exe";
        public string ckanFilename    = $"{Path.GetTempPath()}{Guid.NewGuid()}.exe";

        public abstract IList<NetAsyncDownloader.DownloadTarget> Targets { get; }
    }
}
