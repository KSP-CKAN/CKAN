using System;

namespace CKAN
{
    public class TorrentDownloader : IDownloader
    {
        public IUser User { get; set; }

        public TorrentDownloader(IUser user)
        {
            User = user;
        }
        void DownloadModules(NetFileCache cache, IEnumerable<CkanModule> modules);
        void CancelDownload();
    }
}

