using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

using log4net;

// This WebClient child class does some complicated stuff, let's keep using it for now
#pragma warning disable SYSLIB0014

namespace CKAN
{
    /// <summary>
    /// A WebClient with some CKAN-sepcific adjustments:
    /// - A user agent string (required by GitHub API policy)
    /// - Sets the Accept header to a given MIME type (needed to get raw files from GitHub API)
    /// - Times out after a specified amount of time in milliseconds, 100 000 milliseconds (=100 seconds) by default (https://stackoverflow.com/a/3052637)
    /// - Handles permanent redirects to the same host without clearing the Authorization header (needed to get files from renamed GitHub repositories via API)
    /// </summary>
    internal sealed class RedirectingTimeoutWebClient : WebClient
    {
        /// <summary>
        /// Initialize our special web client
        /// </summary>
        /// <param name="timeout">Timeout for the request in milliseconds, defaulting to 100 000 (=100 seconds)</param>
        /// <param name="mimeType">A mime type sent with the "Accept" header</param>
        public RedirectingTimeoutWebClient(int timeout = 100000, string mimeType = "")
        {
            this.timeout  = timeout;
            this.mimeType = mimeType;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            // Set user agent and MIME type for every request. including redirects
            Headers.Add("User-Agent", Net.UserAgentString);
            if (!string.IsNullOrEmpty(mimeType))
            {
                log.InfoFormat("Setting MIME type {0}", mimeType);
                Headers.Add("Accept", mimeType);
            }
            if (permanentRedirects.TryGetValue(address, out Uri redirUri))
            {
                // Obey a previously received permanent redirect
                address = redirUri;
            }
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest hwr)
            {
                // GitHub API tokens cannot be passed via auto-redirect
                hwr.AllowAutoRedirect = false;
                hwr.Timeout           = timeout;
            }
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var response = base.GetWebResponse(request);
            if (response == null)
            {
                return null;
            }

            if (response is HttpWebResponse hwr)
            {
                int statusCode = (int)hwr.StatusCode;
                var location = hwr.Headers["Location"];
                if (statusCode >= 300 && statusCode <= 399 && location != null)
                {
                    log.InfoFormat("Redirecting to {0}", location);
                    hwr.Close();
                    var redirUri = new Uri(request.RequestUri, location);
                    if (Headers.AllKeys.Contains("Authorization")
                        && request.RequestUri.Host != redirUri.Host)
                    {
                        log.InfoFormat("Host mismatch, purging token for redirect");
                        Headers.Remove("Authorization");
                    }
                    // Moved or PermanentRedirect
                    if (statusCode == 301 || statusCode == 308)
                    {
                        permanentRedirects.Add(request.RequestUri, redirUri);
                    }
                    return GetWebResponse(GetWebRequest(redirUri));
                }
            }
            return response;
        }

        private readonly int    timeout;
        private readonly string mimeType;
        private static readonly Dictionary<Uri, Uri> permanentRedirects = new Dictionary<Uri, Uri>();
        private static readonly ILog log = LogManager.GetLogger(typeof(RedirectingTimeoutWebClient));
    }
}
