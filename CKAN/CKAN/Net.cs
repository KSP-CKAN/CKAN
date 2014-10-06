namespace CKAN {
    using System;
    using System.IO;
    using System.Net;
    using log4net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Doing something with the network? Do it here.
    /// </summary>

    public class Net {

        static readonly ILog log = LogManager.GetLogger (typeof(Net));

        /// <summary>
        /// Downloads the specified url, and stores it in the filename given.
        /// 
        /// If no filename is supplied, a temporary file will be generated.
        /// 
        /// Returns the filename the file was saved to on success.
        /// 
        /// Throws an exception on failure.
        /// 
        /// Throws a MissingCertificateException *and* prints a message to the
        /// console if we detect missing certificates (common on a fresh Linux/mono install)
        /// </summary>

        public static string Download (string url, string filename = null) {

            // Generate a temporary file if none is provided.
            if (filename == null) {
                filename = Path.GetTempFileName ();
            }

            log.DebugFormat ("Downloading {0} to {1}", url, filename);

            WebClient agent = new WebClient ();

            try {
                agent.DownloadFile (url, filename);
            }
            catch (Exception ex) {

                // Clean up our file, it's unlikely to be complete.
                // It's okay if this fails.
                try {
                    log.DebugFormat("Removing {0} after web error failure", filename);
                    File.Delete (filename);
                }
                catch {
                    // Apparently we need a catch, even if we do nothing.
                }

                if (ex is System.Net.WebException && Regex.IsMatch(ex.Message, "authentication or decryption has failed")) {

                    Console.WriteLine ("\nOh no! Our download failed!\n");
                    Console.WriteLine ("\t{0}\n",ex.Message);
                    Console.WriteLine ("If you're on Linux, try running:\n");
                    Console.WriteLine ("\tmozroots --import --ask-remove\n");
                    Console.WriteLine ("on the command-line to update your certificate store, and try again.\n");

                    throw new MissingCertificateException ();

                }

                // Not the exception we were looking for! Throw it further upwards!
                throw;
            }

            return filename;
        }
    }

    class MissingCertificateException : Exception {

    }
}

