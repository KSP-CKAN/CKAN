using CKAN.Versioning;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CKAN.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspReadmeVersionProvider : IGameVersionProvider
    {
        private static readonly Regex VersionPattern = new Regex(@"^Version\s+(?<version>\d+\.\d+\.\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public bool TryGetVersion(string directory, out KspVersion result)
        {
            var readmePath = Path.Combine(directory, "readme.txt");

            if (File.Exists(readmePath))
            {
                var match = File
                    .ReadAllLines(readmePath)
                    .Select(i => VersionPattern.Match(i))
                    .FirstOrDefault(i => i.Success);

                if (match != null)
                {
                    result = KspVersion.Parse(match.Groups["version"].Value);
                    return true;
                }
            }

            result = default(KspVersion);
            return false;
        }
    }
}