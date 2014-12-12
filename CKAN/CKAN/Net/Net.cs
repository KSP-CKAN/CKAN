using System;
using System.Net;
using System.Text.RegularExpressions;
using ChinhDo.Transactions;
using log4net;

namespace CKAN
{
    /// <summary>
    ///     Doing something with the network? Do it here.
    /// </summary>

    public class Net
    {
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
            user = user ?? new ConsoleUser();
            user.DisplayMessage("Downloading {0}", url);

            // Generate a temporary file if none is provided.
            if (filename == null)
            {
                filename = file_transaction.GetTempFileName();
            }

            log.DebugFormat("Downloading {0} to {1}", url, filename);

            var agent = new WebClient();

            try
            {
                agent.DownloadFile(url, filename);
            }
            catch (Exception ex)
            {
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