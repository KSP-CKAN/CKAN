using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        // Maybe overwritten by command line args
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
        {
            // HttpClient apparently is worse than what it was supposed to replace
            #pragma warning disable SYSLIB0014
            WebRequest req = WebRequest.Create(url);
            #pragma warning restore SYSLIB0014
            req.Method = "HEAD";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                string val = resp.Headers["ETag"]?.Replace("\"", "");
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
        public static string Download(Uri url, out string etag, string filename = null, IUser user = null)
            => Download(url.OriginalString, out etag, filename, user);

        public static string Download(Uri url, string filename = null, IUser user = null)
            => Download(url, out _, filename, user);

        public static string Download(string url, string filename = null, IUser user = null)
            => Download(url, out _, filename, user);

        public static string Download(string url, out string etag, string filename = null, IUser user = null)
        {
            user = user ?? new NullUser();
            user.RaiseMessage(Properties.Resources.NetDownloading, url);
            var FileTransaction = new TxFileManager();

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
                var rwClient = new RedirectWebClient();
                using (rwClient.OpenRead(url)) { }
                var location = rwClient.ResponseHeaders["Location"];
                if (location == null)
                {
                    return url;
                }
                else if (Uri.IsWellFormedUriString(location, UriKind.Absolute))
                {
                    url = new Uri(location);
                }
                else if (Uri.IsWellFormedUriString(location, UriKind.Relative))
                {
                    url = new Uri(url, location);
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
        /// Provide an escaped version of the given Uri string, including converting
        /// square brackets to their escaped forms.
        /// </summary>
        /// <returns>
        /// <c>null</c> if the string is not a valid <see cref="Uri"/>, otherwise its normalized form.
        /// </returns>
        public static string NormalizeUri(string uri)
        {
            // Uri.EscapeUriString has been deprecated because its purpose was ambiguous.
            // Is it supposed to turn a "&" into part of the content of a form field,
            // or is it supposed to assume that it separates different form fields?
            // https://github.com/dotnet/runtime/issues/31387
            // So now we have to just substitude certain characters ourselves one by one.

            // Square brackets are "reserved characters" that should not appear
            // in strings to begin with, so C# doesn't try to escape them in case
            // they're being used in a special way. They're not; some mod authors
            // just have crazy ideas as to what should be in a URL, and SD doesn't
            // escape them in its API. There's probably more in RFC 3986.
            var escaped = UriEscapeAll(uri.Replace(" ", "+"),
                                       '"', '<', '>', '^', '`',
                                       '{', '|', '}', '[', ']');

            // Make sure we have a "http://" or "https://" start.
            if (!Regex.IsMatch(escaped, "(?i)^(http|https)://"))
            {
                // Prepend "http://", as we do not know if the site supports https.
                escaped = "http://" + escaped;
            }

            if (Uri.IsWellFormedUriString(escaped, UriKind.Absolute))
            {
                return escaped;
            }
            else
            {
                return null;
            }
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
