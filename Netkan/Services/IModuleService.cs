using CKAN.NetKAN.Sources.Avc;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Services
{
    internal interface IModuleService
    {
        AvcVersion GetInternalAvc(CkanModule module, string filePath, string internalFilePath = null);
        JObject GetInternalCkan(string filePath);
        bool HasInstallableFiles(CkanModule module, string filePath);
    }
}
