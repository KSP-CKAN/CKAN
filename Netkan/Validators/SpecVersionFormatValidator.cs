using System.Text.RegularExpressions;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class SpecVersionFormatValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            if (json["spec_version"] == null
                || !specVersionFormat.IsMatch((string)json["spec_version"]))
            {
                throw new Kraken("spec version must be 1 or in the 'vX.X' format");
            }
        }

        private static readonly Regex specVersionFormat = new Regex(
            @"^1$|^v\d\.\d\d?$",
            RegexOptions.Compiled
        );
    }
}
