using System;
using System.Collections.Generic;

namespace CKAN
{
    public class TorrentDownloader : IDownloader
    {
        private static readonly HashSet<string> torrentable_licenses = new HashSet<string> {
            "public-domain", "CC0",
            "Apache", "Apache-1.0", "Apache-2.0",
            "Artistic", "Artistic-1.0", "Artistic-2.0",
            "BSD-2-clause", "BSD-3-clause", "BSD-4-clause",
            "ISC",
            "CC-BY", "CC-BY-1.0", "CC-BY-2.0", "CC-BY-2.5", "CC-BY-3.0", "CC-BY-4.0",
            "CC-BY-SA", "CC-BY-SA-1.0", "CC-BY-SA-2.0", "CC-BY-SA-2.5", "CC-BY-SA-3.0", "CC-BY-SA-4.0",
            "CC-BY-NC", "CC-BY-NC-1.0", "CC-BY-NC-2.0", "CC-BY-NC-2.5", "CC-BY-NC-3.0", "CC-BY-NC-4.0",
            "CC-BY-NC-SA", "CC-BY-NC-SA-1.0", "CC-BY-NC-SA-2.0", "CC-BY-NC-SA-2.5", "CC-BY-NC-SA-3.0", "CC-BY-NC-SA-4.0",
            "CC-BY-NC-ND", "CC-BY-NC-ND-1.0", "CC-BY-NC-ND-2.0", "CC-BY-NC-ND-2.5", "CC-BY-NC-ND-3.0", "CC-BY-NC-ND-4.0",
            "CDDL", "CPL",
            "EFL-1.0", "EFL-2.0",
            "Expat", "MIT",
            "GPL-1.0", "GPL-2.0", "GPL-3.0",
            "LGPL-2.0", "LGPL-2.1", "LGPL-3.0",
            "GFDL-1.0", "GFDL-1.1", "GFDL-1.2", "GFDL-1.3",
            "GFDL-NIV-1.0", "GFDL-NIV-1.1", "GFDL-NIV-1.2", "GFDL-NIV-1.3",
            "LPPL-1.0", "LPPL-1.1", "LPPL-1.2", "LPPL-1.3c",
            "MPL-1.1",
            "Perl",
            "Python-2.0",
            "QPL-1.0",
            "W3C",
            "WTFPL",
            "Zlib",
            "Zope",
            //"open-source", //this is much too ambiguous - err on the side of caution. Mod author could mean "Yes you can look at my source, all other rights reserved"
            //"restricted",
            "unrestricted",
            //"unknown"
        };
        public IUser User { get; set; }

        public TorrentDownloader(IUser user)
        {
            User = user;
        }

        /// <summary>
        /// Download modules via an external torrent client when allowed, fall
        /// back on NetAsyncDownloader when not allowed.
        /// </summary>
        /// <param name="cache">Cache.</param>
        /// <param name="modules">Modules.</param>
        public void DownloadModules(NetFileCache cache, IEnumerable<CkanModule> modules)
        {
            List<CkanModule> torrentable = new List<CkanModule>();
            List<CkanModule> fallback_list = new List<CkanModule>();

            foreach (CkanModule module in modules)
            {
                if (torrentable_licenses.Contains(module.license.ToString()))
                {
                    torrentable.Add(module);
                }
                else
                {
                    fallback_list.Add(module);
                }
            }
            //run torrent downloader
            //run fallback downloader as usual
            IDownloader fallback_downloader
        }

        /// <summary>
        /// Unimplemented - can't stop arbitrary torrent clients.
        /// </summary>
        public void CancelDownload(){}
    }
}

