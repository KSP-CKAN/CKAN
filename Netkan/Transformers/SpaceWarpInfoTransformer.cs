using System;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.SpaceWarp;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;
using CKAN.Versioning;
using CKAN.Games;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class SpaceWarpInfoTransformer : ITransformer
    {
        public SpaceWarpInfoTransformer(IHttpService httpSvc, IModuleService modSvc, IGame game)
        {
            this.httpSvc = httpSvc;
            this.modSvc  = modSvc;
            this.game    = game;
        }

        public string Name => "space_warp_info";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Vref != null && metadata.Vref.Source == "space-warp")
            {
                var moduleJson = metadata.Json();
                moduleJson.SafeAdd("version", "1");
                CkanModule    mod    = CkanModule.FromJson(moduleJson.ToString());
                GameInstance  inst   = new GameInstance(game, "/", "dummy", new NullUser());
                ZipFile       zip    = new ZipFile(httpSvc.DownloadModule(metadata));
                SpaceWarpInfo swinfo = modSvc.GetSpaceWarpInfo(mod, zip, inst, metadata.Vref.Id);
                if (swinfo != null)
                {
                    log.Info("Found swinfo.json file");
                    var json = metadata.Json();
                    json.SafeAdd("name",     swinfo.name);
                    json.SafeAdd("author",   swinfo.author);
                    json.SafeAdd("abstract", swinfo.description);
                    json.SafeAdd("version",  swinfo.version);
                    GameVersion minVer = null, maxVer = null;
                    if (GameVersion.TryParse(swinfo.ksp2_version.min, out minVer)
                        || GameVersion.TryParse(swinfo.ksp2_version.max, out maxVer))
                    {
                        log.InfoFormat("Found compatibility: {0}–{1}", minVer, maxVer);
                        ModuleService.ApplyVersions(json, null, minVer, maxVer);
                    }
                    log.DebugFormat("Transformed metadata:{0}{1}",
                                    Environment.NewLine, json);
                    yield return new Metadata(json);
                    yield break;
                }
            }
            yield return metadata;
        }

        private readonly IHttpService   httpSvc;
        private readonly IModuleService modSvc;
        private readonly IGame          game;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoTransformer));
    }
}
