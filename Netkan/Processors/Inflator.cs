using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autofac;
using log4net;

using CKAN.Configuration;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Validators;
using CKAN.Games;
using CKAN.Extensions;

namespace CKAN.NetKAN.Processors
{
    public class Inflator
    {
        public Inflator(string? cacheDir,
                        bool    overwriteCache,
                        string? githubToken,
                        string? gitlabToken,
                        string? userAgent,
                        bool?   prerelease,
                        IGame   game)
            : this(overwriteCache, githubToken, gitlabToken,
                   userAgent, prerelease, game,
                   FindCache(ServiceLocator.Container.Resolve<IConfiguration>(),
                              cacheDir))
        {
        }

        internal Inflator(bool         overwriteCache,
                          string?      githubToken,
                          string?      gitlabToken,
                          string?      userAgent,
                          bool?        prerelease,
                          IGame        game,
                          NetFileCache cache)
            : this(githubToken, gitlabToken,
                   userAgent, prerelease, game,
                   cache,
                   new CachingHttpService(cache, overwriteCache, userAgent),
                   new ModuleService(game),
                   new FileService(cache))
        {
        }

        internal Inflator(string?        githubToken,
                          string?        gitlabToken,
                          string?        userAgent,
                          bool?          prerelease,
                          IGame          game,
                          NetFileCache   cache,
                          IHttpService   http,
                          IModuleService moduleService,
                          IFileService   fileService)
        {
            log.Debug("Initializing inflator");
            this.cache    = cache;
            this.http     = http;
            ckanValidator = new CkanValidator(http, moduleService, game, githubToken);
            transformer   = new NetkanTransformer(http, fileService, moduleService,
                                                  githubToken, gitlabToken, userAgent,
                                                  prerelease, game, netkanValidator);
        }

        internal IEnumerable<Metadata> Inflate(string           filename,
                                               Metadata[]       netkans,
                                               TransformOptions opts)
        {
            log.DebugFormat("Inflating {0}", filename);
            try
            {
                // Tell the downloader that we're starting a new request
                http.ClearRequestedURLs();

                if (netkans.Length > 1)
                {
                    // Mix properties between sections if they don't start with x_netkan
                    var stripped = netkans.Select(nk => nk.Json())
                                          .Select(StripNetkanMetadataTransformer.Strip)
                                          .ToArray();
                    netkans = netkans.Select(nk => nk.MergeFrom(stripped))
                                     .ToArray();
                }

                foreach (var netkan in netkans)
                {
                    netkanValidator.ValidateNetkan(netkan, filename);
                }
                log.Debug("Input successfully passed pre-validation");

                var ckans = netkans.SelectManyTasks(netkan => transformer.Transform(netkan, opts))
                                   .GroupBy(module => module.Version)
                                   .Select(grp => Metadata.Merge(grp.ToArray()))
                                   .SelectMany(merged => specVersionTransformer.Transform(merged, opts))
                                   .SelectMany(withSpecVersion => sortTransformer.Transform(withSpecVersion, opts))
                                   .OrderByDescending(m => m.Prerelease)
                                   .ToList();
                log.Debug("Finished transformation");

                // null means "all"
                if (opts.Releases is int relsRequested)
                {
                    if (ckans.Count(m => !m.Prerelease) > relsRequested)
                    {
                        throw new Kraken(string.Format("Generated {0} modules but only {1} requested: {2}",
                                                       ckans.Count,
                                                       relsRequested,
                                                       string.Join("; ", ckans.Select(DescribeHosting))));
                    }
                    ckans = ckans.Take(relsRequested).ToList();
                }

                foreach (Metadata ckan in ckans)
                {
                    ckanValidator.ValidateCkan(ckan, netkans.First());
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

        private static NetFileCache FindCache(IConfiguration cfg, string? cacheDir)
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
                return new NetFileCache(cfg.DownloadCacheDir ?? GameInstanceManager.DefaultDownloadCacheDir);
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
                cache.Remove(http.RequestedURLs);
            }
        }

        private string DescribeHosting(Metadata metadata)
            => metadata.Prerelease
                   ? $"{metadata.Version} (pre-release) on {string.Join(", ", metadata.Hosts)}"
                   : $"{metadata.Version} on {string.Join(", ", metadata.Hosts)}";

        private readonly NetFileCache cache;
        private readonly IHttpService http;

        private readonly NetkanTransformer       transformer;
        private readonly SpecVersionTransformer  specVersionTransformer = new SpecVersionTransformer();
        private readonly PropertySortTransformer sortTransformer        = new PropertySortTransformer();

        private readonly NetkanValidator netkanValidator = new NetkanValidator();
        private readonly CkanValidator   ckanValidator;

        private static readonly ILog log = LogManager.GetLogger(typeof(Inflator));
    }
}
