using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Model;

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
                    throw new BadMetadataKraken(null, "Invalid epoch: " + epoch + " In " + json["identifier"]);
                }
            }

            JToken allowOOO;
            if (json.TryGetValue("x_netkan_allow_out_of_order", out allowOOO) && (bool)allowOOO)
            {
                Log.Debug("Out of order versions enabled in netkan, skipping OOO check");
            }
            else if (opts.HighestVersion != null)
            {
                // Ensure we are greater or equal to the previous max
                ModuleVersion currentV = new ModuleVersion((string)json["version"]);
                while (currentV < opts.HighestVersion)
                {
                    Log.DebugFormat("Auto-epoching out of order version: {0} < {1}",
                        currentV, opts.HighestVersion);
                    // Tell the Indexer to be careful
                    opts.Staged = true;
                    opts.StagingReason = $"Auto-epoching out of order version: {currentV} < {opts.HighestVersion}";
                    // Increment epoch if too small
                    currentV = currentV.IncrementEpoch();
                }
                json["version"] = currentV.ToString();
            }

            yield return new Metadata(json);
        }
    }
}
