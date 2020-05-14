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
                var file = _http.DownloadPackage(metadata.Download, metadata.Identifier, metadata.RemoteTimestamp);
                if (!string.IsNullOrEmpty(file))
                {
                    var avc = _moduleService.GetInternalAvc(mod, file, null);

                    bool hasVref = (metadata.Vref != null);

                    bool hasVersionFile = (avc != null);

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
