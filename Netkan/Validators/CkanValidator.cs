using System.Collections.Generic;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class CkanValidator : IValidator
    {
        private readonly List<IValidator> _validators;

        public CkanValidator(IHttpService   downloader,
                             IModuleService moduleService,
                             ICacheService  cacheService,
                             IGame          game,
                             string?        githubToken)        {
            var ghApi = new GithubApi(downloader, githubToken);
            var swLoader = new SpaceWarpInfoLoader(downloader, ghApi);
            _validators = new List<IValidator>
            {
                new IsCkanModuleValidator(),
                new DownloadArrayValidator(),
                new TagsValidator(),
                new LicensesValidator(),
                new RelationshipsValidator(),
                new VersionStrictValidator(),
                new ReplacedByValidator(),
                new InstallValidator(),
                new InstallsFilesValidator(downloader, moduleService, game),
                new MatchesKnownGameVersionsValidator(game),
                new ObeysCKANSchemaValidator(),
                new KindValidator(),
                new HarmonyValidator(moduleService, cacheService, game),
                new ModuleManagerDependsValidator(moduleService, cacheService),
                new PluginsValidator(moduleService, cacheService, game),
                new CraftsInShipsValidator(moduleService, cacheService),
                new SpaceWarpInfoValidator(swLoader, moduleService, cacheService),
            };
        }

        public void Validate(Metadata metadata)
        {
            foreach (var validator in _validators)
            {
                validator.Validate(metadata);
            }
        }

        public void ValidateCkan(Metadata metadata, Metadata netkan)
        {
            Validate(metadata);
            new MatchingIdentifiersValidator(netkan.Identifier).Validate(metadata);
        }

    }
}
