using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ChinhDo.Transactions;
using CurlSharp;
using log4net;

namespace CKAN
{
    /// <summary>
    ///     Doing something with the network? Do it here.
    /// </summary>

    public class Net
    {
        // The user agent that we report to web sites
        public static string UserAgentString = "Mozilla/4.0 (compatible; CKAN)";

        private static readonly ILog Log = LogManager.GetLogger(typeof (Net));
        private static readonly TxFileManager FileTransaction = new TxFileManager();

        public static readonly Dictionary<string, Uri> ThrottledHosts = new Dictionary<string, Uri>()
        {
            {
                "api.github.com",
                new Uri("https://github.com/KSP-CKAN/CKAN/wiki/Adding-a-GitHub-API-authtoken")
            }
        };

        /// <summary>
        ///     Downloads the specified url, and stores it in the filename given.
        ///     If no filename is supplied, a temporary file will be generated.
        ///     Returns the filename the file was saved to on success.
        ///     Throws an exception on failure.
        ///     Throws a MissingCertificateException *and* prints a message to the
        ///     console if we detect missing certificates (common on a fresh Linux/mono install)
        /// </summary>
        public static string Download(Uri url, string filename = null, IUser user = null)
        {
            return Download(url.OriginalString, filename, user);
        }

        public static string Download(string url, string filename = null, IUser user = null)
        {
            user = user ?? new NullUser();
            user.RaiseMessage("Downloading {0}", url);

            // Generate a temporary file if none is provided.
            if (filename == null)
            {
                filename = FileTransaction.GetTempFileName();
            }

            Log.DebugFormat("Downloading {0} to {1}", url, filename);

            var agent = MakeDefaultHttpClient();

            try
            {
                agent.DownloadFile(url, filename);
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Download failed, trying with curlsharp...");

                try
                {
                    Curl.Init();

                    using (FileStream stream = File.OpenWrite(filename))
                    using (var curl = Curl.CreateEasy(url, stream))
                    {
                        CurlCode result = curl.Perform();
                        if (result != CurlCode.Ok)
                        {
                            throw new Kraken("curl download of " + url + " failed with CurlCode " + result);
                        }
                        else
                        {
                            Log.Debug("curlsharp download successful");
                        }
                    }

                    Curl.CleanUp();
                    return filename;
                }
                catch
                {
                    // D'oh, failed again. Fall through to clean-up handling.
                }

                // Clean up our file, it's unlikely to be complete.
                // We do this even though we're using transactional files, as we may not be in a transaction.
                // It's okay if this fails.
                try
                {
                    Log.DebugFormat("Removing {0} after web error failure", filename);
                    FileTransaction.Delete(filename);
                }
                catch
                {
                    // Apparently we need a catch, even if we do nothing.
                }

                // Look for an exception regarding the authentication.
                if (Regex.IsMatch(ex.ToString(), "The authentication or decryption has failed."))
                {
                    throw new MissingCertificateKraken("Failed downloading " + url, ex);
                }

                // Not the exception we were looking for! Throw it further upwards!
                throw;
            }

            return filename;
        }

        public class DownloadTarget
        {
            public Uri    url         { get; private set; }
            public Uri    fallbackUrl { get; private set; }
            public string filename    { get; private set; }
            public long   size        { get; private set; }
            public string mimeType    { get; private set; }

            public DownloadTarget(Uri url, Uri fallback = null, string filename = null, long size = 0, string mimeType = "")
            {
                this.url         = url;
                this.fallbackUrl = fallback;
                this.filename    = string.IsNullOrEmpty(filename)
                    ? FileTransaction.GetTempFileName()
                    : filename;
                this.size        = size;
                this.mimeType    = mimeType;
            }
        }

        public static string DownloadWithProgress(string url, string filename = null, IUser user = null)
        {
            return DownloadWithProgress(new Uri(url), filename, user);
        }

        public static string DownloadWithProgress(Uri url, string filename = null, IUser user = null)
        {
            var targets = new[] {new DownloadTarget(url, null, filename)};
            DownloadWithProgress(targets, user);
            return targets.First().filename;
        }

        public static void DownloadWithProgress(ICollection<DownloadTarget> downloadTargets, IUser user = null)
        {
            new NetAsyncDownloader(user ?? new NullUser())
            {
                onCompleted = (urls, filenames, errors) =>
                {
                    if (filenames == null || urls == null) return;
                    for (var i = 0; i < Math.Min(urls.Length, filenames.Length); i++)
                    {
                        File.Move(filenames[i], downloadTargets.First(p => p.url == urls[i]).filename);
                    }
                }
            }.DownloadAndWait(downloadTargets);
        }

        public static string DownloadText(Uri url)
        {
            return DownloadText(url.OriginalString);
        }

        public static string DownloadText(string url)
        {
            Log.DebugFormat("About to download {0}", url);

            var agent = MakeDefaultHttpClient();

            try
            {
                return agent.DownloadString(url);
            }
            catch (Exception)
            {
                try
                {
                    Log.InfoFormat("Download failed, trying with curlsharp...");

                    var content = string.Empty;

                    var client = Curl.CreateEasy(url, delegate (byte[] buf, int size, int nmemb, object extraData)
                    {
                        content += Encoding.UTF8.GetString(buf);
                        return size * nmemb;
                    });

                    using (client)
                    {
                        var result = client.Perform();

                        if (result != CurlCode.Ok)
                        {
                            throw new Exception("Curl download failed with error " + result);
                        }

                        Log.DebugFormat("Download from {0}:\r\n\r\n{1}", url, content);

                        return content;
                    }
                }
                catch(Exception e)
                {
                    throw new Kraken("Downloading using cURL failed", e);
                }
            }
        }

        public static Uri ResolveRedirect(Uri uri)
        {
            const int maximumRequest = 5;

            var currentUri = uri;
            for (var i = 0; i < maximumRequest; i++)
            {
                var client = new RedirectWebClient();

                // The empty using is so that we dispose of the stream and don't block
                // We don't ACTUALLY attempt to download the file, but it appears that if the client sees a
                // Content Length in the response it thinks there will be a response body and blocks.
                using (client.OpenRead(currentUri)) { }

                var location = client.ResponseHeaders["Location"];

                if (location == null)
                {
                    return currentUri;
                }
                else
                {
                    Log.InfoFormat("{0} redirected to {1}", currentUri, location);

                    if (Uri.IsWellFormedUriString(location, UriKind.Absolute))
                    {
                        currentUri = new Uri(location);
                    }
                    else if (Uri.IsWellFormedUriString(location, UriKind.Relative))
                    {
                        currentUri = new Uri(currentUri, location);
                        Log.DebugFormat("Relative URL {0} is absolute URL {1}", location, currentUri);
                    }
                    else
                    {
                        throw new Kraken("Invalid URL in Location header: " + location);
                    }
                }
            }

            return null;
        }

        private static WebClient MakeDefaultHttpClient()
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", UserAgentString);

            return client;
        }

        // HACK: The ancient WebClient doesn't support setting the request type to HEAD and WebRequest doesn't support
        // setting the User-Agent header.
        // Maybe one day we'll be able to use HttpClient (https://msdn.microsoft.com/en-us/library/system.net.http.httpclient%28v=vs.118%29.aspx)
        private sealed class RedirectWebClient : WebClient
        {
            public RedirectWebClient()
            {
                Headers.Add("User-Agent", UserAgentString);
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var webRequest = (HttpWebRequest)base.GetWebRequest(address);
                webRequest.AllowAutoRedirect = false;

                webRequest.Method = "HEAD";

                return webRequest;
            }
        }
    }
}
