using CKAN.Types;
using System;
using System.IO;

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
            switch (exportFileType)
            {
                case ExportFileType.PlainText:
                    _exporter = new PlainTextExporter();
                    break;

                case ExportFileType.Markdown:
                    _exporter = new MarkdownExporter();
                    break;

                case ExportFileType.BbCode:
                    _exporter = new BbCodeExporter();
                    break;

                case ExportFileType.Csv:
                    _exporter = new DelimeterSeperatedValueExporter(DelimeterSeperatedValueExporter.Delimter.Comma);
                    break;

                case ExportFileType.Tsv:
                    _exporter = new DelimeterSeperatedValueExporter(DelimeterSeperatedValueExporter.Delimter.Tab);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Export(IRegistryQuerier registry, Stream stream)
        {
            _exporter.Export(registry, stream);
        }
    }
}