using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

using log4net;

using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildIdVersionProvider : IGameVersionProvider
    {
        public KspBuildIdVersionProvider(IKspBuildMap kspBuildMap)
        {
            _kspBuildMap = kspBuildMap;
        }

        public static readonly string[] buildIDfilenames =
        {
            "buildID64.txt", "buildID.txt"
        };

        public bool TryGetVersion(string directory,
                                  [NotNullWhen(true)] out GameVersion? result)
        {
            var foundVersions = buildIDfilenames
                .Select(filename => TryGetVersionFromFile(Path.Combine(directory, filename),
                                                          out GameVersion? v)
                                        ? v : null)
                .OfType<GameVersion>()
                .Distinct()
                .ToArray();

            if (foundVersions.Length > 0)
            {
                if (foundVersions.Length > 1)
                {
                    Log.WarnFormat("Found different KSP versions in {0}: {1}",
                                   string.Join(" and ", buildIDfilenames),
                                   string.Join<GameVersion>(", ", foundVersions));
                }
                result = foundVersions.Max() ?? foundVersions.First();
                return true;
            }

            result = default;
            return false;
        }

        private bool TryGetVersionFromFile(string file,
                                           [NotNullWhen(true)] out GameVersion? result)
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

            result = default;
            return false;
        }

        private readonly IKspBuildMap _kspBuildMap;

        private static readonly Regex BuildIdPattern =
            new Regex(@"^build id\s+=\s+0*(?<buildid>\d+)",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildIdVersionProvider));
    }
}
