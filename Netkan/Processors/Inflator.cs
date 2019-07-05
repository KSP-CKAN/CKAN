using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using log4net;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Validators;
using CKAN.Win32Registry;
using Autofac;

namespace CKAN.NetKAN.Processors
{
    public class Inflator
    {
        public Inflator(string cacheDir, bool overwriteCache, string githubToken, bool prerelease)
        {
            log.Debug("Initializing inflator");
            cache = FindCache(
                new KSPManager(new ConsoleUser(false)),
                ServiceLocator.Container.Resolve<IWin32Registry>(),
                cacheDir
            );

            IModuleService moduleService = new ModuleService();
            IFileService   fileService   = new FileService();
            http          = new CachingHttpService(cache, overwriteCache);
            ckanValidator = new CkanValidator(http, moduleService);
            transformer   = new NetkanTransformer(http, fileService, moduleService, githubToken, prerelease);
        }

        internal IEnumerable<Metadata> Inflate(string filename, Metadata netkan, int? releases)
        {
            log.DebugFormat("Inflating {0}", filename);
            try
            {
                // Tell the downloader that we're starting a new request
                http.ClearRequestedURLs();

                netkanValidator.ValidateNetkan(netkan, filename);
                log.Info("Input successfully passed pre-validation");

                IEnumerable<Metadata> ckans = transformer
                    .Transform(netkan, new TransformOptions(releases))
                    .ToList();
                log.Info("Finished transformation");

                foreach (Metadata ckan in ckans)
                {
                    ckanValidator.ValidateCkan(ckan, netkan);
                }
                log.Info("Output successfully passed post-validation");
                return ckans;
            }
            catch (Exception)
            {
                // Purge anything we download for a failed indexing attempt from the cache to allow re-downloads
                PurgeDownloads(http, cache);
                throw;
            }
        }

        private static NetFileCache FindCache(KSPManager kspManager, IWin32Registry reg, string cacheDir)
        {
            if (cacheDir != null)
            {
                log.InfoFormat("Using user-supplied cache at {0}", cacheDir);
                return new NetFileCache(cacheDir);
            }

            try
            {
                log.InfoFormat("Using main CKAN meta-cache at {0}", reg.DownloadCacheDir);
                // Create a new file cache in the same location so NetKAN can download pure URLs not sourced from CkanModules
                return new NetFileCache(kspManager, reg.DownloadCacheDir);
            }
            catch
            {
                // Meh, can't find KSP. 'Scool, bro.
            }

            var tempdir = Path.GetTempPath();
            log.InfoFormat("Using tempdir for cache: {0}", tempdir);

            return new NetFileCache(tempdir);
        }

        private static void PurgeDownloads(IHttpService http, NetFileCache cache)
        {
            log.Debug("Deleting downloads for failed inflation");
            if (http != null && cache != null)
            {
                foreach (Uri url in http.RequestedURLs)
                {
                    cache.Remove(url);
                }
            }
        }

        private NetFileCache cache;
        private IHttpService http;

        private NetkanTransformer transformer;

        private NetkanValidator netkanValidator = new NetkanValidator();
        private CkanValidator   ckanValidator;

        private static readonly ILog log = LogManager.GetLogger(typeof(Inflator));
    }
}
