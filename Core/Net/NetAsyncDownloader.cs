using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using CurlSharp;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public class NetAsyncDownloader
    {

        public IUser User { get; set; }

        // Private utility class for tracking downloads
        private class NetAsyncDownloaderDownloadPart
        {
            public Uri url;
            public WebClient agent = new WebClient();
            public DateTime lastProgressUpdateTime;
            public string path;
            public long bytesLeft;
            public long size;
            public int bytesPerSecond;
            public Exception error;
            public int lastProgressUpdateSize;

            public NetAsyncDownloaderDownloadPart(Uri url, long expectedSize, string path = null)
            {
                this.url = url;
                this.path = path ?? Path.GetTempFileName();
                bytesLeft = expectedSize;
                size = expectedSize;
                lastProgressUpdateTime = DateTime.Now;

                agent.Headers.Add("user-agent", Net.UserAgentString);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncDownloader));

        private List<NetAsyncDownloaderDownloadPart> downloads;
        private int completed_downloads;

        //Used for inter-thread communication.
        private volatile bool download_canceled;
        private readonly ManualResetEvent complete_or_canceled;

        // Called on completion (including on error)
        // Called with ALL NULLS on error.
        public delegate void NetAsyncCompleted(Uri[] urls, string[] filenames, Exception[] errors);
        public NetAsyncCompleted onCompleted;

        // When using the curlsharp downloader, this contains all the threads
        // that are working for us.
        private List<Thread> curl_threads = new List<Thread>();

        /// <summary>
        /// Returns a perfectly boring NetAsyncDownloader.
        /// </summary>
        public NetAsyncDownloader(IUser user)
        {
            User = user;
            downloads = new List<NetAsyncDownloaderDownloadPart>();
            complete_or_canceled = new ManualResetEvent(false);
        }

        /// <summary>
        /// Downloads our files, returning an array of filenames that we're writing to.
        /// The sole argument is a collection of KeyValuePair(s) containing the download URL and the expected download size
        /// The .onCompleted delegate will be called on completion.
        /// </summary>
        private void Download(ICollection<KeyValuePair<Uri, long>> urls)
        {
            foreach (var download in urls.Select(url => new NetAsyncDownloaderDownloadPart(url.Key, url.Value)))
            {
                downloads.Add(download);
            }

            DownloadNative();

        }

        /// <summary>
        /// Download all our files using the native .NET hanlders.
        /// </summary>
        /// <returns>The native.</returns>
        private void DownloadNative()
        {
            for (int i = 0; i < downloads.Count; i++)
            {
                User.RaiseMessage("Downloading \"{0}\"", downloads[i].url);

                // We need a new variable for our closure/lambda, hence index = i.
                int index = i;

                // Schedule for us to get back progress reports.
                downloads[i].agent.DownloadProgressChanged +=
                    (sender, args) =>
                        FileProgressReport(index, args.ProgressPercentage, args.BytesReceived,
                            args.TotalBytesToReceive);

                // And schedule a notification if we're done (or if something goes wrong)
                downloads[i].agent.DownloadFileCompleted += (sender, args) => FileDownloadComplete(index, args.Error);

                // Start the download!
                downloads[i].agent.DownloadFileAsync(downloads[i].url, downloads[i].path);
            }
        }

        /// <summary>
        /// Use curlsharp to handle our downloads.
        /// </summary>
        private void DownloadCurl()
        {
            log.Debug("Curlsharp async downloader engaged");

            // Make sure our environment is set up.

            Curl.Init();

            // We'd *like* to use CurlMulti, but it just hangs when I try to retrieve
            // messages from it. So we're spawning a thread for each curleasy that does
            // the same thing. Ends up this is a little easier in handling, anyway.

            for (int i = 0; i < downloads.Count; i++)
            {
                log.DebugFormat("Downloading {0}", downloads[i].url);
                User.RaiseMessage("Downloading \"{0}\" (libcurl)", downloads[i].url);

                // Open our file, and make an easy object...
                FileStream stream = File.OpenWrite(downloads[i].path);
                CurlEasy easy = Curl.CreateEasy(downloads[i].url, stream);

                // We need a separate variable for our closure, this is it.
                int index = i;

                // Curl recommends xferinfofunction, but this doesn't seem to
                // be supported by curlsharp, so we use the progress function
                // instead.
                easy.ProgressFunction = delegate(object extraData, double dlTotal, double dlNow, double ulTotal, double ulNow)
                {
                    log.DebugFormat("Progress function called... {0}/{1}", dlNow,dlTotal);

                    int percent;

                    if (dlTotal > 0)
                    {
                        percent = (int) dlNow * 100 / (int) dlTotal;
                    }
                    else
                    {
                        log.Debug("Unknown download size, skipping progress..");
                        return 0;
                    }

                    FileProgressReport(
                        index,
                        percent,
                        Convert.ToInt64(dlNow),
                        Convert.ToInt64(dlTotal)
                    );

                    // If the user has told us to cancel, then bail out now.
                    if (download_canceled)
                    {
                        log.InfoFormat("Bailing out of download {0} at user request", index);
                        // Bail out!
                        return 1;
                    }

                    // Returning 0 means we want to continue the download.
                    return 0;
                };

                // Download, little curl, fulfill your destiny!
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    CurlWatchThread(index, easy, stream);
                }));

                // Keep track of our threads so we can clean them up later.
                curl_threads.Add(thread);

                // Background threads will mostly look after themselves.
                thread.IsBackground = true;

                // Let's go!
                thread.Start();
            }
        }

        /// <summary>
        /// Starts a thread to watch download progress. Invoked by DownloadCUrl. Not for
        /// public consumption.
        /// </summary>
        private void CurlWatchThread(int index, CurlEasy easy, FileStream stream)
        {
            log.Debug("Curlsharp download thread started");

            // This should run until completion or failture.
            CurlCode result = easy.Perform();

            log.Debug("Curlsharp download complete");

            // Dispose of all our disposables.
            // We have to do this *BEFORE* we call FileDownloadComplete, as it
            // ensure we've written everything out to disk.
            stream.Dispose();
            easy.Dispose();

            if (result == CurlCode.Ok)
            {
                FileDownloadComplete(index, null);
            }
            else
            {
                // The CurlCode result expands to a human-friendly string, so we can just
                // throw a kraken containing it and nothing else. The FileDownloadComplete
                // code collects these into a larger DownloadErrorsKraken aggregate.

                FileDownloadComplete(
                    index,
                    new Kraken(result.ToString())
                );
            }
        }
        
        public void DownloadAndWait(ICollection<KeyValuePair<Uri, long>> urls)
        {
            // Start the download!
            Download(urls);

            log.Debug("Waiting for downloads to finish...");
            complete_or_canceled.WaitOne();

            var old_download_canceled = download_canceled;
            // Set up the inter-thread comms for next time. Can not be done at the start
            // of the method as the thread could pause on the opening line long enough for
            // a user to cancel.

            download_canceled = false;
            complete_or_canceled.Reset();


            // If the user cancelled our progress, then signal that.
            // This *should* be harmless if we're using the curlsharp downloader,
            // which watches for downloadCanceled all by itself. :)
            if (old_download_canceled)
            {
                // Abort all our traditional downloads, if there are any.
                foreach (var download in downloads.ToList())
                {
                    download.agent.CancelAsync();
                }

                // Abort all our curl downloads, if there are any.
                foreach (var thread in curl_threads.ToList())
                {
                    thread.Abort();
                }

                // Signal to the caller that the user cancelled the download.
                throw new CancelledActionKraken("Download cancelled by user");
            }

            // Check to see if we've had any errors. If so, then release the kraken!
            var exceptions = downloads
                .Select(x => x.error)
                .Where(ex => ex != null)
                .ToList();

            // Let's check if any of these are certificate errors. If so,
            // we'll report that instead, as this is common (and user-fixable)
            // under Linux.
            if (exceptions.Any(ex => ex is WebException &&
                Regex.IsMatch(ex.Message, "authentication or decryption has failed")))
            {
                throw new MissingCertificateKraken();
            }

            if (exceptions.Count > 0)
            {
                throw new DownloadErrorsKraken(exceptions);
            }

            // Yay! Everything worked!
        }

        /// <summary>
        /// <see cref="IDownloader.CancelDownload()"/>
        /// This will also call onCompleted with all null arguments.
        /// </summary>
        public void CancelDownload()
        {
            log.Info("Cancelling download");
            download_canceled = true;
            triggerCompleted(null, null, null);
        }

        private void triggerCompleted(Uri[] file_urls, string[] file_paths, Exception[] errors)
        {
            if (onCompleted != null)
            {
                onCompleted.Invoke(file_urls, file_paths, errors);
            }
            // Signal that we're done.
            complete_or_canceled.Set();
            User.RaiseDownloadsCompleted(file_urls, file_paths, errors);
        }

        /// <summary>
        /// Generates a download progress reports, and sends it to
        /// onProgressReport if it's set. This takes the index of the file
        /// being downloaded, the percent complete, the bytes downloaded,
        /// and the total amount of bytes we expect to download.
        /// </summary>
        private void FileProgressReport(int index, int percent, long bytesDownloaded, long bytesToDownload)
        {
            if (download_canceled)
            {
                return;
            }

            NetAsyncDownloaderDownloadPart download = downloads[index];

            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - download.lastProgressUpdateTime;
            if (timeSpan.Seconds >= 3.0)
            {
                long bytesChange = bytesDownloaded - download.lastProgressUpdateSize;
                download.lastProgressUpdateSize = (int) bytesDownloaded;
                download.lastProgressUpdateTime = now;
                download.bytesPerSecond = (int) bytesChange/timeSpan.Seconds;
            }

            download.size = bytesToDownload;
            download.bytesLeft = download.size - bytesDownloaded;
            downloads[index] = download;

            int totalBytesPerSecond = 0;
            long totalBytesLeft = 0;
            long totalSize = 0;

            foreach (NetAsyncDownloaderDownloadPart t in downloads.ToList())
            {
                if (t.bytesLeft > 0)
                {
                    totalBytesPerSecond += t.bytesPerSecond;
                }

                totalBytesLeft += t.bytesLeft;
                totalSize += t.size;
            }

            int totalPercentage = (int)(((totalSize - totalBytesLeft) * 100) / (totalSize));

            if (!download_canceled)
            {
                // Math.Ceiling was added to avoid showing 0 MiB left when finishing
                User.RaiseProgress(
                    String.Format("{0} kbps - downloading - {1:f0} MB left",
                        totalBytesPerSecond/1024,
                        Math.Ceiling((double)totalBytesLeft/1024/1024)),
                    totalPercentage);
            }
        }

        /// <summary>
        /// This method gets called back by `WebClient` or our
        /// curl downloader when a download is completed. It in turn
        /// calls the onCompleted hook when *all* downloads are finished.
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
                triggerCompleted(fileUrls, filePaths, errors);
            }
        }
    }
}
