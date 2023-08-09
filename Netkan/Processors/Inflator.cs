using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using log4net;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Validators;
using CKAN.Games;

namespace CKAN.NetKAN.Processors
{
    public class Inflator
    {
        public Inflator(string cacheDir, bool overwriteCache, string githubToken, string gitlabToken, bool prerelease, IGame game)
        {
            log.Debug("Initializing inflator");
            cache = FindCache(
                new GameInstanceManager(new ConsoleUser(false)),
                ServiceLocator.Container.Resolve<IConfiguration>(),
                cacheDir
            );

            IModuleService moduleService = new ModuleService(game);
            IFileService   fileService   = new FileService(cache);
            http          = new CachingHttpService(cache, overwriteCache);
            ckanValidator = new CkanValidator(http, moduleService, game);
            transformer   = new NetkanTransformer(http, fileService, moduleService, githubToken, gitlabToken, prerelease, game, netkanValidator);
        }

        internal IEnumerable<Metadata> Inflate(string filename, Metadata[] netkans, TransformOptions opts)
        {
            log.DebugFormat("Inflating {0}", filename);
            try
            {
                // Tell the downloader that we're starting a new request
                http.ClearRequestedURLs();

                foreach (var netkan in netkans)
                {
                    netkanValidator.ValidateNetkan(netkan, filename);
                }
                log.Debug("Input successfully passed pre-validation");

                var ckans = netkans
                    .SelectMany(netkan => transformer.Transform(netkan, opts))
                    .GroupBy(module => module.Version)
                    .Select(grp => Metadata.Merge(grp.ToArray()))
                    .ToList();
                log.Debug("Finished transformation");

                foreach (Metadata ckan in ckans)
                {
                    ckanValidator.ValidateCkan(ckan, netkans[0]);
                }
                log.Debug("Output successfully passed post-validation");
                return ckans;
            }
            catch (Exception)
            {
                try
                {
                    // Purge anything we download for a failed indexing attempt from the cache to allow re-downloads
                    PurgeDownloads(http, cache);
                }
                catch
                {
                    // Don't freak out if we can't delete
                }
                throw;
            }
        }

        internal void ValidateCkan(Metadata ckan)
        {
            netkanValidator.Validate(ckan);
            ckanValidator.Validate(ckan);
        }

        private static NetFileCache FindCache(GameInstanceManager kspManager, IConfiguration cfg, string cacheDir)
        {
            if (cacheDir != null)
            {
                log.InfoFormat("Using user-supplied cache at {0}", cacheDir);
                return new NetFileCache(cacheDir);
            }

            try
            {
                log.InfoFormat("Using main CKAN meta-cache at {0}", cfg.DownloadCacheDir);
                // Create a new file cache in the same location so NetKAN can download pure URLs not sourced from CkanModules
                return new NetFileCache(kspManager, cfg.DownloadCacheDir);
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
