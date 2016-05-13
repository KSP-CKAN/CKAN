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
    public class NetAsyncModulesDownloader : IDownloader
    {
        public IUser User
        {
            get { return downloader.User; }
            set { downloader.User = value; }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncModulesDownloader));
        
        private List<CkanModule> modules;
        private readonly NetAsyncDownloader downloader;

        /// <summary>
        /// Returns a perfectly boring NetAsyncModulesDownloader.
        /// </summary>
        public NetAsyncModulesDownloader(IUser user)
        {
            modules = new List<CkanModule>();
            downloader = new NetAsyncDownloader(user);
        }


        /// <summary>
        /// <see cref="IDownloader.DownloadModules(NetFileCache, IEnumerable{CkanModule})"/>
        /// </summary>
        public void DownloadModules(
            NetFileCache cache,
            IEnumerable<CkanModule> modules
            )
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

            // retrieve the expected download size for each mod
            List<KeyValuePair<Uri, long>> downloads_with_size = unique_downloads
                .Select(item => new KeyValuePair<Uri, long>(item.Key, item.Value.download_size))
                .ToList();

            // Start the download!
            downloader.DownloadAndWait(downloads_with_size);
        }

        /// <summary>
        /// Stores all of our files in the cache once done.
        /// Called by NetAsyncDownloader on completion.
        /// Called with all nulls on download cancellation.
        /// </summary>
        private void ModuleDownloadsComplete(NetFileCache cache, Uri[] urls, string[] filenames, Exception[] errors)
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
