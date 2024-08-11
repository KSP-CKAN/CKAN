using System.Collections.Generic;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class CkanValidator : IValidator
    {
        private readonly List<IValidator> _validators;

        public CkanValidator(IHttpService downloader, IModuleService moduleService, IGame game)
        {
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
                new HarmonyValidator(downloader, moduleService, game),
                new ModuleManagerDependsValidator(downloader, moduleService, game),
                new PluginsValidator(downloader, moduleService, game),
                new CraftsInShipsValidator(downloader, moduleService, game),
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
