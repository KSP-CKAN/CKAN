using System.Text.RegularExpressions;

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
        public static bool TryMatch(this Regex regex, string value, out Match match)
        {
            if (value == null)
            {
                // Nothing matches null
                match = null;
                return false;
            }
            match = regex.Match(value);
            return match.Success;
        }
    }
}
