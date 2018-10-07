using System;
using System.Collections.Generic;

namespace CKAN.NetKAN.Services
{
    internal sealed class CachingHttpService : IHttpService
    {
        private readonly NetFileCache _cache;
        private          HashSet<Uri> _requestedURLs = new HashSet<Uri>();

        public CachingHttpService(NetFileCache cache)
        {
            _cache = cache;
        }

        public string DownloadPackage(Uri url, string identifier, DateTime? updated)
        {
            _requestedURLs.Add(url);

            var cachedFile = _cache.GetCachedFilename(url, updated);

            if (!string.IsNullOrWhiteSpace(cachedFile))
            {
                return cachedFile;
            }
            else
            {
                var downloadedFile = Net.Download(url);

                string extension;

                switch (FileIdentifier.IdentifyFile(downloadedFile))
                {
                    case FileType.ASCII:
                        extension = "txt";
                        break;
                    case FileType.GZip:
                        extension = "gz";
                        break;
                    case FileType.Tar:
                        extension = "tar";
                        break;
                    case FileType.TarGz:
                        extension = "tar.gz";
                        break;
                    case FileType.Zip:
                        extension = "zip";
                        string invalidReason;
                        if (!NetFileCache.ZipValid(downloadedFile, out invalidReason))
                        {
                            throw new Kraken($"{downloadedFile} is not a valid ZIP file: {invalidReason}");
                        }
                        break;
                    default:
                        extension = "ckan-package";
                        break;
                }

                return _cache.Store(
                    url,
                    downloadedFile,
                    string.Format("netkan-{0}.{1}", identifier, extension),
                    move: true
                );
            }
        }

        public string DownloadText(Uri url)
        {
            return Net.DownloadText(url);
        }

        public IEnumerable<Uri> RequestedURLs { get { return _requestedURLs; } }

    }
}
