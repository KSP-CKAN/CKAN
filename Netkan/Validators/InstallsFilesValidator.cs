using System;
using System.Linq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.Extensions;
using CKAN.Games;

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
                        mod.DescribeInstallStanzas(new KerbalSpaceProgram())
                    ));
                }

                // Get the files the module will install
                var allFiles = _moduleService.FileDestinations(mod, file).Memoize();

                // Make sure no paths include GameData other than at the start
                var gamedatas = allFiles
                    .Where(p => p.StartsWith("GameData", StringComparison.InvariantCultureIgnoreCase)
                         && p.LastIndexOf("/GameData/", StringComparison.InvariantCultureIgnoreCase) > 0)
                    .OrderBy(f => f)
                    .ToList();
                if (gamedatas.Any())
                {
                    var badPaths = string.Join("\r\n", gamedatas);
                    throw new Kraken($"GameData directory found within GameData:\r\n{badPaths}");
                }

                // Make sure we won't try to overwrite our own files
                var duplicates = allFiles
                    .GroupBy(f => f)
                    .SelectMany(grp => grp.Skip(1).OrderBy(f => f))
                    .ToList();
                if (duplicates.Any())
                {
                    var badPaths = string.Join("\r\n", duplicates);
                    throw new Kraken($"Multiple files attempted to install to:\r\n{badPaths}");
                }
            }
        }
    }
}
