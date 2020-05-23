using System.Linq;
﻿using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Validators
{
    internal sealed class InstallsFilesValidator : IValidator
    {
        private readonly IHttpService _http;
        private readonly IModuleService _moduleService;

        public InstallsFilesValidator(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            if (!mod.IsDLC)
            {
                var file = _http.DownloadModule(metadata);

                // Make sure this would actually generate an install.
                if (!_moduleService.HasInstallableFiles(mod, file))
                {
                    throw new Kraken(string.Format(
                        "Module contains no files matching: {0}",
                        mod.DescribeInstallStanzas()
                    ));
                }
                
                // Make sure no paths include GameData other than at the start
                var gamedatas = _moduleService.FileDestinations(mod, file)
                    .Where(p => p.StartsWith("GameData") && p.LastIndexOf("/GameData/") > 0)
                    .ToList();
                if (gamedatas.Any())
                {
                    var badPaths = string.Join("\r\n", gamedatas);
                    throw new Kraken($"GameData directory found within GameData:\r\n{badPaths}");
                }
            }
        }
    }
}
