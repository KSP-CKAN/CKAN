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

        /// <summary>
        /// Turn a possibly invalid identifier string into a valid identifier string
        /// </summary>
        /// <param name="ident">String that we want to turn into an identifier</param>
        /// <returns>
        /// ident with any non-alphanumeric characters removed from the start so it
        /// begins with alphanumeric, and any remaining non-alphanumeric characters
        /// replaced with dashes so the overall string format is preserved
        /// </returns>
        public static string Sanitize(string ident)
        {
            return InvalidIdentifierCharacterPattern.Replace(
                InvalidPrefixPattern.Replace(ident, ""),
                "-"
            );
        }

        private static readonly Regex InvalidIdentifierCharacterPattern = new Regex(
            @"[^A-Za-z0-9-]",
            RegexOptions.Compiled
        );
        private static readonly Regex InvalidPrefixPattern = new Regex(
            @"^[^A-Za-z0-9]+",
            RegexOptions.Compiled
        );
    }
}
