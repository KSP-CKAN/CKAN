using System.IO;

namespace CKAN.Exporters
{
    public sealed class CkanExporter : IExporter
    {
        public void Export(RegistryManager  manager,
                           IRegistryQuerier registry,
                           Stream           stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                writer.Write(manager.GenerateModpack(false, false).ToJson());
            }
        }
    }
}
