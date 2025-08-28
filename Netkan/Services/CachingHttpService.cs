using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using LazyCache;
using log4net;

using CKAN.IO;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Services
{
    [ExcludeFromCodeCoverage]
    internal sealed class CachingHttpService : IHttpService
    {
        public CachingHttpService(NetFileCache cache,
                                  bool         overwrite = false,
                                  string?      userAgent = null)
        {
            _cache          = cache;
            _userAgent      = userAgent;
            _overwriteCache = overwrite;
        }

        public string? DownloadModule(Metadata metadata)
        {
            if (metadata.Download is not { Count: > 0 })
            {
                return null;
            }
            try
            {
                return DownloadPackage(metadata.Download[0],
                                       metadata.Identifier,
                                       metadata.ReleaseDate);
            }
            catch
            {
                var fallback = metadata.FallbackDownload;
                if (fallback == null)
                {
                    throw;
                }
                else
                {
                    log.InfoFormat("Trying fallback URL: {0}", fallback);
                    return DownloadPackage(fallback,
                                           metadata.Identifier,
                                           metadata.ReleaseDate,
                                           metadata.Download[0]);
                }
            }
        }

        private string DownloadPackage(Uri       url,
                                       string    identifier,
                                       DateTime? updated,
                                       Uri?      primaryUrl = null)
        {
            if (primaryUrl == null)
            {
                primaryUrl = url;
            }
            if (_overwriteCache && !_requestedURLs.ContainsKey(url))
            {
                // Discard cached file if command line says so,
                // but only the first time in each run
                _cache.Remove(url);
            }

            // There is no ConcurrentHashSet, so we have to store a null byte as the Value
            _requestedURLs.TryAdd(url, default);

            var cachedFile = _cache.GetCachedFilename(primaryUrl, updated);

            if (cachedFile != null && !string.IsNullOrWhiteSpace(cachedFile))
            {
                return cachedFile;
            }
            else
            {
                var downloadedFile = Net.Download(url, out _, _userAgent,
                                                  _cache.GetInProgressFileName(url,
                                                                               $"netkan-{identifier}")
                                                        .FullName);

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
                        if (!NetModuleCache.ZipValid(downloadedFile, out string? invalidReason, null))
                        {
                            log.Debug($"{url} is not a valid ZIP file: {invalidReason}");
                            File.Delete(downloadedFile);
                            throw new Kraken($"{url} is not a valid ZIP file: {invalidReason}");
                        }
                        extension = "zip";
                        break;
                    default:
                        extension = "ckan-package";
                        break;
                }

                var destName = $"netkan-{identifier}.{extension}";

                try
                {
                    return _cache.Store(primaryUrl, downloadedFile,
                                        destName, move: true);
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

        public string? DownloadText(Uri     url,
                                    string? authToken = null,
                                    string? mimeType  = null)
            => Utilities.WithRethrowInner(() =>
                   _stringCache.GetOrAdd(url.OriginalString,
                                         () => Net.DownloadText(url, _userAgent,
                                                                authToken, mimeType,
                                                                10000),
                                         DateTimeOffset.Now + stringCacheLifetime));

        public IEnumerable<Uri> RequestedURLs => _requestedURLs.Keys;
        public void ClearRequestedURLs()
        {
            _requestedURLs.Clear();
        }

        public Uri? ResolveRedirect(Uri url, string? userAgent)
            => Net.ResolveRedirect(url, userAgent);

        private readonly NetFileCache                    _cache;
        private readonly string?                         _userAgent;
        // Microsoft declined to implement ConcurrentHashSet in favor of this
        private readonly ConcurrentDictionary<Uri, byte> _requestedURLs  = new ConcurrentDictionary<Uri, byte>();
        private readonly bool                            _overwriteCache = false;
        private readonly IAppCache                       _stringCache    = new CachingService();

        // Re-use string value URLs within 15 minutes
        private static readonly TimeSpan stringCacheLifetime = new TimeSpan(0, 15, 0);

        private static readonly ILog log = LogManager.GetLogger(typeof(CachingHttpService));
    }
}
