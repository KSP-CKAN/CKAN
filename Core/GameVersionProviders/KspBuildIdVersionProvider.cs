using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN.Versioning;

namespace CKAN.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildIdVersionProvider : IGameVersionProvider
    {
        private static readonly Regex BuildIdPattern = new Regex(@"^build id\s+=\s+0*(?<buildid>\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private readonly IKspBuildMap _kspBuildMap;

        public KspBuildIdVersionProvider(IKspBuildMap kspBuildMap)
        {
            _kspBuildMap = kspBuildMap;
        }

        public bool TryGetVersion(string directory, out KspVersion result)
        {
            var buildIdPath = Path.Combine(directory, "buildID.txt");

            if (File.Exists(buildIdPath))
            {
                var match = File
                    .ReadAllLines(buildIdPath)
                    .Select(i => BuildIdPattern.Match(i))
                    .FirstOrDefault(i => i.Success);

                if (match != null)
                {
                    var version = _kspBuildMap[match.Groups["buildid"].Value];

                    if (version != null)
                    {
                        result = version;
                        return true;
                    }
                }
            }

            result = default(KspVersion);
            return false;
        }
    }
}
