using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using KSPMMCfgParser;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Extensions;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ModuleManagerDependsValidator : IValidator
    {
        public ModuleManagerDependsValidator(IHttpService http, IModuleService moduleService, IConfigParser parser)
        {
            _http          = http;
            _moduleService = moduleService;
            _parser        = parser;
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
                    GameInstance inst = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());
                    var mmConfigs = _parser.GetConfigNodes(mod, zip, inst)
                        .Where(kvp => kvp.Value.Any(node => HasAnyModuleManager(node)))
                        .Select(kvp => kvp.Key)
                        .ToArray();

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

        private string[] identifiers = new string[] { "ModuleManager" };

        private static bool HasAnyModuleManager(KSPConfigNode node)
            => node.Operator != MMOperator.Insert
            || node.Filters  != null
            || node.Needs    != null
            || node.Has      != null
            || node.Index    != null
            || node.Properties.Any(prop => HasAnyModuleManager(prop))
            || node.Children.Any( child => HasAnyModuleManager(child));

        private static bool HasAnyModuleManager(KSPConfigProperty prop)
            => prop.Operator           != MMOperator.Insert
            || prop.Needs              != null
            || prop.Index              != null
            || prop.ArrayIndex         != null
            || prop.AssignmentOperator != null;

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IConfigParser  _parser;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleManagerDependsValidator));
    }
}
