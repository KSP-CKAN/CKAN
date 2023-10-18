using System.Linq;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class CraftsInShipsValidator : IValidator
    {
        public CraftsInShipsValidator(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http          = http;
            _moduleService = moduleService;
            _game          = game;
        }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that craft files are installed into Ships");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    var zip       = new ZipFile(package);
                    var inst      = new GameInstance(_game, "/", "dummy", null);
                    var badCrafts = _moduleService.GetCrafts(mod, zip, inst)
                        .Where(f => !AllowedCraftPath(inst.ToRelativeGameDir(f.destination)))
                        .ToList();

                    if (badCrafts.Any())
                    {
                        Log.WarnFormat(
                            "Craft files installed outside Ships folder: {0}",
                            string.Join(", ", badCrafts.Select(f => inst.ToRelativeGameDir(f.destination)).OrderBy(f => f)));
                    }
                }
            }
        }

        private bool AllowedCraftPath(string path)
            => path.StartsWith("Ships/")
               || path.StartsWith("Missions/")
               || path.StartsWith("GameData/ContractPacks/");

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IGame          _game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CraftsInShipsValidator));
    }
}
