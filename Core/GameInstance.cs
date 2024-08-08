using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;

using ChinhDo.Transactions.FileManager;
using log4net;
using Newtonsoft.Json;

using CKAN.Games;
using CKAN.Versioning;

namespace CKAN
{

    /// <summary>
    /// Everything for dealing with a game folder.
    /// </summary>
    public class GameInstance : IEquatable<GameInstance>
    {
        #region Fields and Properties

        public IUser User { get; private set; }

        private readonly string gameDir;
        public readonly IGame game;
        private GameVersion version;
        private List<GameVersion> _compatibleVersions = new List<GameVersion>();

        public TimeLog playTime;

        public string Name { get; set; }

        /// <summary>
        /// Returns a file system safe version of the instance name that can be used within file names.
        /// </summary>
        public string SanitizedName => string.Join("", Name.Split(Path.GetInvalidFileNameChars()));
        public GameVersion GameVersionWhenCompatibleVersionsWereStored { get; private set; }
        public bool CompatibleVersionsAreFromDifferentGameVersion
            => _compatibleVersions.Count > 0
               && GameVersionWhenCompatibleVersionsWereStored != Version();

        private static readonly ILog log = LogManager.GetLogger(typeof(GameInstance));

        #endregion

        #region Construction and Initialisation

        /// <summary>
        /// Returns a KSP object.
        /// Will initialise a CKAN instance in the KSP dir if it does not already exist,
        /// if the directory contains a valid KSP install.
        /// </summary>
        public GameInstance(IGame game, string gameDir, string name, IUser user)
        {
            this.game = game;
            Name = name;
            User = user;
            // Make sure our path is absolute and has normalised slashes.
            this.gameDir = CKANPathUtils.NormalizePath(Path.GetFullPath(gameDir));
            if (Platform.IsWindows)
            {
                // Normalized slashes are bad for pure drive letters,
                // Path.Combine turns them into drive-relative paths like
                // K:GameData/whatever
                if (Regex.IsMatch(this.gameDir, @"^[a-zA-Z]:$"))
                {
                    this.gameDir = $"{this.gameDir}/";
                }
            }
            if (Valid)
            {
                SetupCkanDirectories();
                LoadCompatibleVersions();
            }
        }

        /// <returns>
        /// true if the game seems to be here and a version is found,
        /// false otherwise
        /// </returns>
        public bool Valid => game.GameInFolder(new DirectoryInfo(gameDir)) && Version() != null;

        /// <returns>
        /// true if the instance may be locked, false otherwise.
        /// Note that this is a tentative value; if it's true,
        /// we still need to try to acquire the lock to confirm it isn't stale.
        /// NOTE: Will throw NotKSPDirKraken if the instance isn't valid!
        ///       Either be prepared to catch that exception, or check Valid first to avoid it.
        /// </returns>
        public bool IsMaybeLocked => RegistryManager.IsInstanceMaybeLocked(CkanDir());

        /// <summary>
        /// Create the CKAN directory and any supporting files.
        /// </summary>
        private void SetupCkanDirectories()
        {
            log.InfoFormat("Initialising {0}", CkanDir());

            // TxFileManager knows if we are in a transaction
            TxFileManager txFileMgr = new TxFileManager();

            if (!Directory.Exists(CkanDir()))
            {
                User.RaiseMessage(Properties.Resources.GameInstanceSettingUp);
                User.RaiseMessage(Properties.Resources.GameInstanceCreatingDir, CkanDir());
                txFileMgr.CreateDirectory(CkanDir());
            }

            playTime = TimeLog.Load(TimeLog.GetPath(CkanDir())) ?? new TimeLog();

            if (!Directory.Exists(InstallHistoryDir()))
            {
                User.RaiseMessage(Properties.Resources.GameInstanceCreatingDir, InstallHistoryDir());
                txFileMgr.CreateDirectory(InstallHistoryDir());
            }
            log.InfoFormat("Initialised {0}", CkanDir());
        }

        #endregion

        #region Settings

        public void SetCompatibleVersions(List<GameVersion> compatibleVersions)
        {
            _compatibleVersions = compatibleVersions.Distinct()
                                                    .OrderByDescending(v => v)
                                                    .ToList();
            SaveCompatibleVersions();
        }

        private void SaveCompatibleVersions()
        {
            File.WriteAllText(
                CompatibleGameVersionsFile(),
                JsonConvert.SerializeObject(new CompatibleGameVersions()
                {
                    GameVersionWhenWritten = Version()?.ToString(),
                    Versions = _compatibleVersions.Select(v => v.ToString()).ToList()
                })
            );
            GameVersionWhenCompatibleVersionsWereStored = Version();
        }

        private void LoadCompatibleVersions()
        {
            string path = CompatibleGameVersionsFile();
            if (File.Exists(path))
            {
                CompatibleGameVersions compatibleGameVersions = JsonConvert.DeserializeObject<CompatibleGameVersions>(File.ReadAllText(path));

                _compatibleVersions = compatibleGameVersions.Versions
                    .Select(v => GameVersion.Parse(v)).ToList();

                // Get version without throwing exceptions for null
                GameVersion.TryParse(compatibleGameVersions.GameVersionWhenWritten, out GameVersion mainVer);
                GameVersionWhenCompatibleVersionsWereStored = mainVer;
            }
        }

        private string CompatibleGameVersionsFile()
            => Path.Combine(CkanDir(), game.CompatibleVersionsFile);

        public List<GameVersion> GetCompatibleVersions()
            => new List<GameVersion>(_compatibleVersions);

        public HashSet<string> GetSuppressedCompatWarningIdentifiers =>
            SuppressedCompatWarningIdentifiers.LoadFrom(Version(), SuppressedCompatWarningIdentifiersFile).Identifiers;

        public void AddSuppressedCompatWarningIdentifiers(HashSet<string> idents)
        {
            var scwi = SuppressedCompatWarningIdentifiers.LoadFrom(Version(), SuppressedCompatWarningIdentifiersFile);
            scwi.Identifiers.UnionWith(idents);
            scwi.SaveTo(SuppressedCompatWarningIdentifiersFile);
        }

        private string SuppressedCompatWarningIdentifiersFile =>
            Path.Combine(CkanDir(), "suppressed_compat_warning_identifiers.json");

        public string[] InstallFilters
        {
            get => File.Exists(InstallFiltersFile)
                    ? JsonConvert.DeserializeObject<string[]>(File.ReadAllText(InstallFiltersFile))
                    : Array.Empty<string>();

            #pragma warning disable IDE0027
            set
            {
                File.WriteAllText(InstallFiltersFile, JsonConvert.SerializeObject(value));
            }
            #pragma warning restore IDE0027
        }

        private string InstallFiltersFile => Path.Combine(CkanDir(), "install_filters.json");

        #endregion

        #region KSP Directory Detection and Versioning

        /// <summary>
        /// Returns the path to our portable version of KSP if ckan.exe is in the same
        /// directory as the game, or if the game is in the current directory.
        /// Otherwise, returns null.
        /// </summary>
        public static string PortableDir(IGame game)
        {
            string curDir = Directory.GetCurrentDirectory();

            log.DebugFormat("Checking if {0} is in my current dir: {1}",
                game.ShortName, curDir);

            if (game.GameInFolder(new DirectoryInfo(curDir)))
            {
                log.InfoFormat("{0} found at {1}", game.ShortName, curDir);
                return curDir;
            }

            // Find the directory our executable is stored in.
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(exeDir))
            {
                exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                if (string.IsNullOrEmpty(exeDir))
                {
                    log.InfoFormat("Executing assembly path and main module path not found");
                    return null;
                }
                log.InfoFormat("Executing assembly path not found, main module path is {0}", exeDir);
            }

            log.DebugFormat("Checking if {0} is in my exe dir: {1}",
                game.ShortName, exeDir);

            if (curDir != exeDir && game.GameInFolder(new DirectoryInfo(exeDir)))
            {
                log.InfoFormat("{0} found at {1}", game.ShortName, exeDir);
                return exeDir;
            }

            return null;
        }

        /// <summary>
        /// Detects the version of a game in a given directory.
        /// </summary>
        private GameVersion DetectVersion(string directory)
        {
            GameVersion version = game.DetectVersion(new DirectoryInfo(directory));
            if (version != null)
            {
                log.DebugFormat("Found version {0}", version);
            }
            return version;
        }

        #endregion

        #region Things which would be better as Properties

        public string GameDir()
            => gameDir;

        public string CkanDir()
        {
            if (!Valid)
            {
                log.Error("Could not find game version");
                throw new NotKSPDirKraken(gameDir, Properties.Resources.GameInstanceVersionNotFound);
            }
            return CKANPathUtils.NormalizePath(
                Path.Combine(GameDir(), "CKAN"));
        }

        public string DownloadCacheDir()
            => CKANPathUtils.NormalizePath(Path.Combine(CkanDir(), "downloads"));

        public string InstallHistoryDir()
            => CKANPathUtils.NormalizePath(Path.Combine(CkanDir(), "history"));

        public IOrderedEnumerable<FileInfo> InstallHistoryFiles()
            => Directory.EnumerateFiles(InstallHistoryDir(), "*.ckan")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.CreationTime);

        public GameVersion Version()
        {
            if (version == null)
            {
                version = DetectVersion(GameDir());
            }
            return version;
        }

        public GameVersionCriteria VersionCriteria()
            => new GameVersionCriteria(Version(), _compatibleVersions);

        #endregion

        /// <summary>
        /// Returns path relative to this KSP's GameDir.
        /// </summary>
        public string ToRelativeGameDir(string path)
            => CKANPathUtils.ToRelative(path, GameDir());

        /// <summary>
        /// Given a path relative to this KSP's GameDir, returns the
        /// absolute path on the system.
        /// </summary>
        public string ToAbsoluteGameDir(string path)
            => CKANPathUtils.ToAbsolute(path, GameDir());

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
        /// <param name="relative_path">Path of the DLL relative to game root</param>
        /// <returns>
        /// Identifier if found otherwise null
        /// </returns>
        public string DllPathToIdentifier(string relative_path)
        {
            var paths = Enumerable.Repeat(game.PrimaryModDirectoryRelative, 1)
                                  .Concat(game.AlternateModDirectoriesRelative);
            if (!paths.Any(p => relative_path.StartsWith($"{p}/", Platform.PathComparison)))
            {
                // DLLs only live in the primary or alternate mod directories
                return null;
            }
            Match match = dllPattern.Match(relative_path);
            return match.Success
                ? Identifier.Sanitize(match.Groups["identifier"].Value)
                : null;
        }

        /// <summary>
        /// Generate a sequence of files in the game folder that weren't installed by CKAN
        /// </summary>
        /// <param name="registry">A Registry object that knows which files CKAN installed in this folder</param>
        /// <returns>Relative file paths as strings</returns>
        public IEnumerable<string> UnmanagedFiles(Registry registry)
            => Directory.EnumerateFiles(gameDir, "*", SearchOption.AllDirectories)
                        .Select(CKANPathUtils.NormalizePath)
                        .Where(absPath => !absPath.StartsWith(CkanDir()))
                        .Select(ToRelativeGameDir)
                        .Where(relPath =>
                            !game.StockFolders.Any(f => relPath.StartsWith($"{f}/"))
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

        public void PlayGame(string command, Action onExit = null)
        {
            var split = (command ?? "").Split(' ');
            if (split.Length == 0)
            {
                return;
            }

            split = game.AdjustCommandLine(split, Version());
            var binary = split[0];
            var args = string.Join(" ", split.Skip(1));

            try
            {
                Directory.SetCurrentDirectory(GameDir());
                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = binary,
                        Arguments = args
                    },
                    EnableRaisingEvents = true
                };

                p.Exited += (sender, e) =>
                {
                    playTime.Stop(CkanDir());
                    onExit?.Invoke();
                };

                p.Start();
                playTime.Start();
            }
            catch (Exception exception)
            {
                User.RaiseError(Properties.Resources.GameInstancePlayGameFailed, exception.Message);
            }
        }

        public override string ToString()
            => string.Format(Properties.Resources.GameInstanceToString, game.ShortName, gameDir);

        public bool Equals(GameInstance other)
            => other != null && gameDir.Equals(other.GameDir());

        public override bool Equals(object obj)
            => Equals(obj as GameInstance);

        public override int GetHashCode()
            => gameDir.GetHashCode();
    }

}
