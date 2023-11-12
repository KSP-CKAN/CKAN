using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Services
{
    internal sealed class CachingHttpService : IHttpService
    {
        private readonly NetFileCache _cache;
        private readonly HashSet<Uri> _requestedURLs  = new HashSet<Uri>();
        private readonly bool         _overwriteCache = false;
        private readonly Dictionary<Uri, StringCacheEntry> _stringCache = new Dictionary<Uri, StringCacheEntry>();

        // Re-use string value URLs within 15 minutes
        private static readonly TimeSpan stringCacheLifetime = new TimeSpan(0, 15, 0);

        public CachingHttpService(NetFileCache cache, bool overwrite = false)
        {
            _cache          = cache;
            _overwriteCache = overwrite;
        }

        public string DownloadModule(Metadata metadata)
        {
            try
            {
                return DownloadPackage(metadata.Download, metadata.Identifier, metadata.RemoteTimestamp);
            }
            catch (Exception)
            {
                var fallback = metadata.FallbackDownload;
                if (fallback == null)
                {
                    throw;
                }
                else
                {
                    log.InfoFormat("Trying fallback URL: {0}", fallback);
                    return DownloadPackage(fallback, metadata.Identifier, metadata.RemoteTimestamp, metadata.Download);
                }
            }
        }

        private string DownloadPackage(Uri url, string identifier, DateTime? updated, Uri primaryUrl = null)
        {
            if (primaryUrl == null)
            {
                primaryUrl = url;
            }
            if (_overwriteCache && !_requestedURLs.Contains(url))
            {
                // Discard cached file if command line says so,
                // but only the first time in each run
                _cache.Remove(url);
            }

            _requestedURLs.Add(url);

            var cachedFile = _cache.GetCachedFilename(primaryUrl, updated);

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
                        if (!NetModuleCache.ZipValid(downloadedFile, out invalidReason, null))
                        {
                            log.Debug($"{url} is not a valid ZIP file: {invalidReason}");
                            File.Delete(downloadedFile);
                            throw new Kraken($"{url} is not a valid ZIP file: {invalidReason}");
                        }
                        break;
                    default:
                        extension = "ckan-package";
                        break;
                }

                try
                {
                    return _cache.Store(
                        primaryUrl,
                        downloadedFile,
                        $"netkan-{identifier}.{extension}",
                        move: true
                    );
                }
                catch (IOException exc)
                {
                    // If cache is full, don't also fill /tmp
                    log.Debug($"Failed to store to cache: {exc.Message}");
                    File.Delete(downloadedFile);
                    throw;
                }
            }
        }

        public string DownloadText(Uri url)
        {
            return TryGetCached(url, () => Net.DownloadText(url, timeout:10000));
        }
        public string DownloadText(Uri url, string authToken, string mimeType = null)
        {
            return TryGetCached(url, () => Net.DownloadText(url, authToken, mimeType, 10000));
        }

        private string TryGetCached(Uri url, Func<string> uncached)
        {
            if (_stringCache.TryGetValue(url, out StringCacheEntry entry))
            {
                if (DateTime.Now - entry.Timestamp < stringCacheLifetime)
                {
                    // Re-use recent cached request of this URL
                    return entry.Value;
                }
                else
                {
                    // Too old, purge it
                    _stringCache.Remove(url);
                }
            }
            string val = uncached();
            _stringCache.Add(url, new StringCacheEntry()
            {
                Value     = val,
                Timestamp = DateTime.Now
            });
            return val;
        }

        public IEnumerable<Uri> RequestedURLs => _requestedURLs;
        public void ClearRequestedURLs()
        {
            _requestedURLs?.Clear();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CachingHttpService));
    }

    public class StringCacheEntry
    {
        public string   Value;
        public DateTime Timestamp;
    }

}
