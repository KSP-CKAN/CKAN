using log4net;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
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
                var zipFilePath = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(zipFilePath))
                {
                    bool hasVref = (metadata.Vref != null);

                    string path        = null;
                    bool   installable = false;
                    try
                    {
                        using (var zipfile = new ZipFile(zipFilePath))
                        {
                            // Pass a regex that matches anything so it returns the first if found
                            var verFileAndInstallable = _moduleService.FindInternalAvc(mod, zipfile, ".");
                            if (verFileAndInstallable != null)
                            {
                                // This will throw if there's a syntax error
                                var avc = ModuleService.GetInternalAvc(zipfile, verFileAndInstallable.Item1);
                                path        = verFileAndInstallable.Item1.Name;
                                installable = verFileAndInstallable.Item2;
                            }
                        }
                    }
                    catch (BadMetadataKraken)
                    {
                        // This means the install stanzas don't match any files.
                        // That's not our problem; someone else will report it.
                    }
                    catch (Kraken)
                    {
                        // If FindInternalAvc throws anything else, then there's a version file with a syntax error.
                        // This shouldn't cause the inflation to fail, but it does deprive us of the path.
                        path = "";
                        installable = false;
                    }

                    bool hasVersionFile = (path != null);

                    if (hasVref && !hasVersionFile)
                    {
                        Log.Warn("$vref present, version file missing");
                    }
                    else if (!hasVref && hasVersionFile && installable)
                    {
                        Log.WarnFormat("$vref absent, version file present: {0}", path);
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(VrefValidator));
    }
}
