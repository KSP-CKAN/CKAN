using System;
using System.Collections.Generic;

using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Extensions;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that removes metadata useful only to NetKAN.
    /// </summary>
    internal sealed class StripNetkanMetadataTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StripNetkanMetadataTransformer));

        public string Name => "strip_netkan_metadata";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            var json = metadata.Json();

            Log.Debug("Executing strip Netkan metadata transformation");
            Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

            json.StripProperties(IsNetkanProperty);

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            yield return new Metadata(json);
        }

        private static bool IsNetkanProperty(JProperty prop)
            => prop.Name.StartsWith("x_netkan") || prop.Name is "$kref" or "$vref";
    }
}
