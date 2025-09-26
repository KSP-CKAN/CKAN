using System;
using System.Linq;

using log4net;

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
        private readonly IGame _game;

        public InstallsFilesValidator(IHttpService http, IModuleService moduleService, IGame game)
        {
            _http = http;
            _moduleService = moduleService;
            _game = game;
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.AllJson.ToString());
            if (!mod.IsDLC && _http.DownloadModule(metadata) is string file)
            {
                // Make sure this would actually generate an install.
                if (!_moduleService.HasInstallableFiles(mod, file))
                {
                    throw new Kraken(string.Format(
                        "Module contains no files matching: {0}",
                        mod.DescribeInstallStanzas(_game)));
                }

                // Get the files the module will install
                var allFiles = _moduleService.FileDestinations(mod, file).Memoize();

                // Make sure no paths include GameData other than at the start
                foreach (var dir in Enumerable.Repeat(_game.PrimaryModDirectoryRelative, 1)
                                              .Concat(_game.AlternateModDirectoriesRelative))
                {
                    var gamedatas = allFiles
                        .Where(p => p.StartsWith(dir, StringComparison.InvariantCultureIgnoreCase)
                                    && p.LastIndexOf($"/{dir}/", StringComparison.InvariantCultureIgnoreCase) > 0)
                        .Order()
                        .ToList();
                    if (gamedatas.Count != 0)
                    {
                        var badPaths = string.Join(Environment.NewLine, gamedatas);
                        throw new Kraken($"{dir} directory found within {dir}:{Environment.NewLine}{badPaths}");
                    }
                }

                // Make sure we won't try to overwrite our own files
                var duplicates = allFiles.GroupBy(f => f)
                                         .SelectMany(grp => grp.Skip(1).Order())
                                         .ToList();
                if (duplicates.Count != 0)
                {
                    var badPaths = string.Join(Environment.NewLine, duplicates);
                    throw new Kraken($"Multiple files attempted to install to:{Environment.NewLine}{badPaths}");
                }

                // Not a perfect check (subject to false negatives)
                // but better than nothing
                if (mod.install != null)
                {
                    var unmatchedIncludeOnlys = mod.install
                        .SelectMany(stanza => stanza.include_only ?? Enumerable.Empty<string>())
                        .Distinct()
                        .Where(incl => !allFiles.Any(f => f.Contains(incl)))
                        .ToList();
                    if (unmatchedIncludeOnlys.Count != 0)
                    {
                        log.WarnFormat("No matches for include_only: {0}",
                                       string.Join(", ", unmatchedIncludeOnlys));
                    }
                }
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(InstallsFilesValidator));
    }
}
