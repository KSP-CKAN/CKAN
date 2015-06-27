using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using log4net;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that populates version data from an AVC version file included in the
    /// distribution package.
    /// </summary>
    internal sealed class AvcTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AvcTransformer));

        private readonly IHttpService _http;
        private readonly IModuleService _moduleService;

        public AvcTransformer(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Vref != null && metadata.Vref.Source == "ksp-avc")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Interal AVC transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var noVersion = metadata.Version == null;

                if (noVersion)
                {
                    json["version"] = "0"; // TODO: DBB: Dummy version necessary to the next statement doesn't throw
                }
                
                var mod = CkanModule.FromJson(json.ToString());

                if (noVersion)
                {
                    json.Remove("version");
                }

                var file = _http.DownloadPackage(metadata.Download, metadata.Identifier);
                var avc = _moduleService.GetInternalAvc(mod, file, metadata.Vref.Id);

                if (avc != null)
                {
                    Log.Info("Found internal AVC version file");

                    // The CKAN spec states that only a KSP version can be supplied, *or* a min/max can
                    // be provided. Since min/max are more descriptive, we check and use them first.
                    if (avc.ksp_version_min != null || avc.ksp_version_max != null)
                    {
                        json.Remove("ksp_version");

                        if (avc.ksp_version_min != null)
                            json["ksp_version_min"] = avc.ksp_version_min.ToString();

                        if (avc.ksp_version_max != null)
                            json["ksp_version_max"] = avc.ksp_version_max.ToString();
                    }
                    else if (avc.ksp_version != null)
                    {
                        json["ksp_version"] = avc.ksp_version.ToString();
                    }

                    if (avc.version != null)
                    {
                        json["version"] = avc.version.ToString();
                    }

                    // It's cool if we don't have version info at all, it's optional in the AVC spec.

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }

                return new Metadata(json);
            }
            else
            {
                return metadata;
            }
        }
    }
}
