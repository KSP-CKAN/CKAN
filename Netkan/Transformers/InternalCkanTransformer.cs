using System;
using System.Collections.Generic;
using log4net;

using CKAN.Versioning;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.Games;

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
        private readonly IGame _game;

        public string Name => "internal_ckan";

        public InternalCkanTransformer(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http = http;
            _moduleService = moduleService;
            _game = game;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Download != null)
            {
                var json = metadata.Json();

                // We run before the AVC transformer, which sets "version" for Jenkins.
                // Set it to a default if missing so CkanModule can initialize.
                var moduleJson = metadata.Json();
                moduleJson.SafeAdd("version", "1");
                CkanModule mod = CkanModule.FromJson(moduleJson.ToString());
                GameInstance inst = new GameInstance(_game, "/", "dummy", new NullUser());

                var internalJson = _moduleService.GetInternalCkan(mod, _http.DownloadModule(metadata), inst);

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
