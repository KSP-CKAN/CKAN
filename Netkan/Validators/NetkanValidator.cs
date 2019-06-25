using System.IO;
﻿using System.Collections.Generic;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class NetkanValidator : IValidator
    {
        private readonly List<IValidator> _validators;

        public NetkanValidator(string filename)
        {
            _validators = new List<IValidator>()
            {
                new SpecVersionFormatValidator(),
                new HasIdentifierValidator(),
                new KrefValidator(),
                new MatchingIdentifiersValidator(Path.GetFileNameWithoutExtension(filename)),
                new AlphaNumericIdentifierValidator(),
                new RelationshipsValidator(),
                new LicensesValidator(),
                new KrefDownloadMutexValidator(),
                new DownloadVersionValidator(),
                new OverrideValidator(),
                new VersionStrictValidator(),
                new ReplacedByValidator(),
                new InstallValidator(),
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
