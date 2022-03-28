using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.Extensions;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class PluginsValidator : IValidator
    {
        public PluginsValidator(IHttpService http, IModuleService moduleService, IConfigParser parser)
        {
            _http          = http;
            _moduleService = moduleService;
            _parser        = parser;
        }

        public void Validate(Metadata metadata)
        {
            Log.Info("Validating that metadata is appropriate for DLLs");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile      zip  = new ZipFile(package);
                    GameInstance inst = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());

                    var plugins    = _moduleService.GetPlugins(mod, zip, inst).ToList();
                    bool hasPlugin = plugins.Any();
                    if (hasPlugin)
                    {
                        var dllPaths = plugins
                            .Select(f => inst.ToRelativeGameDir(f.destination))
                            .OrderBy(f => f)
                            .ToList();
                        var dllIdentifiers = dllPaths
                            .SelectMany(p => inst.game.IdentifiersFromFileName(inst, p))
                            .Where(ident => !string.IsNullOrEmpty(ident)
                                && !identifiersToIgnore.Contains(ident))
                            .ToHashSet();
                        if (dllIdentifiers.Any() && !dllIdentifiers.Contains(metadata.Identifier))
                        {
                            // Check for :FOR[identifier] in .cfg files
                            var cfgIdentifiers = KerbalSpaceProgram.IdentifiersFromConfigNodes(
                                _parser.GetConfigNodes(mod, zip, inst)
                                       .SelectMany(kvp => kvp.Value));
                            if (!cfgIdentifiers.Contains(metadata.Identifier))
                            {
                                Log.WarnFormat(
                                    "No plugin or :FOR[] clause matching the identifier, manual installations won't be detected: {0}",
                                    string.Join(", ", dllPaths));
                            }
                        }

                        bool boundedCompatibility = json.ContainsKey("ksp_version") || json.ContainsKey("ksp_version_max");
                        if (!boundedCompatibility)
                        {
                            Log.Warn("Unbounded future compatibility for module with a plugin, consider setting $vref or ksp_version or ksp_version_max");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// These identifiers will not be treated as potential auto-detected mods
        /// for purposes of the identifier-matching warning,
        /// because they are commonly bundled and installed by other mods,
        /// which may or may not have their own plugins.
        /// </summary>
        private string[] identifiersToIgnore = new string[]
        {
            "MiniAVC"
        };

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IConfigParser  _parser;

        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginsValidator));
    }
}
