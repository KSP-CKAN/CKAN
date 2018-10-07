using System;
using System.Collections.Generic;

namespace CKAN.NetKAN.Services
{
    internal interface IHttpService
    {
        string DownloadPackage(Uri url, string identifier, DateTime? updated);
        string DownloadText(Uri url);

        IEnumerable<Uri> RequestedURLs { get; }
    }
}
