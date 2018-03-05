using System;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using log4net;

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

        public Metadata Transform(Metadata metadata)
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
                        json.SafeAdd(property.Name, property.Value);
                    }

                    json["spec_version"] = Version.Max(metadata.SpecVersion, new Metadata(internalJson).SpecVersion)
                        .ToSpecVersionJson();

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
