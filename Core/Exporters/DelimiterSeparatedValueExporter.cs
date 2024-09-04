using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CKAN.Exporters
{
    public sealed class DelimiterSeparatedValueExporter : IExporter
    {
        public DelimiterSeparatedValueExporter(Delimiter delimiter)
        {
            _delimiter = delimiter switch
            {
                Delimiter.Comma => ",",
                Delimiter.Tab   => "\t",
                _               => throw new ArgumentOutOfRangeException(nameof(delimiter),
                                                                         delimiter.ToString()),
            };
        }

        public void Export(RegistryManager manager, IRegistryQuerier registry, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(string.Join(_delimiter,
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
                                             "curse"));

                foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
                {
                    writer.WriteLine(string.Join(_delimiter,
                                                 mod.Module.identifier,
                                                 mod.Module.version,
                                                 QuoteIfNecessary(mod.Module.name),
                                                 QuoteIfNecessary(mod.Module.@abstract),
                                                 QuoteIfNecessary(mod.Module.description),
                                                 QuoteIfNecessary(mod.Module.author == null ? "" : string.Join(";", mod.Module.author)),
                                                 QuoteIfNecessary(mod.Module.kind),
                                                 WriteUri(mod.Module.download),
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
                                                 WriteCurse(mod.Module.resources)));
                }
            }
        }

        private string WriteUri(Uri? uri)
            => uri != null ? QuoteIfNecessary(uri.ToString())
                           : string.Empty;

        private string WriteUri(List<Uri>? uris)
            => WriteUri(//uris is [Uri uri, ..]
                        uris != null
                        && uris.Count > 0
                        && uris[0] is Uri uri
                            ? uri
                            : null);

        private string WriteRepository(ResourcesDescriptor? resources)
            => resources != null && resources.repository != null
                ? QuoteIfNecessary(resources.repository.ToString())
                : string.Empty;

        private string WriteHomepage(ResourcesDescriptor? resources)
            => resources != null && resources.homepage != null
                ? QuoteIfNecessary(resources.homepage.ToString())
                : string.Empty;

        private string WriteBugtracker(ResourcesDescriptor? resources)
            => resources != null && resources.bugtracker != null
                ? QuoteIfNecessary(resources.bugtracker.ToString())
                : string.Empty;

        private string WriteDiscussions(ResourcesDescriptor? resources)
            => resources != null && resources.discussions != null
                ? QuoteIfNecessary(resources.discussions.ToString())
                : string.Empty;

        private string WriteSpaceDock(ResourcesDescriptor? resources)
            => resources != null && resources.spacedock != null
                ? QuoteIfNecessary(resources.spacedock.ToString())
                : string.Empty;

        private string WriteCurse(ResourcesDescriptor? resources)
            => resources != null && resources.curse != null
                ? QuoteIfNecessary(resources.curse.ToString())
                : string.Empty;

        private string QuoteIfNecessary(string? value)
            => value == null
                ? ""
                : value.IndexOf(_delimiter, StringComparison.Ordinal) >= 0
                    ? "\"" + value + "\""
                    : value;

        public enum Delimiter
        {
            Comma,
            Tab
        }

        private readonly string _delimiter;
    }
}
