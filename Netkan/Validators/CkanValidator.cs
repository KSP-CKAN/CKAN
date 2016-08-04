using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using System.Collections.Generic;

namespace CKAN.NetKAN.Validators
{
    internal sealed class CkanValidator : IValidator
    {
        private readonly List<IValidator> _validators;

        public CkanValidator(Metadata netkan, IHttpService downloader, IModuleService moduleService)
        {
            _validators = new List<IValidator>
            {
                new IsCkanModuleValidator(),
                new MatchingIdentifiersValidator(netkan.Identifier),
                new InstallsFilesValidator(downloader, moduleService)
            };
        }

        public void Validate(Metadata metadata)
        {
            foreach (var validator in _validators)
            {
                validator.Validate(metadata);
            }
        }
    }
}