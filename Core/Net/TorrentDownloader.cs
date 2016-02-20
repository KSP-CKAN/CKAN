using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using System.IO;
using System.Web;

namespace CKAN
{
    public class TorrentDownloader : IDownloader
    {
        private static readonly HashSet<string> torrentable_licenses = new HashSet<string>
        {
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

        private static readonly ILog log = LogManager.GetLogger(typeof(TorrentDownloader));
        private NetFileCache _cache;

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
            _cache = cache;

            foreach (CkanModule module in modules)
            {
                if (String.IsNullOrEmpty(module.btih))
                {
                    fallback_list.Add(module);
                }
                else
                {
                    bool istorrentable = true;
                    foreach (License license in module.license)
                    {
                        if (!torrentable_licenses.Contains(license.ToString()))
                        {
                            fallback_list.Add(module);
                            istorrentable = false;
                            break;
                        }
                    }
                    if (istorrentable)
                        torrentable.Add(module);
                }
            }
            //run torrent downloader
            _DownloadModules(torrentable);

            //run fallback downloader
            IDownloader fallback_downloader = new NetAsyncDownloader(User);
            fallback_downloader.DownloadModules(cache, fallback_list);
        }

        /// <summary>
        /// Unimplemented - can't stop arbitrary torrent clients.
        /// </summary>
        public void CancelDownload()
        {
        }

        private void _DownloadModules(IEnumerable<CkanModule> modules)
        {
            //TODO: check that $TORRENT_COMPLETED_DIR exists
            List<Task<string>> tasks = new List<Task<string>>();
            foreach (CkanModule module in modules)
            {
                Task<string> task = _DownloadModule(module);
                tasks.Add(task);
            }
            log.Debug("Waiting for downloads to finish.");
            Task.WaitAll(tasks.ToArray());
        }

        private Task<string> _DownloadModule(CkanModule module)
        {
            User.RaiseMessage("Generating magnet link for \"{0}\"", module.name);
            string filename = module.StandardName();
            string filepath = Path.Combine(""/*TODO:$TORRENT_COMPLETED_DIR*/, filename);
            string link = GenerateMagnetLink(module, filename);
            System.Diagnostics.Process.Start(link);

            var tcs = new TaskCompletionSource<string>();
            FileSystemWatcher watcher = new FileSystemWatcher(/*TODO:$TORRENT_COMPLETED_DIR*/);
            FileSystemEventHandler created = (s, e) =>
            {
                if (e.Name.Equals(filename))
                {
                    try
                    {
                        //explicitly copy, so the torrent software can continue seeding, if permitted
                        _cache.Store(module.download, filepath, module.StandardName(), false);
                        tcs.TrySetResult(filename);
                    }
                    catch (FileNotFoundException ex)
                    {
                        log.WarnFormat("cache.Store(): FileNotFoundException: {0}", ex.Message);
                    }
                }
            };
            watcher.Created += created;
            return tcs.Task;
        }

        /// <summary>
        /// Generates a magnet link for a file. Example: magnet:?xt=urn:btih:8e7e8089f22cbb70d0d4fe06528fb1e416d1bd1e&dn=1C28BC18-Chatterer-0.9.7.zip&ws=https%3a%2f%2fs3-us-west-2.amazonaws.com%2fksp-ckan%2f1C28BC18.zip
        /// </summary>
        /// <returns>The magnet link.</returns>
        /// <param name="module">Module.</param>
        /// <param name="filename">Filename.</param>
        public static string GenerateMagnetLink(CkanModule module, string filename)
        {
            string magnet = "magnet:";
            string btihpart = "?xt=+urn:btih:" + module.btih;
            string namepart = "&dn=" + Uri.EscapeDataString(filename);
            string websourcepart = "&ws=" + Uri.EscapeDataString(module.download.ToString());
            return magnet + btihpart + namepart + websourcepart;
        }
    }
}

