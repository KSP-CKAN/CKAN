using System;
using System.IO;
using System.Linq;

namespace CKAN.Exporters
{
    public sealed class DelimeterSeperatedValueExporter : IExporter
    {
        private const string WritePattern = "{1}{0}{2}{0}{3}{0}{4}{0}{5}" +
                                            "{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}" +
                                            "{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}" +
                                            "{0}{16}{0}{17}{0}{18}";
        private readonly string _delimter;

        public DelimeterSeperatedValueExporter(Delimter delimter)
        {
            switch (delimter)
            {
                case Delimter.Comma:
                    _delimter = ",";
                    break;
                case Delimter.Tab:
                    _delimter = "\t";
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
                    _delimter,
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
                    "kerbalstuff"
                );

                foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
                {
                    writer.WriteLine(WritePattern,
                        _delimter,
                        mod.Module.identifier,
                        mod.Module.version,
                        QuoteIfNecessary(mod.Module.name),
                        QuoteIfNecessary(mod.Module.@abstract),
                        QuoteIfNecessary(mod.Module.description),
                        QuoteIfNecessary(string.Join(";", mod.Module.author)),
                        QuoteIfNecessary(mod.Module.kind),
                        WriteUri(mod.Module.download),
                        mod.Module.download_size,
                        mod.Module.ksp_version,
                        mod.Module.ksp_version_min,
                        mod.Module.ksp_version_max,
                        mod.Module.license,
                        mod.Module.release_status,
                        WriteRepository(mod.Module.resources),
                        WriteHomepage(mod.Module.resources),
                        WriteBugtracker(mod.Module.resources),
                        WriteKerbalStuff(mod.Module.resources)
                    );
                }
            }
        }

        private string WriteUri(Uri uri)
        {
            return uri != null ? QuoteIfNecessary(uri.ToString()) : string.Empty;
        }

        private string WriteRepository(ResourcesDescriptor resources)
        {
            if (resources != null && resources.repository != null)
            {
                return QuoteIfNecessary(resources.repository.ToString());
            }

            return string.Empty;
        }

        private string WriteHomepage(ResourcesDescriptor resources)
        {
            if (resources != null && resources.homepage != null)
            {
                return QuoteIfNecessary(resources.homepage.ToString());
            }
            else if (resources != null && resources.homepage = null && resources.kerbalstuff != null)
            {
                return QuoteIfNecessary(resources.kerbalstuff.ToString());
            }
            
            return string.Empty;
        }

        private string WriteBugtracker(ResourcesDescriptor resources)
        {
            if (resources != null && resources.bugtracker != null)
            {
                return QuoteIfNecessary(resources.bugtracker.ToString());
            }

            return string.Empty;
        }

        private string WriteKerbalStuff(ResourcesDescriptor resources)
        {
            if (resources != null && resources.kerbalstuff != null)
            {
                return QuoteIfNecessary(resources.kerbalstuff.ToString());
            }

            return string.Empty;
        }

        private string QuoteIfNecessary(string value)
        {
            if (value != null && value.IndexOf(_delimter, StringComparison.Ordinal) >= 0)
            {
                return "\"" + value + "\"";
            }
            else
            {
                return value;
            }
        }

        public enum Delimter
        {
            Comma,
            Tab
        }


    }
}
