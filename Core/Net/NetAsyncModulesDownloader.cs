using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public class NetAsyncModulesDownloader : IDownloader
    {
        public IUser User
        {
            get { return downloader.User;  }
            set { downloader.User = value; }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncModulesDownloader));

        private          List<CkanModule>   modules;
        private readonly NetAsyncDownloader downloader;
        private const    string             defaultMimeType = "application/octet-stream";

        /// <summary>
        /// Returns a perfectly boring NetAsyncModulesDownloader.
        /// </summary>
        public NetAsyncModulesDownloader(IUser user)
        {
            modules    = new List<CkanModule>();
            downloader = new NetAsyncDownloader(user);
        }


        /// <summary>
        /// <see cref="IDownloader.DownloadModules(NetFileCache, IEnumerable{CkanModule})"/>
        /// </summary>
        public void DownloadModules(NetModuleCache cache, IEnumerable<CkanModule> modules)
        {
            // Walk through all our modules, but only keep the first of each
            // one that has a unique download path.
            Dictionary<Uri, CkanModule> unique_downloads = modules
                .GroupBy(module => module.download)
                .ToDictionary(p => p.First().download, p => p.First());

            this.modules.AddRange(unique_downloads.Values);

            // Schedule us to process our modules on completion.
            downloader.onCompleted =
                (_uris, paths, errors) =>
                    ModuleDownloadsComplete(cache, _uris, paths, errors);

            try
            {
                // Start the downloads!
                downloader.DownloadAndWait(
                    unique_downloads.Select(item => new Net.DownloadTarget(
                        item.Key,
                        item.Value.InternetArchiveDownload,
                        // Use a temp file name
                        null,
                        item.Value.download_size,
                        // Send the MIME type to use for the Accept header
                        // The GitHub API requires this to include application/octet-stream
                        string.IsNullOrEmpty(item.Value.download_content_type)
                            ? defaultMimeType
                            : $"{item.Value.download_content_type};q=1.0,{defaultMimeType};q=0.9"
                    )).ToList()
                );
            }
            catch (DownloadErrorsKraken kraken)
            {
                // Associate the errors with the affected modules
                throw new ModuleDownloadErrorsKraken(this.modules, kraken);
            }
        }

        /// <summary>
        /// Stores all of our files in the cache once done.
        /// Called by NetAsyncDownloader on completion.
        /// Called with all nulls on download cancellation.
        /// </summary>
        private void ModuleDownloadsComplete(NetModuleCache cache, Uri[] urls, string[] filenames, Exception[] errors)
        {
            if (filenames != null)
            {
                for (int i = 0; i < errors.Length; i++)
                {
                    if (errors[i] == null)
                    {
                        // Cache the downloads that succeeded.
                        try
                        {
                            cache.Store(modules[i], filenames[i], modules[i].StandardName());
                        }
                        catch (FileNotFoundException e)
                        {
                            log.WarnFormat("cache.Store(): FileNotFoundException: {0}", e.Message);
                        }
                    }
                }

                // Finally, remove all our temp files.
                // We probably *could* have used Store's integrated move function above, but if we managed
                // to somehow get two URLs the same in our download set, that could cause right troubles!
                foreach (string tmpfile in filenames)
                {
                    log.DebugFormat("Cleaning up {0}", tmpfile);
                    File.Delete(tmpfile);
                }
            }
        }

        /// <summary>
        /// <see cref="IDownloader.CancelDownload()"/>
        /// </summary>
        public void CancelDownload()
        {
            downloader.CancelDownload();
        }
    }
}
