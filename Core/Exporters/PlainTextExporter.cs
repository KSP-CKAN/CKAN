using System.IO;

namespace CKAN.Exporters
{
    public sealed class PlainTextExporter : IExporter
    {
        public void Export(Registry registry, TextWriter writer)
        {
            foreach (var mod in registry.InstalledModules)
            {
                writer.WriteLine(@"{0} ({1} {2})", mod.Module.name, mod.identifier, mod.Module.version);
            }

            writer.Flush();
        }
    }
}
