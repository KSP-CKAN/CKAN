using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using ChinhDo.Transactions;
using log4net;
using CurlSharp;

namespace CKAN
{
    /// <summary>
    ///     Doing something with the network? Do it here.
    /// </summary>

    public class Net
    {
        public static string UserAgentString = "Mozilla/4.0 (compatible; CKAN)";

        private static readonly ILog log = LogManager.GetLogger(typeof (Net));
        private static TxFileManager file_transaction = new TxFileManager();

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
            return Download(url.ToString(), filename, user);
        }

        public static string Download(string url, string filename = null, IUser user = null)
        {
            user = user ?? new NullUser();
            user.RaiseMessage("Downloading {0}", url);

            // Generate a temporary file if none is provided.
            if (filename == null)
            {
                filename = file_transaction.GetTempFileName();
            }

            log.DebugFormat("Downloading {0} to {1}", url, filename);

            var agent = new WebClient();
            agent.Headers.Add("user-agent", UserAgentString);
           
            try
            {
                agent.DownloadFile(url, filename);
            }
            catch (Exception ex)
            {

                log.InfoFormat("Download failed, trying with curlsharp...");

                try
                {
                    Curl.GlobalInit(CurlInitFlag.All);

                    using(var curl = new CurlEasy())
                    using (FileStream stream = File.OpenWrite(filename))
                    {
                        curl.Url = url;
                        curl.WriteData = null; // Can we give it a C#-ish file here?
                        curl.WriteFunction = delegate(byte[] buf, int size, int nmemb, object extraData) {
                            stream.Write(buf, 0, size*nmemb);
                            return size*nmemb;
                        };
                        CurlCode result = curl.Perform();
                        if (result != CurlCode.Ok)
                        {
                            throw new Kraken("curl download of " + url + " failed with CurlCode " + result);
                        }
                    }

                    Curl.GlobalCleanup();
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
                    log.DebugFormat("Removing {0} after web error failure", filename);
                    file_transaction.Delete(filename);
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
    }
}