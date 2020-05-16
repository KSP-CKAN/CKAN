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
        private IUser User
        {
            get { return downloader.User;  }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof (NetAsyncModulesDownloader));

        private const    string             defaultMimeType = "application/octet-stream";

        public event Action<CkanModule, long, long> Progress;
        public event Action                         AllComplete;

        private          List<CkanModule>   modules;
        private readonly NetAsyncDownloader downloader;
        private readonly NetModuleCache     cache;

        /// <summary>
        /// Returns a perfectly boring NetAsyncModulesDownloader.
        /// </summary>
        public NetAsyncModulesDownloader(IUser user, NetModuleCache cache)
        {
            modules    = new List<CkanModule>();
            downloader = new NetAsyncDownloader(user)
            {
                // Schedule us to process each module on completion.
                onOneCompleted = ModuleDownloadComplete
            };
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

            this.modules.AddRange(unique_downloads.Values);

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
                if (AllComplete != null)
                {
                    AllComplete();
                }
            }
            catch (DownloadErrorsKraken kraken)
            {
                // Associate the errors with the affected modules
                throw new ModuleDownloadErrorsKraken(this.modules, kraken);
            }
        }

        private void ModuleDownloadComplete(Uri url, string filename, Exception error)
        {
            if (error != null)
            {
                User.RaiseError(error.ToString());
            }
            else
            {
                // Cache if this download succeeded
                try
                {
                    CkanModule module = modules.First(m => m.download == url);
                    cache.Store(module, filename, module.StandardName());
                    File.Delete(filename);
                }
                catch (InvalidModuleFileKraken kraken)
                {
                    User.RaiseError(kraken.ToString());
                }
                catch (FileNotFoundException e)
                {
                    log.WarnFormat("cache.Store(): FileNotFoundException: {0}", e.Message);
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
