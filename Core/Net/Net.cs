using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;

using Autofac;
using ChinhDo.Transactions.FileManager;
using log4net;

using CKAN.Configuration;

namespace CKAN
{
    /// <summary>
    /// Doing something with the network? Do it here.
    /// </summary>

    public static class Net
    {
        // The user agent that we report to web sites
        public static string UserAgentString = "Mozilla/4.0 (compatible; CKAN)";

        private const int MaxRetries             = 3;
        private const int RetryDelayMilliseconds = 100;

        private static readonly ILog log = LogManager.GetLogger(typeof(Net));

        public static readonly Dictionary<string, Uri> ThrottledHosts = new Dictionary<string, Uri>()
        {
            {
                "api.github.com",
                new Uri(HelpURLs.AuthTokens)
            }
        };

        /// <summary>
        /// Make a HEAD request to get the ETag of a URL without downloading it
        /// </summary>
        /// <param name="url">Remote URL to check</param>
        /// <returns>
        /// ETag value of the URL if any, otherwise null, see
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag
        /// </returns>
        public static string CurrentETag(Uri url)
            => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url),
                                    HttpCompletionOption.ResponseHeadersRead)
                         .Result
                         .Headers.ETag.ToString()
                         .Replace("\"", "");

        /// <summary>
        /// "HttpClient is intended to be instantiated once and reused
        ///  throughout the life of an application. In .NET Core and .NET 5+,
        ///  HttpClient pools connections inside the handler instance and
        ///  reuses a connection across multiple requests. If you instantiate
        ///  an HttpClient class for every request, the number of sockets
        ///  available under heavy loads will be exhausted. This exhaustion
        ///  will result in SocketException errors."
        /// </summary>
        public static readonly HttpClient httpClient = new HttpClient();
        public static readonly HttpClient nonRedirectingHttpClient = new HttpClient(
            new HttpClientHandler()
            {
                AllowAutoRedirect = false,
            });

        /// <summary>
        /// Downloads the specified url, and stores it in the filename given.
        /// If no filename is supplied, a temporary file will be generated.
        /// Returns the filename the file was saved to on success.
        /// Throws an exception on failure.
        /// Throws a MissingCertificateException *and* prints a message to the
        /// console if we detect missing certificates (common on a fresh Linux/mono install)
        /// </summary>
        public static string Download(Uri url, out string etag, string filename = null, IUser user = null)
            => Download(url.OriginalString, out etag, filename, user);

        public static string Download(Uri url, string filename = null, IUser user = null)
            => Download(url, out _, filename, user);

        public static string Download(string url, string filename = null, IUser user = null)
            => Download(url, out _, filename, user);

        public static string Download(string url, out string etag, string filename = null, IUser user = null)
        {
            TxFileManager FileTransaction = new TxFileManager();

            user = user ?? new NullUser();
            user.RaiseMessage(Properties.Resources.NetDownloading, url);

            // Generate a temporary file if none is provided.
            if (filename == null)
            {
                filename = FileTransaction.GetTempFileName();
            }

            log.DebugFormat("Downloading {0} to {1}", url, filename);

            try
            {
                // This WebClient child class does some complicated stuff, let's keep using it for now
                #pragma warning disable SYSLIB0014
                var agent = new RedirectingTimeoutWebClient();
                #pragma warning restore SYSLIB0014
                agent.DownloadFile(url, filename);
                etag = agent.ResponseHeaders.Get("ETag")?.Replace("\"", "");
            }
            catch (Exception exc)
            {
                var wexc = exc as WebException;
                if (wexc?.Status == WebExceptionStatus.ProtocolError)
                {
                    // Get redirect if redirected.
                    // This is needed when redirecting from HTTPS to HTTP on .NET Core.
                    var response = wexc.Response as HttpWebResponse;
                    if (response?.StatusCode == HttpStatusCode.Redirect)
                    {
                        return Download(response.GetResponseHeader("Location"), out etag, filename, user);
                    }
                    // Otherwise it's a valid failure from the server (probably a 404), keep it
                }

                // Clean up our file, it's unlikely to be complete.
                // We do this even though we're using transactional files, as we may not be in a transaction.
                // It's okay if this fails.
                try
                {
                    log.DebugFormat("Removing {0} after web error failure", filename);
                    FileTransaction.Delete(filename);
                }
                catch
                {
                    // Apparently we need a catch, even if we do nothing.
                }

                // Look for an exception regarding the authentication.
                if (Regex.IsMatch(exc.ToString(), "The authentication or decryption has failed."))
                {
                    throw new MissingCertificateKraken(
                        string.Format(Properties.Resources.NetMissingCertFailed, url),
                        exc);
                }

                // Not the exception we were looking for! Throw it further upwards!
                throw;
            }

            return filename;
        }

        public class DownloadTarget
        {
            public List<Uri> urls     { get; private set; }
            public string    filename { get; private set; }
            public long      size     { get; set; }
            public string    mimeType { get; private set; }

            public DownloadTarget(List<Uri> urls, string filename = null, long size = 0, string mimeType = "")
            {
                TxFileManager FileTransaction = new TxFileManager();

                this.urls        = urls;
                this.filename    = string.IsNullOrEmpty(filename)
                    ? FileTransaction.GetTempFileName()
                    : filename;
                this.size        = size;
                this.mimeType    = mimeType;
            }
        }

        public static string DownloadWithProgress(string url, string filename = null, IUser user = null)
            => DownloadWithProgress(new Uri(url), filename, user);

        public static string DownloadWithProgress(Uri url, string filename = null, IUser user = null)
        {
            var targets = new[] {
                new DownloadTarget(new List<Uri> { url }, filename)
            };
            DownloadWithProgress(targets, user);
            return targets.First().filename;
        }

        public static void DownloadWithProgress(IList<DownloadTarget> downloadTargets, IUser user = null)
        {
            var downloader = new NetAsyncDownloader(user ?? new NullUser());
            downloader.onOneCompleted += (url, filename, error, etag) =>
            {
                if (error != null)
                {
                    user?.RaiseError(error.ToString());
                }
                else
                {
                    File.Move(filename, downloadTargets.First(p => p.urls.Contains(url)).filename);
                }
            };
            downloader.DownloadAndWait(downloadTargets);
        }

        /// <summary>
        /// Download a string from a URL
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="authToken">An authentication token sent with the "Authorization" header.
        ///                         Attempted to be looked up from the configuraiton if not specified</param>
        /// <param name="mimeType">A mime type sent with the "Accept" header</param>
        /// <param name="timeout">Timeout for the request in milliseconds, defaulting to 100 000 (=100 seconds)</param>
        /// <returns>The text content returned by the server</returns>
        public static string DownloadText(Uri url, string authToken = "", string mimeType = null, int timeout = 100000)
        {
            log.DebugFormat("About to download {0}", url.OriginalString);

            // This WebClient child class does some complicated stuff, let's keep using it for now
            #pragma warning disable SYSLIB0014
            WebClient agent = new RedirectingTimeoutWebClient(timeout, mimeType);
            #pragma warning restore SYSLIB0014

            // Check whether to use an auth token for this host
            if (!string.IsNullOrEmpty(authToken)
                || (ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(url.Host, out authToken)
                    && !string.IsNullOrEmpty(authToken)))
            {
                log.InfoFormat("Using auth token for {0}", url.Host);
                // Send our auth token to the GitHub API (or whoever else needs one)
                agent.Headers.Add("Authorization", $"token {authToken}");
            }

            for (int whichAttempt = 0; whichAttempt < MaxRetries + 1; ++whichAttempt)
            {
                try
                {
                    string content = agent.DownloadString(url.OriginalString);
                    string header  = agent.ResponseHeaders.ToString();

                    log.DebugFormat("Response from {0}:\r\n\r\n{1}\r\n{2}", url, header, content);

                    return content;
                }
                catch (WebException wex) when (wex.Status != WebExceptionStatus.ProtocolError && whichAttempt < MaxRetries)
                {
                    log.DebugFormat("Web request failed with non-protocol error, retrying in {0} milliseconds: {1}", RetryDelayMilliseconds * whichAttempt, wex.Message);
                    Thread.Sleep(RetryDelayMilliseconds * whichAttempt);
                }
            }
            // Should never get here, because we don't catch any exceptions
            // in the final iteration of the above for loop. They should be
            // thrown to the calling code, or the call should succeed.
            return null;
        }

        public static Uri ResolveRedirect(Uri url)
        {
            const int maxRedirects = 6;
            for (int redirects = 0; redirects <= maxRedirects; ++redirects)
            {
                var req = new HttpRequestMessage(HttpMethod.Head, url);
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.Add(new ProductInfoHeaderValue(UserAgentString));
                var response = nonRedirectingHttpClient.SendAsync(
                    req, HttpCompletionOption.ResponseHeadersRead).Result;
                if (response.Headers.Location == null)
                {
                    return url;
                }

                var location = response.Headers.Location.ToString();
                if (Uri.IsWellFormedUriString(location, UriKind.Absolute))
                {
                    url = response.Headers.Location;
                }
                else if (Uri.IsWellFormedUriString(location, UriKind.Relative))
                {
                    url = new Uri(url, response.Headers.Location);
                    log.DebugFormat("Relative URL {0} is absolute URL {1}", location, url);
                }
                else
                {
                    throw new Kraken(string.Format(Properties.Resources.NetInvalidLocation, location));
                }
            }
            return null;
        }

        /// <summary>
        /// Translate a URL into a form that returns the raw contents of a file
        /// Only changes GitHub URLs, others are left as-is
        /// </summary>
        /// <param name="remoteUri">URL to handle</param>
        /// <returns>
        /// URL pointing to the raw contents of the input URL
        /// </returns>
        public static Uri GetRawUri(Uri remoteUri)
        {
            // Authors may use the URI of the GitHub file page instead of the URL to the actual raw file.
            // Detect that case and automatically transform the remote URL to one we can use.
            // This is hacky and fragile but it's basically what KSP-AVC itself does in its
            // FormatCompatibleUrl(string) method so we have to go along with the flow:
            // https://github.com/CYBUTEK/KSPAddonVersionChecker/blob/ff94000144a666c8ff637c71b802e1baee9c15cd/KSP-AVC/AddonInfo.cs#L199
            // However, this implementation is more robust as it actually parses the URI rather than doing
            // basic string replacements.
            if (string.Compare(remoteUri.Host, "github.com", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // We expect a non-raw URI to be in one of two forms:
                //  1. https://github.com/<USER>/<PROJECT>/blob/<BRANCH>/<PATH>
                //  2. https://github.com/<USER>/<PROJECT>/tree/<BRANCH>/<PATH>
                //
                // Therefore, we expect at least six segments in the path:
                //  1. "/"
                //  2. "<USER>/"
                //  3. "<PROJECT>/"
                //  4. "blob/" or "tree/"
                //  5. "<BRANCH>/"
                //  6+. "<PATH>"
                //
                // And that the fourth segment (index 3) is either "blob/" or "tree/"

                var remoteUriBuilder = new UriBuilder(remoteUri)
                {
                    // Replace host with raw host
                    Host = "raw.githubusercontent.com"
                };

                // Check that the path is what we expect
                var segments = remoteUri.Segments.ToList();

                if (segments.Count >= 4
                    && string.Compare(segments[3], "raw/", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    log.InfoFormat("Remote GitHub URL is in raw format, using as is.");
                    return remoteUri;
                }
                if (segments.Count < 6
                    || (string.Compare(segments[3], "blob/", StringComparison.OrdinalIgnoreCase) != 0
                        && string.Compare(segments[3], "tree/", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    log.WarnFormat("Remote non-raw GitHub URL is in an unknown format, using as is.");
                    return remoteUri;
                }

                // Remove "blob/" or "tree/" segment from raw URI
                segments.RemoveAt(3);
                remoteUriBuilder.Path = string.Join("", segments);

                log.InfoFormat("Canonicalized non-raw GitHub URL to: {0}", remoteUriBuilder.Uri);

                return remoteUriBuilder.Uri;
            }
            else
            {
                return remoteUri;
            }
        }
    }
}
