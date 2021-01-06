using log4net;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class VrefValidator : IValidator
    {
        public VrefValidator(IHttpService http, IModuleService moduleService)
        {
            _http          = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            Log.Info("Validating that metadata vref is consistent with download contents");

            JObject json = metadata.Json();
            var noVersion = metadata.Version == null;
            if (noVersion)
            {
                json["version"] = "0";
            }
            var mod = CkanModule.FromJson(json.ToString());
            if (noVersion)
            {
                json.Remove("version");
            }

            if (!mod.IsDLC)
            {
                var file = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(file))
                {
                    bool hasVref = (metadata.Vref != null);

                    bool hasVersionFile = false;
                    try
                    {
                        // Pass a regex that matches anything so it returns the first if found
                        var avc = _moduleService.GetInternalAvc(mod, file, ".");
                        hasVersionFile = (avc != null);
                    }
                    catch (BadMetadataKraken k)
                    {
                        // This means the install stanzas don't match any files.
                        // That's not our problem; someone else will report it.
                    }
                    catch (Kraken k)
                    {
                        // If GetInternalAvc throws anything else, then there's a version file with a syntax error.
                        // This shouldn't cause the inflation to fail.
                        hasVersionFile = true;
                        Log.Warn(k.Message);
                    }

                    if (hasVref && !hasVersionFile)
                    {
                        Log.Warn("$vref present, version file missing");
                    }
                    else if (!hasVref && hasVersionFile)
                    {
                        Log.Warn("$vref absent, version file present");
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(VrefValidator));
    }
}
