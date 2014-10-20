using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{
    /// <summary>
    ///     Doing something with the network? Do it here.
    /// </summary>
    public delegate void NetAsyncProgressReport(int percent, int bytesPerSecond, long bytesLeft);

    public delegate void NetAsyncCompleted(Uri[] urls, string[] filenames, Exception[] errors);

    public struct NetAsyncDownloaderDownloadPart
    {
        public WebClient agent;
        public long bytesDownloaded;
        public long bytesLeft;
        public int bytesPerSecond;
        public Exception error;
        public TransactionalFileWriter fileWriter;
        public int lastProgressUpdateSize;
        public DateTime lastProgressUpdateTime;
        public string path;
        public int percentComplete;
        public Uri url;
    }

    public class NetAsyncDownloader
    {
//        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncDownloader));

        private readonly NetAsyncDownloaderDownloadPart[] downloads;
        public NetAsyncCompleted onCompleted = null;
        public NetAsyncProgressReport onProgressReport = null;
        private int queuePointer;

        private FilesystemTransaction transaction;

        public NetAsyncDownloader(Uri[] urls, string[] filenames = null)
        {
            downloads = new NetAsyncDownloaderDownloadPart[urls.Length];

            for (int i = 0; i < downloads.Length; i++)
            {
                downloads[i].url = urls[i];
                if (filenames != null)
                {
                    downloads[i].path = filenames[i];
                }
                else
                {
                    downloads[i].path = null;
                }
            }
        }

        // starts the download and return the destination filename
        public string[] StartDownload()
        {
            var filePaths = new string[downloads.Length];
            transaction = new FilesystemTransaction();

            for (int i = 0; i < downloads.Length; i++)
            {
                User.WriteLine("Downloading \"{0}\"", downloads[i].url);

                // Generate a temporary file if none is provided.
                if (downloads[i].path == null)
                {
                    downloads[i].path = Path.GetTempFileName();
                }

                filePaths[i] = downloads[i].path;

                downloads[i].agent = new WebClient();
                downloads[i].error = null;
                downloads[i].percentComplete = 0;
                downloads[i].lastProgressUpdateTime = DateTime.Now;
                downloads[i].lastProgressUpdateSize = 0;
                downloads[i].bytesPerSecond = 0;
                downloads[i].bytesLeft = 0;

                int index = i;
                downloads[i].agent.DownloadProgressChanged +=
                    (sender, args) =>
                        FileProgressReport(index, args.ProgressPercentage, args.BytesReceived,
                            args.TotalBytesToReceive - args.BytesReceived);

                downloads[i].agent.DownloadFileCompleted += (sender, args) => FileDownloadComplete(index, args.Error);

                downloads[i].fileWriter = transaction.OpenFileWrite(downloads[i].path, false);
                downloads[i].agent.DownloadFileAsync(downloads[i].url, downloads[i].fileWriter.TemporaryPath);
            }

            return filePaths;
        }

        private void FileProgressReport(int index, int percent, long bytesDownloaded, long bytesLeft)
        {
            NetAsyncDownloaderDownloadPart download = downloads[index];

            download.percentComplete = percent;

            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - download.lastProgressUpdateTime;
            if (timeSpan.Seconds >= 1.0)
            {
                long bytesChange = bytesDownloaded - download.lastProgressUpdateSize;
                download.lastProgressUpdateSize = (int) bytesDownloaded;
                download.lastProgressUpdateTime = now;
                download.bytesPerSecond = (int) bytesChange/timeSpan.Seconds;
            }

            download.bytesLeft = bytesLeft;
            download.bytesDownloaded = bytesDownloaded;
            downloads[index] = download;

            if (onProgressReport != null)
            {
                int totalPercentage = 0;
                int totalBytesPerSecond = 0;
                long totalBytesLeft = 0;
                long totalBytesDownloaded = 0;

                for (int i = 0; i < downloads.Length; i++)
                {
                    totalBytesPerSecond += downloads[i].bytesPerSecond;
                    totalBytesLeft += downloads[i].bytesLeft;
                    totalBytesDownloaded += downloads[i].bytesDownloaded;
                    totalBytesLeft += downloads[i].bytesLeft;
                }

                totalPercentage = (int)((totalBytesDownloaded * 100) / (totalBytesLeft + totalBytesDownloaded + 1));

                onProgressReport(totalPercentage, totalBytesPerSecond, totalBytesLeft);
            }
        }

        private void FileDownloadComplete(int index, Exception error)
        {
            queuePointer++;
            downloads[index].error = error;

            if (queuePointer == downloads.Length)
            {
                // verify no errors before commit

                bool err = false;
                for (int i = 0; i < downloads.Length; i++)
                {
                    if (downloads[i].error != null)
                    {
                        err = true;
                    }
                }

                if (!err)
                {
                    transaction.Commit();
                }

                if (onCompleted != null)
                {
                    var fileUrls = new Uri[downloads.Length];
                    var filePaths = new string[downloads.Length];
                    var errors = new Exception[downloads.Length];

                    for (int i = 0; i < downloads.Length; i++)
                    {
                        fileUrls[i] = downloads[i].url;
                        filePaths[i] = downloads[i].path;
                        errors[i] = downloads[i].error;
                    }

                    onCompleted(fileUrls, filePaths, errors);
                }
            }
        }

        public void WaitForAllDownloads()
        {
            while (queuePointer < downloads.Length)
            {
            }
        }
    }

    public class Net
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Net));

        /// <summary>
        ///     Downloads the specified url, and stores it in the filename given.
        ///     If no filename is supplied, a temporary file will be generated.
        ///     Returns the filename the file was saved to on success.
        ///     Throws an exception on failure.
        ///     Throws a MissingCertificateException *and* prints a message to the
        ///     console if we detect missing certificates (common on a fresh Linux/mono install)
        /// </summary>
        public static string Download(Uri url, string filename = null)
        {
            return Download(url.ToString(), filename);
        }

        public static string Download(string url, string filename = null)
        {
            User.WriteLine("Downloading {0}", url);

            // Generate a temporary file if none is provided.
            if (filename == null)
            {
                filename = Path.GetTempFileName();
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
                // It's okay if this fails.
                try
                {
                    log.DebugFormat("Removing {0} after web error failure", filename);
                    File.Delete(filename);
                }
                catch
                {
                    // Apparently we need a catch, even if we do nothing.
                }

                if (ex is WebException && Regex.IsMatch(ex.Message, "authentication or decryption has failed"))
                {
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

    internal class MissingCertificateException : Exception
    {
    }
}