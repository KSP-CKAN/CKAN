using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using Autofac;
using ChinhDo.Transactions.FileManager;
using log4net;

using CKAN.Extensions;
using CKAN.Configuration;

namespace CKAN
{
    /// <summary>
    /// Doing something with the network? Do it here.
    /// </summary>

    public static class Net
    {
        /// <summary>
        /// Make a HEAD request to get the ETag of a URL without downloading it
        /// </summary>
        /// <param name="url">Remote URL to check</param>
        /// <returns>
        /// ETag value of the URL if any, otherwise null, see
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag
        /// </returns>
        public static string? CurrentETag(Uri url)
        {
            // HttpClient apparently is worse than what it was supposed to replace
            #pragma warning disable SYSLIB0014
            WebRequest req = WebRequest.Create(url);
            #pragma warning restore SYSLIB0014
            req.Method = "HEAD";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                var val = resp.Headers["ETag"]?.Replace("\"", "");
                resp.Close();
                return val;
            }
            catch (WebException exc)
            {
                // Let the calling code keep going to get the actual problem
                log.Debug($"Failed to get ETag from {url}", exc);
                return null;
            }
        }

        /// <summary>
        /// Downloads the specified url, and stores it in the filename given.
        /// If no filename is supplied, a temporary file will be generated.
        /// Returns the filename the file was saved to on success.
        /// Throws an exception on failure.
        /// Throws a MissingCertificateException *and* prints a message to the
        /// console if we detect missing certificates (common on a fresh Linux/mono install)
        /// </summary>
        public static string Download(Uri         url,
                                      out string? etag,
                                      string?     userAgent = null,
                                      string?     filename  = null,
                                      IUser?      user      = null)
        {
            user?.RaiseMessage(Properties.Resources.NetDownloading, url);
            var FileTransaction = new TxFileManager();

            // Generate a temporary file if none is provided.
            filename ??= FileTransaction.GetTempFileName();

            log.DebugFormat("Downloading {0} to {1}", url, filename);

            try
            {
                // This WebClient child class does some complicated stuff, let's keep using it for now
                #pragma warning disable SYSLIB0014
                var agent = new RedirectingTimeoutWebClient(userAgent ?? UserAgentString);
                #pragma warning restore SYSLIB0014
                agent.DownloadFile(url, filename);
                etag = agent.ResponseHeaders?.Get("ETag")?.Replace("\"", "");
            }
            catch (WebException wex)
            when (wex is
                  {
                      Status:   WebExceptionStatus.ProtocolError,
                      Response: HttpWebResponse
                                {
                                    StatusCode: HttpStatusCode.Redirect
                                }
                                response,
                  })
            {
                // Get redirect if redirected.
                // This is needed when redirecting from HTTPS to HTTP on .NET Core.
                var loc = new Uri(response.GetResponseHeader("Location"));
                log.InfoFormat("Redirected to {0}", loc);
                return Download(loc, out etag, userAgent, filename, user);
            }
            catch (WebException wex)
            when (wex is { Status: WebExceptionStatus.SecureChannelFailure })
            {
                throw new MissingCertificateKraken(url, null, wex);
            }
            catch (WebException wex)
            when (wex is { InnerException: Exception inner })
            {
                throw new DownloadErrorsKraken(new NetAsyncDownloader.DownloadTargetFile(url),
                                               inner);
            }
            // Otherwise it's a valid failure from the server (probably a 404), keep it
            catch
            {
                try
                {
                    // Clean up our file, it's unlikely to be complete.
                    // We do this even though we're using transactional files, as we may not be in a transaction.
                    log.DebugFormat("Removing {0} after web error failure", filename);
                    FileTransaction.Delete(filename);
                }
                catch
                {
                    // It's okay if this fails.
                }
                throw;
            }

            return filename;
        }

        /// <summary>
        /// Download a string from a URL
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="userAgent">User agent to send with the request</param>
        /// <param name="authToken">An authentication token sent with the "Authorization" header.
        ///                         Attempted to be looked up from the configuraiton if not specified</param>
        /// <param name="mimeType">A mime type sent with the "Accept" header</param>
        /// <param name="timeout">Timeout for the request in milliseconds, defaulting to 100 000 (=100 seconds)</param>
        /// <returns>The text content returned by the server</returns>
        public static string? DownloadText(Uri     url,
                                           string? userAgent = null,
                                           string? authToken = "",
                                           string? mimeType = null,
                                           int     timeout = 100000)
        {
            log.DebugFormat("About to download {0}", url.OriginalString);

            #pragma warning disable SYSLIB0014
            WebClient agent = new RedirectingTimeoutWebClient(userAgent ?? UserAgentString,
                                                              timeout, mimeType ?? "");
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
                    var content = Utilities.WithRethrowInner(() => agent.DownloadString(url));
                    var header  = agent.ResponseHeaders?.ToString();

                    log.DebugFormat("Response from {0}:\r\n\r\n{1}\r\n{2}", url, header, content);

                    return content;
                }
                catch (WebException wex)
                when (wex.Status == WebExceptionStatus.Timeout)
                {
                    throw new RequestTimedOutKraken(url, wex);
                }
                catch (WebException wex)
                when (wex.Status != WebExceptionStatus.ProtocolError
                      && whichAttempt < MaxRetries)
                {
                    log.DebugFormat("Web request failed with non-protocol error, retrying in {0} milliseconds: {1}", RetryDelayMilliseconds * whichAttempt, wex.Message);
                    // Exponential backoff with jitter
                    Thread.Sleep((int)(RetryDelayMilliseconds
                                 * (Math.Pow(2, whichAttempt) + random.NextDouble())));
                }
            }
            // Should never get here, because we don't catch any exceptions
            // in the final iteration of the above for loop. They should be
            // thrown to the calling code, or the call should succeed.
            return null;
        }

        public static Uri? ResolveRedirect(Uri     url,
                                           string? userAgent,
                                           int     maxRedirects = 6)
        {
            var urls = url.TraverseNodes(u => new RedirectWebClient(userAgent ?? UserAgentString) is RedirectWebClient rwClient
                                              && rwClient.OpenRead(u) is Stream s && DisposeStream(s)
                                              && rwClient.ResponseHeaders is WebHeaderCollection headers
                                              && headers["Location"] is string location
                                                  ? Uri.IsWellFormedUriString(location, UriKind.Absolute)
                                                      ? new Uri(location)
                                                      : Uri.IsWellFormedUriString(location, UriKind.Relative)
                                                          ? new Uri(u, location)
                                                          : throw new Kraken(string.Format(Properties.Resources.NetInvalidLocation,
                                                                                           location))
                                                  : null)
                          // The first element is the input, so e.g. if we want two redirects, that's three elements
                          .Take(maxRedirects + 1)
                          .ToArray();
            if (log.IsDebugEnabled)
            {
                foreach ((Uri from, Uri to) in urls.Zip(urls.Skip(1)))
                {
                    log.DebugFormat("Redirected {0} to {1}", from, to);
                }
            }
            return urls.LastOrDefault();
        }

        private static bool DisposeStream(Stream s)
        {
            s.Dispose();
            return true;
        }

        /// <summary>
        /// Provide an escaped version of the given Uri string, including converting
        /// square brackets to their escaped forms.
        /// </summary>
        /// <returns>
        /// <c>null</c> if the string is not a valid <see cref="Uri"/>, otherwise its normalized form.
        /// </returns>
        public static string? NormalizeUri(string uri)
        {
            // Uri.EscapeUriString has been deprecated because its purpose was ambiguous.
            // Is it supposed to turn a "&" into part of the content of a form field,
            // or is it supposed to assume that it separates different form fields?
            // https://github.com/dotnet/runtime/issues/31387
            // So now we have to just substitute certain characters ourselves one by one.

            // Square brackets are "reserved characters" that should not appear
            // in strings to begin with, so C# doesn't try to escape them in case
            // they're being used in a special way. They're not; some mod authors
            // just have crazy ideas as to what should be in a URL, and SD doesn't
            // escape them in its API. There's probably more in RFC 3986.
            var escaped = UriEscapeAll(uri.Replace(" ", "+"),
                                       '"', '<', '>', '^', '`',
                                       '(', ')',
                                       '{', '|', '}', '[', ']');

            // Make sure we have a "http://" or "https://" start.
            if (!Regex.IsMatch(escaped, "(?i)^(http|https)://"))
            {
                // Prepend "http://", as we do not know if the site supports https.
                escaped = "http://" + escaped;
            }

            return Uri.IsWellFormedUriString(escaped, UriKind.Absolute)
                       ? escaped
                       : null;
        }

        private static string UriEscapeAll(string orig, params char[] characters)
            => characters.Aggregate(orig,
                                    (s, c) => s.Replace(c.ToString(),
                                                        Uri.HexEscape(c)));

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
                // We expect a non-raw URI to be in one of these forms:
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

                // Check that the path is what we expect
                var segments = remoteUri.Segments.ToList();

                if (//segments is [_, _, _, "raw/", ..]
                    segments.Count > 3
                    && segments[3] is "raw/")
                {
                    log.InfoFormat("Remote GitHub URL is in raw format, using as is.");
                    return remoteUri;
                }
                if (//segments is [_, _, _, "releases/", "latest/", "download/", ..]
                    segments.Count > 6
                    && segments[3] is "releases/"
                    && segments[4] is "latest/"
                    && segments[5] is "download/")
                {
                    log.InfoFormat("Remote GitHub URL is in release asset format, using as is.");
                    return remoteUri;
                }
                if (//segments is not [_, _, _, "blob/" or "tree/", _, _, ..]
                    segments.Count < 6
                    || segments[3] is not ("blob/" or "tree/"))
                {
                    log.InfoFormat("Remote non-raw GitHub URL is in an unknown format, using as is.");
                    return remoteUri;
                }

                var remoteUriBuilder = new UriBuilder(remoteUri)
                {
                    // Replace host with raw host
                    Host = "raw.githubusercontent.com",
                    // Remove "blob/" or "tree/" segment from raw URI
                    Path = string.Join("", segments.Take(3)
                                                   .Concat(segments.Skip(4))),
                };

                log.InfoFormat("Canonicalized non-raw GitHub URL to: {0}",
                               remoteUriBuilder.Uri);

                return remoteUriBuilder.Uri;
            }
            else
            {
                return remoteUri;
            }
        }

        // The user agent that we report to web sites
        // Maybe overwritten by command line args
        public static readonly string UserAgentString = $"Mozilla/5.0 (compatible; CKAN/{Meta.ReleaseVersion})";

        private const int MaxRetries             = 5;
        private const int RetryDelayMilliseconds = 100;

        private static readonly ILog log = LogManager.GetLogger(typeof(Net));

        private static readonly Random random = new Random();

        public static readonly Dictionary<string, Uri> ThrottledHosts = new Dictionary<string, Uri>()
        {
            {
                "api.github.com",
                new Uri(HelpURLs.AuthTokens)
            }
        };
    }
}
