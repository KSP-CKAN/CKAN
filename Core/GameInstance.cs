using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Transactions;
using ChinhDo.Transactions.FileManager;
using log4net;
using Newtonsoft.Json;

using CKAN.Games;
using CKAN.Extensions;
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

        public GUIConfiguration configuration;

        public string Name { get; set; }
        /// <summary>
        /// Returns a file system safe version of the instance name that can be used within file names.
        /// </summary>
        public string SanitizedName => string.Join("", Name.Split(Path.GetInvalidFileNameChars()));
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
                configuration = GUIConfiguration.LoadOrCreateConfiguration(Path.Combine(CkanDir(), "GUIConfig.xml"));
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
        private void SetupCkanDirectories(bool scan = true)
        {
            log.InfoFormat("Initialising {0}", CkanDir());

            // TxFileManager knows if we are in a transaction
            TxFileManager txFileMgr = new TxFileManager();

            if (!Directory.Exists(CkanDir()))
            {
                User.RaiseMessage(Properties.Resources.GameInstanceSettingUp);
                User.RaiseMessage(Properties.Resources.GameInstanceCreatingDir, CkanDir());
                txFileMgr.CreateDirectory(CkanDir());

                if (scan)
                {
                    User.RaiseMessage(Properties.Resources.GameInstanceScanning);
                    Scan();
                }
            }

            playTime = TimeLog.Load(TimeLog.GetPath(CkanDir())) ?? new TimeLog();

            if (!Directory.Exists(InstallHistoryDir()))
            {
                User.RaiseMessage(Properties.Resources.GameInstanceCreatingDir, InstallHistoryDir());
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

        #endregion

        #region Settings

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
            get
            {
                return File.Exists(InstallFiltersFile)
                    ? JsonConvert.DeserializeObject<string[]>(File.ReadAllText(InstallFiltersFile))
                    : new string[] { };
            }

            set
            {
                File.WriteAllText(InstallFiltersFile, JsonConvert.SerializeObject(value));
            }
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
                log.Error("Could not find game version");
                throw new NotKSPDirKraken(gameDir, Properties.Resources.GameInstanceVersionNotFound);
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

        public void LaunchGame(IUser user, Func<string, string, Tuple<bool, bool>> launchAnyWay)
        {
            string[] arguments = configuration.CommandLineArguments.Split(' ');

            var registry = RegistryManager.Instance(this).registry;

            var suppressedIdentifiers = this.GetSuppressedCompatWarningIdentifiers;
            var incomp = registry.IncompatibleInstalled(this.VersionCriteria())
                .Where(m => !m.Module.IsDLC && !suppressedIdentifiers.Contains(m.identifier))
                .ToList();
            if (incomp.Any())
            {
                // Warn that it might not be safe to run Game with incompatible modules installed
                string incompatDescrip = incomp
                    .Select(m => $"{m.Module} ({registry.CompatibleGameVersions(this.game, m.Module)})")
                    .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");
                var ver = this.Version();
                
                // Need to internationalize this
                var result = launchAnyWay(
                    string.Format("Some installed modules are incompatible! It might not be safe to launch the game. Really launch?\n\n{0}", incompatDescrip),
                    string.Format("Don't show this again for these mods on {0} {1}",
                        this.game.ShortName,
                        new GameVersion(ver.Major, ver.Minor, ver.Patch))
                );
                
                if (!result.Item1)
                {
                    return;
                }
                else if (result.Item2)
                {
                    this.AddSuppressedCompatWarningIdentifiers(
                        incomp.Select(m => m.identifier).ToHashSet()
                    );
                }
            }

            arguments = this.game.AdjustCommandLine(arguments, this.Version());
            var binary = arguments[0];
            var args = string.Join(" ", arguments.Skip(1));

            try
            {
                Directory.SetCurrentDirectory(this.GameDir());

                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = binary,
                        Arguments = args
                    },
                    EnableRaisingEvents = true
                };

                p.Exited += (sender, e) => this.playTime.Stop(this.CkanDir());

                p.Start();
                this.playTime.Start();
            }
            catch (Exception exception)
            {
                // Need to internationalize this.
                user.RaiseError("Couldn't start game. \n\n {0}", exception.Message);
            }
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
            if (Directory.Exists(game.PrimaryModDirectory(this)))
            {
                var manager = RegistryManager.Instance(this);
                using (TransactionScope tx = CkanTransaction.CreateTransactionScope())
                {
                    log.DebugFormat("Scanning for DLLs in {0}",
                        game.PrimaryModDirectory(this));
                    var oldDlls = manager.registry.InstalledDlls.ToHashSet();
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
                        .Where(file => file.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        .Select(CKANPathUtils.NormalizePath)
                        .Where(absPath => !game.StockFolders.Any(f =>
                            ToRelativeGameDir(absPath).StartsWith($"{f}/")));

                    foreach (string dll in files)
                    {
                        manager.registry.RegisterDll(this, dll);
                    }
                    var newDlls = manager.registry.InstalledDlls.ToHashSet();
                    bool dllChanged = !oldDlls.SetEquals(newDlls);
                    bool dlcChanged = manager.ScanDlc();

                    if (dllChanged || dlcChanged)
                    {
                        manager.Save(false);
                    }

                    log.Debug("Scan completed, committing transaction");
                    tx.Complete();

                    return dllChanged || dlcChanged;
                }
            }
            return false;
        }

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
            if (!relative_path.StartsWith($"{game.PrimaryModDirectoryRelative}/", StringComparison.CurrentCultureIgnoreCase))
            {
                // DLLs only live in the primary mod directory
                return null;
            }
            Match match = dllPattern.Match(relative_path);
            return match.Success
                ? Identifier.Sanitize(match.Groups["identifier"].Value)
                : null;
        }

        public override string ToString()
        {
            return string.Format(Properties.Resources.GameInstanceToString, game.ShortName, gameDir);
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
