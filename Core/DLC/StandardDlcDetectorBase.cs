using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CKAN.Versioning;

namespace CKAN.DLC
{
    /// <summary>
    /// Base class for DLC Detectors that follow standard conventions.
    /// </summary>
    /// <remarks>
    /// "Standard conventions" is defined as detecting installation by the presence of directory with the name
    /// DirectoryBaseName in the [GameData]/SquadExpansion directory, detecting version by parsing a version line in
    /// a readme.txt file in the same directory, and having an identifier of IdentifierBaseName-DLC.
    /// </remarks>
    public abstract class StandardDlcDetectorBase : IDlcDetector
    {
        private readonly string IdentifierBaseName;
        private readonly string DirectoryBaseName;
        private readonly Dictionary<string, string> CanonicalVersions;

        private static readonly Regex VersionPattern = new Regex(
            @"^Version\s+(?<version>\S+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        protected StandardDlcDetectorBase(string identifierBaseName, Dictionary<string, string> canonicalVersions = null)
            : this(identifierBaseName, identifierBaseName, canonicalVersions) { }

        protected StandardDlcDetectorBase(string identifierBaseName, string directoryBaseName, Dictionary<string, string> canonicalVersions = null)
        {
            if (string.IsNullOrWhiteSpace(identifierBaseName))
                throw new ArgumentException("Value must be provided.", nameof(identifierBaseName));

            if (string.IsNullOrWhiteSpace(directoryBaseName))
                throw new ArgumentException("Value must be provided.", nameof(directoryBaseName));

            IdentifierBaseName = identifierBaseName;
            DirectoryBaseName = directoryBaseName;
            CanonicalVersions = canonicalVersions ?? new Dictionary<string, string>();
        }

        public virtual bool IsInstalled(KSP ksp, out string identifier, out UnmanagedModuleVersion version)
        {
            identifier = $"{IdentifierBaseName}-DLC";
            version = null;

            var directoryPath = Path.Combine(ksp.GameData(), "SquadExpansion", DirectoryBaseName);
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
