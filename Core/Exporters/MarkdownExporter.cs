using System.IO;
using System.Linq;

namespace CKAN.Exporters
{
    public sealed class MarkdownExporter : IExporter
    {
        public void Export(Registry registry, TextWriter writer)
        {
            foreach (var mod in registry.InstalledModules.OrderBy(i => i.Module.name))
            {
                writer.WriteLine(@"- *{0}* `{1} {2}`", mod.Module.name, mod.identifier, mod.Module.version);
            }

            writer.Flush();
        }
    }
}
