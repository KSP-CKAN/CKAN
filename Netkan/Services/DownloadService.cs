using System;
using System.Globalization;
using System.IO;
using System.Net;

using log4net;

// HttpWebRequest is obsolete on net10.0, but it's what the rest of CKAN uses
#pragma warning disable SYSLIB0014

namespace CKAN.NetKAN.Services
{
    internal sealed class DownloadOptions
    {
        public DownloadOptions(string?   etag         = null,
                               DateTime? lastModified = null)
        {
            ETag         = etag;
            LastModified = lastModified;
        }

        public string? ETag { get; }

        public DateTime? LastModified { get; }

        public bool IsConditional => ETag != null || LastModified != null;
    }

    internal sealed class DownloadResult
    {
        public DownloadResult(string?   path,
                              bool      notModified  = false,
                              string?   etag         = null,
                              DateTime? lastModified = null)
        {
            Path         = path;
            NotModified  = notModified;
            ETag         = etag;
            LastModified = lastModified;
        }

        /// <summary>
        /// Path to the downloaded file in the local cache, if present.
        /// </summary>
        public string? Path { get; }

        /// <summary>
        /// True if the server answered 304 Not Modified to a conditional request
        /// </summary>
        public bool NotModified { get; }

        /// <summary>
        /// ETag of the final response (after redirects), as sent by the server.
        /// </summary>
        public string? ETag { get; }

        /// <summary>
        /// Last-Modified time (UTC) of the final response (after redirects).
        /// </summary>
        public DateTime? LastModified { get; }
    }

    internal static class DownloadService
    {
        internal readonly struct Response
        {
            public Response(bool notModified, string? etag, DateTime? lastModified, Uri finalUrl)
            {
                NotModified  = notModified;
                ETag         = etag;
                LastModified = lastModified;
                FinalUrl     = finalUrl;
            }

            public readonly bool      NotModified;
            public readonly string?   ETag;
            public readonly DateTime? LastModified;
            public readonly Uri       FinalUrl;
        }

        /// <summary>
        /// Download <paramref name="url"/> to the path specified by
        /// <paramref name="filename"/>. If <paramref name="options"/> are provided
        /// this may do a conditional request, skip the download, and return a
        /// response with <see cref="Response.NotModified" /> set to true.
        /// </summary>
        public static Response Download(Uri              url,
                                        string           filename,
                                        string           userAgent,
                                        DownloadOptions? options      = null,
                                        int              timeout      = 100000,
                                        int              maxRedirects = 10)
        {
            var current = url;
            for (int redirects = 0; ; ++redirects)
            {
                var request = CreateRequest(current, userAgent, options, timeout);
                using var response = GetResponse(request, url);

                var status = (int)response.StatusCode;
                if (IsRedirect(status))
                {
                    var location = response.Headers[HttpResponseHeader.Location];
                    if (string.IsNullOrEmpty(location))
                    {
                        throw new Kraken($"Download of {url} failed: {current} returned {status} with no Location header");
                    }

                    if (redirects >= maxRedirects)
                    {
                        throw new Kraken($"Download of {url} failed: more than {maxRedirects} redirects");
                    }

                    // Location may be relative to the current URL
                    var next = new Uri(current, location);
                    log.DebugFormat("Redirected {0} to {1}", current, next);
                    current = next;
                    continue;
                }

                var etag         = response.Headers[HttpResponseHeader.ETag];
                var lastModified = ParseHttpDate(response.Headers[HttpResponseHeader.LastModified]);

                if (status == (int)HttpStatusCode.NotModified)
                {
                    log.DebugFormat("{0} not modified", url);
                    // A 304 should repeat the validators, but fall back to the ones we sent
                    return new Response(true,
                                        etag         ?? options?.ETag,
                                        lastModified ?? options?.LastModified?.ToUniversalTime(),
                                        current);
                }

                if (status is < 200 or >= 300)
                {
                    throw new Kraken($"Download of {url} failed: {current} returned {status} {response.StatusDescription}");
                }

                log.DebugFormat("Downloading {0} to {1}", current, filename);
                try
                {
                    using var body = response.GetResponseStream();
                    using var file = File.Create(filename);

                    body.CopyTo(file);
                }
                catch (Exception exc)
                {
                    TryDelete(filename);
                    if (exc is WebException { Status: WebExceptionStatus.Timeout } wex)
                    {
                        throw new RequestTimedOutKraken(url, wex);
                    }
                    throw;
                }
                return new Response(false, etag, lastModified, current);
            }
        }

        private static HttpWebRequest CreateRequest(Uri              url,
                                                    string           userAgent,
                                                    DownloadOptions? options,
                                                    int              timeout)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method            = "GET";
            request.UserAgent         = userAgent;
            request.AllowAutoRedirect = false;
            request.Timeout           = timeout;

            if (options?.ETag is string etag)
            {
                request.Headers[HttpRequestHeader.IfNoneMatch] = etag;
            }

            if (options?.LastModified is DateTime lastModified)
            {
                request.IfModifiedSince = lastModified.ToUniversalTime();
            }

            return request;
        }

        private static HttpWebResponse GetResponse(HttpWebRequest request, Uri originalUrl)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            when (wex is { Status: WebExceptionStatus.ProtocolError,
                           Response: HttpWebResponse response })
            {
                return response;
            }
            catch (WebException wex)
            when (wex.Status == WebExceptionStatus.SecureChannelFailure)
            {
                throw new MissingCertificateKraken(originalUrl, null, wex);
            }
            catch (WebException wex)
            when (wex.Status == WebExceptionStatus.Timeout)
            {
                throw new RequestTimedOutKraken(originalUrl, wex);
            }
        }

        private static bool IsRedirect(int status)
            => status is 301 or 302 or 303 or 307 or 308;

        private static DateTime? ParseHttpDate(string? value)
            => !string.IsNullOrEmpty(value)
               && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeUniversal,
                                          out DateTimeOffset parsed)
                   ? parsed.UtcDateTime
                   : null;

        private static void TryDelete(string filename)
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception e)
            {
                // Not much we can do
                log.Debug($"Failed to delete {filename}: {e}");
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadService));
    }
}
