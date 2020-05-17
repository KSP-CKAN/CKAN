using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;

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
                var package = _http.DownloadPackage(
                    metadata.Download,
                    metadata.Identifier,
                    metadata.RemoteTimestamp
                );
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile zip  = new ZipFile(package);

                    bool hasMMsyntax = _moduleService.GetConfigFiles(mod, zip)
                        .Select(cfg => new StreamReader(zip.GetInputStream(cfg.source)).ReadToEnd())
                        .Any(contents => moduleManagerRegex.IsMatch(contents));

                    bool dependsOnMM = mod?.depends?.Any(r => r.ContainsAny(identifiers)) ?? false;

                    if (hasMMsyntax && !dependsOnMM)
                    {
                        Log.Warn("ModuleManager syntax used without ModuleManager dependency");
                    }
                    else if (!hasMMsyntax && dependsOnMM)
                    {
                        Log.Warn("ModuleManager dependency may not be needed, no ModuleManager syntax found");
                    }
                }
            }
        }

        private string[] identifiers = new string[] { "ModuleManager" };

        private static readonly Regex moduleManagerRegex = new Regex(
            @"^\s*[@+$\-!%]",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleManagerDependsValidator));
    }
}
