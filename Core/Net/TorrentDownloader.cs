using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using System.IO;
using System.Web;
using System.Linq;

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
        private Uri _downloaddir;

        public TorrentDownloader(IUser user, Uri downloadDir)
        {
            User = user;
            _downloaddir = downloadDir;
        }

        /// <summary>
        /// Download modules via an external torrent client when allowed, fall
        /// back on NetAsyncDownloader when not allowed.
        /// </summary>
        /// <param name="cache">Cache.</param>
        /// <param name="modules">Modules.</param>
        public void DownloadModules(
            NetFileCache cache,
            IEnumerable<CkanModule> modules)
        {
            List<CkanModule> torrentable = new List<CkanModule>();
            List<CkanModule> fallback_list = new List<CkanModule>();
            _cache = cache;

            foreach (CkanModule module in modules)
            {
                String[] licensestrings =
                    (from license in module.license
                     select license.ToString())
                    .ToArray();
                if (!String.IsNullOrEmpty(module.btih)
                    && IsTorrentable(licensestrings))
                    torrentable.Add(module);
                else
                    fallback_list.Add(module);
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

        public static bool IsTorrentable(String[] licenses){
            bool istorrentable = true;
            foreach (String license in licenses)
            {
                if (!torrentable_licenses.Contains(license))
                {
                    istorrentable = false;
                    break;
                }
            }
            return istorrentable;
        }

        private void _DownloadModules(IEnumerable<CkanModule> modules)
        {
            //TODO: check that downloaddir exists

            // We only need to download each torrent once - some clients will
            // automatically detect this, but it's best to be on the safe side.
            List<String> unique_btihs = new List<String>();
            List<CkanModule> unique_modules = new List<CkanModule>();
            foreach (CkanModule module in modules
                .Where(module => !unique_btihs.Contains(module.btih)))
            {
                unique_btihs.Add(module.btih);
                unique_modules.Add(module);
            }

            List<Task<string>> tasks = new List<Task<string>>();
            foreach (CkanModule module in unique_modules)
            {
                Task<string> task = _DownloadModule(module);
                tasks.Add(task);
            }
            log.Debug("Waiting for downloads to finish.");
            Task.WaitAll(tasks.ToArray());
            //TODO: notify user of errors
        }

        private Task<string> _DownloadModule(CkanModule module)
        {
            User.RaiseMessage("Generating magnet link for \"{0}\"", module.name);
            string filename = String.Format("{0}-{1}", NetFileCache.CreateURLHash(module.download), module.StandardName());
            string filepath = Path.Combine(_downloaddir.AbsolutePath, filename);
            string link = GenerateMagnetLink(module, filename);
            System.Diagnostics.Process.Start(link);

            var tcs = new TaskCompletionSource<string>();
            FileSystemWatcher watcher = new FileSystemWatcher(_downloaddir.AbsolutePath);
            FileSystemEventHandler created = null;
            created = (s, e) =>
            {
                if (e.Name.Equals(filepath))
                {
                    try
                    {
                        //explicitly copy, so the torrent software can continue seeding, if permitted
                        _cache.Store(module.download, filepath, module.StandardName(), false);
                        watcher.Created -= created;
                        watcher.Dispose();
                        tcs.TrySetResult(filename);
                    }
                    catch (FileNotFoundException ex)
                    {
                        log.WarnFormat("cache.Store(): FileNotFoundException: {0}", ex.Message);
                    }
                }
            };
            RenamedEventHandler renamed = null;
            renamed = (s, e) =>
            {
                if (e.Name.Equals(filepath))
                {
                    try
                    {
                        //explicitly copy, so the torrent software can continue seeding, if permitted
                        _cache.Store(module.download, filepath, module.StandardName(), false);
                        watcher.Renamed -= renamed;
                        watcher.Dispose();
                        tcs.TrySetResult(filename);
                    }
                    catch (FileNotFoundException ex)
                    {
                        log.WarnFormat("cache.Store(): FileNotFoundException: {0}", ex.Message);
                    }
                }
            };
            watcher.Created += created;
            watcher.Renamed += renamed;
            watcher.EnableRaisingEvents = true;
            return tcs.Task;
        }

        /// <summary>
        /// Generates a magnet link, for downloading a module via the bittorrent protocol.
        /// 
        /// Example:
        /// magnet:?xt=urn:btih:8e7e8089f22cbb70d0d4fe06528fb1e416d1bd1e&dn=1C28BC18-Chatterer-0.9.7.zip&ws=https%3a%2f%2fs3-us-west-2.amazonaws.com%2fksp-ckan%2f1C28BC18.zip
        /// </summary>
        /// <returns>The magnet link.</returns>
        /// <param name="module">Module</param>
        /// <param name="filename">Filename.</param>
        public static string GenerateMagnetLink(CkanModule module, string filename)
        {
            if (string.IsNullOrEmpty(module.btih))
            {
                throw new BadMetadataKraken(module, "GenerateMagnetLink called on module with missing btih.");
            }
            string magnet = "magnet:";
            string btihpart = "?xt=urn:btih:" + module.btih;
            string namepart = "&dn=" + Uri.EscapeDataString(filename);
            string websourcepart = "";
            if (!string.IsNullOrEmpty(module.download.ToString()))
                websourcepart = "&as=" + Uri.EscapeDataString(module.download.ToString());
            
            return magnet + btihpart + namepart + websourcepart;
        }
    }
}

