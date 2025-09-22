using System;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Sources.Github;
using CKAN.Versioning;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class SpaceWarpInfoTransformer : ITransformer
    {
        public SpaceWarpInfoTransformer(IHttpService httpSvc, IGithubApi githubApi, IModuleService modSvc)
        {
            this.httpSvc   = httpSvc;
            this.githubApi = githubApi;
            this.modSvc    = modSvc;
        }

        public string Name => "space_warp_info";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Vref != null && metadata.Vref.Source == "space-warp")
            {
                var moduleJson = metadata.Json();
                moduleJson.SafeAdd("version", "1");
                CkanModule    mod    = CkanModule.FromJson(moduleJson.ToString());
                ZipFile       zip    = new ZipFile(httpSvc.DownloadModule(metadata));
                var swinfo = modSvc.GetInternalSpaceWarpInfo(mod, zip, metadata.Vref.Id);
                if (swinfo != null)
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

                        try
                        {
                            var remoteInfo = modSvc.ParseSpaceWarpJson(
                                githubApi?.DownloadText(swinfo.version_check)
                                ?? httpSvc.DownloadText(swinfo.version_check));
                            if (remoteInfo != null && swinfo.version == remoteInfo.version)
                            {
                                log.InfoFormat("Using remote swinfo.json file: {0}",
                                               swinfo.version_check);
                                swinfo = remoteInfo;
                            }
                        }
                        catch (Exception exc)
                        {
                            throw new Kraken($"Error fetching remote swinfo {swinfo.version_check}: {exc.Message}");
                        }
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

        private readonly IHttpService   httpSvc;
        private readonly IGithubApi     githubApi;
        private readonly IModuleService modSvc;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoTransformer));
    }
}
