using System.Collections.Generic;

namespace CKAN
{
    //Todo. Have not specified the user of IUser to report progress as part of the interface
    // Need to decide if we wish to include a nicer method of reporting such a callbacks or events.
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