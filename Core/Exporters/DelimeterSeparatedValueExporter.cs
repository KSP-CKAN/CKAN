using System;
using System.IO;
using System.Linq;

namespace CKAN.Exporters
{
    public sealed class DelimeterSeparatedValueExporter : IExporter
    {
        private const string WritePattern = "{1}{0}{2}{0}{3}{0}{4}{0}{5}" +
                                            "{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}" +
                                            "{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}" +
                                            "{0}{16}{0}{17}{0}{18}";
        private readonly string _delimeter;

        public DelimeterSeparatedValueExporter(Delimeter delimeter)
        {
            switch (delimeter)
            {
                case Delimeter.Comma:
                    _delimeter = ",";
                    break;
                case Delimeter.Tab:
                    _delimeter = "\t";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Export(IRegistryQuerier registry, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(WritePattern,
                                 _delimeter,
                                 "identifier",
                                 "version",
                                 "name",
                                 "abstract",
                                 "description",
                                 "author",
                                 "kind",
                                 "download",
                                 "download_size",
                                 "ksp_version",
                                 "ksp_version_min",
                                 "ksp_version_max",
                                 "license",
                                 "release_status",
                                 "repository",
                                 "homepage",
                                 "bugtracker",
                                 "discussions",
                                 "spacedock",
                                 "curse");

                foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
                {
                    writer.WriteLine(WritePattern,
                                     _delimeter,
                                     mod.Module.identifier,
                                     mod.Module.version,
                                     QuoteIfNecessary(mod.Module.name),
                                     QuoteIfNecessary(mod.Module.@abstract),
                                     QuoteIfNecessary(mod.Module.description),
                                     QuoteIfNecessary(string.Join(";", mod.Module.author)),
                                     QuoteIfNecessary(mod.Module.kind),
                                     WriteUri(mod.Module.download?[0]),
                                     mod.Module.download_size,
                                     mod.Module.ksp_version,
                                     mod.Module.ksp_version_min,
                                     mod.Module.ksp_version_max,
                                     QuoteIfNecessary(string.Join(";",mod.Module.license)),
                                     mod.Module.release_status,
                                     WriteRepository(mod.Module.resources),
                                     WriteHomepage(mod.Module.resources),
                                     WriteBugtracker(mod.Module.resources),
                                     WriteDiscussions(mod.Module.resources),
                                     WriteSpaceDock(mod.Module.resources),
                                     WriteCurse(mod.Module.resources));
                }
            }
        }

        private string WriteUri(Uri uri)
            => uri != null
            ? QuoteIfNecessary(uri.ToString())
            : string.Empty;

        private string WriteRepository(ResourcesDescriptor resources)
            => resources != null && resources.repository != null
                ? QuoteIfNecessary(resources.repository.ToString())
                : string.Empty;

        private string WriteHomepage(ResourcesDescriptor resources)
            => resources != null && resources.homepage != null
                ? QuoteIfNecessary(resources.homepage.ToString())
                : string.Empty;

        private string WriteBugtracker(ResourcesDescriptor resources)
            => resources != null && resources.bugtracker != null
                ? QuoteIfNecessary(resources.bugtracker.ToString())
                : string.Empty;

        private string WriteDiscussions(ResourcesDescriptor resources)
            => resources != null && resources.discussions != null
                ? QuoteIfNecessary(resources.discussions.ToString())
                : string.Empty;

        private string WriteSpaceDock(ResourcesDescriptor resources)
            => resources != null && resources.spacedock != null
                ? QuoteIfNecessary(resources.spacedock.ToString())
                : string.Empty;

        private string WriteCurse(ResourcesDescriptor resources)
            => resources != null && resources.curse != null
                ? QuoteIfNecessary(resources.curse.ToString())
                : string.Empty;

        private string QuoteIfNecessary(string value)
            => value != null && value.IndexOf(_delimeter, StringComparison.Ordinal) >= 0
                ? "\"" + value + "\""
                : value;

        public enum Delimeter
        {
            Comma,
            Tab
        }

    }
}
