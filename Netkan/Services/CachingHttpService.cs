﻿using System;
using System.Collections.Generic;
using log4net;

namespace CKAN.NetKAN.Services
{
    internal sealed class CachingHttpService : IHttpService
    {
        private readonly NetFileCache _cache;
        private          HashSet<Uri> _requestedURLs  = new HashSet<Uri>();
        private          bool         _overwriteCache = false;

        public CachingHttpService(NetFileCache cache, bool overwrite = false)
        {
            _cache          = cache;
            _overwriteCache = overwrite;
        }

        public string DownloadPackage(Uri url, string identifier, DateTime? updated)
        {
            if (_overwriteCache && !_requestedURLs.Contains(url))
            {
                // Discard cached file if command line says so,
                // but only the first time in each run
                _cache.Remove(url);
            }

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
                            log.Debug($"{downloadedFile} is not a valid ZIP file: {invalidReason}");
                            throw new Kraken($"{url} is not a valid ZIP file: {invalidReason}");
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
        public string DownloadText(Uri url, string authToken)
        {
            return Net.DownloadText(url, authToken);
        }

        public IEnumerable<Uri> RequestedURLs { get { return _requestedURLs; } }
        public void ClearRequestedURLs()
        {
            _requestedURLs?.Clear();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CachingHttpService));
    }
}
