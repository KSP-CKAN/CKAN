using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class MatchingIdentifiersValidator : IValidator
    {
        private readonly string _originalIdentifier;

        public MatchingIdentifiersValidator(string originalIdentifier)
        {
            _originalIdentifier = originalIdentifier;
        }

        public void Validate(Metadata metadata)
        {
            if (metadata.Identifier != _originalIdentifier)
            {
                throw new Kraken(string.Format("Error: Have metadata for {0}, but wanted {1}",
                    metadata.Identifier,
                    _originalIdentifier
                ));
            }
        }
    }
}
