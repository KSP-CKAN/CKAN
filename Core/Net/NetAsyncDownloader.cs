using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using Autofac;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public partial class NetAsyncDownloader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncDownloader));

        public readonly IUser User;

        /// <summary>
        /// Raised when data arrives for a download
        /// </summary>
        public event Action<DownloadTarget, long, long> Progress;

        private readonly object dlMutex = new object();
        // NOTE: Never remove anything from this, because closures have indexes into it!
        // (Clearing completely after completion is OK)
        private readonly List<DownloadPart> downloads       = new List<DownloadPart>();
        private readonly List<DownloadPart> queuedDownloads = new List<DownloadPart>();
        private int completed_downloads;

        // For inter-thread communication
        private volatile bool download_canceled;
        private readonly ManualResetEvent complete_or_canceled;

        /// <summary>
        /// Invoked when a download completes or fails.
        /// </summary>
        /// <param>The download that is done</param>
        /// <param>Exception thrown if failed</param>
        /// <param>ETag of the URL</param>
        public event Action<DownloadTarget, Exception, string> onOneCompleted;

        /// <summary>
        /// Returns a perfectly boring NetAsyncDownloader
        /// </summary>
        public NetAsyncDownloader(IUser user)
        {
            User = user;
            complete_or_canceled = new ManualResetEvent(false);
        }

        public static string DownloadWithProgress(string url, string filename = null, IUser user = null)
            => DownloadWithProgress(new Uri(url), filename, user);

        public static string DownloadWithProgress(Uri url, string filename = null, IUser user = null)
        {
            var targets = new[]
            {
                new DownloadTargetFile(url, filename)
            };
            DownloadWithProgress(targets, user);
            return targets.First().filename;
        }

        public static void DownloadWithProgress(IList<DownloadTarget> downloadTargets, IUser user = null)
        {
            var downloader = new NetAsyncDownloader(user ?? new NullUser());
            downloader.onOneCompleted += (target, error, etag) =>
            {
                if (error != null)
                {
                    user?.RaiseError(error.ToString());
                }
            };
            downloader.DownloadAndWait(downloadTargets);
        }

        /// <summary>
        /// Start a new batch of downloads
        /// </summary>
        /// <param name="targets">The downloads to begin</param>
        public void DownloadAndWait(IList<DownloadTarget> targets)
        {
            lock (dlMutex)
            {
                if (downloads.Count + queuedDownloads.Count > completed_downloads)
                {
                    // Some downloads are still in progress, add to the current batch
                    foreach (var target in targets)
                    {
                        DownloadModule(new DownloadPart(target));
                    }
                    // Wait for completion along with original caller
                    // so we can handle completion tasks for the added mods
                    complete_or_canceled.WaitOne();
                    return;
                }

                completed_downloads = 0;
                // Make sure we are ready to start a fresh batch
                complete_or_canceled.Reset();

                // Start the downloads!
                Download(targets);
            }

            log.Debug("Waiting for downloads to finish...");
            complete_or_canceled.WaitOne();

            log.Debug("Downloads finished");

            var old_download_canceled = download_canceled;
            // Set up the inter-thread comms for next time. Can not be done at the start
            // of the method as the thread could pause on the opening line long enough for
            // a user to cancel.

            download_canceled = false;
            complete_or_canceled.Reset();

            log.Debug("Completion signal reset");

            // If the user cancelled our progress, then signal that.
            if (old_download_canceled)
            {
                log.DebugFormat("User clicked cancel, discarding {0} queued downloads: {1}", queuedDownloads.Count, string.Join(", ", queuedDownloads.SelectMany(dl => dl.target.urls)));
                // Ditch anything we haven't started
                queuedDownloads.Clear();
                // Abort all our traditional downloads, if there are any.
                var inProgress = downloads.Where(dl => dl.bytesLeft > 0 && dl.error == null).ToList();
                log.DebugFormat("Telling {0} in progress downloads to abort: {1}", inProgress.Count, string.Join(", ", inProgress.SelectMany(dl => dl.target.urls)));
                foreach (var download in inProgress)
                {
                    log.DebugFormat("Telling download of {0} to abort", string.Join(", ", download.target.urls));
                    download.Abort();
                    log.DebugFormat("Done requesting abort of {0}", string.Join(", ", download.target.urls));
                }

                log.Debug("Throwing cancellation kraken");
                // Signal to the caller that the user cancelled the download.
                throw new CancelledActionKraken(Properties.Resources.NetAsyncDownloaderCancelled);
            }

            // Check to see if we've had any errors. If so, then release the kraken!
            var exceptions = new List<KeyValuePair<int, Exception>>();
            for (int i = 0; i < downloads.Count; ++i)
            {
                if (downloads[i].error != null)
                {
                    // Check if it's a certificate error. If so, report that instead,
                    // as this is common (and user-fixable) under Linux.
                    if (downloads[i].error is WebException)
                    {
                        WebException wex = downloads[i].error as WebException;
                        if (certificatePattern.IsMatch(wex.Message))
                        {
                            throw new MissingCertificateKraken();
                        }
                        #pragma warning disable IDE0011
                        else switch ((wex.Response as HttpWebResponse)?.StatusCode)
                        #pragma warning restore IDE0011
                        {
                            // Handle HTTP 403 used for throttling
                            case HttpStatusCode.Forbidden:
                                Uri infoUrl = null;
                                var throttledUri = downloads[i].target.urls.FirstOrDefault(uri =>
                                    uri.IsAbsoluteUri
                                    && Net.ThrottledHosts.TryGetValue(uri.Host, out infoUrl));
                                if (throttledUri != null)
                                {
                                    throw new DownloadThrottledKraken(throttledUri, infoUrl);
                                }
                                break;
                        }
                    }
                    // Otherwise just note the error and which download it came from,
                    // then throw them all at once later.
                    exceptions.Add(new KeyValuePair<int, Exception>(
                        targets.IndexOf(downloads[i].target), downloads[i].error));
                }
            }
            if (exceptions.Count > 0)
            {
                throw new DownloadErrorsKraken(exceptions);
            }

            // Yay! Everything worked!
            log.Debug("Done downloading");
        }

        private static readonly Regex certificatePattern = new Regex(
            @"authentication or decryption has failed",
            RegexOptions.Compiled
        );

        /// <summary>
        /// <see cref="IDownloader.CancelDownload()"/>
        /// This will also call onCompleted with all null arguments.
        /// </summary>
        public void CancelDownload()
        {
            log.Info("Cancelling download");
            download_canceled = true;
            triggerCompleted();
        }

        /// <summary>
        /// Downloads our files.
        /// </summary>
        /// <param name="targets">A collection of DownloadTargets</param>
        private void Download(ICollection<DownloadTarget> targets)
        {
            downloads.Clear();
            queuedDownloads.Clear();
            foreach (var t in targets)
            {
                DownloadModule(new DownloadPart(t));
            }
        }

        private void DownloadModule(DownloadPart dl)
        {
            if (shouldQueue(dl.CurrentUri))
            {
                if (!queuedDownloads.Contains(dl))
                {
                    log.DebugFormat("Enqueuing download of {0}", string.Join(", ", dl.target.urls));
                    // Throttled host already downloading, we will get back to this later
                    queuedDownloads.Add(dl);
                }
            }
            else
            {
                log.DebugFormat("Beginning download of {0}", string.Join(", ", dl.target.urls));

                lock (dlMutex)
                {
                    if (!downloads.Contains(dl))
                    {
                        // We need a new variable for our closure/lambda, hence index = 1+prev max
                        int index = downloads.Count;

                        downloads.Add(dl);

                        // Schedule for us to get back progress reports.
                        dl.Progress += (ProgressPercentage, BytesReceived, TotalBytesToReceive) =>
                            FileProgressReport(index, BytesReceived, TotalBytesToReceive);

                        // And schedule a notification if we're done (or if something goes wrong)
                        dl.Done += (sender, args, etag) =>
                            FileDownloadComplete(index, args.Error, args.Cancelled, etag);
                    }
                    queuedDownloads.Remove(dl);
                }

                // Encode spaces to avoid confusing URL parsers
                User.RaiseMessage(Properties.Resources.NetAsyncDownloaderDownloading,
                    dl.CurrentUri.ToString().Replace(" ", "%20"));

                // Start the download!
                dl.Download();
            }
        }

        /// <summary>
        /// Check whether a given download should be deferred to be started later.
        /// Decision is made based on whether we're already downloading something
        /// else from the same host.
        /// </summary>
        /// <param name="url">A URL we want to download</param>
        /// <returns>
        /// true to queue, false to start immediately
        /// </returns>
        private bool shouldQueue(Uri url)
            => !url.IsFile && url.IsAbsoluteUri
               // Ignore inactive downloads
               && downloads.Except(queuedDownloads)
                           .Any(dl => dl.CurrentUri != url
                                      // Look for active downloads from the same host
                                      && (dl.CurrentUri.IsAbsoluteUri && dl.CurrentUri.Host == url.Host)
                                      // Consider done if no bytes left
                                      && dl.bytesLeft > 0
                                      // Consider done if already tried and failed
                                      && dl.error == null);

        private void triggerCompleted()
        {
            // Signal that we're done.
            complete_or_canceled.Set();
        }

        /// <summary>
        /// Generates a download progress report.
        /// </summary>
        /// <param name="index">Index of the file being downloaded</param>
        /// <param name="percent">The percent complete</param>
        /// <param name="bytesDownloaded">The bytes downloaded</param>
        /// <param name="bytesToDownload">The total amount of bytes we expect to download</param>
        private void FileProgressReport(int index, long bytesDownloaded, long bytesToDownload)
        {
            DownloadPart download = downloads[index];

            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - download.lastProgressUpdateTime;
            if (timeSpan.Seconds >= 3.0)
            {
                long bytesChange = bytesDownloaded - download.lastProgressUpdateSize;
                download.lastProgressUpdateSize = bytesDownloaded;
                download.lastProgressUpdateTime = now;
                download.bytesPerSecond = bytesChange / timeSpan.Seconds;
            }

            download.size = bytesToDownload;
            download.bytesLeft = download.size - bytesDownloaded;

            Progress?.Invoke(download.target, download.bytesLeft, download.size);

            long totalBytesPerSecond = 0;
            long totalBytesLeft = 0;
            long totalSize = 0;

            lock (dlMutex)
            {
                foreach (var t in downloads)
                {
                    if (t == null)
                    {
                        continue;
                    }

                    if (t.bytesLeft > 0)
                    {
                        totalBytesPerSecond += t.bytesPerSecond;
                    }

                    totalBytesLeft += t.bytesLeft;
                    totalSize += t.size;
                }
                foreach (var dl in queuedDownloads)
                {
                    // Somehow managed to get a NullRef for dl here
                    if (dl == null)
                    {
                        continue;
                    }

                    totalBytesLeft += dl.target.size;
                    totalSize += dl.target.size;
                }
            }

            int totalPercentage = (int)(((totalSize - totalBytesLeft) * 100) / (totalSize));
            User.RaiseProgress(totalPercentage, totalBytesPerSecond, totalBytesLeft);
        }

        private void PopFromQueue(string host)
        {
            // Make sure the threads don't trip on one another
            lock (dlMutex)
            {
                var next = queuedDownloads.FirstOrDefault(qDl =>
                    !qDl.CurrentUri.IsAbsoluteUri || qDl.CurrentUri.Host == host);
                if (next != null)
                {
                    log.DebugFormat("Attempting to start queued download {0}", string.Join(", ", next.target.urls));
                    // Start this host's next queued download
                    DownloadModule(next);
                }
            }
        }

        /// <summary>
        /// This method gets called back by `WebClient` when a download is completed.
        /// It in turncalls the onCompleted hook when *all* downloads are finished.
        /// </summary>
        private void FileDownloadComplete(int index, Exception error, bool canceled, string etag)
        {
            var dl      = downloads[index];
            var doneUri = dl.CurrentUri;
            if (error != null)
            {
                log.InfoFormat("Error downloading {0}: {1}", doneUri, error.Message);

                // Check whether there are any alternate download URLs remaining
                if (!canceled && dl.HaveMoreUris)
                {
                    dl.NextUri();
                    // Either re-queue this or start the next one, depending on active downloads
                    DownloadModule(dl);
                    // Check the host that just failed for queued downloads
                    PopFromQueue(doneUri.Host);
                    // Short circuit the completion process so the fallback can run
                    return;
                }
                else
                {
                    dl.error = error;
                }
            }
            else
            {
                log.InfoFormat("Finished downloading {0}", string.Join(", ", dl.target.urls));
                dl.bytesLeft = 0;
                // Let calling code find out how big this file is
                dl.target.CalculateSize();
            }

            PopFromQueue(doneUri.Host);

            try
            {
                // Tell calling code that this file is ready
                onOneCompleted?.Invoke(dl.target, dl.error, etag);
            }
            catch (Exception exc)
            {
                if (dl.error == null)
                {
                    // Capture anything that goes wrong with the post-download process as well
                    dl.error = exc;
                }
            }

            // Make sure the threads don't trip on one another
            lock (dlMutex)
            {
                if (++completed_downloads >= downloads.Count + queuedDownloads.Count)
                {
                    log.DebugFormat("Triggering completion at {0} completed, {1} started, {2} queued", completed_downloads, downloads.Count, queuedDownloads.Count);
                    triggerCompleted();
                }
            }
        }
    }
}
