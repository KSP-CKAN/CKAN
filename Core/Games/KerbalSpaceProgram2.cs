using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mono.Cecil;

using CKAN.DLC;
using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram2
{
    public class KerbalSpaceProgram2 : IGame
    {
        public string ShortName => "KSP2";
        public DateTime FirstReleaseDate => new DateTime(2023, 2, 24);

        public bool GameInFolder(DirectoryInfo where)
            => InstanceAnchorFiles.Any(f => File.Exists(Path.Combine(where.FullName, f)))
                && Directory.Exists(Path.Combine(where.FullName, DataDir));

        /// <summary>
        /// Get the default non-Steam path to KSP on macOS
        /// </summary>
        /// <returns>
        /// "/Applications/Kerbal Space Program" if it exists and we're on a Mac, else null
        /// </returns>
        public DirectoryInfo MacPath()
        {
            if (Platform.IsMac)
            {
                string installPath = Path.Combine(
                    // This is "/Applications" in Mono on Mac
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Kerbal Space Program 2"
                );
                return Directory.Exists(installPath) ? new DirectoryInfo(installPath)
                                                     : null;
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
            DataDir,
            "MonoBleedingEdge",
            "PDLauncher",
        };

        public string[] LeaveEmptyInClones => new string[]
        {
            "CKAN/history",
            "CKAN/downloads",
        };

        public string[] ReservedPaths => Array.Empty<string>();

        public string[] CreateableDirs => new string[]
        {
            "GameData",
            "GameData/Mods",
            "BepInEx",
            "BepInEx/plugins",
        };

        public string[] AutoRemovableDirs => Array.Empty<string>();

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
            var path = Path.Combine(absGameRoot, DataDir);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string[] DefaultCommandLines(SteamLibrary steamLib, DirectoryInfo path)
            => Enumerable.Repeat(Platform.IsMac
                                     ? "./KSP2.app/Contents/MacOS/KSP2"
                                     : string.Format(Platform.IsUnix ? "./{0} -single-instance"
                                                                     : "{0} -single-instance",
                                                     InstanceAnchorFiles.FirstOrDefault(f =>
                                                         File.Exists(Path.Combine(path.FullName, f)))
                                                     ?? InstanceAnchorFiles.First()),
                                 1)
                         .Concat(steamLib.GameAppURLs(path)
                                         .Select(url => url.ToString()))
                         .ToArray();

        public string[] AdjustCommandLine(string[] args, GameVersion installedVersion)
            => args;

        public IDlcDetector[] DlcDetectors => Array.Empty<IDlcDetector>();

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

        public GameVersion[] EmbeddedGameVersions
            => JsonConvert.DeserializeObject<GameVersion[]>(
                new StreamReader(Assembly.GetExecutingAssembly()
                                         .GetManifestResourceStream("CKAN.builds-ksp2.json"))
                    .ReadToEnd());

        public GameVersion[] ParseBuildsJson(JToken json)
            => json.ToObject<GameVersion[]>();

        public GameVersion DetectVersion(DirectoryInfo where)
            => VersionFromAssembly(Path.Combine(where.FullName,
                                                DataDir,
                                                "Managed",
                                                "Assembly-CSharp.dll"))
                ?? VersionFromExecutable(Path.Combine(where.FullName,
                                                      "KSP2_x64.exe"))
                // Fall back to the most recent version
                ?? KnownVersions.Last();

        private static GameVersion VersionFromAssembly(string assemblyPath)
            => File.Exists(assemblyPath)
                && GameVersion.TryParse(
                    AssemblyDefinition.ReadAssembly(assemblyPath)
                                      .Modules
                                      .SelectMany(m => m.GetTypes())
                                      .Where(t => t.Name == "VersionID")
                                      .SelectMany(t => t.Fields)
                                      .Where(f => f.Name == "VERSION_TEXT")
                                      .Select(f => (string)f.Constant)
                                      .Select(ver => string.Join(".", ver.Split('.').Take(4)))
                                      .FirstOrDefault(),
                    out GameVersion v)
                        ? v
                        : null;

        private static GameVersion VersionFromExecutable(string exePath)
            => File.Exists(exePath)
                && GameVersion.TryParse(FileVersionInfo.GetVersionInfo(exePath).ProductVersion
                                        // Fake instances have an EXE containing just the version string
                                        ?? File.ReadAllText(exePath),
                                        out GameVersion v)
                    ? v
                    : null;

        public string CompatibleVersionsFile => "compatible_game_versions.json";

        public string[] InstanceAnchorFiles =>
            Platform.IsUnix
                ? new string[]
                {
                    // Native Linux port, if/when it arrives
                    "KSP2.x86_64",
                    // Windows EXE via Proton on Linux
                    "KSP2_x64.exe",
                }
                : new string[]
                {
                    "KSP2_x64.exe",
                };

        public Uri DefaultRepositoryURL => new Uri("https://github.com/KSP-CKAN/KSP2-CKAN-meta/archive/main.tar.gz");

        public Uri RepositoryListURL => new Uri("https://raw.githubusercontent.com/KSP-CKAN/KSP2-CKAN-meta/main/repositories.json");

        public Uri MetadataBugtrackerURL => new Uri("https://github.com/KSP-CKAN/KSP2-NetKAN/issues/new/choose");

        public Uri ModSupportURL => new Uri("https://forum.kerbalspaceprogram.com/forum/137-ksp2-technical-support-pc-modded-installs/");

        private const string DataDir = "KSP2_x64_Data";

        // Key: Allowed value of install_to
        // Value: Relative path
        // (PrimaryModDirectoryRelative is allowed implicitly)
        private readonly Dictionary<string, string> allowedFolders = new Dictionary<string, string>
        {
            { "BepInEx",         "BepInEx"         },
            { "BepInEx/plugins", "BepInEx/plugins" },
        };

        private static readonly ILog log = LogManager.GetLogger(typeof(KerbalSpaceProgram2));
    }
}
