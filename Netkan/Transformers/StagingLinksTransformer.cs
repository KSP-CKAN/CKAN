using System.Collections.Generic;
using System.Text;
using System.Linq;

using log4net;

#if NETFRAMEWORK
using CKAN.Extensions;
#endif
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

        private static string LinkTable(Metadata metadata)
        {
            StringBuilder table = new StringBuilder();
            table.AppendLine("Resource | URL");
            table.AppendLine(":-- | :--");
            var downloads = string.Join(" ", metadata.Download?.Select(d => $"<{d}>")
                                                              ?? Enumerable.Empty<string>());
            table.AppendLine($"download | {downloads}");
            if (metadata.Resources != null)
            {
                foreach ((string name, string value) in metadata.Resources
                                                                .OrderBy(kvp => kvp.Key))
                {
                    table.AppendLine($"{name} | <{value}>");
                }
            }
            return table.ToString();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(StagingLinksTransformer));
    }
}
