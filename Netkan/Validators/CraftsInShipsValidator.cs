using System.Linq;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class CraftsInShipsValidator : IValidator
    {
        public CraftsInShipsValidator(IHttpService http, IModuleService moduleService)
        {
            _http          = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that craft files are installed into Ships");

            JObject    json = metadata.AllJson;
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    var zip       = new ZipFile(package);
                    var badCrafts = _moduleService.GetCrafts(mod, zip)
                        .Where(f => !AllowedCraftPath(f.destination))
                        .ToList();

                    if (badCrafts.Count != 0)
                    {
                        Log.WarnFormat(
                            "Craft files installed outside Ships folder: {0}",
                            string.Join(", ", badCrafts.Select(f => f.destination).Order()));
                    }
                }
            }
        }

        private static bool AllowedCraftPath(string path)
            => path.StartsWith("Ships/")
               || path.StartsWith("Missions/")
               || path.StartsWith("GameData/ContractPacks/");

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CraftsInShipsValidator));
    }
}
