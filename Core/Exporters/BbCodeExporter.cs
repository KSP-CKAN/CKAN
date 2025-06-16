using System.IO;
using System.Linq;
using System.Text;

namespace CKAN.Exporters
{
    public sealed class BbCodeExporter : IExporter
    {
        public void Export(RegistryManager manager, IRegistryQuerier registry, Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
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
