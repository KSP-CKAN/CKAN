using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspReadmeVersionProvider : IGameVersionProvider
    {
        private static readonly Regex VersionPattern = new Regex(@"^Version\s+(?<version>\d+\.\d+\.\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public bool TryGetVersion(string directory, out GameVersion result)
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
                    result = GameVersion.Parse(match.Groups["version"].Value);
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
