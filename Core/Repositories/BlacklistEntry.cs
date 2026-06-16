using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace CKAN
{
    public class BlacklistEntry
    {
        [ExcludeFromCodeCoverage]
        public BlacklistEntry(Regex uri_regex, string description)
        {
            this.uri_regex   = uri_regex;
            this.description = description;
        }

        public readonly Regex  uri_regex;

        public readonly string description;
    }
}
