using System.Text.RegularExpressions;

namespace CKAN.NetKAN
{
    internal static class Constants
    {
        public static readonly Regex DefaultAssetMatchPattern = new Regex(
            @"\.zip$", RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
    }
}