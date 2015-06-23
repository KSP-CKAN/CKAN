using System.Collections.Generic;

namespace CKAN
{
    public interface IDownloader
    {
        /// <summary>
        /// Downloads all the modules specified to the cache.
        /// Even if modules share download URLs, they will only be downloaded once.
        /// Blocks until the downloads are complete, cancelled, or errored.
        /// </summary>
        void DownloadModules(NetFileCache cache, IEnumerable<CkanModule> modules);

        /// <summary>
        /// Cancel any running downloads.
        /// </summary>
        void CancelDownload();
    }
}