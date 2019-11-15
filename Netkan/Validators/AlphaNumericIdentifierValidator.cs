using System.Text.RegularExpressions;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class AlphaNumericIdentifierValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            if (!alphanumeric.IsMatch(metadata.Identifier))
            {
                throw new Kraken("CKAN identifiers must consist only of letters, numbers, and dashes, and must start with a letter or number.");
            }
        }

        private static readonly Regex alphanumeric = new Regex(
            @"^[A-Za-z0-9-]+$",
            RegexOptions.Compiled
        );
    }
}
