using System;
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

        public Metadata Transform(Metadata metadata)
        {
            Log.Debug("Fixing version strings (if required)...");

            var json = metadata.Json();

            JToken epoch;
            if (json.TryGetValue("x_netkan_epoch", out epoch))
            {
                Log.InfoFormat("Executing Epoch transformation with {0}", metadata.Kref);
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
                    Log.Error("Invaild epoch: " + epoch);
                    throw new BadMetadataKraken(null, "Invaild epoch: " + epoch + "In " + json["identifier"]);
                }
            }

            return new Metadata(json);
        }
    }
}
