using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using log4net;
using ValveKeyValue;

namespace CKAN
{
    public class SteamLibrary
    {
        public SteamLibrary()
        {
            var libraryPath = SteamPaths.Where(p => !string.IsNullOrEmpty(p))
                                        .FirstOrDefault(Directory.Exists);
            if (libraryPath == null)
            {
                log.Info("Steam not found");
                Games = Array.Empty<NonSteamGame>();
            }
            else
            {
                log.InfoFormat("Found Steam at {0}", libraryPath);
                var txtParser     = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
                var appPaths      = txtParser.Deserialize<List<LibraryFolder>>(
                                                  File.OpenRead(Path.Combine(libraryPath,
                                                                             "config",
                                                                             "libraryfolders.vdf")))
                                             .Select(lf => appRelPaths.Select(p => Path.Combine(lf.Path, p))
                                                                      .FirstOrDefault(Directory.Exists))
                                             .Where(p => p != null)
                                             .ToArray();
                var steamGames    = appPaths.SelectMany(p => LibraryPathGames(txtParser, p));
                var binParser     = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
                var nonSteamGames = Directory.EnumerateDirectories(Path.Combine(libraryPath, "userdata"))
                                             .Select(dirName => Path.Combine(dirName, "config"))
                                             .Where(Directory.Exists)
                                             .Select(dirName => Path.Combine(dirName, "shortcuts.vdf"))
                                             .Where(File.Exists)
                                             .SelectMany(p => ShortcutsFileGames(binParser, p));
                Games = steamGames.Concat(nonSteamGames)
                                  .ToArray();
                log.DebugFormat("Games: {0}",
                                string.Join(", ", Games.Select(g => $"{g.LaunchUrl} ({g.GameDir})")));
            }
        }

        public IEnumerable<Uri> GameAppURLs(DirectoryInfo gameDir)
            => Games.Where(g => gameDir.FullName.Equals(g.GameDir.FullName, Platform.PathComparison))
                    .Select(g => g.LaunchUrl);

        public readonly GameBase[] Games;

        private static IEnumerable<GameBase> LibraryPathGames(KVSerializer acfParser,
                                                              string       appPath)
            => Directory.EnumerateFiles(appPath, "*.acf")
                        .Select(acfFile => acfParser.Deserialize<SteamGame>(File.OpenRead(acfFile))
                                                    .NormalizeDir(Path.Combine(appPath, "common")));

        private static IEnumerable<GameBase> ShortcutsFileGames(KVSerializer vdfParser,
                                                                string       path)
            => vdfParser.Deserialize<List<NonSteamGame>>(File.OpenRead(path))
                        .Select(nsg => nsg.NormalizeDir(path));

        private const string registryKey   = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        private const string registryValue = @"SteamPath";
        private static string[] SteamPaths
            => Platform.IsWindows ? new string[]
            {
                // First check the registry
                (string)Microsoft.Win32.Registry.GetValue(registryKey, registryValue, null),
            }
            : Platform.IsUnix ? new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                             ".local", "share", "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                             ".steam", "steam"),
            }
            : Platform.IsMac ? new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                             "Library", "Application Support", "Steam"),
            }
            : Array.Empty<string>();

        private static readonly string[] appRelPaths = new string[] { "SteamApps", "steamapps" };

        private static readonly ILog log = LogManager.GetLogger(typeof(SteamLibrary));
    }

    public class LibraryFolder
    {
        [KVProperty("path")] public string Path { get; set; }
    }

    public abstract class GameBase
    {
        public abstract string Name { get; set; }

        [KVIgnore] public          DirectoryInfo GameDir   { get; set; }
        [KVIgnore] public abstract Uri           LaunchUrl { get;      }

        public abstract GameBase NormalizeDir(string appPath);
    }

    public class SteamGame : GameBase
    {
        [KVProperty("appid")]      public          ulong  AppId      { get; set; }
        [KVProperty("name")]       public override string Name       { get; set; }
        [KVProperty("installdir")] public          string InstallDir { get; set; }

        [KVIgnore]
        public override Uri LaunchUrl => new Uri($"steam://rungameid/{AppId}");

        public override GameBase NormalizeDir(string commonPath)
        {
            GameDir = new DirectoryInfo(CKANPathUtils.NormalizePath(Path.Combine(commonPath, InstallDir)));
            return this;
        }
    }

    public class NonSteamGame : GameBase
    {
        [KVProperty("appid")]
        public          int    AppId    { get; set; }
        [KVProperty("AppName")]
        public override string Name     { get; set; }
        public          string Exe      { get; set; }
        public          string StartDir { get; set; }

        [KVIgnore]
        private ulong UrlId => (unchecked((ulong)AppId) << 32) | 0x02000000;

        [KVIgnore]
        public override Uri LaunchUrl => new Uri($"steam://rungameid/{UrlId}");

        public override GameBase NormalizeDir(string appPath)
        {
            GameDir = new DirectoryInfo(CKANPathUtils.NormalizePath(StartDir.Trim('"')));
            return this;
        }
    }
}
