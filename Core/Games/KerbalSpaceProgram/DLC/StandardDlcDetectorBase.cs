using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using CKAN.DLC;
using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram.DLC
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

        private readonly IGame game;

        private static readonly Regex VersionPattern = new Regex(
            @"^Version\s+(?<version>\S+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        protected StandardDlcDetectorBase(IGame game,
                                          string identifierBaseName,
                                          GameVersion releaseGameVersion,
                                          Dictionary<string, string> canonicalVersions = null)
            : this(game, identifierBaseName, identifierBaseName, releaseGameVersion, canonicalVersions) { }

        protected StandardDlcDetectorBase(IGame game, string identifierBaseName, string directoryBaseName, GameVersion releaseGameVersion, Dictionary<string, string> canonicalVersions = null)
        {
            if (string.IsNullOrWhiteSpace(identifierBaseName))
            {
                throw new ArgumentException("Value must be provided.", nameof(identifierBaseName));
            }

            if (string.IsNullOrWhiteSpace(directoryBaseName))
            {
                throw new ArgumentException("Value must be provided.", nameof(directoryBaseName));
            }

            this.game = game;
            IdentifierBaseName = identifierBaseName;
            DirectoryBaseName = directoryBaseName;
            ReleaseGameVersion = releaseGameVersion;
            CanonicalVersions = canonicalVersions ?? new Dictionary<string, string>();
        }

        public virtual bool IsInstalled(GameInstance ksp, out string identifier, out UnmanagedModuleVersion version)
        {
            var directoryPath = Path.Combine(ksp.GameDir(), InstallPath());
            var readmeFilePath = Path.Combine(directoryPath, "readme.txt");
            // Steam leaves empty folders behind when you "disable" a DLC,
            // so only return true if the readme exists
            if (Directory.Exists(directoryPath) && File.Exists(readmeFilePath))
            {
                identifier = $"{IdentifierBaseName}-DLC";
                version = new UnmanagedModuleVersion(
                              File.ReadAllLines(readmeFilePath)
                                  .Select(line => VersionPattern.Match(line))
                                  .Where(match => match.Success)
                                  .Select(match => match.Groups["version"].Value)
                                  .Select(verStr => CanonicalVersions.TryGetValue(verStr, out string overrideVer)
                                                        ? overrideVer
                                                        : verStr)
                                  // A null string results in UnmanagedModuleVersion with IsUnknownVersion==true
                                  .FirstOrDefault());
                return true;
            }
            identifier = null;
            version = null;
            return false;
        }

        public virtual string InstallPath()
            => Path.Combine(game.PrimaryModDirectoryRelative,
                            "SquadExpansion",
                            DirectoryBaseName);

        public bool AllowedOnBaseVersion(GameVersion baseVersion)
            => baseVersion >= ReleaseGameVersion;
    }
}
