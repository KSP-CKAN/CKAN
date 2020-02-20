using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using log4net;
using CKAN.Extensions;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class StagingTransformer : ITransformer
    {
        public string Name { get { return "staging"; } }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            JObject json = metadata.Json();
            var matchingKeys = kspVersionKeys
                .Where(vk => json.ContainsKey(vk)
                          && !string.IsNullOrEmpty((string)json[vk])
                          && (string)json[vk] != "any")
                .Memoize();
            if (matchingKeys.Any())
            {
                string msg = string.Join(", ", matchingKeys.Select(mk => $"{mk} = {json[mk]}"));
                Log.DebugFormat("Enabling staging, found KSP version keys in netkan: {0}", msg);
                opts.Staged = true;
                opts.StagingReason = $"Game version keys found in netkan: {msg}.\r\n\r\nPlease check that their values match the forum thread.";
            }
            // This transformer never changes the metadata
            yield return metadata;
        }

        private static readonly string[] kspVersionKeys = new string[]
        {
            "ksp_version",
            "ksp_version_min",
            "ksp_version_max",
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(StagingTransformer));
    }
}
