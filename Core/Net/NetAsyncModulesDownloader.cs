using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Threading;
using System.Security.Cryptography;

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
        public event Action<CkanModule, long, long>? DownloadProgress;
        public event Action<ByteRateCounter>?        OverallDownloadProgress;
        public event Action<CkanModule, long, long>? StoreProgress;
        public event Action<CkanModule>?             OneComplete;
        public event Action?                         AllComplete;

        /// <summary>
        /// Returns a perfectly boring NetAsyncModulesDownloader.
        /// </summary>
        public NetAsyncModulesDownloader(IUser user, NetModuleCache cache, string? userAgent = null)
        {
            modules    = new List<CkanModule>();
            downloader = new NetAsyncDownloader(user, SHA256.Create, userAgent);
            // Schedule us to process each module on completion.
            downloader.onOneCompleted += ModuleDownloadComplete;
            downloader.TargetProgress += (target, remaining, total) =>
            {
                if (targetModules?[target].First() is CkanModule mod)
                {
                    DownloadProgress?.Invoke(mod, remaining, total);
                }
            };
            downloader.OverallProgress += brc => OverallDownloadProgress?.Invoke(brc);
            this.cache = cache;
        }

        internal NetAsyncDownloader.DownloadTarget TargetFromModuleGroup(
                HashSet<CkanModule> group,
                string?[]           preferredHosts)
            => TargetFromModuleGroup(group, group.OrderBy(m => m.identifier).First(), preferredHosts);

        private NetAsyncDownloader.DownloadTargetFile TargetFromModuleGroup(
                HashSet<CkanModule> group,
                CkanModule          first,
                string?[]           preferredHosts)
            => new NetAsyncDownloader.DownloadTargetFile(
                group.SelectMany(mod => mod.download ?? Enumerable.Empty<Uri>())
                     .Concat(group.Select(mod => mod.InternetArchiveDownload)
                                  .OfType<Uri>()
                                  .OrderBy(uri => uri.ToString()))
                     .Distinct()
                     .OrderBy(u => u,
                              new PreferredHostUriComparer(preferredHosts))
                     .ToList(),
                cache.GetInProgressFileName(first)?.FullName,
                first.download_size,
                string.IsNullOrEmpty(first.download_content_type)
                    ? defaultMimeType
                    : $"{first.download_content_type};q=1.0,{defaultMimeType};q=0.9");

        /// <summary>
        /// <see cref="IDownloader.DownloadModules(NetFileCache, IEnumerable{CkanModule})"/>
        /// </summary>
        public void DownloadModules(IEnumerable<CkanModule> modules)
        {
            var activeURLs = this.modules.SelectMany(m => m.download ?? Enumerable.Empty<Uri>())
                                         .OfType<Uri>()
                                         .ToHashSet();
            var moduleGroups = CkanModule.GroupByDownloads(modules);
            // Make sure we have enough space to download and cache
            cache.CheckFreeSpace(moduleGroups.Select(grp => grp.First().download_size)
                                             .Sum());
            // Add all the requested modules
            this.modules.AddRange(moduleGroups.SelectMany(grp => grp));

            var preferredHosts = ServiceLocator.Container.Resolve<IConfiguration>().PreferredHosts;
            targetModules = moduleGroups
                // Skip any group that already has a URL in progress
                .Where(grp => grp.All(mod => mod.download?.All(dlUri => !activeURLs.Contains(dlUri)) ?? false))
                // Each group gets one target containing all the URLs
                .ToDictionary(grp => TargetFromModuleGroup(grp, preferredHosts),
                              grp => grp.ToArray());
            try
            {
                cancelTokenSrc = new CancellationTokenSource();
                // Start the downloads!
                downloader.DownloadAndWait(targetModules.Keys);
                this.modules.Clear();
                targetModules.Clear();
                AllComplete?.Invoke();
            }
            catch (DownloadErrorsKraken kraken)
            {
                // Associate the errors with the affected modules
                var exc = new ModuleDownloadErrorsKraken(
                    kraken.Exceptions
                          .SelectMany(kvp => targetModules[kvp.Key]
                                             .Select(m => new KeyValuePair<CkanModule, Exception>(
                                                              m, kvp.Value.GetBaseException() ?? kvp.Value)))
                          .ToList());
                // Clear this.modules because we're done with these
                this.modules.Clear();
                targetModules.Clear();
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

        public IEnumerable<CkanModule> ModulesAsTheyFinish(ICollection<CkanModule> cached,
                                                           ICollection<CkanModule> toDownload)
        {
            var (dlTask, blockingQueue) = DownloadsCollection(toDownload);
            return ModulesAsTheyFinish(cached, dlTask, blockingQueue);
        }

        private static IEnumerable<CkanModule> ModulesAsTheyFinish(ICollection<CkanModule>        cached,
                                                                   Task                           dlTask,
                                                                   BlockingCollection<CkanModule> blockingQueue)
        {
            foreach (var m in cached)
            {
                yield return m;
            }
            foreach (var m in blockingQueue.GetConsumingEnumerable())
            {
                yield return m;
            }
            blockingQueue.Dispose();
            if (dlTask.Exception is AggregateException { InnerException: Exception exc })
            {
                throw exc;
            }
        }

        private (Task dlTask, BlockingCollection<CkanModule> blockingQueue) DownloadsCollection(ICollection<CkanModule> toDownload)
        {
            var blockingQueue = new BlockingCollection<CkanModule>(new ConcurrentQueue<CkanModule>());
            Action<CkanModule> oneComplete = m => blockingQueue.Add(m);
            OneComplete += oneComplete;
            return (Task.Factory.StartNew(() => DownloadModules(toDownload))
                                .ContinueWith(t =>
                                              {
                                                  blockingQueue.CompleteAdding();
                                                  OneComplete -= oneComplete;
                                              }),
                    blockingQueue);
        }

        private void ModuleDownloadComplete(NetAsyncDownloader.DownloadTarget target,
                                            Exception?                        error,
                                            string?                           etag,
                                            string?                           sha256)
        {
            if (target is NetAsyncDownloader.DownloadTargetFile fileTarget && targetModules != null)
            {
                var url      = fileTarget.urls.First();
                var filename = fileTarget.filename;

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
                    CkanModule? module = null;
                    try
                    {
                        var completedMods = targetModules[fileTarget];
                        module = completedMods.First();

                        // Check hash if defined in module
                        if (module.download_hash?.sha256 != null
                            && sha256 != module.download_hash.sha256)
                        {
                            throw new InvalidModuleFileKraken(
                                module, filename,
                                string.Format(Properties.Resources.NetModuleCacheMismatchSHA256,
                                              module, filename,
                                              sha256, module.download_hash.sha256));
                        }

                        User.RaiseMessage(Properties.Resources.NetAsyncDownloaderValidating,
                                          module.name);
                        var fileSize = new FileInfo(filename).Length;
                        cache.Store(module, filename,
                                    new ProgressImmediate<long>(bytes => StoreProgress?.Invoke(module,
                                                                                               fileSize - bytes,
                                                                                               fileSize)),
                                    module.StandardName(),
                                    false,
                                    cancelTokenSrc?.Token);
                        File.Delete(filename);
                        foreach (var m in completedMods)
                        {
                            OneComplete?.Invoke(m);
                        }
                    }
                    catch (InvalidModuleFileKraken kraken)
                    {
                        if (module != null)
                        {
                            // Finish out the progress bar
                            StoreProgress?.Invoke(module, 0, 100);
                        }
                        // If there was an error in STORING, delete the file so we can try it from scratch later
                        File.Delete(filename);

                        // Tell downloader there is a problem with this file
                        throw new DownloadErrorsKraken(target, kraken);
                    }
                    catch (OperationCanceledException exc)
                    {
                        log.WarnFormat("Cancellation token threw, validation incomplete: {0}", filename);
                        User.RaiseMessage("{0}", exc.Message);
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

        private static readonly ILog log = LogManager.GetLogger(typeof(NetAsyncModulesDownloader));

        private const    string                  defaultMimeType = "application/octet-stream";

        private readonly List<CkanModule>         modules;
        private          IDictionary<NetAsyncDownloader.DownloadTarget, CkanModule[]>? targetModules;
        private readonly NetAsyncDownloader       downloader;
        private          IUser                    User => downloader.User;
        private readonly NetModuleCache           cache;
        private          CancellationTokenSource? cancelTokenSrc;
    }
}
