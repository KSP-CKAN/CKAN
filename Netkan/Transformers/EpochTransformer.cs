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

        public string Name => "epoch";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            Log.Debug("Fixing version strings (if required)...");

            var json = metadata.Json();

            if (json.TryGetValue("x_netkan_epoch", out JToken epoch))
            {
                Log.InfoFormat("Executing epoch transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                if (uint.TryParse(epoch.ToString(), out uint epochNumber))
                {
                    //Implicit if zero. No need to add
                    if (epochNumber != 0)
                    {
                        json["version"] = epochNumber + ":" + json["version"];
                    }

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }
                else
                {
                    throw new BadMetadataKraken(null, "Invalid epoch: " + epoch + " In " + json["identifier"]);
                }
            }

            if (json.TryGetValue("x_netkan_allow_out_of_order", out JToken allowOOO) && (bool)allowOOO)
            {
                Log.Debug("Out of order versions enabled in netkan, skipping OOO check");
            }
            else if (opts.HighestVersion != null)
            {
                // Ensure we are greater or equal to the previous max
                ModuleVersion startV = new ModuleVersion((string)json["version"]);
                ModuleVersion currentV = startV;
                while (currentV < opts.HighestVersion)
                {
                    Log.DebugFormat("Auto-epoching out of order version: {0} < {1}",
                        currentV, opts.HighestVersion);
                    // Increment epoch if too small
                    currentV = currentV.IncrementEpoch();
                }
                if (!opts.HighestVersion.EpochEquals(currentV)
                    && startV < opts.HighestVersion && opts.HighestVersion < currentV)
                {
                    if (opts.FlakyAPI)
                    {
                        throw new Kraken($"Out-of-order version found on unreliable server: {startV} < {opts.HighestVersion} < {currentV}");
                    }
                    else
                    {
                        // New file, tell the Indexer to be careful
                        opts.Staged = true;
                        opts.StagingReasons.Add($"Auto-epoching out of order version: {startV} < {opts.HighestVersion} < {currentV}");
                    }
                }
                json["version"] = currentV.ToString();
            }

            yield return new Metadata(json);
        }
    }
}
