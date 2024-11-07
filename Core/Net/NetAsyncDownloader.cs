using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        public  readonly IUser  User;
        private readonly string userAgent;
        private readonly Func<HashAlgorithm?> getHashAlgo;

        /// <summary>
        /// Raised when data arrives for a download
        /// </summary>
        public event Action<DownloadTarget, long, long>? TargetProgress;
        public event Action<ByteRateCounter>?            OverallProgress;

        private readonly object dlMutex = new object();
        private readonly List<DownloadPart> downloads       = new List<DownloadPart>();
        private readonly List<DownloadPart> queuedDownloads = new List<DownloadPart>();
        private int completed_downloads;

        private readonly ByteRateCounter rateCounter = new ByteRateCounter();

        // For inter-thread communication
        private volatile bool download_canceled;
        private readonly ManualResetEvent complete_or_canceled;
        private readonly CancellationToken cancelToken;

        /// <summary>
        /// Invoked when a download completes or fails.
        /// </summary>
        /// <param>The download that is done</param>
        /// <param>Exception thrown if failed</param>
        /// <param>ETag of the URL</param>
        public event Action<DownloadTarget, Exception?, string?, string>? onOneCompleted;

        /// <summary>
        /// Returns a perfectly boring NetAsyncDownloader
        /// </summary>
        public NetAsyncDownloader(IUser user,
                                  Func<HashAlgorithm?> getHashAlgo,
                                  string? userAgent = null,
                                  CancellationToken cancelToken = default)
        {
            User = user;
            this.userAgent = userAgent ?? Net.UserAgentString;
            this.getHashAlgo = getHashAlgo;
            this.cancelToken = cancelToken;
            complete_or_canceled = new ManualResetEvent(false);
        }

        public static void DownloadWithProgress(IList<DownloadTarget> downloadTargets,
                                                string?               userAgent,
                                                IUser?                user        = null)
        {
            var downloader = new NetAsyncDownloader(user ?? new NullUser(), () => null, userAgent);
            downloader.onOneCompleted += (target, error, etag, hash) =>
            {
                if (error != null)
                {
                    user?.RaiseError("{0}", error.ToString());
                }
            };
            downloader.DownloadAndWait(downloadTargets);
        }

        /// <summary>
        /// Start a new batch of downloads
        /// </summary>
        /// <param name="targets">The downloads to begin</param>
        public void DownloadAndWait(ICollection<DownloadTarget> targets)
        {
            lock (dlMutex)
            {
                if (downloads.Count + queuedDownloads.Count > completed_downloads)
                {
                    // Some downloads are still in progress, add to the current batch
                    foreach (var target in targets)
                    {
                        DownloadModule(new DownloadPart(target, userAgent, SHA256.Create()));
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

            rateCounter.Start();

            log.Debug("Waiting for downloads to finish...");
            complete_or_canceled.WaitOne();

            rateCounter.Stop();

            log.Debug("Downloads finished");

            var old_download_canceled = download_canceled;
            // Set up the inter-thread comms for next time. Can not be done at the start
            // of the method as the thread could pause on the opening line long enough for
            // a user to cancel.

            download_canceled = false;
            complete_or_canceled.Reset();

            log.Debug("Completion signal reset");

            // If the user cancelled our progress, then signal that.
            if (old_download_canceled || cancelToken.IsCancellationRequested)
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
            var exceptions = downloads.Select(dl => dl.error != null
                                                        ? new KeyValuePair<DownloadTarget, Exception>(dl.target, dl.error)
                                                        : (KeyValuePair<DownloadTarget, Exception>?)null)
                                      .OfType<KeyValuePair<DownloadTarget, Exception>>()
                                      .ToList();

            if (exceptions.Select(kvp => kvp.Value)
                          .OfType<WebException>()
                          // Check if it's a certificate error. If so, report that instead,
                          // as this is common (and user-fixable) under Linux.
                          .Any(exc => exc.Status == WebExceptionStatus.SecureChannelFailure))
            {
                throw new MissingCertificateKraken();
            }

            var throttled = exceptions.Select(kvp => kvp.Value is WebException wex
                                                     && wex.Response is HttpWebResponse hresp
                                                     // Handle HTTP 403 used for throttling
                                                     && hresp.StatusCode == HttpStatusCode.Forbidden
                                                     && kvp.Key.urls.LastOrDefault() is Uri url
                                                     && url.IsAbsoluteUri
                                                     && Net.ThrottledHosts.TryGetValue(url.Host, out Uri? infoUrl)
                                                     && infoUrl is not null
                                                         ? new DownloadThrottledKraken(url, infoUrl)
                                                         : null)
                                      .OfType<DownloadThrottledKraken>()
                                      .FirstOrDefault();
            if (throttled is not null)
            {
                throw throttled;
            }

            if (exceptions.Count > 0)
            {
                throw new DownloadErrorsKraken(exceptions);
            }

            // Yay! Everything worked!
            log.Debug("Done downloading");
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
                DownloadModule(new DownloadPart(t, userAgent, getHashAlgo?.Invoke()));
            }
        }

        private void DownloadModule(DownloadPart dl)
        {
            if (shouldQueue(dl.CurrentUri))
            {
                if (!queuedDownloads.Contains(dl))
                {
                    log.DebugFormat("Enqueuing download of {0}", dl.CurrentUri);
                    // Throttled host already downloading, we will get back to this later
                    queuedDownloads.Add(dl);
                }
            }
            else
            {
                log.DebugFormat("Beginning download of {0}", dl.CurrentUri);

                lock (dlMutex)
                {
                    if (!downloads.Contains(dl))
                    {
                        downloads.Add(dl);

                        // Schedule for us to get back progress reports.
                        dl.Progress += FileProgressReport;

                        // And schedule a notification if we're done (or if something goes wrong)
                        dl.Done += FileDownloadComplete;
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
        /// <param name="download">The download that progressed</param>
        /// <param name="percent">The percent complete</param>
        /// <param name="bytesDownloaded">The bytes downloaded</param>
        /// <param name="bytesToDownload">The total amount of bytes we expect to download</param>
        private void FileProgressReport(DownloadPart download, long bytesDownloaded, long bytesToDownload)
        {
            download.size      = bytesToDownload;
            download.bytesLeft = download.size - bytesDownloaded;
            TargetProgress?.Invoke(download.target, download.bytesLeft, download.size);

            lock (dlMutex)
            {
                var queuedSize = queuedDownloads.Sum(dl => dl.target.size);
                rateCounter.Size      = queuedSize + downloads.Sum(dl => dl.size);
                rateCounter.BytesLeft = queuedSize + downloads.Sum(dl => dl.bytesLeft);
            }

            OverallProgress?.Invoke(rateCounter);

            if (cancelToken.IsCancellationRequested)
            {
                download_canceled = true;
                triggerCompleted();
            }
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
        private void FileDownloadComplete(DownloadPart dl,
                                          Exception?   error,
                                          bool         canceled,
                                          string?      etag,
                                          string       hash)
        {
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
                onOneCompleted?.Invoke(dl.target, dl.error, etag, hash);
            }
            catch (Exception exc)
            {
                // Capture anything that goes wrong with the post-download process as well
                dl.error ??= exc;
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
