using System;
using CKAN.NetKAN.Model;
using log4net;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from an arbitrary HTTP endpoint.
    /// </summary>
    internal sealed class HttpTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpTransformer));

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "http")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing HTTP transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                json["download"] = metadata.Kref.Id;

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }
            else
            {
                return metadata;
            }
        }
    }
}
