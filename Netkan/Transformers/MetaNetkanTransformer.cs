using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using Newtonsoft.Json.Linq;

using CKAN.Versioning;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;

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
        private readonly IGithubApi   _github;

        public string Name => "metanetkan";

        public MetaNetkanTransformer(IHttpService http, IGithubApi github)
        {
            _http   = http;
            _github = github;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref != null && metadata.Kref.Source == KrefSource)
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing MetaNetkan transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // Make sure resources exist, save metanetkan
                if (json["resources"] == null)
                {
                    json["resources"] = new JObject();
                }

                var resourcesJson = (JObject)json["resources"];
                resourcesJson.SafeAdd("metanetkan", metadata.Kref.Id);

                var uri = new Uri(metadata.Kref.Id);
                var targetFileText = _github?.DownloadText(uri)
                    ?? _http.DownloadText(Net.GetRawUri(uri));

                Log.DebugFormat("Target netkan:{0}{1}", Environment.NewLine, targetFileText);

                var targetJsons = YamlExtensions.Parse(targetFileText)
                                                .Select(ymap => ymap.ToJObject())
                                                .ToArray();

                foreach (var targetJson in targetJsons)
                {
                    var targetMetadata =  new Metadata(targetJson);
                    if (targetMetadata.Kref == null || targetMetadata.Kref.Source != "netkan")
                    {
                        if (targetJson["$kref"] != null)
                        {
                            json["$kref"] = targetJson["$kref"];
                        }
                        else
                        {
                            json.Remove("$kref");
                        }

                        json.SafeMerge("resources", targetJson["resources"]);

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
            }
            else
            {
                yield return metadata;
            }
        }
    }
}
