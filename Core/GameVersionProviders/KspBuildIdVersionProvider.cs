using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN.Versioning;
using log4net;

namespace CKAN.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildIdVersionProvider : IGameVersionProvider
    {
        private static readonly Regex BuildIdPattern = new Regex(@"^build id\s+=\s+0*(?<buildid>\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildIdVersionProvider));

        private readonly IKspBuildMap _kspBuildMap;

        public KspBuildIdVersionProvider(IKspBuildMap kspBuildMap)
        {
            _kspBuildMap = kspBuildMap;
        }

        public bool TryGetVersion(string directory, out KspVersion result)
        {
            KspVersion buildIdVersion;
            var hasBuildId = TryGetVersionFromFile(Path.Combine(directory, "buildID.txt"), out buildIdVersion);

            KspVersion buildId64Version;
            var hasBuildId64 = TryGetVersionFromFile(Path.Combine(directory, "buildID64.txt"), out buildId64Version);

            if (hasBuildId && hasBuildId64)
            {
                result = KspVersion.Max(buildIdVersion, buildId64Version);

                if (buildIdVersion != buildId64Version)
                {
                    Log.WarnFormat(
                        "Found different KSP versions in buildID.txt ({0}) and buildID64.txt ({1}), assuming {2}.",
                        buildIdVersion,
                        buildId64Version,
                        result
                    );
                }

                return true;
            }
            else if (hasBuildId64)
            {
                result = buildId64Version;
                return true;
            }
            else if (hasBuildId)
            {
                result = buildIdVersion;
                return true;
            }
            else
            {
                result = default(KspVersion);
                return false;
            }
        }

        private bool TryGetVersionFromFile(string file, out KspVersion result)
        {
            if (File.Exists(file))
            {
                var match = File
                    .ReadAllLines(file)
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
