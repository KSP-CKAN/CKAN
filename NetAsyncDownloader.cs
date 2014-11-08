using System;
using System.IO;
using System.Net;
using ChinhDo.Transactions;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace CKAN
{
    public delegate void NetAsyncProgressReport(int percent, int bytesPerSecond, long bytesLeft);

    public delegate void NetAsyncCompleted(Uri[] urls, string[] filenames, Exception[] errors);

    public struct NetAsyncDownloaderDownloadPart
    {
        public WebClient agent;
        public long bytesDownloaded;
        public long bytesLeft;
        public int bytesPerSecond;
        public Exception error;
        public int lastProgressUpdateSize;
        public DateTime lastProgressUpdateTime;
        public string path;
        public int percentComplete;
        public Uri url;
    }

    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public class NetAsyncDownloader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncDownloader));

        private readonly NetAsyncDownloaderDownloadPart[] downloads;
        public NetAsyncCompleted onCompleted = null;
        public NetAsyncProgressReport onProgressReport = null;
        private int completed_downloads;

        private bool downloadCanceled = false;

        /// <summary>
        /// Prepares to download the list of URLs to the file paths specified.
        /// Any URLs missing file paths will be written to temporary files.
        /// Use .StartDownload() to actually start the download.
        /// </summary>
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

        /// <summary>
        /// Downloads our files, returning an array of filenames upon completion.
        /// </summary>
        public string[] StartDownload()
        {
            var filePaths = new string[downloads.Length];
            var file_transaction = new TxFileManager ();

            log.Debug("Starting download");

            for (int i = 0; i < downloads.Length; i++)
            {
                User.WriteLine("Downloading \"{0}\"", downloads[i].url);

                // Generate a temporary file if none is provided.
                if (downloads[i].path == null)
                {
                    downloads[i].path = file_transaction.GetTempFileName();
                }

                filePaths[i] = downloads[i].path;

                downloads[i].agent = new WebClient();
                downloads[i].error = null;
                downloads[i].percentComplete = 0;
                downloads[i].lastProgressUpdateTime = DateTime.Now;
                downloads[i].lastProgressUpdateSize = 0;
                downloads[i].bytesPerSecond = 0;
                downloads[i].bytesLeft = 0;

                // We need a new variable for our closure/lambda, hence index = i.
                int index = i;

                // Schedule for us to get back progress reports.
                downloads[i].agent.DownloadProgressChanged +=
                    (sender, args) =>
                        FileProgressReport(index, args.ProgressPercentage, args.BytesReceived,
                                           args.TotalBytesToReceive - args.BytesReceived);

                // And schedule a notification if we're done (or if something goes wrong)
                downloads[i].agent.DownloadFileCompleted += (sender, args) => FileDownloadComplete(index, args.Error);

                // Snapshot whatever was in that location, in case we need to roll-back.
                file_transaction.Snapshot(downloads[i].path);

                // Bytes ahoy!
                downloads[i].agent.DownloadFileAsync(downloads[i].url, downloads[i].path);
            }

            return filePaths;
        }

        /// <summary>
        /// Cancel any running downloads.
        /// </summary>
        public void CancelDownload()
        {
            log.Debug("Cancelling download");

            foreach (var download in downloads)
            {
                download.agent.CancelAsync();
            }

            if (onCompleted != null)
            {
                onCompleted(null, null, null);
            }

            downloadCanceled = true;
        }

        private void FileProgressReport(int index, int percent, long bytesDownloaded, long bytesLeft)
        {
            log.Debug("Reporting file progress");

            if (downloadCanceled)
            {
                return;
            }

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

                if (!downloadCanceled)
                {
                    onProgressReport(totalPercentage, totalBytesPerSecond, totalBytesLeft);
                }
            }
        }

        /// <summary>
        /// This method gets called back by `WebClient` when a download is completed.
        /// Throws a DownloadErrorsKraken if anything went wrong.
        /// </summary>
        private void FileDownloadComplete(int index, Exception error)
        {
            log.DebugFormat("File {0} finished downloading", index);
            completed_downloads++;

            // If there was an error, remember it, but we won't raise it until
            // all downloads are finished or cancelled.
            downloads[index].error = error;

            if (completed_downloads == downloads.Length)
            {
                log.Debug("All files finished downloading");

                List<Exception> exceptions = downloads.Select(x => x.error).Where(ex => ex != null).ToList();

                if (exceptions.Count > 0)
                {
                    throw new DownloadErrorsKraken(exceptions);
                }

                // If we have a callback, then signal that we're done.
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

                    log.Debug("Signalling completion via callback");
                    onCompleted(fileUrls, filePaths, errors);
                }
            }
        }
    }
}

