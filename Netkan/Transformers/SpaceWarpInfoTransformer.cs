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
        public SpaceWarpInfoTransformer(IHttpService         httpSvc,
                                        ISpaceWarpInfoLoader loader,
                                        IModuleService       modSvc)
        {
            this.httpSvc = httpSvc;
            this.loader  = loader;
            this.modSvc  = modSvc;
        }

        public string Name => "space_warp_info";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Vref != null && metadata.Vref.Source == "space-warp")
            {
                var moduleJson = metadata.Json();
                moduleJson.SafeAdd("version", "1");
                CkanModule mod = CkanModule.FromJson(moduleJson.ToString());
                ZipFile    zip = new ZipFile(httpSvc.DownloadModule(metadata));
                if (modSvc.GetInternalSpaceWarpInfos(mod, zip, metadata.Vref.Id)
                          .Select(loader.Load)
                          .FirstOrDefault()
                    is SpaceWarpInfo swinfo)
                {
                    log.Info("Found swinfo.json file");
                    var json = metadata.Json();

                    if (swinfo.version_check != null
                        && Uri.IsWellFormedUriString(swinfo.version_check.OriginalString, UriKind.Absolute))
                    {
                        var resourcesJson = (JObject?)json["resources"];
                        if (resourcesJson == null)
                        {
                            json["resources"] = resourcesJson = new JObject();
                        }
                        resourcesJson.SafeAdd("remote-swinfo", swinfo.version_check.OriginalString);
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
                    yield return new Metadata(json);
                    yield break;
                }
            }
            yield return metadata;
        }

        private readonly IHttpService         httpSvc;
        private readonly ISpaceWarpInfoLoader loader;
        private readonly IModuleService       modSvc;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoTransformer));
    }
}
