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
    internal sealed class CraftsInShipsValidator : IValidator
    {
        public CraftsInShipsValidator(IHttpService http, IModuleService moduleService)
        {
            _http          = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            Log.Info("Validating that craft files are installed into Ships");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    var zip       = new ZipFile(package);
                    var ksp       = new KSP("/", "dummy", null, false);
                    var badCrafts = _moduleService.GetCrafts(mod, zip, ksp)
                        .Where(f => !ksp.ToRelativeGameDir(f.destination).StartsWith("Ships/"))
                        .ToList();

                    if (badCrafts.Any())
                    {
                        Log.WarnFormat(
                            "Craft files installed outside Ships folder: {0}",
                            string.Join(", ", badCrafts.Select(f =>
                                ksp.ToRelativeGameDir(f.destination)
                            ))
                        );
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CraftsInShipsValidator));
    }
}
