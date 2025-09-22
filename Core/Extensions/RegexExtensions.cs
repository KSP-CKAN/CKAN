using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace CKAN.Extensions
{
    public static class RegexExtensions
    {
        /// <summary>
        /// Functional-friendly wrapper around Regex.Match
        /// </summary>
        /// <param name="regex">The regex to match</param>
        /// <param name="value">The string to check</param>
        /// <param name="match">Object representing the match, if any</param>
        /// <returns>True if the regex matched the value, false otherwise</returns>
        public static bool TryMatch(this Regex regex, string value,
                                    [NotNullWhen(returnValue: true)] out Match? match)
        {
            match = regex.Match(value);
            return match.Success;
        }
    }
}
