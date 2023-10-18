using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Extensions;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ModuleManagerDependsValidator : IValidator
    {
        public ModuleManagerDependsValidator(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http          = http;
            _moduleService = moduleService;
            _game          = game;
        }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata dependencies are consistent with cfg file syntax");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile zip = new ZipFile(package);
                    GameInstance inst = new GameInstance(_game, "/", "dummy", new NullUser());
                    var mmConfigs = _moduleService.GetConfigFiles(mod, zip, inst)
                        .Where(cfg => moduleManagerRegex.IsMatch(
                            new StreamReader(zip.GetInputStream(cfg.source)).ReadToEnd()))
                        .Memoize();

                    bool dependsOnMM = mod?.depends?.Any(r => r.ContainsAny(identifiers)) ?? false;

                    if (!dependsOnMM && mmConfigs.Any())
                    {
                        Log.WarnFormat(
                            "ModuleManager syntax used without ModuleManager dependency: {0}",
                            string.Join(", ", mmConfigs.Select(cfg => cfg.source.Name).OrderBy(f => f))
                        );
                    }
                    else if (dependsOnMM && !mmConfigs.Any())
                    {
                        Log.Warn("ModuleManager dependency may not be needed, no ModuleManager syntax found");
                    }
                }
            }
        }

        private readonly string[] identifiers = new string[] { "ModuleManager" };

        private static readonly Regex moduleManagerRegex = new Regex(
            @"^\s*[@+$\-!%]|^\s*[a-zA-Z0-9_]+:",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IGame          _game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleManagerDependsValidator));
    }
}
