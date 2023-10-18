using CKAN.NetKAN.Model;
using log4net;

namespace CKAN.NetKAN.Validators
{
    internal sealed class HasIdentifierValidator : IValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HasIdentifierValidator));

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata contains an identifier");

            if (string.IsNullOrWhiteSpace(metadata.Identifier))
            {
                throw new Kraken("Metadata must have an identifier property");
            }
        }
    }
}
