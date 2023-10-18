using System.IO;
using System.Linq;

namespace CKAN.Exporters
{
    public sealed class PlainTextExporter : IExporter
    {

        public void Export(IRegistryQuerier registry, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
                {
                    writer.WriteLine(@"{0} ({1} {2})", mod.Module.name, mod.identifier, mod.Module.version);
                }
            }
        }
    }
}
