using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace CKAN {
    using System;
    using System.IO;
    using System.Net;
    using log4net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Doing something with the network? Do it here.
    /// </summary>

    public delegate void NetAsyncProgressReport(int percent, int bytesPerSecond, long bytesLeft);

    public delegate void NetAsyncCompleted(Uri[] urls, string[] filenames, Exception[] errors);

    public class NetAsyncDownloader {

        static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncDownloader));

        public NetAsyncProgressReport onProgressReport = null;
        public NetAsyncCompleted onCompleted = null;

        private Uri[] fileUrls = null;
        private string[] filePaths = null;
        private Exception[] errors = null;
        private int queuePointer = 0;
        private WebClient[] agents = null;
        private int[] percentageComplete = null;
        private DateTime[] lastProgressUpdateTime = null;
        private int[] lastProgressUpdateSize = null;
        private int[] bytesPerSecond = null;
        private long[] bytesLeft = null;

        public NetAsyncDownloader(Uri[] urls, string[] filenames = null) {
            fileUrls = urls;
            filePaths = filenames;
        }

        // starts the download and return the destination filename
        public string[] StartDownload() {
            foreach (var url in fileUrls) {
                User.WriteLine("Downloading \"{0}\"", url);
            }

            // Generate a temporary file if none is provided.
            if (filePaths == null) {
                filePaths = new string[fileUrls.Length];
                for (int i = 0; i < fileUrls.Length; i++)
                {
                    filePaths[i] = Path.GetTempFileName();
                }
            }

            agents = new WebClient[fileUrls.Length];
            errors = new Exception[fileUrls.Length];
            percentageComplete = new int[fileUrls.Length];
            lastProgressUpdateTime = new DateTime[fileUrls.Length];
            lastProgressUpdateSize = new int[fileUrls.Length];
            bytesPerSecond = new int[fileUrls.Length];
            bytesLeft = new long[fileUrls.Length];

            for (int i = 0; i < fileUrls.Length; i++) {
                agents[i] = new WebClient();
                errors[i] = null;
                percentageComplete[i] = 0;
                lastProgressUpdateTime[i] = DateTime.Now;
                lastProgressUpdateSize[i] = 0;
                bytesPerSecond[i] = 0;
                bytesLeft[i] = 0;

                int index = i;
                agents[i].DownloadProgressChanged +=
                        (sender, args) => FileProgressReport(index, args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive - args.BytesReceived);

                agents[i].DownloadFileCompleted += (sender, args) => FileDownloadComplete(index, args.Error);
                agents[i].DownloadFileAsync(fileUrls[i], filePaths[i]);
            }

            return filePaths;
        }

        private void FileProgressReport(int index, int percent, long bytesDownloaded, long _bytesLeft) {
            percentageComplete[index] = percent;

            var now = DateTime.Now;
            var timeSpan = now - lastProgressUpdateTime[index];
            if (timeSpan.Seconds >= 1.0) {
                var bytesChange = bytesDownloaded - lastProgressUpdateSize[index];
                lastProgressUpdateSize[index] = (int)bytesDownloaded;
                lastProgressUpdateTime[index] = now;
                bytesPerSecond[index] = (int)bytesChange / timeSpan.Seconds;
            }

            bytesLeft[index] = _bytesLeft;

            if (onProgressReport != null) {
                int totalPercentage = 0;
                for (int i = 0; i < percentageComplete.Length; i++) {
                    totalPercentage += percentageComplete[i];
                }

                totalPercentage /= percentageComplete.Length;

                int totalBytesPerSecond = 0;
                for (int i = 0; i < bytesPerSecond.Length; i++) {
                    totalBytesPerSecond += bytesPerSecond[i];
                }

                long totalBytesLeft = 0;
                for (int i = 0; i < bytesLeft.Length; i++) {
                    totalBytesLeft += bytesLeft[i];
                }

                onProgressReport(totalPercentage, totalBytesPerSecond, totalBytesLeft);
            }
        }

        private void FileDownloadComplete(int index, Exception error)
        {
            queuePointer++;
            errors[index] = error;

            if (queuePointer == fileUrls.Length) {
                if (onCompleted != null) {
                    onCompleted(fileUrls, filePaths, errors);
                }

                return;
            }
        }

        public void WaitForAllDownloads()
        {
            while (queuePointer < fileUrls.Length) {
            }
        }

        public static string Download(string url, string filename = null) {
            User.WriteLine("Downloading {0}", url);

            // Generate a temporary file if none is provided.
            if (filename == null) {
                filename = Path.GetTempFileName();
            }

            log.DebugFormat("Downloading {0} to {1}", url, filename);

            WebClient agent = new WebClient();

            try {
                agent.DownloadFile(url, filename);
            }
            catch (Exception ex) {
                // Clean up our file, it's unlikely to be complete.
                // It's okay if this fails.
                try {
                    log.DebugFormat("Removing {0} after web error failure", filename);
                    File.Delete(filename);
                }
                catch {
                    // Apparently we need a catch, even if we do nothing.
                }

                if (ex is System.Net.WebException && Regex.IsMatch(ex.Message, "authentication or decryption has failed")) {

                    User.WriteLine("\nOh no! Our download failed!\n");
                    User.WriteLine("\t{0}\n", ex.Message);
                    User.WriteLine("If you're on Linux, try running:\n");
                    User.WriteLine("\tmozroots --import --ask-remove\n");
                    User.WriteLine("on the command-line to update your certificate store, and try again.\n");

                    throw new MissingCertificateException();

                }

                // Not the exception we were looking for! Throw it further upwards!
                throw;
            }

            return filename;
        }

    }

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

        public static string Download (Uri url, string filename = null) {
            return Download (url.ToString (), filename);
        }

        public static string Download (string url, string filename = null) {
            User.WriteLine("Downloading {0}", url);

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

                    User.WriteLine ("\nOh no! Our download failed!\n");
                    User.WriteLine ("\t{0}\n",ex.Message);
                    User.WriteLine ("If you're on Linux, try running:\n");
                    User.WriteLine ("\tmozroots --import --ask-remove\n");
                    User.WriteLine ("on the command-line to update your certificate store, and try again.\n");

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

