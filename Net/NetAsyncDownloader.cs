using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using ChinhDo.Transactions;
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
            public long bytesDownloaded;
            public long bytesLeft;
            public int bytesPerSecond;
            public Exception error;
            public int lastProgressUpdateSize;

            public NetAsyncDownloaderDownloadPart(Uri url, long expectedSize, string path = null)
            {
                this.url = url;
                this.path = path ?? Path.GetTempFileName();
                this.bytesLeft = expectedSize == 0 ? -1 : expectedSize;
                lastProgressUpdateTime = DateTime.Now;

                agent.Headers.Add("user-agent", Net.UserAgentString);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncDownloader));

        private List<NetAsyncDownloaderDownloadPart> downloads;
        private List<CkanModule> modules;
        private int completed_downloads;

        private Object download_complete_lock = new Object();

        private bool downloadCanceled;

        // Called on completion (including on error)
        // Called with ALL NULLS on error.
        // Can be set by ourself in the DownloadModules method.
        private delegate void NetAsyncCompleted(Uri[] urls, string[] filenames, Exception[] errors);
        private NetAsyncCompleted onCompleted;

        /// <summary>
        /// Returns a perfectly boring NetAsyncDownloader.
        /// </summary>
        public NetAsyncDownloader(IUser user)
        {
            User = user;
            downloads = new List<NetAsyncDownloaderDownloadPart>();
            modules = new List<CkanModule>();
        }

        /// <summary>
        /// Downloads our files, returning an array of filenames that we're writing to.
        /// The sole argument is a collection of KeyValuePair(s) containing the download URL and the expected download size
        /// The .onCompleted delegate will be called on completion.
        /// </summary>
        public string[] Download(ICollection<KeyValuePair<Uri, long>> urls)
        {
            foreach (var download in urls.Select(url => new NetAsyncDownloaderDownloadPart(url.Key, url.Value)))
            {
                downloads.Add(download);
            }

            var filePaths = new string[downloads.Count];

            for (int i = 0; i < downloads.Count; i++)
            {
                User.RaiseMessage("Downloading \"{0}\"", downloads[i].url);

                // We need a new variable for our closure/lambda, hence index = i.
                int index = i;

                // Schedule for us to get back progress reports.
                downloads[i].agent.DownloadProgressChanged +=
                    (sender, args) =>
                        FileProgressReport(index, args.ProgressPercentage, args.BytesReceived,
                            args.TotalBytesToReceive - args.BytesReceived);

                // And schedule a notification if we're done (or if something goes wrong)
                downloads[i].agent.DownloadFileCompleted += (sender, args) => FileDownloadComplete(index, args.Error);

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
            IEnumerable<CkanModule> modules
            )
        {
            var unique_downloads = new Dictionary<Uri, CkanModule>();

            // Walk through all our modules, but only keep the first of each
            // one that has a unique download path.
            foreach (CkanModule module in modules.Where(module => !unique_downloads.ContainsKey(module.download)))
            {
                unique_downloads[module.download] = module;
            }
            this.modules.AddRange(unique_downloads.Values);

            // Attach our progress report, if requested.            
            onCompleted =
                (_uris, paths, errors) =>
                    ModuleDownloadsComplete(cache, _uris, paths, errors);

            // retrieve the expected download size for each mod
            List<KeyValuePair<Uri, long>> downloads_with_size = new List<KeyValuePair<Uri, long>>();

            foreach(var item in unique_downloads)
            {
                downloads_with_size.Add(new KeyValuePair<Uri, long>(item.Key, item.Value.download_size));
            }

            // Start the download!
            Download(downloads_with_size);

            // The Monitor.Wait function releases a lock, and then waits until it can re-acquire it.
            // Elsewhere, our downloading callback pulses the lock, which causes us to wake up and
            // continue.
            lock (download_complete_lock)
            {
                log.Debug("Waiting for downloads to finish...");
                Monitor.Wait(download_complete_lock);
            }

            // If the user cancelled our progress, then signal that.
            if (downloadCanceled)
            {
                foreach (var download in downloads)
                {
                    download.agent.CancelAsync();
                }

                throw new CancelledActionKraken("Download cancelled by user");
            }

            // Check to see if we've had any errors. If so, then release the kraken!
            List<Exception> exceptions = downloads
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
        /// Stores all of our files in the cache once done.
        /// Called by NetAsyncDownloader on completion.
        /// Called with all nulls on download cancellation.
        /// </summary>
        private void ModuleDownloadsComplete(NetFileCache cache, Uri[] urls, string[] filenames,
            Exception[] errors)
        {
            if (urls != null)
            {
                // spawn up to 3 dialogs
                int errorDialogsLeft = 3;

                for (int i = 0; i < errors.Length; i++)
                {
                    if (errors[i] != null)
                    {
                        if (errorDialogsLeft > 0)
                        {
                            User.RaiseError("Failed to download \"{0}\" - error: {1}", urls[i], errors[i].Message);
                            errorDialogsLeft--;
                        }
                    }
                    else
                    {   
                        // Even if some of our downloads failed, we want to cache the
                        // ones which succeeded.

                        // This doesn't work :( 
                        // for some reason the tmp files get deleted before we get here and we get a nasty exception
                        // not only that but then we try _to install_ the rest of the mods and then CKAN crashes
                        // and the user's registry gets corrupted forever
                        // commenting out until this is resolved
                        // ~ nlight

                        try
                        {
                            cache.Store(urls[i], filenames[i], modules[i].StandardName());
                        }
                        catch (FileNotFoundException e)
                        {
                            log.WarnFormat("cache.Store(): FileNotFoundException: {0}", e.Message);
                        }
                    }
                }
            }

            // TODO: If we've had our download cancelled, how do we clean our tmpfiles?

            if (filenames != null)
            {
                // Finally, remove all our temp files.
                // We probably *could* have used Store's integrated move function above, but if we managed
                // to somehow get two URLs the same in our download set, that could cause right troubles!

                foreach (string tmpfile in filenames)
                {
                    log.DebugFormat("Cleaning up {0}", tmpfile);
                    File.Delete(tmpfile);
                }
            }

            // Signal that we're done.
            lock (download_complete_lock)
            {
                Monitor.Pulse(download_complete_lock);
            }
            User.RaiseDownloadsCompleted(urls, filenames, errors);
        }

        /// <summary>
        /// Cancel any running downloads. This will also call onCompleted with
        /// all null arguments.
        /// </summary>
        public void CancelDownload()
        {
            log.Debug("Cancelling download");

            downloadCanceled = true;

            lock (download_complete_lock)
            {
                Monitor.Pulse(download_complete_lock);
            }

            if (onCompleted != null)
            {
                onCompleted(null, null, null);
            }
        }

        /// <summary>
        /// Generates a download progress reports, and sends it to
        /// onProgressReport if it's set.
        /// </summary>
        private void FileProgressReport(int index, int percent, long bytesDownloaded, long bytesLeft)
        {
            if (downloadCanceled)
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

            if (download.bytesLeft == -1)
            {
                download.bytesLeft = bytesLeft;
            }

            download.bytesDownloaded = bytesDownloaded;
            downloads[index] = download;


            int totalPercentage = 0;
            int totalBytesPerSecond = 0;
            long totalBytesLeft = 0;
            long totalBytesDownloaded = 0;

            foreach (NetAsyncDownloaderDownloadPart t in downloads.ToList())
            {
                if (t.bytesLeft > 0)
                {
                    totalBytesPerSecond += t.bytesPerSecond;
                }

                totalBytesLeft += t.bytesLeft;
                totalBytesDownloaded += t.bytesDownloaded;
                totalBytesLeft += t.bytesLeft;
            }
            totalPercentage = (int) ((totalBytesDownloaded*100)/(totalBytesLeft + totalBytesDownloaded + 1));

            if (!downloadCanceled)
            {
                User.RaiseProgress(
                    String.Format("{0} kbps - downloading - {1} MiB left",
                        totalBytesPerSecond/1024,
                        (totalBytesLeft-totalBytesDownloaded)/1024/1024),
                    totalPercentage);
            }
        }

        /// <summary>
        /// This method gets called back by `WebClient` when a download is completed.
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
                onCompleted(fileUrls, filePaths, errors);
            }
        }
    }
}
