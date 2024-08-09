using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using log4net;
using Autofac;

using CKAN.Configuration;
using CKAN.Extensions;

namespace CKAN
{
    /// <summary>
    /// Download lots of files at once!
    /// </summary>
    public class NetAsyncModulesDownloader : IDownloader
    {
        public event Action<CkanModule, long, long> Progress;
        public event Action<CkanModule, long, long> StoreProgress;
        public event Action                         AllComplete;

        /// <summary>
        /// Returns a perfectly boring NetAsyncModulesDownloader.
        /// </summary>
        public NetAsyncModulesDownloader(IUser user, NetModuleCache cache)
        {
            modules    = new List<CkanModule>();
            downloader = new NetAsyncDownloader(user);
            // Schedule us to process each module on completion.
            downloader.onOneCompleted += ModuleDownloadComplete;
            downloader.Progress += (target, remaining, total) =>
            {
                var mod = modules.FirstOrDefault(m => m.download?.Any(dlUri => target.urls.Contains(dlUri))
                                                      ?? false);
                if (mod != null && Progress != null)
                {
                    Progress(mod, remaining, total);
                }
            };
            this.cache = cache;
        }

        internal NetAsyncDownloader.DownloadTargetFile TargetFromModuleGroup(
                HashSet<CkanModule> group,
                string[]            preferredHosts)
            => TargetFromModuleGroup(group, group.OrderBy(m => m.identifier).First(), preferredHosts);

        private NetAsyncDownloader.DownloadTargetFile TargetFromModuleGroup(
                HashSet<CkanModule> group,
                CkanModule          first,
                string[]            preferredHosts)
            => new NetAsyncDownloader.DownloadTargetFile(
                group.SelectMany(mod => mod.download)
                     .Concat(group.Select(mod => mod.InternetArchiveDownload)
                                  .Where(uri => uri != null)
                                  .OrderBy(uri => uri.ToString()))
                     .Distinct()
                     .OrderBy(u => u,
                              new PreferredHostUriComparer(preferredHosts))
                     .ToList(),
                cache.GetInProgressFileName(first),
                first.download_size,
                string.IsNullOrEmpty(first.download_content_type)
                    ? defaultMimeType
                    : $"{first.download_content_type};q=1.0,{defaultMimeType};q=0.9");

        /// <summary>
        /// <see cref="IDownloader.DownloadModules(NetFileCache, IEnumerable{CkanModule})"/>
        /// </summary>
        public void DownloadModules(IEnumerable<CkanModule> modules)
        {
            var activeURLs = this.modules.SelectMany(m => m.download)
                                         .ToHashSet();
            var moduleGroups = CkanModule.GroupByDownloads(modules);
            // Make sure we have enough space to download and cache
            cache.CheckFreeSpace(moduleGroups.Select(grp => grp.First().download_size)
                                             .Sum());
            // Add all the requested modules
            this.modules.AddRange(moduleGroups.SelectMany(grp => grp));

            try
            {
                cancelTokenSrc = new CancellationTokenSource();
                var preferredHosts = ServiceLocator.Container.Resolve<IConfiguration>().PreferredHosts;
                // Start the downloads!
                downloader.DownloadAndWait(moduleGroups
                    // Skip any group that already has a URL in progress
                    .Where(grp => grp.All(mod => mod.download.All(dlUri => !activeURLs.Contains(dlUri))))
                    // Each group gets one target containing all the URLs
                    .Select(grp => TargetFromModuleGroup(grp, preferredHosts))
                    .ToArray());
                this.modules.Clear();
                AllComplete?.Invoke();
            }
            catch (DownloadErrorsKraken kraken)
            {
                // Associate the errors with the affected modules
                var exc = new ModuleDownloadErrorsKraken(this.modules.ToList(), kraken);
                // Clear this.modules because we're done with these
                this.modules.Clear();
                throw exc;
            }
        }

        /// <summary>
        /// <see cref="IDownloader.CancelDownload()"/>
        /// </summary>
        public void CancelDownload()
        {
            // Cancel downloads
            downloader.CancelDownload();
            // Cancel validation/store
            cancelTokenSrc?.Cancel();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncModulesDownloader));

        private const    string                  defaultMimeType = "application/octet-stream";

        private readonly List<CkanModule>        modules;
        private readonly NetAsyncDownloader      downloader;
        private          IUser                   User => downloader.User;
        private readonly NetModuleCache          cache;
        private          CancellationTokenSource cancelTokenSrc;

        private void ModuleDownloadComplete(NetAsyncDownloader.DownloadTarget target,
                                            Exception error,
                                            string etag)
        {
            var url      = target.urls.First();
            var filename = (target as NetAsyncDownloader.DownloadTargetFile)?.filename;

            log.DebugFormat("Received download completion: {0}, {1}, {2}",
                            url, filename, error?.Message);
            if (error != null)
            {
                // If there was an error in DOWNLOADING, keep the file so we can retry it later
                log.Info(error.Message);
            }
            else
            {
                // Cache if this download succeeded
                CkanModule module = null;
                try
                {
                    module = modules.First(m => (m.download?.Any(dlUri => dlUri == url)
                                                 ?? false)
                                                || m.InternetArchiveDownload == url);
                    User.RaiseMessage(Properties.Resources.NetAsyncDownloaderValidating, module);
                    cache.Store(module, filename,
                        new Progress<int>(percent => StoreProgress?.Invoke(module, 100 - percent, 100)),
                        module.StandardName(),
                        false,
                        cancelTokenSrc.Token);
                    File.Delete(filename);
                }
                catch (InvalidModuleFileKraken kraken)
                {
                    User.RaiseError(kraken.ToString());
                    if (module != null)
                    {
                        // Finish out the progress bar
                        StoreProgress?.Invoke(module, 0, 100);
                    }
                    // If there was an error in STORING, delete the file so we can try it from scratch later
                    File.Delete(filename);

                    // Tell downloader there is a problem with this file
                    throw;
                }
                catch (OperationCanceledException exc)
                {
                    log.WarnFormat("Cancellation token threw, validation incomplete: {0}", filename);
                    User.RaiseMessage(exc.Message);
                    if (module != null)
                    {
                        // Finish out the progress bar
                        StoreProgress?.Invoke(module, 0, 100);
                    }
                    // Don't delete because there might be nothing wrong
                }
                catch (FileNotFoundException e)
                {
                    log.WarnFormat("cache.Store(): FileNotFoundException: {0}", e.Message);
                }
                catch (InvalidOperationException)
                {
                    log.WarnFormat("No module found for completed URL: {0}", url);
                }
            }
        }

    }
}
