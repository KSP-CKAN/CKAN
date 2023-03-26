using log4net;
using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class VrefValidator : IValidator
    {
        public VrefValidator(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http          = http;
            _moduleService = moduleService;
            _game          = game;
        }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata vref is consistent with download contents");

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
                    bool hasAvcVref = (metadata.Vref?.Source == "ksp-avc");

                    string avcPath     = null;
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
                                avcPath     = verFileAndInstallable.Item1.Name;
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
                        avcPath = "";
                        installable = false;
                    }

                    bool hasVersionFile = (avcPath != null);

                    if (hasAvcVref && !hasVersionFile)
                    {
                        Log.Warn("$vref is ksp-avc, version file missing");
                    }
                    else if (!hasAvcVref && hasVersionFile && installable)
                    {
                        Log.WarnFormat("$vref is not ksp-avc, version file present: {0}", avcPath);
                    }

                    bool hasSWVref = (metadata.Vref?.Source == "space-warp");
                    GameInstance inst = new GameInstance(_game, "/", "dummy", new NullUser());
                    using (var zipfile = new ZipFile(zipFilePath))
                    {
                        bool hasSWInfo = _moduleService.GetSpaceWarpInfo(mod, zipfile, inst) != null;
                        if (hasSWVref && !hasSWInfo)
                        {
                            Log.Warn("$vref is space-warp, swinfo.json file missing");
                        }
                        else if (!hasSWVref && hasSWInfo)
                        {
                            Log.Warn("$vref is not space-warp, swinfo.json file present");
                        }
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IGame          _game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(VrefValidator));
    }
}
