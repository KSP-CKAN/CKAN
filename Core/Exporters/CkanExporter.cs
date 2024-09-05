using System.IO;

namespace CKAN.Exporters
{
    public sealed class CkanExporter : IExporter
    {
        public void Export(RegistryManager  manager,
                           IRegistryQuerier registry,
                           Stream           stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(CkanModule.ToJson(manager.GenerateModpack(false, false)));
            }
        }
    }
}
