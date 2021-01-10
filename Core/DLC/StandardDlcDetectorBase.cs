using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CKAN.Versioning;
using CKAN.Games;

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
        public GameVersion ReleaseGameVersion { get; }
        public string IdentifierBaseName { get; }

        private readonly string DirectoryBaseName;
        private readonly Dictionary<string, string> CanonicalVersions;

        private IGame game;

        private static readonly Regex VersionPattern = new Regex(
            @"^Version\s+(?<version>\S+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        protected StandardDlcDetectorBase(IGame game, string identifierBaseName, GameVersion releaseGameVersion, Dictionary<string, string> canonicalVersions = null)
            : this(game, identifierBaseName, identifierBaseName, releaseGameVersion, canonicalVersions) { }

        protected StandardDlcDetectorBase(IGame game, string identifierBaseName, string directoryBaseName, GameVersion releaseGameVersion, Dictionary<string, string> canonicalVersions = null)
        {
            if (string.IsNullOrWhiteSpace(identifierBaseName))
                throw new ArgumentException("Value must be provided.", nameof(identifierBaseName));

            if (string.IsNullOrWhiteSpace(directoryBaseName))
                throw new ArgumentException("Value must be provided.", nameof(directoryBaseName));

            this.game = game;
            IdentifierBaseName = identifierBaseName;
            DirectoryBaseName = directoryBaseName;
            ReleaseGameVersion = releaseGameVersion;
            CanonicalVersions = canonicalVersions ?? new Dictionary<string, string>();
        }

        public virtual bool IsInstalled(GameInstance ksp, out string identifier, out UnmanagedModuleVersion version)
        {
            identifier = $"{IdentifierBaseName}-DLC";
            version = null;

            var directoryPath = Path.Combine(game.PrimaryModDirectory(ksp), "SquadExpansion", DirectoryBaseName);
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

        public virtual string InstallPath()
        {
            return Path.Combine("GameData", "SquadExpansion", DirectoryBaseName);
        }

        public bool AllowedOnBaseVersion(GameVersion baseVersion)
        {
            return baseVersion >= ReleaseGameVersion;
        }
    }
}
