using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using ValveKeyValue;

using CKAN.Extensions;

namespace CKAN
{
    public class SteamLibrary
    {
        public SteamLibrary()
            : this(SteamPaths.FirstOrDefault(p => !string.IsNullOrEmpty(p)
                                                  && Directory.Exists(p)
                                                  && File.Exists(LibraryFoldersConfigPath(p))))
        {
        }

        public SteamLibrary(string? libraryPath)
        {
            if (libraryPath != null
                && LibraryFoldersConfigPath(libraryPath) is string libFoldersConfigPath
                && OpenRead(libFoldersConfigPath) is FileStream stream)
            {
                log.InfoFormat("Found Steam at {0}", libraryPath);
                var txtParser     = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
                var appPaths      = (Utilities.DefaultIfThrows(
                                                   () => txtParser.Deserialize<Dictionary<int, LibraryFolder>>(stream),
                                                   exc =>
                                                   {
                                                       log.Warn($"Failed to parse {libFoldersConfigPath}", exc);
                                                       return null;
                                                   })
                                              ?.Values
                                               .Select(lf => lf.Path is string libPath
                                                             ? appRelPaths.Select(p => Path.Combine(libPath, p))
                                                                          .FirstOrDefault(Directory.Exists)
                                                             : null)
                                               .OfType<string>()
                                              ?? Enumerable.Empty<string>())
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
            else
            {
                log.Info("Steam not found");
                Games = Array.Empty<NonSteamGame>();
            }
        }

        public IEnumerable<Uri> GameAppURLs(DirectoryInfo gameDir)
            => Games.Where(g => gameDir.FullName.Equals(g.GameDir?.FullName, Platform.PathComparison))
                    .Select(g => g.LaunchUrl);

        public readonly GameBase[] Games;

        private static string LibraryFoldersConfigPath(string libraryPath)
            => Path.Combine(libraryPath, "config", "libraryfolders.vdf");

        private static FileStream? OpenRead(string path)
            => Utilities.DefaultIfThrows(() => File.OpenRead(path),
                                         exc =>
                                         {
                                             log.Warn($"Failed to open {path}", exc);
                                             return null;
                                         });

        private static IEnumerable<GameBase> LibraryPathGames(KVSerializer acfParser,
                                                              string       appPath)
            => Directory.EnumerateFiles(appPath, "*.acf")
                        .SelectWithCatch(acfFile => acfParser.Deserialize<SteamGame>(File.OpenRead(acfFile))
                                                             .NormalizeDir(Path.Combine(appPath, "common")),
                                         (acfFile, exc) =>
                                         {
                                             log.Warn($"Failed to parse {acfFile}:", exc);
                                             return default;
                                         })
                        .OfType<GameBase>();

        private static IEnumerable<GameBase> ShortcutsFileGames(KVSerializer vdfParser,
                                                                string       path)
            => Utilities.DefaultIfThrows(() => vdfParser.Deserialize<Dictionary<int, NonSteamGame>>(File.OpenRead(path)),
                                         exc =>
                                         {
                                             log.Warn($"Failed to parse {path}", exc);
                                             return null;
                                         })
                        ?.Values
                         .Select(nsg => nsg.NormalizeDir(path))
                        ?? Enumerable.Empty<NonSteamGame>();

        /// <summary>
        /// Find the location where the current user's application data resides. Specific to macOS.
        /// </summary>
        /// <returns>
        ///     The application data folder, e.g. <code>/Users/USER/Library/Application Support</code>
        /// </returns>
        private static string GetMacOSApplicationDataFolder()
        {
            Debug.Assert(Platform.IsMac);

#if NET8_0_OR_GREATER
                // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support");
#endif
        }

        private const  string   registryKey   = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        private const  string   registryValue = @"SteamPath";
        private static string[] SteamPaths
            => Platform.IsWindows
               // First check the registry
               && Microsoft.Win32.Registry.GetValue(registryKey, registryValue, "") is string val
               && !string.IsNullOrEmpty(val)
            ? new string[]
            {
                val,
            }
            : Platform.IsUnix ? new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                             "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                             ".steam", "steam"),
            }
            : Platform.IsMac ? new string[]
            {
                Path.Combine(GetMacOSApplicationDataFolder(), "Steam"),
            }
            : Array.Empty<string>();

        private static readonly string[] appRelPaths = new string[] { "SteamApps", "steamapps" };

        private static readonly ILog log = LogManager.GetLogger(typeof(SteamLibrary));
    }

    public class LibraryFolder
    {
        [KVProperty("path")] public string? Path { get; set; }
    }

    public abstract class GameBase
    {
        public abstract string? Name { get; set; }

        [KVIgnore] public          DirectoryInfo? GameDir   { get; set; }
        [KVIgnore] public abstract Uri            LaunchUrl { get;      }

        public abstract GameBase NormalizeDir(string appPath);
    }

    public class SteamGame : GameBase
    {
        [KVProperty("appid")]      public          ulong   AppId      { get; set; }
        [KVProperty("name")]       public override string? Name       { get; set; }
        [KVProperty("installdir")] public          string? InstallDir { get; set; }

        [KVIgnore]
        public override Uri LaunchUrl => new Uri($"steam://rungameid/{AppId}");

        public override GameBase NormalizeDir(string commonPath)
        {
            if (InstallDir != null)
            {
                GameDir = new DirectoryInfo(CKANPathUtils.NormalizePath(Path.Combine(commonPath, InstallDir)));
            }
            return this;
        }
    }

    public class NonSteamGame : GameBase
    {
        [KVProperty("appid")]
        public          int     AppId    { get; set; }
        [KVProperty("AppName")]
        public override string? Name     { get; set; }
        public          string? Exe      { get; set; }
        public          string? StartDir { get; set; }

        [KVIgnore]
        private ulong UrlId => (unchecked((ulong)AppId) << 32) | 0x02000000;

        [KVIgnore]
        public override Uri LaunchUrl => new Uri($"steam://rungameid/{UrlId}");

        public override GameBase NormalizeDir(string appPath)
        {
            GameDir = StartDir == null ? null
                                       : Utilities.DefaultIfThrows(() =>
                                           new DirectoryInfo(CKANPathUtils.NormalizePath(StartDir.Trim('"'))));
            return this;
        }
    }
}
