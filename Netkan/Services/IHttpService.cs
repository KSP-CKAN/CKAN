using System;

namespace CKAN.NetKAN.Services
{
    internal interface IHttpService
    {
        string DownloadPackage(Uri url, string identifier, DateTime? updated);
        string DownloadText(Uri url);
    }
}
