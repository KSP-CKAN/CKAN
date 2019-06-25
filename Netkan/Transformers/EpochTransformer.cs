using System;
using System.Collections.Generic;
using CKAN.NetKAN.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that uses a particular version epoch if necessary.
    /// </summary>
    internal sealed class EpochTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EpochTransformer));

        public string Name { get { return "epoch"; } }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            Log.Debug("Fixing version strings (if required)...");

            var json = metadata.Json();

            JToken epoch;
            if (json.TryGetValue("x_netkan_epoch", out epoch))
            {
                Log.InfoFormat("Executing epoch transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                uint epochNumber;
                if (uint.TryParse(epoch.ToString(), out epochNumber))
                {
                    //Implicit if zero. No need to add
                    if (epochNumber != 0)
                        json["version"] = epochNumber + ":" + json["version"];

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }
                else
                {
                    throw new BadMetadataKraken(null, "Invalid epoch: " + epoch + "In " + json["identifier"]);
                }
            }

            yield return new Metadata(json);
        }
    }
}
