using System;
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
        void DownloadModules(IEnumerable<CkanModule> modules);

        event Action<ByteRateCounter> OverallDownloadProgress;

        /// <summary>
        /// Raised when data arrives for a module
        /// </summary>
        event Action<CkanModule, long, long> DownloadProgress;

        /// <summary>
        /// Raised while we are checking that a ZIP is valid
        /// </summary>
        event Action<CkanModule, long, long> StoreProgress;

        /// <summary>
        /// Raised when one module finishes
        /// </summary>
        event Action<CkanModule> OneComplete;

        /// <summary>
        /// Raised when a batch of downloads is all done
        /// </summary>
        event Action AllComplete;

        IEnumerable<CkanModule> ModulesAsTheyFinish(ICollection<CkanModule> cached,
                                                    ICollection<CkanModule> toDownload);
    }
}
