using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that can perform a recursive lookup of another NetKAN file.
    /// </summary>
    internal sealed class MetaNetkanTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetaNetkanTransformer));

        private const string KrefSource = "netkan";

        private readonly IHttpService _http;

        public string Name { get { return "metanetkan"; } }

        public MetaNetkanTransformer(IHttpService http)
        {
            _http = http;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref != null && metadata.Kref.Source == KrefSource)
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing MetaNetkan transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var uri = new Uri(metadata.Kref.Id);
                var targetFileText = _http.DownloadText(CKAN.Net.GetRawUri(uri));

                Log.DebugFormat("Target netkan:{0}{1}", Environment.NewLine, targetFileText);

                var targetJson = JObject.Parse(targetFileText);
                var targetMetadata = new Metadata(targetJson);

                if (targetMetadata.Kref == null || targetMetadata.Kref.Source != "netkan")
                {
                    json["spec_version"] = ModuleVersion.Max(metadata.SpecVersion, targetMetadata.SpecVersion)
                        .ToSpecVersionJson();

                    if (targetJson["$kref"] != null)
                    {
                        json["$kref"] = targetJson["$kref"];
                    }
                    else
                    {
                        json.Remove("$kref");
                    }

                    foreach (var property in targetJson.Properties())
                    {
                        json.SafeAdd(property.Name, property.Value);
                    }

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                    yield return new Metadata(json);
                }
                else
                {
                    throw new Kraken("The target of a metanetkan may not also be a metanetkan.");
                }
            }
            else
            {
                yield return metadata;
            }
        }
    }
}
