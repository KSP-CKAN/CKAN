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
        public ModuleManagerDependsValidator(IModuleService moduleService, ICacheService cacheService)
        {
            _moduleService = moduleService;
            _cacheService  = cacheService;
        }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata dependencies are consistent with cfg file syntax");

            JObject    json = metadata.AllJson;
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (mod.IsDLC)
            {
                return ;
            }

            _cacheService.ExecuteCached(metadata, typeof(ModuleManagerDependsValidator), package =>
            {
                if (string.IsNullOrEmpty(package))
                {
                    return;
                }

                using ZipFile zip = new ZipFile(package);
                var mmConfigs = _moduleService.GetConfigFiles(mod, zip)
                    .Where(cfg => moduleManagerRegex.IsMatch(
                        new StreamReader(zip.GetInputStream(cfg.source)).ReadToEnd()))
                    .Memoize();

                bool dependsOnMM = mod?.depends?.Any(r => r.ContainsAny(identifiers)) ?? false;

                if (!dependsOnMM && mmConfigs.Any())
                {
                    Log.WarnFormat(
                        "ModuleManager syntax used without ModuleManager dependency: {0}",
                        string.Join(", ", mmConfigs.Select(cfg => cfg.source.Name).Order())
                    );
                }
                else if (dependsOnMM && !mmConfigs.Any())
                {
                    Log.Warn("ModuleManager dependency may not be needed, no ModuleManager syntax found");
                }
            });
        }

        private readonly string[] identifiers = new string[] { "ModuleManager" };

        private static readonly Regex moduleManagerRegex = new Regex(
            @"^\s*[@+$\-!%]|^\s*[a-zA-Z0-9_]+:",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );

        private readonly IModuleService _moduleService;
        private readonly ICacheService  _cacheService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleManagerDependsValidator));
    }
}
