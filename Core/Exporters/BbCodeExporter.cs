using System.IO;
using System.Linq;

namespace CKAN.Exporters
{
    public sealed class BbCodeExporter : IExporter
    {
        public void Export(IRegistryQuerier registry, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("[LIST]");

                foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
                {
                    writer.WriteLine(@"[*][B]{0}[/B] ({1} {2})", mod.Module.name, mod.identifier, mod.Module.version);
                }

                writer.WriteLine("[/LIST]");
            }
        }
    }
}
