using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Autofac;
using log4net;
using Newtonsoft.Json;

using CKAN.DLC;
using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram2
{
    public class KerbalSpaceProgram2 : IGame
    {
        public string ShortName => "KSP2";

        public bool GameInFolder(DirectoryInfo where)
            => where.EnumerateFiles().Any(f => f.Name == "KSP2_x64.exe")
                && where.EnumerateDirectories().Any(d => d.Name == "KSP2_x64_Data");

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
            string installPath = GameDirectory(steamPath);
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

                            installPath = GameDirectory(split_line[3]);
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
                    "Kerbal Space Program 2"
                );
                return Directory.Exists(installPath) ? installPath : null;
            }
            return null;
        }

        public string PrimaryModDirectoryRelative => "GameData/Mods";
        public string[] AlternateModDirectoriesRelative => new string[] { "BepInEx/plugins" };

        public string PrimaryModDirectory(GameInstance inst)
            => CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), PrimaryModDirectoryRelative));

        public string[] StockFolders => new string[]
        {
            "KSP2_x64_Data",
            "MonoBleedingEdge",
            "PDLauncher",
        };

        public string[] ReservedPaths => new string[]
        {
        };

        public string[] CreateableDirs => new string[]
        {
            "GameData",
            "GameData/Mods",
            "BepInEx",
            "BepInEx/plugins",
        };

        public string[] AutoRemovableDirs => new string[] { };

        /// <summary>
        /// Checks the path against a list of reserved game directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsReservedDirectory(GameInstance inst, string path)
            => path == inst.GameDir() || path == inst.CkanDir()
                || path == PrimaryModDirectory(inst);

        public bool AllowInstallationIn(string name, out string path)
            => allowedFolders.TryGetValue(name, out path);

        public void RebuildSubdirectories(string absGameRoot)
        {
            const string dataDir = "KSP2_x64_Data";
            var path = Path.Combine(absGameRoot, dataDir);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public string DefaultCommandLine =>
                  Platform.IsUnix ? "./KSP2.x86_64 -single-instance"
                : Platform.IsMac  ? "./KSP2.app/Contents/MacOS/KSP"
                :                   "KSP2_x64.exe -single-instance";

        public string[] AdjustCommandLine(string[] args, GameVersion installedVersion)
            => args;

        public IDlcDetector[] DlcDetectors => new IDlcDetector[] { };

        private static readonly Uri BuildMapUri =
            new Uri("https://raw.githubusercontent.com/KSP-CKAN/KSP2-CKAN-meta/main/builds.json");
        private static readonly string cachedBuildMapPath =
            Path.Combine(CKANPathUtils.AppDataPath, "builds-ksp2.json");

        private List<GameVersion> versions = JsonConvert.DeserializeObject<List<GameVersion>>(
            File.Exists(cachedBuildMapPath)
                ? File.ReadAllText(cachedBuildMapPath)
                : new StreamReader(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("CKAN.builds-ksp2.json"))
                        .ReadToEnd());

        public void RefreshVersions()
        {
            try
            {
                var json = Net.DownloadText(BuildMapUri);
                versions = JsonConvert.DeserializeObject<List<GameVersion>>(json);
                // Save to disk if download and parse succeeds
                new FileInfo(cachedBuildMapPath).Directory.Create();
                File.WriteAllText(cachedBuildMapPath, json);
            }
            catch (Exception e)
            {
                log.WarnFormat("Could not retrieve latest build map from: {0}", BuildMapUri);
                log.Debug(e);
            }
        }

        public List<GameVersion> KnownVersions => versions;

        public GameVersion DetectVersion(DirectoryInfo where)
            => VersionFromFile(Path.Combine(where.FullName, "KSP2_x64.exe"));

        private GameVersion VersionFromFile(string path)
            => File.Exists(path)
                ? GameVersion.Parse(
                    FileVersionInfo.GetVersionInfo(path).ProductVersion
                    ?? versions.Last().ToString())
                : null;

        public string CompatibleVersionsFile => "compatible_game_versions.json";

        public string[] BuildIDFiles => new string[]
        {
            "KSP2_x64.exe",
        };

        public Uri DefaultRepositoryURL => new Uri("https://github.com/KSP-CKAN/KSP2-CKAN-meta/archive/main.tar.gz");

        public Uri RepositoryListURL => new Uri("https://raw.githubusercontent.com/KSP-CKAN/KSP2-CKAN-meta/main/repositories.json");

        // Key: Allowed value of install_to
        // Value: Relative path
        // (PrimaryModDirectoryRelative is allowed implicitly)
        private readonly Dictionary<string, string> allowedFolders = new Dictionary<string, string>
        {
            { "BepInEx",         "BepInEx"         },
            { "BepInEx/plugins", "BepInEx/plugins" },
        };

        /// <summary>
        /// Finds the KSP path under a Steam Library. Returns null if the folder cannot be located.
        /// </summary>
        /// <param name="steamPath">Steam Library Path</param>
        /// <returns>The KSP path.</returns>
        private static string GameDirectory(string steamPath)
        {
            // There are several possibilities for the path under Linux.
            // Try with the uppercase version.
            string installPath = Path.Combine(steamPath, "SteamApps", "common", "Kerbal Space Program 2");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }

            // Try with the lowercase version.
            installPath = Path.Combine(steamPath, "steamapps", "common", "Kerbal Space Program 2");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }
            return null;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(KerbalSpaceProgram2));
    }
}
