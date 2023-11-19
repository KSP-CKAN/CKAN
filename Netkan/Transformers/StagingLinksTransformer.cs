using System.Collections.Generic;
using System.Text;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class StagingLinksTransformer : ITransformer
    {
        public string Name => "staging_links";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (opts.Staged && opts.StagingReasons.Count > 0)
            {
                var table = LinkTable(metadata);
                if (!string.IsNullOrEmpty(table))
                {
                    log.DebugFormat("Adding links to staging reason: {0}",
                        table);
                    opts.StagingReasons.Add(table);
                }
            }
            // This transformer never changes the metadata
            yield return metadata;
        }

        private string LinkTable(Metadata metadata)
        {
            var resourcesJson = (JObject)metadata?.Json()?["resources"];
            if (resourcesJson == null)
            {
                // No resources, no links to append
                return "";
            }
            StringBuilder table = new StringBuilder();
            // Blank lines to separate the table from the description
            table.AppendLine("Resource | URL");
            table.AppendLine(":-- | :--");
            table.AppendLine($"download | <{metadata.Download}>");
            foreach (var prop in resourcesJson.Properties().OrderBy(prop => prop.Name))
            {
                table.AppendLine($"{prop.Name} | <{prop.Value}>");
            }
            return table.ToString();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(StagingLinksTransformer));
    }
}
