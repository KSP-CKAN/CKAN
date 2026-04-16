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
        public CkanModuleVersion? Version         { get; protected set; }
        public string?            ReleaseNotes    { get; protected set; }

        public string updaterFilename = $"{Path.GetTempPath()}{Guid.NewGuid()}.exe";
        public string ckanFilename    = $"{Path.GetTempPath()}{Guid.NewGuid()}.exe";

        protected static string ExeName =>
            #if NET10_0_OR_GREATER
            Environment.ProcessPath is string p
            && !p.EndsWith("testhost.exe", StringComparison.OrdinalIgnoreCase)
            && !p.EndsWith("dotnet",       StringComparison.OrdinalIgnoreCase)
                ? new FileInfo(p).Name :
            #endif
            "ckan.exe";

        public abstract IReadOnlyCollection<NetAsyncDownloader.DownloadTarget> Targets { get; }
    }
}
