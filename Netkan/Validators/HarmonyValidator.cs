using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.Games;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Validators
{
    internal sealed class HarmonyValidator : IValidator
    {
        public HarmonyValidator(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http          = http;
            _moduleService = moduleService;
            _game          = game;
        }

        public void Validate(Metadata metadata)
        {
            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            // The Harmony2 module is allowed to install a Harmony DLL;
            // anybody else must have "provides":["Harmony1"] to do so
            if (_game.ShortName == "KSP" && !mod.IsDLC && mod.identifier != "Harmony2")
            {
                // Need to peek at the mod's files
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile zip = new ZipFile(package);
                    GameInstance inst = new GameInstance(_game, "/", "dummy", new NullUser());

                    var harmonyDLLs = _moduleService.GetPlugins(mod, zip, inst)
                        .Select(instF => instF.source.Name)
                        .Where(f => f.IndexOf("Harmony", Math.Max(0, f.LastIndexOf('/')),
                                              StringComparison.InvariantCultureIgnoreCase) != -1)
                        .OrderBy(f => f)
                        .ToList();
                    bool bundlesHarmony   = harmonyDLLs.Any();
                    bool providesHarmony1 = mod.ProvidesList.Contains("Harmony1");
                    if (bundlesHarmony && !providesHarmony1)
                    {
                        throw new Kraken($"Harmony DLL found at {string.Join(", ", harmonyDLLs)}, but Harmony1 is not in the provides list");
                    }
                    else if (providesHarmony1 && !bundlesHarmony)
                    {
                        Log.Warn("Harmony1 provided by module that doesn't install a Harmony DLL, consider removing from provides list");
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IGame          _game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(HarmonyValidator));
    }
}
