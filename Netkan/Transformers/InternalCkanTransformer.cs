using System;
using System.Collections.Generic;
using log4net;
using CKAN.Versioning;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

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

        public string Name { get { return "internal_ckan"; } }

        public InternalCkanTransformer(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download != null)
            {
                var json = metadata.Json();

                string file = _http.DownloadPackage(metadata.Download, metadata.Identifier, metadata.RemoteTimestamp);

                var internalJson = _moduleService.GetInternalCkan(file);

                if (internalJson != null)
                {
                    Log.InfoFormat("Executing internal CKAN transformation with {0}", metadata.Kref);
                    Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

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

                    json["spec_version"] = ModuleVersion.Max(metadata.SpecVersion, new Metadata(internalJson).SpecVersion)
                        .ToSpecVersionJson();

                    json.SafeMerge("resources", internalJson["resources"]);

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }

                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }
    }
}
