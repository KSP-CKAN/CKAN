using System.IO;
using System.Linq;

namespace CKAN.Exporters
{
    public sealed class BbCodeExporter : IExporter
    {
        public void Export(Registry registry, TextWriter writer)
        {
            writer.WriteLine("[LIST]");

            foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
            {
                writer.WriteLine(@"[*][I]{0}[/I] ({1} {2})", mod.Module.name, mod.identifier, mod.Module.version);
            }

            writer.WriteLine("[/LIST]");

            writer.Flush();
        }
    }
}
