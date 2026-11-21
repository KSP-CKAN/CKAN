using System;
using System.Linq;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using log4net;

using CKAN.Versioning;
using CKAN.SpaceWarp;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class SpaceWarpInfoTransformer : ITransformer
    {
        public SpaceWarpInfoTransformer(ISpaceWarpInfoLoader loader,
                                        IModuleService       modSvc,
                                        ICacheService        cacheSvc)
        {
            this.loader   = loader;
            this.modSvc   = modSvc;
            this.cacheSvc = cacheSvc;
        }

        public string Name => "space_warp_info";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Vref?.Source != "space-warp")
            {
                return new Metadata[] { metadata };
            }

            var moduleJson = metadata.Json();
            moduleJson.SafeAdd("version", "1");

            return cacheSvc.ExecuteCached(metadata, typeof(SpaceWarpInfoTransformer), file =>
            {
                CkanModule mod = CkanModule.FromJson(moduleJson.ToString());
                ZipFile    zip = new ZipFile(file);
                if (modSvc.GetInternalSpaceWarpInfos(mod, zip, metadata.Vref.Id)
                            .Select(loader.Load)
                            .FirstOrDefault()
                    is not SpaceWarpInfo swinfo)
                {
                    return metadata;
                }

                log.Info("Found swinfo.json file");
                var json = metadata.Json();

                if (swinfo.version_check != null
                    && Uri.IsWellFormedUriString(swinfo.version_check.OriginalString, UriKind.Absolute))
                {
                    json.SafeMerge("resources",
                                    new JObject()
                                    {
                                        { "remote-swinfo", swinfo.version_check.OriginalString },
                                    });
                }

                json.SafeAdd("name",     swinfo.name);
                json.SafeAdd("author",   swinfo.author);
                json.SafeAdd("abstract", swinfo.description);
                json.SafeAdd("version",  swinfo.version);
                bool hasMin = GameVersion.TryParse(swinfo.ksp2_version?.min, out GameVersion? minVer);
                bool hasMax = GameVersion.TryParse(swinfo.ksp2_version?.max, out GameVersion? maxVer);
                if (hasMin || hasMax)
                {
                    log.InfoFormat("Found compatibility: {0}–{1}", minVer?.WithoutBuild,
                                                                    maxVer?.WithoutBuild);
                    GameVersion.SetJsonCompatibility(json, null, minVer?.WithoutBuild,
                                                                    maxVer?.WithoutBuild);
                }
                log.DebugFormat("Transformed metadata:{0}{1}",
                                Environment.NewLine, json);
                return new Metadata(json);
            });
        }

        private readonly ISpaceWarpInfoLoader loader;
        private readonly IModuleService       modSvc;
        private readonly ICacheService        cacheSvc;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoTransformer));
    }
}
