using System;
using CKAN.NetKAN.Model;
using log4net;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that adds a property to indicate the program responsible for generating the
    /// metadata.
    /// </summary>
    internal sealed class LastModifiedTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LastModifiedTransformer));

        public string Name { get { return "last_modified"; } }

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();

            Log.InfoFormat("Executing Last Updated transformation with {0}", metadata.Kref);
            Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

            json["last_modified"] = DateTime.UtcNow;

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            return new Metadata(json);
        }
    }
}
