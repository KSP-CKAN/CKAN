using System;
using System.IO;
using System.Net;

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
        public FileStream fileStream;
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
        private bool downloadCanceled = false;

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

                transaction.Snapshot(downloads[i].path);
                downloads[i].agent.DownloadFileAsync(downloads[i].url, downloads[i].path);
            }

            return filePaths;
        }

        public void CancelDownload()
        {
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
}

