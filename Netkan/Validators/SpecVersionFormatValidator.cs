using System.Text.RegularExpressions;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class SpecVersionFormatValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.AllJson;
            if (json?.Value<string>("spec_version") is not string s
                || !specVersionFormat.IsMatch(s))
            {
                throw new Kraken("spec version must be 1 or in the 'vX.X' format");
            }
        }

        private static readonly Regex specVersionFormat =
            new Regex(@"^1$|^v\d\.\d\d?$",
                      RegexOptions.Compiled);
    }
}
