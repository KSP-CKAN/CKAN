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
        public HarmonyValidator(IModuleService moduleService, ICacheService cacheService,
                                IGame game)
        {
            _moduleService = moduleService;
            _cacheService  = cacheService;
            _game          = game;
        }

        public void Validate(Metadata metadata)
        {
            JObject    json = metadata.AllJson;
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            // The Harmony2 module is allowed to install a Harmony DLL;
            // anybody else must have "provides":["Harmony1"] to do so
            if (_game.ShortName != "KSP" || mod.IsDLC || mod.identifier == "Harmony2")
            {
                return;
            }

            // Need to peek at the mod's files
            _cacheService.ExecuteCached(metadata, typeof(HarmonyValidator), package =>
            {
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile zip = new ZipFile(package);

                    var harmonyDLLs = _moduleService.GetPlugins(mod, zip)
                        .Select(instF => instF.source.Name)
                        .Where(f => f.IndexOf("Harmony", Math.Max(0, f.LastIndexOf('/')),
                                              StringComparison.InvariantCultureIgnoreCase) != -1)
                        .Order()
                        .ToList();
                    bool bundlesHarmony   = harmonyDLLs.Count != 0;
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
            });
        }

        private readonly IModuleService _moduleService;
        private readonly ICacheService  _cacheService;
        private readonly IGame          _game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(HarmonyValidator));
    }
}
