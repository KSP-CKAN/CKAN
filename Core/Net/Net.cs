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
        public static string UserAgentString = "Mozilla/4.0 (compatible; CKAN)";

        private static readonly ILog Log = LogManager.GetLogger(typeof (Net));
        private static readonly TxFileManager FileTransaction = new TxFileManager();

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
            public Uri uri { get; private set; }
            public string filename { get; private set; }
            public long size { get; private set; }

            public DownloadTarget(Uri uri, string filename = null, long size = 0)
            {
                if (filename == null)
                {
                    filename = FileTransaction.GetTempFileName();
                }

                this.uri = uri;
                this.filename = filename;
                this.size = size;
            }
        }

        public static string DownloadWithProgress(string url, string filename = null, IUser user = null)
        {
            return DownloadWithProgress(new Uri(url), filename, user);
        }

        public static string DownloadWithProgress(Uri url, string filename = null, IUser user = null)
        {
            var targets = new[] {new DownloadTarget(url, filename)};
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
                        File.Move(filenames[i], downloadTargets.First(p => p.uri == urls[i]).filename);
                    }
                }
            }.DownloadAndWait(downloadTargets.ToDictionary(p => p.uri, p => p.size));
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
                Log.InfoFormat("Download failed, trying with curlsharp...");

                var content = string.Empty;

                var client = Curl.CreateEasy(url, delegate(byte[] buf, int size, int nmemb, object extraData)
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

                    Log.DebugFormat("Download from {0}:\n\n{1}", url, content);

                    return content;
                }
            }
        }

        private static WebClient MakeDefaultHttpClient()
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", UserAgentString);

            return client;
        }
    }
}