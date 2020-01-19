using System.Text.RegularExpressions;

namespace CKAN
{
    /// <summary>
    /// Definition of format of `CkanModule.identifier` as per CKAN.schema
    /// </summary>
    public static class Identifier
    {
        /// <summary>
        /// A valid identifier must match this pattern
        /// </summary>
        public static readonly Regex ValidIdentifierPattern = new Regex(
            @"^[A-Za-z0-9][A-Za-z0-9-]+$",
            RegexOptions.Compiled
        );
    }
}
