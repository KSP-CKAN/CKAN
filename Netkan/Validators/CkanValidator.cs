using System.Collections.Generic;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Validators
{
    internal sealed class CkanValidator : IValidator
    {
        private readonly List<IValidator> _validators;

        public CkanValidator(IHttpService downloader, IModuleService moduleService)
        {
            _validators = new List<IValidator>
            {
                new IsCkanModuleValidator(),
                new InstallsFilesValidator(downloader, moduleService),
                new MatchesKnownGameVersionsValidator(),
                new ObeysCKANSchemaValidator()
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
