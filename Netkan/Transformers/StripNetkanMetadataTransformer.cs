using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.NetKAN.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that removes metadata useful only to NetKAN.
    /// </summary>
    internal sealed class StripNetkanMetadataTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StripNetkanMetadataTransformer));

        public string Name { get { return "strip_netkan_metadata"; } }

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();

            Log.InfoFormat("Executing Strip Netkan Metadata transformation with {0}", metadata.Kref);
            Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

            Strip(json);

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            return new Metadata(json);
        }

        private static void Strip(JObject metadata)
        {
            var propertiesToRemove = new List<string>();

            foreach (var property in metadata.Properties())
            {
                if (property.Name.StartsWith("x_netkan"))
                {
                    propertiesToRemove.Add(property.Name);
                }
                else switch (property.Value.Type)
                {
                    case JTokenType.Object:
                        Strip((JObject)property.Value);
                        break;
                    case JTokenType.Array:
                        foreach (var element in ((JArray)property.Value).Where(i => i.Type == JTokenType.Object))
                        {
                            Strip((JObject)element);
                        }
                        break;
                }
            }

            foreach (var property in propertiesToRemove)
            {
                metadata.Remove(property);
            }

            if (metadata["$kref"] != null)
            {
                metadata.Remove("$kref");
            }

            if (metadata["$vref"] != null)
            {
                metadata.Remove("$vref");
            }
        }
    }
}
