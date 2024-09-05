using System;
using System.IO;

using CKAN.Types;

namespace CKAN.Exporters
{
    /// <summary>
    /// An implementation of <see cref="IExporter"/> that knows how to export to the different types of
    /// <see cref="ExportFileType"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="ExportFileType.Ckan"/> is currently unhandled as that requires use of the
    /// <see cref="RegistryManager"/> rather than just the <see cref="Registry"/>.
    /// </remarks>
    public sealed class Exporter : IExporter
    {
        private readonly IExporter _exporter;

        public Exporter(ExportFileType exportFileType)
        {
            _exporter = exportFileType switch
            {
                ExportFileType.Ckan      => new CkanExporter(),
                ExportFileType.PlainText => new PlainTextExporter(),
                ExportFileType.Markdown  => new MarkdownExporter(),
                ExportFileType.BbCode    => new BbCodeExporter(),
                ExportFileType.Csv       => new DelimiterSeparatedValueExporter(DelimiterSeparatedValueExporter.Delimiter.Comma),
                ExportFileType.Tsv       => new DelimiterSeparatedValueExporter(DelimiterSeparatedValueExporter.Delimiter.Tab),
                _                        => throw new ArgumentOutOfRangeException(nameof(exportFileType),
                                                                                  exportFileType.ToString()),
            };
        }

        public void Export(RegistryManager  manager,
                           IRegistryQuerier registry,
                           Stream           stream)
        {
            _exporter.Export(manager, registry, stream);
        }
    }
}
