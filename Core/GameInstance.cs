using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using ChinhDo.Transactions;
using log4net;
using Newtonsoft.Json;

using CKAN.IO;
using CKAN.Configuration;
using CKAN.Games;
using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN
{
    /// <summary>
    /// Everything for dealing with a game folder.
    /// </summary>
    public class GameInstance : IEquatable<GameInstance>
    {
        #region Construction and Initialisation

        /// <summary>
        /// Returns a game instance object.
        /// Will initialise a CKAN instance in the game dir if it does not already exist.
        /// </summary>
        public GameInstance(IGame game, string gameDir, string name, IUser? user)
        {
            Game = game;
            Name = name;
            User = user ?? new NullUser();
            // Make sure our path is absolute and has normalised slashes.
            GameDir = CKANPathUtils.NormalizePath(Path.GetFullPath(gameDir));
            if (Platform.IsWindows)
            {
                // Normalized slashes are bad for pure drive letters,
                // Path.Combine turns them into drive-relative paths like
                // K:GameData/whatever
                if (Regex.IsMatch(GameDir, @"^[a-zA-Z]:$"))
                {
                    GameDir = $"{GameDir}/";
                }
            }
            SetupCkanDirectories();
            LoadCompatibleVersions();
        }

        /// <returns>
        /// true if the game seems to be here and a version is found,
        /// false otherwise
        /// </returns>
        public bool Valid => Game.GameInFolder(new DirectoryInfo(GameDir)) && Version() != null;

        /// <returns>
        /// true if the instance may be locked, false otherwise.
        /// Note that this is a tentative value; if it's true,
        /// we still need to try to acquire the lock to confirm it isn't stale.
        /// </returns>
        public bool IsMaybeLocked => RegistryManager.IsInstanceMaybeLocked(CkanDir);

        /// <summary>
        /// Create the CKAN directory and any supporting files.
        /// </summary>
        [MemberNotNull(nameof(playTime))]
        private void SetupCkanDirectories()
        {
            log.InfoFormat("Initialising {0}", CkanDir);

            // TxFileManager knows if we are in a transaction
            var txFileMgr = new TxFileManager(CkanDir);

            if (!Directory.Exists(CkanDir))
            {
                User.RaiseMessage(Properties.Resources.GameInstanceSettingUp);
                User.RaiseMessage(Properties.Resources.GameInstanceCreatingDir, CkanDir);
                txFileMgr.CreateDirectory(CkanDir);
            }

            playTime = TimeLog.Load(TimeLog.GetPath(CkanDir)) ?? new TimeLog();

            if (!Directory.Exists(InstallHistoryDir))
            {
                User.RaiseMessage(Properties.Resources.GameInstanceCreatingDir, InstallHistoryDir);
                txFileMgr.CreateDirectory(InstallHistoryDir);
            }
            log.InfoFormat("Initialised {0}", CkanDir);
        }

        #endregion

        #region Fields and Properties

        public IUser User { get; private set; }

        public string Name { get; set; }

        /// <summary>
        /// Returns a file system safe version of the instance name that can be used within file names.
        /// </summary>
        public string SanitizedName => string.Join("", Name.Split(Path.GetInvalidFileNameChars()));

        public readonly string GameDir;
        public readonly IGame Game;
        private GameVersion? version;

        public TimeLog? playTime;

        public GameVersion? GameVersionWhenCompatibleVersionsWereStored
            => _compatibleVersions.GameVersionWhenWritten;

        public bool CompatibleVersionsAreFromDifferentGameVersion
            => GameVersionWhenCompatibleVersionsWereStored != Version();

        private CompatibleGameVersions _compatibleVersions;

        private static readonly ILog log = LogManager.GetLogger(typeof(GameInstance));

        public string CkanDir
            => ckanDir ??= CKANPathUtils.NormalizePath(Path.Combine(GameDir, "CKAN"));

        public string DownloadCacheDir
            => downloadDir ??= CKANPathUtils.NormalizePath(Path.Combine(CkanDir, "downloads"));

        public string InstallHistoryDir
            => historyDir ??= CKANPathUtils.NormalizePath(Path.Combine(CkanDir, "history"));

        private string? ckanDir;
        private string? downloadDir;
        private string? historyDir;

        public IOrderedEnumerable<FileInfo> InstallHistoryFiles()
            => Directory.EnumerateFiles(InstallHistoryDir, "*.ckan")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.CreationTime);

        public GameVersion? Version()
            => version ??= DetectVersion(GameDir);

        public GameVersionCriteria VersionCriteria()
            => new GameVersionCriteria(Version(), _compatibleVersions.Versions);

        #endregion

        #region Settings

        [MemberNotNull(nameof(_compatibleVersions))]
        private void LoadCompatibleVersions()
        {
            string path = CompatibleGameVersionsFile;
            if (File.Exists(path)
                && JsonConvert.DeserializeObject<CompatibleGameVersions>(File.ReadAllText(path))
                   is CompatibleGameVersions compatibleGameVersions)
            {
                _compatibleVersions = compatibleGameVersions;
            }
            else
            {
                _compatibleVersions = new CompatibleGameVersions()
                {
                    GameVersionWhenWritten = null,
                    Versions               = Version() is GameVersion gv
                                                 ? Game.DefaultCompatibleVersions(gv).ToList()
                                                 : new List<GameVersion>(),
                };
            }
        }

        [MemberNotNull(nameof(_compatibleVersions))]
        public void SetCompatibleVersions(IReadOnlyCollection<GameVersion> compatibleVersions)
        {
            _compatibleVersions = new CompatibleGameVersions()
            {
                GameVersionWhenWritten = Version(),
                Versions               = compatibleVersions.Distinct()
                                                           .OrderDescending()
                                                           .ToList(),
            };
            JsonConvert.SerializeObject(_compatibleVersions)
                       .WriteThroughTo(CompatibleGameVersionsFile);
        }

        private string CompatibleGameVersionsFile
            => Path.Combine(CkanDir, Game.CompatibleVersionsFile);

        public IReadOnlyCollection<GameVersion> CompatibleVersions => _compatibleVersions.Versions;

        public HashSet<string> GetSuppressedCompatWarningIdentifiers
            => SuppressedCompatWarningIdentifiers.LoadFrom(Version(), SuppressedCompatWarningIdentifiersFile)
                                                 .Identifiers;

        public void AddSuppressedCompatWarningIdentifiers(HashSet<string> idents)
        {
            var scwi = SuppressedCompatWarningIdentifiers.LoadFrom(Version(), SuppressedCompatWarningIdentifiersFile);
            scwi.Identifiers.UnionWith(idents);
            scwi.SaveTo(SuppressedCompatWarningIdentifiersFile);
        }

        private string SuppressedCompatWarningIdentifiersFile
            => Path.Combine(CkanDir, "suppressed_compat_warning_identifiers.json");

        public string[] InstallFilters
        {
            get => (File.Exists(InstallFiltersFile)
                        ? JsonConvert.DeserializeObject<string[]>(File.ReadAllText(InstallFiltersFile))
                        : null)
                   ?? Array.Empty<string>();

            #pragma warning disable IDE0027
            set
            {
                JsonConvert.SerializeObject(value)
                           .WriteThroughTo(InstallFiltersFile);
            }
            #pragma warning restore IDE0027
        }

        private string InstallFiltersFile => Path.Combine(CkanDir, "install_filters.json");

        public StabilityToleranceConfig StabilityToleranceConfig
            => stabilityToleranceConfig ??= new StabilityToleranceConfig(StabilityToleranceFile);

        private StabilityToleranceConfig? stabilityToleranceConfig;

        private string StabilityToleranceFile
            => Path.Combine(CkanDir, "stability_tolerance.json");

        #endregion

        #region Game Directory Detection and Versioning

        /// <summary>
        /// Returns the path to our portable version of game if ckan.exe is in the same
        /// directory as the game, or if the game is in the current directory.
        /// Otherwise, returns null.
        /// </summary>
        public static string? PortableDir(IGame game)
            => new string?[]
               {
                   Assembly.GetExecutingAssembly()?.Location,
                   Process.GetCurrentProcess()?.MainModule?.FileName,
               }
                   .OfType<string>()
                   .Select(Path.GetDirectoryName)
                   .OfType<string>()
                   .Prepend(Directory.GetCurrentDirectory())
                   .Select(path => new DirectoryInfo(path))
                   .FirstOrDefault(game.GameInFolder)
                   ?.FullName;

        /// <summary>
        /// Detects the version of a game in a given directory.
        /// </summary>
        private GameVersion? DetectVersion(string directory)
            => Game.DetectVersion(new DirectoryInfo(directory));

        #endregion

        /// <summary>
        /// Returns path relative to this instance's GameDir.
        /// </summary>
        public string ToRelativeGameDir(string path)
            => CKANPathUtils.ToRelative(path, GameDir);

        /// <summary>
        /// Given a path relative to this instance's GameDir, returns the
        /// absolute path on the system.
        /// </summary>
        public string ToAbsoluteGameDir(string path)
            => CKANPathUtils.ToAbsolute(path, GameDir);

        /// <summary>
        /// https://xkcd.com/208/
        /// This regex matches things like GameData/Foo/Foo.1.2.dll
        /// </summary>
        private static readonly Regex dllPattern = new Regex(
            @"
                ^(?:.*/)?             # Directories (ending with /)
                (?<identifier>[^.]+)  # Our DLL name, up until the first dot.
                .*\.dll$              # Everything else, ending in dll
            ",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled
        );

        /// <summary>
        /// Find the identifier associated with a manually installed DLL
        /// </summary>
        /// <param name="relPath">Path of the DLL relative to game root</param>
        /// <returns>
        /// Identifier if found otherwise null
        /// </returns>
        public string? DllPathToIdentifier(string relPath)
            => DllPathToIdentifier(Game, relPath);

        public static string? DllPathToIdentifier(IGame game, string relPath)
            // DLLs only live in the primary or alternate mod directories
            => game.AlternateModDirectoriesRelative
                   .Prepend(game.PrimaryModDirectoryRelative)
                   .Any(p => relPath.StartsWith($"{p}/", Platform.PathComparison))
               && dllPattern.Match(relPath) is { Success: true } match
                   ? Identifier.Sanitize(match.Groups["identifier"].Value)
                   : null;

        /// <summary>
        /// Generate a sequence of files in the game folder that weren't installed by CKAN
        /// </summary>
        /// <param name="registry">A Registry object that knows which files CKAN installed in this folder</param>
        /// <returns>Relative file paths as strings</returns>
        public IEnumerable<string> UnmanagedFiles(Registry registry)
            => Directory.EnumerateFiles(GameDir, "*", SearchOption.AllDirectories)
                        .Select(CKANPathUtils.NormalizePath)
                        .Where(absPath => !absPath.StartsWith(CkanDir))
                        .Select(ToRelativeGameDir)
                        .Where(relPath =>
                            !Game.StockFolders.Any(f => relPath.StartsWith($"{f}/"))
                            && registry.FileOwner(relPath) == null);

        /// <summary>
        /// Check whether a given path contains any files or folders installed by CKAN
        /// </summary>
        /// <param name="registry">A Registry object that knows which files CKAN installed in this folder</param>
        /// <param name="absPath">Absolute path to a folder to check</param>
        /// <returns>true if any descendants of given path were installed by CKAN, false otherwise</returns>
        public bool HasManagedFiles(Registry registry, string absPath)
            => registry.FileOwner(ToRelativeGameDir(absPath)) != null
               || (Directory.Exists(absPath)
                   && Directory.EnumerateFileSystemEntries(absPath, "*", SearchOption.AllDirectories)
                               .Any(f => registry.FileOwner(ToRelativeGameDir(f)) != null));

        [ExcludeFromCodeCoverage]
        public void PlayGame(string command, Action? onExit = null)
        {
            if (Game.AdjustCommandLine(command.Split(' '), Version())
                //is [string binary, ..] and string[] split
                is string[] split
                && split.Length > 0
                && split[0] is string binary)
            {
                try
                {
                    Directory.SetCurrentDirectory(GameDir);
                    Process p = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName  = binary,
                            Arguments = string.Join(" ", split.Skip(1))
                        },
                        EnableRaisingEvents = true
                    };

                    var isSteam = SteamLibrary.IsSteamCmdLine(command);
                    p.Exited += (sender, e) =>
                    {
                        if (!isSteam)
                        {
                            playTime?.Stop(CkanDir);
                        }
                        onExit?.Invoke();
                    };

                    p.Start();
                    if (!isSteam)
                    {
                        playTime?.Start();
                    }
                }
                catch (Exception exception)
                {
                    User.RaiseError(Properties.Resources.GameInstancePlayGameFailed, exception.Message);
                }
            }
        }

        public override string ToString()
            => string.Format(Properties.Resources.GameInstanceToString, Game.ShortName, GameDir);

        public bool Equals(GameInstance? other)
            => other != null && GameDir.Equals(other.GameDir);

        public override bool Equals(object? obj)
            => Equals(obj as GameInstance);

        public override int GetHashCode()
            => GameDir.GetHashCode();
    }
}
