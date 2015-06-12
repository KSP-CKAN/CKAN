using System;
using System.IO;
using CKAN.Types;

namespace CKAN.Exporters
{
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

        public void Export(Registry registry, Stream stream)
        {
            _exporter.Export(registry, stream);
        }
    }
}
