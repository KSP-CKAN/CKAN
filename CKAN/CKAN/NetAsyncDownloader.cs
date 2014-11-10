using System;
using System.IO;
using System.Net;
using System.Threading;
using ChinhDo.Transactions;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace CKAN
{
    // Called with status updates on how we'r progressing.
    public delegate void NetAsyncProgressReport(int percent, int bytesPerSecond, long bytesLeft);

    // Called on completion (including on error)
    public delegate void NetAsyncCompleted(Uri[] urls, string[] filenames, Exception[] errors);

    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public class NetAsyncDownloader
    {
        private class NetAsyncDownloaderDownloadPart
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

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncDownloader));
        private static readonly TxFileManager file_transaction = new TxFileManager ();

        private List<NetAsyncDownloaderDownloadPart> downloads;
        public NetAsyncCompleted onCompleted = null;
        public NetAsyncProgressReport onProgressReport = null;
        private int completed_downloads;

        private bool downloadCanceled = false;

        /// <summary>
        /// Returns a perfectly boring NetAsyncDownloader.
        /// </summary>
        public NetAsyncDownloader()
        {
            downloads = new List<NetAsyncDownloaderDownloadPart>();
        }

        /// <summary>
        /// Downloads our files, returning an array of filenames that we're writing to.
        /// The .onCompleted delegate will be called on completion.
        /// </summary>
        public string[] Download(ICollection<Uri> urls)
        {
            foreach (Uri url in urls)
            {
                var download = new NetAsyncDownloaderDownloadPart();

                download.url = url;
                download.path = file_transaction.GetTempFileName();
                download.agent = new WebClient();
                download.error = null;
                download.percentComplete = 0;
                download.lastProgressUpdateTime = DateTime.Now;
                download.lastProgressUpdateSize = 0;
                download.bytesPerSecond = 0;
                download.bytesLeft = 0;

                this.downloads.Add(download);
            }

            var filePaths = new string[downloads.Count];

            for (int i = 0; i < downloads.Count; i++)
            {
                User.WriteLine("Downloading \"{0}\"", downloads[i].url);

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

            // The user hasn't cancelled us yet. :)
            downloadCanceled = false;

            return filePaths;
        }

        /// <summary>
        /// Downloads all the modules specified to the cache.
        /// Even if modules share download URLs, they will only be downloaded once.
        /// Blocks until the download is complete, cancelled, or errored.
        /// </summary>
        public void DownloadModules(
            NetFileCache cache,
            IEnumerable<CkanModule> modules,
            ModuleInstallerReportProgress progress = null
        )
        {
            var unique_downloads = new Dictionary<Uri, CkanModule>();

            // Walk through all our modules, but only keep the first of each
            // one that has a unique download path.
            foreach (CkanModule module in modules)
            {
                if (!unique_downloads.ContainsKey(module.download))
                {
                    unique_downloads[module.download] = module;
                }
            }
                
            // Attach our progress report, if requested.
            if (progress != null)
            {
                this.onProgressReport += (percent, bytesPerSecond, bytesLeft) =>
                    (
                        progress(
                            String.Format("{0} kbps - downloading - {1} MiB left", bytesPerSecond/1024, bytesLeft/1024/1024),
                            percent
                        )
                    );
            }

            this.onCompleted = (_uris, paths, errors) => ModuleDownloadsComplete(cache, _uris, paths, unique_downloads.Values.ToArray(), errors);

            // Start the download!
            this.Download(unique_downloads.Keys);

            // XXX - Locking 'this' seems like a terrible idea.
            lock (this)
            {
                // Wait for our download to finish.
                log.Debug("Waiting for downloads to finish...");
                Monitor.Wait(this);
            }

            // Check to see if we've had any errors. If so, then release the kraken!
            List<Exception> exceptions = downloads
                .Select(x => x.error)
                .Where(ex => ex != null)
                .ToList();

            if (exceptions.Count > 0)
            {
                throw new DownloadErrorsKraken(exceptions);
            }

            // Yay! Everything worked!
        }

        /// <summary>
        /// Stores all of our files in the cache once done.
        /// Called by NetAsyncDownloader on completion.
        /// </summary>
        private void ModuleDownloadsComplete(NetFileCache cache, Uri[] urls, string[] filenames, CkanModule[] modules, Exception[] errors)
        {
            if (urls != null)
            {
                for (int i = 0; i < errors.Length; i++)
                {
                    if (errors[i] != null)
                    {
                        User.Error("Failed to download \"{0}\" - error: {1}", urls[i], errors[i].Message);
                    }
                    else
                    {
                        // Even if some of our downloads failed, we want to cache the
                        // ones which succeeded.
                        cache.Store(urls[i], filenames[i], modules[i].StandardName());

                    }
                }
            }

            // Finally, remove all our temp files.
            // We probably *could* have used Store's integrated move function above, but if we managed
            // to somehow get two URLs the same in our download set, that could cause right troubles!

            foreach (string tmpfile in filenames)
            {
                log.DebugFormat("Cleaing up {0}", tmpfile);
                file_transaction.Delete(tmpfile);
            }

            // Signal that we're done.
            lock (this)
            {
                Monitor.Pulse(this);
            }
        }

        /// <summary>
        /// Cancel any running downloads. This will also call onCompleted with
        /// all null arguments.
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
            if (downloadCanceled)
            {
                return;
            }

            NetAsyncDownloaderDownloadPart download = downloads[index];

            download.percentComplete = percent;

            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - download.lastProgressUpdateTime;
            if (timeSpan.Seconds >= 3.0)
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

                for (int i = 0; i < downloads.Count; i++)
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
            if (error != null)
            {
                log.InfoFormat("Error downloading {0}: {1}", downloads[index].url, error);
            }
            else
            {
                log.InfoFormat("Finished downloading {0}", downloads[index].url);
            }
            completed_downloads++;

            // If there was an error, remember it, but we won't raise it until
            // all downloads are finished or cancelled.
            downloads[index].error = error;

            if (completed_downloads == downloads.Count)
            {
                log.Info("All files finished downloading");

                // If we have a callback, then signal that we're done.
                if (onCompleted != null)
                {
                    var fileUrls = new Uri[downloads.Count];
                    var filePaths = new string[downloads.Count];
                    var errors = new Exception[downloads.Count];

                    for (int i = 0; i < downloads.Count; i++)
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

