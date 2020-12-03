using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class PluginCompatibilityValidator : IValidator
    {
        public PluginCompatibilityValidator(IHttpService http, IModuleService moduleService)
        {
            _http          = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            Log.Info("Validating that metadata compatibility is appropriate for DLLs");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile      zip  = new ZipFile(package);
                    GameInstance inst = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());

                    bool hasPlugin = _moduleService.GetPlugins(mod, zip, inst).Any();

                    bool boundedCompatibility = json.ContainsKey("ksp_version") || json.ContainsKey("ksp_version_max");

                    if (hasPlugin && !boundedCompatibility)
                    {
                        Log.Warn("Unbounded future compatibility for module with a plugin, consider setting $vref or ksp_version or ksp_version_max");
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginCompatibilityValidator));
    }
}
