using System;
using System.Linq;
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

            JObject? json = null;

            // Implicit if zero. No need to add
            if (metadata.Epoch is not null and not 0)
            {
                json ??= metadata.Json();
                Log.InfoFormat("Executing epoch transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);
                json["version"] = $"{metadata.Epoch}:{metadata.Version}";
                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
            }

            if (metadata.AllowOutOfOrder)
            {
                Log.Debug("Out of order versions enabled in netkan, skipping OOO check");
            }
            else if (metadata.Version != null
                     && (metadata.Prerelease
                             ? new ModuleVersion?[]
                               {
                                   opts.HighestVersionPrerelease,
                                   opts.HighestVersion,
                               }
                             : new ModuleVersion?[]
                               {
                                   opts.HighestVersion,
                               })
                            .OfType<ModuleVersion>()
                            .Max()
                        is ModuleVersion highest)
            {
                json ??= metadata.Json();
                json["version"] = CheckOutOfOrder(opts, highest, metadata.Version)
                                      .ToString();
            }

            yield return json == null ? metadata
                                      : new Metadata(json);
        }

        private static ModuleVersion CheckOutOfOrder(TransformOptions opts,
                                                     ModuleVersion    highest,
                                                     ModuleVersion    start)
        {
            // Ensure we are greater or equal to the previous max
            ModuleVersion current = start;
            while (current < highest)
            {
                Log.DebugFormat("Auto-epoching out of order version: {0} < {1}",
                                current, highest);
                // Increment epoch if too small
                current = current.IncrementEpoch();
            }
            if (!highest.EpochEquals(current) && start < highest && highest < current)
            {
                if (opts.FlakyAPI)
                {
                    throw new Kraken($"Out-of-order version found on unreliable server: {start} < {highest} < {current}");
                }
                else
                {
                    // New file, tell the Indexer to be careful
                    opts.Staged = true;
                    opts.StagingReasons.Add($"Auto-epoching out of order version: {start} < {highest} < {current}");
                }
            }
            return current;
        }
    }
}
