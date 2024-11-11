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

        public string Name => "strip_netkan_metadata";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions? opts)
        {
            var json = metadata.Json();

            Log.Debug("Executing strip Netkan metadata transformation");
            Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

            Strip(json);

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            yield return new Metadata(json);
        }

        private static bool IsNetkanProperty(string propertyName)
            => propertyName.StartsWith("x_netkan") || propertyName is "$kref" or "$vref";

        public static JObject Strip(JObject json)
        {
            var propertiesToRemove = new List<string>();
            foreach (var property in json.Properties())
            {
                if (IsNetkanProperty(property.Name))
                {
                    propertiesToRemove.Add(property.Name);
                }
                else
                {
                    switch (property.Value)
                    {
                        case JObject jobj:
                            Strip(jobj);
                            break;
                        case JArray jarr:
                            foreach (var element in jarr.OfType<JObject>())
                            {
                                Strip(element);
                            }
                            break;
                    }
                }
            }
            foreach (var propertyName in propertiesToRemove)
            {
                json.Remove(propertyName);
            }
            return json;
        }
    }
}
