using System;
using System.Collections.Generic;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Services
{
    internal interface IHttpService
    {
        string? DownloadModule(Metadata metadata);
        DownloadResult? DownloadModule(Metadata metadata, DownloadOptions? options);

        string? DownloadText(Uri url, string? authToken = null, string? mimeType = null);

        Uri? ResolveRedirect(Uri url);
        bool HasRedirect(Uri url);

        IEnumerable<Uri> RequestedURLs { get; }
        void ClearRequestedURLs();
    }
}
