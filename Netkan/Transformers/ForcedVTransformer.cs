using System;
using System.Collections.Generic;

using log4net;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that prepends the letter 'v' to the version if necessary.
    /// </summary>
    internal sealed class ForcedVTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ForcedVTransformer));

        public string Name => "forced_v";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {

            if (metadata.ForceV)
            {
                var json = metadata.Json();
                Log.InfoFormat("Executing forced-v transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                // Force a 'v' in front of the version string if it's not there
                // already.

                var version = (string?)json.GetValue("version");
                if (version != null && !version.StartsWith("v"))
                {
                    Log.InfoFormat("Force-adding 'v' to start of {0}", version);
                    version = "v"
                        + (version.StartsWith("V") ? version[1..] : version);
                    json["version"] = version;
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                yield return new Metadata(json);
                yield break;
            }
            yield return metadata;
        }
    }
}
