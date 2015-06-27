using System;

namespace CKAN.NetKAN.Services
{
    internal interface IHttpService : IDisposable
    {
        string DownloadPackage(Uri url, string identifier);
        string DownloadText(Uri url);
    }
}
