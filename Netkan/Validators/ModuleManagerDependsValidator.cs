using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Extensions;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ModuleManagerDependsValidator : IValidator
    {
        public ModuleManagerDependsValidator(IHttpService http, IModuleService moduleService)
        {
            _http          = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            Log.Info("Validating that metadata dependencies are consistent with cfg file syntax");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile zip = new ZipFile(package);
                    var     ksp = new KSP("/", "dummy", null, false);

                    var mmConfigs = _moduleService.GetConfigFiles(mod, zip, ksp)
                        .Where(cfg => moduleManagerRegex.IsMatch(
                            new StreamReader(zip.GetInputStream(cfg.source)).ReadToEnd()))
                        .Memoize();

                    bool dependsOnMM = mod?.depends?.Any(r => r.ContainsAny(identifiers)) ?? false;

                    if (!dependsOnMM && mmConfigs.Any())
                    {
                        Log.WarnFormat(
                            "ModuleManager syntax used without ModuleManager dependency: {0}",
                            string.Join(", ", mmConfigs.Select(cfg => cfg.source))
                        );
                    }
                    else if (dependsOnMM && !mmConfigs.Any())
                    {
                        Log.Warn("ModuleManager dependency may not be needed, no ModuleManager syntax found");
                    }
                }
            }
        }

        private string[] identifiers = new string[] { "ModuleManager" };

        private static readonly Regex moduleManagerRegex = new Regex(
            @"^\s*[@+$\-!%]|^\s*[a-zA-Z0-9_]+:",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleManagerDependsValidator));
    }
}
