using System;
using System.Collections.Generic;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Services
{
    internal interface IHttpService
    {
        string DownloadModule(Metadata metadata);
        string DownloadText(Uri url);
        string DownloadText(Uri url, string authToken, string mimeType);

        IEnumerable<Uri> RequestedURLs { get; }
        void ClearRequestedURLs();
    }
}
