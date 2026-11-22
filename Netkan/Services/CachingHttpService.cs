using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

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
            => DownloadModule(metadata, null)?.Path;

        public DownloadResult? DownloadModule(Metadata         metadata,
                                              DownloadOptions? options)
        {
            if (metadata.Download is not { Count: > 0 })
            {
                return null;
            }
            try
            {
                return DownloadPackage(metadata.Download[0],
                                       metadata.Identifier,
                                       metadata.ReleaseDate,
                                       options);
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
                    // Avoid passing on download options, they are for the
                    // primary URL and don't necessarily apply here.
                    var result = DownloadPackage(fallback,
                                                 metadata.Identifier,
                                                 metadata.ReleaseDate,
                                                 null,
                                                 metadata.Download[0]);
                    return new DownloadResult(result.Path);
                }
            }
        }

        private DownloadResult DownloadPackage(Uri              url,
                                               string           identifier,
                                               DateTime?        updated,
                                               DownloadOptions? options,
                                               Uri?             primaryUrl = null)
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
            if (string.IsNullOrWhiteSpace(cachedFile))
            {
                cachedFile = null;
            }

            // Without options, a cached copy is good enough and we don't ask the server
            if (options == null && cachedFile != null)
            {
                return new DownloadResult(cachedFile);
            }

            // Keep a cache so we don't make repeated conditional requests for every
            // cache use.
            var run = _runResults.GetOrAdd(url, _ => new RunResult());
            lock (run)
            {
                run.Failure?.Throw();

                if (run.Queried)
                {
                    if (options != null && ValidatorsMatch(options, run))
                    {
                        return new DownloadResult(run.Path ?? cachedFile, true,
                                                  run.ETag, run.LastModified);
                    }
                    if (run.Path != null && File.Exists(run.Path))
                    {
                        // Downloaded earlier in this run, so it's current
                        return new DownloadResult(run.Path, false,
                                                  run.ETag, run.LastModified);
                    }
                    // Only a 304 so far, and this caller needs the file; ask again
                }

                try
                {
                    var inProgressFile = _cache.GetInProgressFileName(url, $"netkan-{identifier}")
                                               .FullName;
                    var response = DownloadService.Download(url, inProgressFile,
                                                       _userAgent ?? Net.UserAgentString,
                                                       options);
                    run.Queried      = true;
                    run.ETag         = response.ETag;
                    run.LastModified = response.LastModified;
                    if (response.NotModified)
                    {
                        // The caller knows the file hasn't changed. Give them our copy if we
                        // have one, but they may not need it.
                        return new DownloadResult(cachedFile, true,
                                                  response.ETag, response.LastModified);
                    }

                    run.Path = StoreDownload(inProgressFile, url, primaryUrl, identifier);
                    return new DownloadResult(run.Path, false,
                                              response.ETag, response.LastModified);
                }
                catch (Exception exc)
                {
                    // Later callers get the same error (and go to their fallback URL)
                    // instead of trying the server again
                    run.Failure = ExceptionDispatchInfo.Capture(exc);
                    throw;
                }
            }
        }

        /// <summary>
        /// Would the server have answered 304 to these options, given what it sent
        /// us earlier in this run? Same rules as the server: If-None-Match wins over
        /// If-Modified-Since, and ETags are compared weakly.
        /// </summary>
        private static bool ValidatorsMatch(DownloadOptions options, RunResult run)
            => options.ETag != null
                   ? run.ETag != null && StripWeak(options.ETag) == StripWeak(run.ETag)
                   : options.LastModified is DateTime since
                     && run.LastModified is DateTime lastModified
                     && lastModified <= since.ToUniversalTime();

        private static string StripWeak(string etag)
            => etag.StartsWith("W/", StringComparison.Ordinal) ? etag[2..] : etag;

        /// <summary>
        /// What we learned about one URL in the current run
        /// </summary>
        private sealed class RunResult
        {
            public bool                   Queried;
            public string?                ETag;
            public DateTime?              LastModified;
            // Set only if we downloaded the file in this run
            public string?                Path;
            public ExceptionDispatchInfo? Failure;
        }

        private string StoreDownload(string downloadedFile,
                                     Uri    url,
                                     Uri    primaryUrl,
                                     string identifier)
        {
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
            _runResults.Clear();
        }

        public Uri? ResolveRedirect(Uri url)
            => Net.ResolveRedirect(url, _userAgent);

        public bool HasRedirect(Uri url)
            => ResolveRedirect(url) != url;

        private readonly NetFileCache                    _cache;
        private readonly string?                         _userAgent;
        // Microsoft declined to implement ConcurrentHashSet in favor of this
        private readonly ConcurrentDictionary<Uri, byte> _requestedURLs  = new ConcurrentDictionary<Uri, byte>();
        private readonly ConcurrentDictionary<Uri, RunResult> _runResults = new ConcurrentDictionary<Uri, RunResult>();
        private readonly bool                            _overwriteCache = false;
        private readonly IAppCache                       _stringCache    = new CachingService();

        // Re-use string value URLs within 15 minutes
        private static readonly TimeSpan stringCacheLifetime = new TimeSpan(0, 15, 0);

        private static readonly ILog log = LogManager.GetLogger(typeof(CachingHttpService));
    }
}
