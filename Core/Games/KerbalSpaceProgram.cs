using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Autofac;
using log4net;
using CKAN.GameVersionProviders;
using CKAN.Versioning;

namespace CKAN.Games
{
    public class KerbalSpaceProgram : IGame
    {
        public string ShortName => "KSP";

        public bool GameInFolder(DirectoryInfo where)
        {
            return Directory.Exists(Path.Combine(where.FullName, "GameData"));
        }

        /// <summary>
        /// Finds the Steam KSP path. Returns null if the folder cannot be located.
        /// </summary>
        /// <returns>The KSP path.</returns>
        public string SteamPath()
        {
            // Attempt to get the Steam path.
            string steamPath = CKANPathUtils.SteamPath();

            if (steamPath == null)
            {
                return null;
            }

            // Default steam library
            string installPath = KSPDirectory(steamPath);
            if (installPath != null)
            {
                return installPath;
            }

            // Attempt to find through config file
            string configPath = Path.Combine(steamPath, "config", "config.vdf");
            if (File.Exists(configPath))
            {
                log.InfoFormat("Found Steam config file at {0}", configPath);
                StreamReader reader = new StreamReader(configPath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Found Steam library
                    if (line.Contains("BaseInstallFolder"))
                    {
                        // This assumes config file is valid, we just skip it if it looks funny.
                        string[] split_line = line.Split('"');

                        if (split_line.Length > 3)
                        {
                            log.DebugFormat("Found a Steam Libary Location at {0}", split_line[3]);

                            installPath = KSPDirectory(split_line[3]);
                            if (installPath != null)
                            {
                                log.InfoFormat("Found a KSP install at {0}", installPath);
                                return installPath;
                            }
                        }
                    }
                }
            }

            // Could not locate the folder.
            return null;
        }

        /// <summary>
        /// Get the default non-Steam path to KSP on macOS
        /// </summary>
        /// <returns>
        /// "/Applications/Kerbal Space Program" if it exists and we're on a Mac, else null
        /// </returns>
        public string MacPath()
        {
            if (Platform.IsMac)
            {
                string installPath = Path.Combine(
                    // This is "/Applications" in Mono on Mac
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Kerbal Space Program"
                );
                return Directory.Exists(installPath) ? installPath : null;
            }
            return null;
        }

        public string PrimaryModDirectoryRelative => "GameData";

        public string PrimaryModDirectory(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), PrimaryModDirectoryRelative));
        }

        public string[] StockFolders => new string[]
        {
            "GameData/Squad",
            "GameData/SquadExpansion"
        };

        /// <summary>
        /// Checks the path against a list of reserved game directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsReservedDirectory(GameInstance inst, string path)
        {
            return path == inst.GameDir() || path == inst.CkanDir()
                || path == PrimaryModDirectory(inst)
                || path == Missions(inst)
                || path == Scenarios(inst) || path == Tutorial(inst)
                || path == Ships(inst)     || path == ShipsThumbs(inst)
                || path == ShipsVab(inst)  || path == ShipsThumbsVAB(inst)
                || path == ShipsSph(inst)  || path == ShipsThumbsSPH(inst)
                || path == ShipsScript(inst);
        }

        public bool AllowInstallationIn(string name, out string path)
        {
            return allowedFolders.TryGetValue(name, out path);
        }

        public void RebuildSubdirectories(GameInstance inst)
        {
            string[] FoldersToCheck = { "Ships/VAB", "Ships/SPH", "Ships/@thumbs/VAB", "Ships/@thumbs/SPH" };
            foreach (string sRelativePath in FoldersToCheck)
            {
                string sAbsolutePath = inst.ToAbsoluteGameDir(sRelativePath);
                if (!Directory.Exists(sAbsolutePath))
                    Directory.CreateDirectory(sAbsolutePath);
            }
        }

        public string DefaultCommandLine =>
                  Platform.IsUnix ? "./KSP.x86_64 -single-instance"
                : Platform.IsMac  ? "./KSP.app/Contents/MacOS/KSP"
                :                   "KSP_x64.exe -single-instance";

        public string[] AdjustCommandLine(string[] args, GameVersion installedVersion)
        {
            // -single-instance crashes KSP 1.8 to KSP 1.11 on Linux
            // https://issuetracker.unity3d.com/issues/linux-segmentation-fault-when-running-a-built-project-with-single-instance-argument
            if (Platform.IsUnix)
            {
                var brokenVersionRange = new GameVersionRange(
                    new GameVersion(1, 8),
                    new GameVersion(1, 11)
                );
                args = filterCmdLineArgs(args, installedVersion, brokenVersionRange, "-single-instance");
            }
            return args;
        }

        public List<GameVersion> KnownVersions =>
            ServiceLocator.Container.Resolve<IKspBuildMap>().KnownVersions;

        public GameVersion DetectVersion(DirectoryInfo where)
        {
            var buildIdVersionProvider = ServiceLocator.Container
                .ResolveKeyed<IGameVersionProvider>(GameVersionSource.BuildId);
            GameVersion version;
            if (buildIdVersionProvider.TryGetVersion(where.FullName, out version))
            {
                return version;
            }
            else
            {
                var readmeVersionProvider = ServiceLocator.Container
                    .ResolveKeyed<IGameVersionProvider>(GameVersionSource.Readme);
                return readmeVersionProvider.TryGetVersion(where.FullName, out version) ? version : null;
            }
        }

        public string CompatibleVersionsFile => "compatible_ksp_versions.json";

        public string[] BuildIDFiles => new string[]
        {
            "buildID.txt",
            "buildID64.txt"
        };

        public Uri DefaultRepositoryURL => new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.tar.gz");

        public Uri RepositoryListURL => new Uri("https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/repositories.json");

        private string Missions(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), "Missions"));
        }

        private string Ships(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), "Ships"));
        }

        private string ShipsVab(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(Ships(inst), "VAB"));
        }

        private string ShipsSph(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(Ships(inst), "SPH"));
        }

        private string ShipsThumbs(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(Ships(inst), "@thumbs"));
        }

        private string ShipsThumbsSPH(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(ShipsThumbs(inst), "SPH"));
        }

        private string ShipsThumbsVAB(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(ShipsThumbs(inst), "VAB"));
        }

        private string ShipsScript(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(Ships(inst), "Script"));
        }

        private string Tutorial(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), "saves", "training"));
        }

        private string Scenarios(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), "saves", "scenarios"));
        }

        private readonly Dictionary<string, string> allowedFolders = new Dictionary<string, string>
        {
            { "Tutorial",          "saves/training"    },
            { "Scenarios",         "saves/scenarios"   },
            { "Missions",          "Missions"          },
            { "Ships",             "Ships"             },
            { "Ships/VAB",         "Ships/VAB"         },
            { "Ships/SPH",         "Ships/SPH"         },
            { "Ships/@thumbs",     "Ships/@thumbs"     },
            { "Ships/@thumbs/VAB", "Ships/@thumbs/VAB" },
            { "Ships/@thumbs/SPH", "Ships/@thumbs/SPH" },
            { "Ships/Script",      "Ships/Script"      }
        };

        /// <summary>
        /// Finds the KSP path under a Steam Library. Returns null if the folder cannot be located.
        /// </summary>
        /// <param name="steamPath">Steam Library Path</param>
        /// <returns>The KSP path.</returns>
        private static string KSPDirectory(string steamPath)
        {
            // There are several possibilities for the path under Linux.
            // Try with the uppercase version.
            string installPath = Path.Combine(steamPath, "SteamApps", "common", "Kerbal Space Program");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }

            // Try with the lowercase version.
            installPath = Path.Combine(steamPath, "steamapps", "common", "Kerbal Space Program");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }
            return null;
        }

        /// <summary>
        /// If the installed game version is in the given range,
        /// return the given array without the given parameter,
        /// otherwise return the array as-is.
        /// </summary>
        /// <param name="args">Command line parameters to check</param>
        /// <param name="crashyKspRange">Game versions that should not use this parameter</param>
        /// <param name="parameter">The parameter to remove on version match</param>
        /// <returns>
        /// args or args minus parameter
        /// </returns>
        private string[] filterCmdLineArgs(string[] args, GameVersion installedVersion, GameVersionRange crashyKspRange, string parameter)
        {
            var installedRange = installedVersion.ToVersionRange();
            if (crashyKspRange.IntersectWith(installedRange) != null
                && args.Contains(parameter))
            {
                log.DebugFormat(
                    "Parameter {0} found on incompatible KSP version {1}, pruning",
                    parameter,
                    installedVersion.ToString());
                return args.Where(s => s != parameter).ToArray();
            }
            return args;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(KerbalSpaceProgram));
    }
}
