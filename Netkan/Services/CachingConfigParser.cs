using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using log4net;
using ICSharpCode.SharpZipLib.Zip;
using ParsecSharp;
using KSPMMCfgParser;
using static KSPMMCfgParser.KSPMMCfgParser;

namespace CKAN.NetKAN.Services
{
    using NodeCache  = Dictionary<CkanModule, ConfigNodesCacheEntry>;

    /// <summary>
    /// Since parsing cfg files can be expensive, cache results for 15 minutes
    /// </summary>
    internal sealed class CachingConfigParser : IConfigParser
    {
        public CachingConfigParser(IModuleService modSvc)
        {
            moduleService = modSvc;
        }

        public Dictionary<InstallableFile, KSPConfigNode[]> GetConfigNodes(CkanModule module, ZipFile zip, GameInstance inst)
            => GetCachedNodes(module) ?? AddAndReturn(
                module,
                moduleService.GetConfigFiles(module, zip, inst).ToDictionary(
                    cfg => cfg,
                    cfg => ConfigFile.ToArray()
                        .Parse(zip.GetInputStream(cfg.source))
                        .CaseOf(failure =>
                                {
                                    log.InfoFormat("{0}:{1}:{2}: {3}",
                                                   inst.ToRelativeGameDir(cfg.destination),
                                                   failure.State.Position.Line,
                                                   failure.State.Position.Column,
                                                   failure.Message);
                                    return new KSPConfigNode[] { };
                                },
                                success => success.Value)));

        private Dictionary<InstallableFile, KSPConfigNode[]> AddAndReturn(CkanModule module,
                                                                          Dictionary<InstallableFile, KSPConfigNode[]> nodes)
        {
            log.DebugFormat("Caching config nodes for {0}", module);
            cache.Add(module,
                      new ConfigNodesCacheEntry()
                      {
                          Value     = nodes,
                          Timestamp = DateTime.Now,
                      });
            return nodes;
        }

        private Dictionary<InstallableFile, KSPConfigNode[]> GetCachedNodes(CkanModule module)
        {
            if (cache.TryGetValue(module, out ConfigNodesCacheEntry entry))
            {
                if (DateTime.Now - entry.Timestamp < stringCacheLifetime)
                {
                    log.DebugFormat("Using cached nodes for {0}", module);
                    return entry.Value;
                }
                else
                {
                    log.DebugFormat("Purging stale nodes for {0}", module);
                    cache.Remove(module);
                }
            }
            return null;
        }

        private        readonly IModuleService moduleService;
        private        readonly NodeCache      cache               = new NodeCache();
        // Re-use parse results within 15 minutes
        private static readonly TimeSpan       stringCacheLifetime = new TimeSpan(0, 15, 0);
        private static readonly ILog           log                 = LogManager.GetLogger(typeof(CachingConfigParser));
    }

    public class ConfigNodesCacheEntry
    {
        public Dictionary<InstallableFile, KSPConfigNode[]> Value;
        public DateTime                                     Timestamp;
    }
}
