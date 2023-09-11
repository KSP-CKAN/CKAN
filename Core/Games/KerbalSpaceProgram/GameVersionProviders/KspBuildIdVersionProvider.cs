using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using log4net;

using CKAN.Versioning;

namespace CKAN.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildIdVersionProvider : IGameVersionProvider
    {
        public KspBuildIdVersionProvider(IKspBuildMap kspBuildMap)
        {
            _kspBuildMap = kspBuildMap;
        }

        private static readonly string[] buildIDfilenames =
        {
            "buildID.txt", "buildID64.txt"
        };

        public bool TryGetVersion(string directory, out GameVersion result)
        {
            var foundVersions = buildIDfilenames
                .Select(filename => TryGetVersionFromFile(Path.Combine(directory, filename),
                                                          out GameVersion v)
                                        ? v : null)
                .Where(v => v != null)
                .Distinct()
                .ToList();

            if (foundVersions.Count < 1)
            {
                result = default(GameVersion);
                return false;
            }
            if (foundVersions.Count > 1)
            {
                Log.WarnFormat("Found different KSP versions in {0}: {1}",
                               string.Join(" and ", buildIDfilenames),
                               string.Join(", ", foundVersions));
            }
            result = foundVersions.Max();
            return true;
        }

        private bool TryGetVersionFromFile(string file, out GameVersion result)
        {
            if (File.Exists(file))
            {
                var match = File.ReadAllLines(file)
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

            result = default(GameVersion);
            return false;
        }

        private readonly IKspBuildMap _kspBuildMap;

        private static readonly Regex BuildIdPattern =
            new Regex(@"^build id\s+=\s+0*(?<buildid>\d+)",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildIdVersionProvider));
    }
}
