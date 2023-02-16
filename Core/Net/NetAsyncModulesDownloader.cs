using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using log4net;

namespace CKAN
{
    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public class NetAsyncModulesDownloader : IDownloader
    {
        public event Action<CkanModule, long, long> Progress;
        public event Action<CkanModule, long, long> StoreProgress;
        public event Action                         AllComplete;

        /// <summary>
        /// Returns a perfectly boring NetAsyncModulesDownloader.
        /// </summary>
        public NetAsyncModulesDownloader(IUser user, NetModuleCache cache)
        {
            modules    = new List<CkanModule>();
            downloader = new NetAsyncDownloader(user);
            // Schedule us to process each module on completion.
            downloader.onOneCompleted += ModuleDownloadComplete;
            downloader.Progress += (target, remaining, total) =>
            {
                var mod = modules.FirstOrDefault(m => m.download == target.url);
                if (mod != null && Progress != null)
                {
                    Progress(mod, remaining, total);
                }
            };
            this.cache = cache;
        }

        /// <summary>
        /// <see cref="IDownloader.DownloadModules(NetFileCache, IEnumerable{CkanModule})"/>
        /// </summary>
        public void DownloadModules(IEnumerable<CkanModule> modules)
        {
            // Walk through all our modules, but only keep the first of each
            // one that has a unique download path (including active downloads).
            var currentlyActive = new HashSet<Uri>(this.modules.Select(m => m.download));
            Dictionary<Uri, CkanModule> unique_downloads = modules
                .GroupBy(module => module.download)
                .Where(group => !currentlyActive.Contains(group.Key))
                .ToDictionary(group => group.Key, group => group.First());

            // Make sure we have enough space to download and cache
            cache.CheckFreeSpace(unique_downloads.Values
                .Select(m => m.download_size)
                .Sum());

            this.modules.AddRange(unique_downloads.Values);

            try
            {
                cancelTokenSrc = new CancellationTokenSource();
                // Start the downloads!
                downloader.DownloadAndWait(unique_downloads
                    .Select(item => new Net.DownloadTarget(
                        item.Key,
                        item.Value.InternetArchiveDownload,
                        cache.GetInProgressFileName(item.Value),
                        item.Value.download_size,
                        // Send the MIME type to use for the Accept header
                        // The GitHub API requires this to include application/octet-stream
                        string.IsNullOrEmpty(item.Value.download_content_type)
                            ? defaultMimeType
                            : $"{item.Value.download_content_type};q=1.0,{defaultMimeType};q=0.9"))
                    .ToList());
                this.modules.Clear();
                AllComplete?.Invoke();
            }
            catch (DownloadErrorsKraken kraken)
            {
                // Associate the errors with the affected modules
                var exc = new ModuleDownloadErrorsKraken(this.modules.ToList(), kraken);
                // Clear this.modules because we're done with these
                this.modules.Clear();
                throw exc;
            }
        }

        /// <summary>
        /// <see cref="IDownloader.CancelDownload()"/>
        /// </summary>
        public void CancelDownload()
        {
            // Cancel downloads
            downloader.CancelDownload();
            // Cancel validation/store
            cancelTokenSrc?.Cancel();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncModulesDownloader));

        private const    string                  defaultMimeType = "application/octet-stream";

        private          List<CkanModule>        modules;
        private readonly NetAsyncDownloader      downloader;
        private          IUser                   User => downloader.User;
        private readonly NetModuleCache          cache;
        private          CancellationTokenSource cancelTokenSrc;

        private void ModuleDownloadComplete(Uri url, string filename, Exception error, string etag)
        {
            log.DebugFormat("Received download completion: {0}, {1}, {2}",
                            url, filename, error?.Message);
            if (error != null)
            {
                // If there was an error in DOWNLOADING, keep the file so we can retry it later
                log.Info(error.Message);
            }
            else
            {
                // Cache if this download succeeded
                CkanModule module = null;
                try
                {
                    module = modules.First(m => m.download == url);
                    User.RaiseMessage(Properties.Resources.NetAsyncDownloaderValidating, module);
                    cache.Store(module, filename,
                        new Progress<long>(percent => StoreProgress?.Invoke(module, 100 - percent, 100)),
                        module.StandardName(),
                        false,
                        cancelTokenSrc.Token);
                    File.Delete(filename);
                }
                catch (InvalidModuleFileKraken kraken)
                {
                    User.RaiseError(kraken.ToString());
                    if (module != null)
                    {
                        // Finish out the progress bar
                        StoreProgress?.Invoke(module, 0, 100);
                    }
                    // If there was an error in STORING, delete the file so we can try it from scratch later
                    File.Delete(filename);
                }
                catch (OperationCanceledException exc)
                {
                    log.WarnFormat("Cancellation token threw, validation incomplete: {0}", filename);
                    User.RaiseMessage(exc.Message);
                    if (module != null)
                    {
                        // Finish out the progress bar
                        StoreProgress?.Invoke(module, 0, 100);
                    }
                    // Don't delete because there might be nothing wrong
                }
                catch (FileNotFoundException e)
                {
                    log.WarnFormat("cache.Store(): FileNotFoundException: {0}", e.Message);
                }
                catch (InvalidOperationException)
                {
                    log.WarnFormat("No module found for completed URL: {0}", url);
                }
            }
        }

    }
}
