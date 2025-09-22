using System;
using System.Collections.Generic;
using log4net;

using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.Versioning;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that populates data from a CKAN file included in the distribution package.
    /// </summary>
    internal sealed class InternalCkanTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InternalCkanTransformer));

        private readonly IHttpService _http;
        private readonly IModuleService _moduleService;

        public string Name => "internal_ckan";

        public InternalCkanTransformer(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download != null
                && _http.DownloadModule(metadata) is string contents)
            {

                // We run before the AVC transformer, which sets "version" for Jenkins.
                // Set it to a default if missing so CkanModule can initialize.
                var moduleJson = metadata.Json();
                moduleJson.SafeAdd("version", "1");
                CkanModule   mod  = CkanModule.FromJson(moduleJson.ToString());
                var internalJson = _moduleService.GetInternalCkan(mod, contents);

                if (internalJson != null)
                {
                    var json = metadata.Json();
                    Log.InfoFormat("Executing internal CKAN transformation with {0}", metadata.Kref);
                    Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                    foreach (var property in internalJson.Properties())
                    {
                        // We've already got the file, too late to tell us where it lives
                        if (property.Name == "$kref")
                        {
                            Log.DebugFormat("Skipping $kref property: {0}", property.Value);
                            continue;
                        }
                        json.SafeAdd(property.Name, property.Value);
                    }

                    GameVersion.SetJsonCompatibility(json, null, null, null);
                    json.SafeMerge("resources", internalJson["resources"]);

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                    yield return new Metadata(json);
                    yield break;
                }
            }
            yield return metadata;
        }
    }
}
