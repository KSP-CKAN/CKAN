using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;

        public string Name => "internal_ckan";

        public InternalCkanTransformer(IModuleService moduleService, ICacheService cacheService)
        {
            _moduleService = moduleService;
            _cacheService = cacheService;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download == null)
            {
                return new Metadata[]{ metadata };
            }

            return _cacheService.ExecuteCached(metadata, typeof(InternalCkanTransformer), contents =>
            {
                if (contents is null)
                {
                    return metadata;
                }

                // We run before the AVC transformer, which sets "version" for Jenkins.
                // Set it to a default if missing so CkanModule can initialize.
                var moduleJson = metadata.Json();
                moduleJson.SafeAdd("version", "1");
                CkanModule mod = CkanModule.FromJson(moduleJson.ToString());
                if (_moduleService.GetInternalCkans(mod, contents).ToArray()
                    is not { Length: > 0 } internalJsons)
                {
                    return metadata;
                }

                var json = metadata.Json();
                foreach (var internalJson in internalJsons)
                {
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
                }

                return new Metadata(json);
            });
        }
    }
}
