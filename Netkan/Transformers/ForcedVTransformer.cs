using System;
using CKAN.NetKAN.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that prepends the letter 'v' to the version if necessary.
    /// </summary>
    internal sealed class ForcedVTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ForcedVTransformer));

        public string Name { get { return "forced_v"; } }

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();

            JToken forceV;
            if (json.TryGetValue("x_netkan_force_v", out forceV) && (bool)forceV)
            {
                Log.InfoFormat("Executing Forced-V transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // Force a 'v' in front of the version string if it's not there
                // already.

                var version = (string)json.GetValue("version");

                if (!version.StartsWith("v") && !version.StartsWith("V"))
                {
                    Log.InfoFormat("Force-adding 'v' to start of {0}", version);
                    version = "v" + version;
                    json["version"] = version;
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
            }

            return new Metadata(json);
        }
    }
}
