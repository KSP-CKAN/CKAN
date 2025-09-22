using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class PluginsValidator : IValidator
    {
        public PluginsValidator(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http          = http;
            _moduleService = moduleService;
            _game          = game;
        }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata is appropriate for DLLs");

            var json = metadata.AllJson;
            var mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    var zip  = new ZipFile(package);

                    if (_moduleService.GetPlugins(mod, zip)
                                      .Select(f => f.destination)
                                      .Order()
                                      .ToArray()
                        is { Length: > 0 } plugins)
                    {
                        if (plugins.Select(pl => GameInstance.DllPathToIdentifier(_game, pl))
                                   .OfType<string>()
                                   .Where(ident => ident is { Length: > 0 }
                                                   && !identifiersToIgnore.Contains(ident))
                                   .ToHashSet()
                            is { Count: > 0 } dllIdentifiers
                            && !dllIdentifiers.Contains(metadata.Identifier))
                        {
                            Log.WarnFormat("No plugin matching the identifier, manual installations won't be detected: {0}",
                                           string.Join(", ", plugins));
                        }

                        bool boundedCompatibility = json.ContainsKey("ksp_version")
                                                    || json.ContainsKey("ksp_version_max");
                        if (!boundedCompatibility)
                        {
                            Log.Warn("Unbounded future compatibility for module with a plugin, consider setting $vref or ksp_version or ksp_version_max");
                        }
                    }
                    else if (_moduleService.GetSourceCode(mod, zip)
                                           .Select(f => f.destination)
                                           .Order()
                                           .ToArray()
                             is { Length: > 0 } sourceCode)
                    {
                        Log.WarnFormat("Found C# source code without DLL, mod may not have been compiled: {0}",
                                       string.Join(", ", sourceCode));
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
        private readonly string[] identifiersToIgnore = new string[]
        {
            "MiniAVC"
        };

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IGame          _game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginsValidator));
    }
}
