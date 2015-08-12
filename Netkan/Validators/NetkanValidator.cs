using System.Collections.Generic;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class NetkanValidator : IValidator
    {
        private readonly List<IValidator> _validators;

        public NetkanValidator()
        {
            _validators = new List<IValidator>
            {
                new HasIdentifierValidator()
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
