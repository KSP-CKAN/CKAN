using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Transactions;
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

        public string Name { get; set; }
        public GameVersion GameVersionWhenCompatibleVersionsWereStored { get; private set; }
        public bool CompatibleVersionsAreFromDifferentGameVersion { get { return _compatibleVersions.Count > 0 && GameVersionWhenCompatibleVersionsWereStored != Version(); } }

        private static readonly ILog log = LogManager.GetLogger(typeof(GameInstance));

        #endregion

        #region Construction and Initialisation

        /// <summary>
        /// Returns a KSP object.
        /// Will initialise a CKAN instance in the KSP dir if it does not already exist,
        /// if the directory contains a valid KSP install.
        /// </summary>
        public GameInstance(IGame game, string gameDir, string name, IUser user, bool scan = true)
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
                SetupCkanDirectories(scan);
                LoadCompatibleVersions();
            }
        }

        public bool Valid
        {
            get
            {
                return game.GameInFolder(new DirectoryInfo(gameDir))
                    && Version() != null;
            }
        }

        /// <summary>
        /// Create the CKAN directory and any supporting files.
        /// </summary>
        private void SetupCkanDirectories(bool scan = true)
        {
            log.InfoFormat("Initialising {0}", CkanDir());

            // TxFileManager knows if we are in a transaction
            TxFileManager txFileMgr = new TxFileManager();

            if (!Directory.Exists(CkanDir()))
            {
                User.RaiseMessage("Setting up CKAN for the first time...");
                User.RaiseMessage("Creating {0}", CkanDir());
                txFileMgr.CreateDirectory(CkanDir());

                if (scan)
                {
                    User.RaiseMessage("Scanning for installed mods...");
                    Scan();
                }
            }

            if (!Directory.Exists(InstallHistoryDir()))
            {
                User.RaiseMessage("Creating {0}", InstallHistoryDir());
                txFileMgr.CreateDirectory(InstallHistoryDir());
            }

            // Clear any temporary files we find. If the directory
            // doesn't exist, then no sweat; FilesystemTransaction
            // will auto-create it as needed.
            // Create our temporary directories, or clear them if they
            // already exist.
            if (Directory.Exists(TempDir()))
            {
                var directory = new DirectoryInfo(TempDir());
                foreach (FileInfo file in directory.GetFiles())
                    txFileMgr.Delete(file.FullName);
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                    txFileMgr.DeleteDirectory(subDirectory.FullName);
            }
            log.InfoFormat("Initialised {0}", CkanDir());
        }

        public void SetCompatibleVersions(List<GameVersion> compatibleVersions)
        {
            this._compatibleVersions = compatibleVersions.Distinct().ToList();
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
            String path = CompatibleGameVersionsFile();
            if (File.Exists(path))
            {
                CompatibleGameVersions compatibleGameVersions = JsonConvert.DeserializeObject<CompatibleGameVersions>(File.ReadAllText(path));

                _compatibleVersions = compatibleGameVersions.Versions
                    .Select(v => GameVersion.Parse(v)).ToList();

                // Get version without throwing exceptions for null
                GameVersion mainVer = null;
                GameVersion.TryParse(compatibleGameVersions.GameVersionWhenWritten, out mainVer);
                GameVersionWhenCompatibleVersionsWereStored = mainVer;
            }
        }

        private string CompatibleGameVersionsFile()
        {
            return Path.Combine(CkanDir(), game.CompatibleVersionsFile);
        }

        public List<GameVersion> GetCompatibleVersions()
        {
            return new List<GameVersion>(this._compatibleVersions);
        }

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
        /// Attempts to automatically find a KSP install on this system.
        /// Returns the path to the install on success.
        /// Throws a DirectoryNotFoundException on failure.
        /// </summary>
        public static string FindGameDir(IGame game)
        {
            // See if we can find KSP as part of a Steam install.
            string gameSteamPath = game.SteamPath();
            if (gameSteamPath != null)
            {
                if (game.GameInFolder(new DirectoryInfo(gameSteamPath)))
                {
                    return gameSteamPath;
                }

                log.DebugFormat("Have Steam, but {0} is not at \"{1}\".",
                    game.ShortName, gameSteamPath);
            }

            // See if we can find a non-Steam Mac KSP install
            string kspMacPath = game.MacPath();
            if (kspMacPath != null)
            {
                if (game.GameInFolder(new DirectoryInfo(kspMacPath)))
                {
                    log.InfoFormat("Found a {0} install at {1}",
                        game.ShortName, kspMacPath);
                    return kspMacPath;
                }
                log.DebugFormat("Default Mac {0} folder exists at \"{1}\", but {0} is not installed there.",
                    game.ShortName, kspMacPath);
            }

            // Oh noes! We can't find KSP!
            throw new DirectoryNotFoundException();
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
        {
            return gameDir;
        }

        public string CkanDir()
        {
            if (!Valid)
            {
                log.Error("Could not find KSP version");
                throw new NotKSPDirKraken(gameDir, "Could not find KSP version in buildID.txt or readme.txt");
            }
            return CKANPathUtils.NormalizePath(
                Path.Combine(GameDir(), "CKAN"));
        }

        public string DownloadCacheDir()
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(CkanDir(), "downloads"));
        }

        public string InstallHistoryDir()
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(CkanDir(), "history")
            );
        }

        public string TempDir()
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(CkanDir(), "temp")
            );
        }

        public GameVersion Version()
        {
            if (version == null)
            {
                version = DetectVersion(GameDir());
            }
            return version;
        }

        public GameVersionCriteria VersionCriteria()
        {
            return new GameVersionCriteria(Version(), _compatibleVersions);
        }

        #endregion

        #region CKAN/GameData Directory Maintenance

        /// <summary>
        /// Clears the registry of DLL data, and refreshes it by scanning GameData.
        /// This operates as a transaction.
        /// This *saves* the registry upon completion.
        /// TODO: This would likely be better in the Registry class itself.
        /// </summary>
        /// <returns>
        /// True if found anything different, false if same as before
        /// </returns>
        public bool Scan()
        {
            var manager = RegistryManager.Instance(this);
            using (TransactionScope tx = CkanTransaction.CreateTransactionScope())
            {
                var oldDlls = new HashSet<string>(manager.registry.InstalledDlls);
                manager.registry.ClearDlls();

                // TODO: It would be great to optimise this to skip .git directories and the like.
                // Yes, I keep my GameData in git.

                // Alas, EnumerateFiles is *case-sensitive* in its pattern, which causes
                // DLL files to be missed under Linux; we have to pick .dll, .DLL, or scanning
                // GameData *twice*.
                //
                // The least evil is to walk it once, and filter it ourselves.
                IEnumerable<string> files = Directory
                    .EnumerateFiles(game.PrimaryModDirectory(this), "*", SearchOption.AllDirectories)
                    .Where(file => dllRegex.IsMatch(file))
                    .Select(CKANPathUtils.NormalizePath)
                    .Where(absPath => !game.StockFolders.Any(f =>
                        ToRelativeGameDir(absPath).StartsWith($"{f}/")));

                foreach (string dll in files)
                {
                    manager.registry.RegisterDll(this, dll);
                }
                var newDlls = new HashSet<string>(manager.registry.InstalledDlls);
                bool dllChanged = !oldDlls.SetEquals(newDlls);
                bool dlcChanged = manager.ScanDlc();

                if (dllChanged || dlcChanged)
                {
                    manager.Save(false);
                }

                tx.Complete();

                return dllChanged || dlcChanged;
            }
        }

        private static readonly Regex dllRegex = new Regex(@"\.dll$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// Returns path relative to this KSP's GameDir.
        /// </summary>
        public string ToRelativeGameDir(string path)
        {
            return CKANPathUtils.ToRelative(path, GameDir());
        }

        /// <summary>
        /// Given a path relative to this KSP's GameDir, returns the
        /// absolute path on the system.
        /// </summary>
        public string ToAbsoluteGameDir(string path)
        {
            return CKANPathUtils.ToAbsolute(path, GameDir());
        }

        public override string ToString()
        {
            return $"{game.ShortName} Install: {gameDir}";
        }

        public bool Equals(GameInstance other)
        {
            return other != null && gameDir.Equals(other.GameDir());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameInstance);
        }

        public override int GetHashCode()
        {
            return gameDir.GetHashCode();
        }
    }

}
