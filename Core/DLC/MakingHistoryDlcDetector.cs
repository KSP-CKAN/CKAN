using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CKAN.Versioning;

namespace CKAN.DLC
{
    /// <summary>
    /// Represents an object that can detect the presence of the official Making History DLC in a KSP installation.
    /// </summary>
    public sealed class MakingHistoryDlcDetector : IDlcDetector
    {
        private const string Identifier = "MakingHistory-DLC";

        private static readonly Dictionary<string, string> CanonicalVersions = new Dictionary<string, string>()
        {
            { "1.0", "1.0.0" }
        };

        private static readonly Regex VersionPattern = new Regex(
            @"^Version\s+(?<version>\S+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public bool IsInstalled(KSP ksp, out string identifier, out UnmanagedModuleVersion version)
        {
            identifier = Identifier;
            version = null;

            var directoryPath = Path.Combine(ksp.GameData(), "SquadExpansion", "MakingHistory");
            if (Directory.Exists(directoryPath))
            {
                var readmeFilePath = Path.Combine(directoryPath, "readme.txt");

                if (File.Exists(readmeFilePath))
                {
                    foreach (var line in File.ReadAllLines(readmeFilePath))
                    {
                        var match = VersionPattern.Match(line);

                        if (match.Success)
                        {
                            var versionStr = match.Groups["version"].Value;

                            if (CanonicalVersions.ContainsKey(versionStr))
                                versionStr = CanonicalVersions[versionStr];

                            version = new UnmanagedModuleVersion(versionStr);
                            break;
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
