using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

using Autofac;
using ChinhDo.Transactions.FileManager;
using log4net;

using CKAN.Versioning;
using CKAN.Configuration;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Extensions;
using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;

namespace CKAN
{
    /// <summary>
    /// Manage multiple KSP installs.
    /// </summary>
    public class GameInstanceManager : IDisposable
    {
        /// <summary>
        /// An IUser object for user interaction.
        /// It is initialized during the startup with a ConsoleUser,
        /// do not use in functions that could be called by the GUI.
        /// </summary>
        public IUser User { get; set; }
        public IConfiguration Configuration { get; set; }
        public GameInstance CurrentInstance { get; set; }

        public NetModuleCache Cache { get; private set; }

        public readonly SteamLibrary SteamLibrary = new SteamLibrary();

        private static readonly ILog log = LogManager.GetLogger(typeof (GameInstanceManager));

        private readonly SortedList<string, GameInstance> instances = new SortedList<string, GameInstance>();

        public string[] AllInstanceAnchorFiles => KnownGames.knownGames
            .SelectMany(g => g.InstanceAnchorFiles)
            .Distinct()
            .ToArray();

        public string AutoStartInstance
        {
            get => HasInstance(Configuration.AutoStartInstance)
                ? Configuration.AutoStartInstance
                : null;
            private set
            {
                if (!string.IsNullOrEmpty(value) && !HasInstance(value))
                {
                    throw new InvalidKSPInstanceKraken(value);
                }
                Configuration.AutoStartInstance = value;
            }
        }

        public SortedList<string, GameInstance> Instances => new SortedList<string, GameInstance>(instances);

        public GameInstanceManager(IUser user, IConfiguration configuration = null)
        {
            User = user;
            Configuration = configuration ?? ServiceLocator.Container.Resolve<IConfiguration>();
            LoadInstances();
            LoadCacheSettings();
        }

        /// <summary>
        /// Returns the preferred game instance, or null if none can be found.
        ///
        /// This works by checking to see if we're in a KSP dir first, then the
        /// config for an autostart instance, then will try to auto-populate
        /// by scanning for the game.
        ///
        /// This *will not* touch the config if we find a portable install.
        ///
        /// This *will* run KSP instance autodetection if the config is empty.
        ///
        /// This *will* set the current instance, or throw an exception if it's already set.
        ///
        /// Returns null if we have multiple instances, but none of them are preferred.
        /// </summary>
        public GameInstance GetPreferredInstance()
        {
            CurrentInstance = _GetPreferredInstance();
            return CurrentInstance;
        }

        // Actual worker for GetPreferredInstance()
        internal GameInstance _GetPreferredInstance()
        {
            foreach (IGame game in KnownGames.knownGames)
            {
                // TODO: Check which ones match, prompt user if >1

                // First check if we're part of a portable install
                // Note that this *does not* register in the config.
                string path = GameInstance.PortableDir(game);

                if (path != null)
                {
                    GameInstance portableInst = new GameInstance(
                        game, path, Properties.Resources.GameInstanceManagerPortable, User);
                    if (portableInst.Valid)
                    {
                        return portableInst;
                    }
                }
            }

            // If we only know of a single instance, return that.
            if (instances.Count == 1 && instances.First().Value.Valid)
            {
                return instances.First().Value;
            }

            // Return the autostart, if we can find it.
            // We check both null and "" as we can't write NULL to the config, so we write an empty string instead
            // This is necessary so we can indicate that the user wants to reset the current AutoStartInstance without clearing the config!
            if (!string.IsNullOrEmpty(AutoStartInstance)
                    && instances[AutoStartInstance].Valid)
            {
                return instances[AutoStartInstance];
            }

            // If we know of no instances, try to find one.
            // Otherwise, we know of too many instances!
            // We don't know which one to pick, so we return null.
            return !instances.Any() ? FindAndRegisterDefaultInstances() : null;
        }

        /// <summary>
        /// Find and register default instances by running
        /// game autodetection code. Registers one per known game,
        /// uses first found as default.
        ///
        /// Returns the resulting game instance if found.
        /// </summary>
        public GameInstance FindAndRegisterDefaultInstances()
        {
            if (instances.Any())
            {
                throw new KSPManagerKraken("Attempted to scan for defaults with instances");
            }
            var found = FindDefaultInstances();
            foreach (var inst in found)
            {
                log.DebugFormat("Registering {0} at {1}...",
                                inst.Name, inst.GameDir());
                AddInstance(inst);
            }
            return found.FirstOrDefault();
        }

        public GameInstance[] FindDefaultInstances()
        {
            var found = KnownGames.knownGames.SelectMany(g =>
                            SteamLibrary.Games
                                        .Select(sg => new { name = sg.Name, dir = sg.GameDir })
                                        .Append(new
                                                {
                                                    name = string.Format(Properties.Resources.GameInstanceManagerAuto,
                                                                         g.ShortName),
                                                    dir  = g.MacPath(),
                                                })
                                        .Where(obj => obj.dir != null && g.GameInFolder(obj.dir))
                                        .Select(obj => new GameInstance(g, obj.dir.FullName, obj.name, User)))
                                  .Where(inst => inst.Valid)
                                  .ToArray();
            foreach (var group in found.GroupBy(inst => inst.Name))
            {
                if (group.Count() > 1)
                {
                    // Make sure the names are unique
                    int index = 0;
                    foreach (var inst in group)
                    {
                        // Find an unused name
                        string name;
                        do
                        {
                            ++index;
                            name = $"{group.Key} ({++index})";
                        }
                        while (found.Any(other => other.Name == name));
                        inst.Name = name;
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// Adds a game instance to config.
        /// </summary>
        /// <returns>The resulting GameInstance object</returns>
        /// <exception cref="NotKSPDirKraken">Thrown if the instance is not a valid game instance.</exception>
        public GameInstance AddInstance(GameInstance instance)
        {
            if (instance.Valid)
            {
                string name = instance.Name;
                instances.Add(name, instance);
                Configuration.SetRegistryToInstances(instances);
            }
            else
            {
                throw new NotKSPDirKraken(instance.GameDir());
            }
            return instance;
        }

        /// <summary>
        /// Adds a game instance to config.
        /// </summary>
        /// <param name="path">The path of the instance</param>
        /// <param name="name">The name of the instance</param>
        /// <param name="user">IUser object for interaction</param>
        /// <returns>The resulting GameInstance object</returns>
        /// <exception cref="NotKSPDirKraken">Thrown if the instance is not a valid game instance.</exception>
        public GameInstance AddInstance(string path, string name, IUser user)
        {
            var game = DetermineGame(new DirectoryInfo(path), user);
            return game == null ? null : AddInstance(new GameInstance(game, path, name, user));
        }

        /// <summary>
        /// Clones an existing KSP installation.
        /// </summary>
        /// <param name="existingInstance">The KSP instance to clone.</param>
        /// <param name="newName">The name for the new instance.</param>
        /// <param name="newPath">The path where the new instance should be located.</param>
        /// <exception cref="InstanceNameTakenKraken">Thrown if the instance name is already in use.</exception>
        /// <exception cref="NotKSPDirKraken">Thrown by AddInstance() if created instance is not valid, e.g. if something went wrong with copying.</exception>
        /// <exception cref="DirectoryNotFoundKraken">Thrown by CopyDirectory() if directory doesn't exist. Should never be thrown here.</exception>
        /// <exception cref="PathErrorKraken">Thrown by CopyDirectory() if the target folder already exists and is not empty.</exception>
        /// <exception cref="IOException">Thrown by CopyDirectory() if something goes wrong during the process.</exception>
        public void CloneInstance(GameInstance existingInstance,
                                  string       newName,
                                  string       newPath,
                                  bool         shareStockFolders = false)
        {
            if (HasInstance(newName))
            {
                throw new InstanceNameTakenKraken(newName);
            }
            if (!existingInstance.Valid)
            {
                throw new NotKSPDirKraken(existingInstance.GameDir(), string.Format(
                    Properties.Resources.GameInstanceCloneInvalid, existingInstance.game.ShortName));
            }

            log.Debug("Copying directory.");
            Utilities.CopyDirectory(existingInstance.GameDir(), newPath,
                                    shareStockFolders ? existingInstance.game.StockFolders
                                                      : Array.Empty<string>(),
                                    existingInstance.game.LeaveEmptyInClones);

            // Add the new instance to the config
            AddInstance(new GameInstance(existingInstance.game, newPath, newName, User));
        }

        /// <summary>
        /// Create a new fake KSP instance
        /// </summary>
        /// <param name="game">The game of the new instance.</param>
        /// <param name="newName">The name for the new instance.</param>
        /// <param name="newPath">The location of the new instance.</param>
        /// <param name="version">The version of the new instance. Should have a build number.</param>
        /// <param name="dlcs">The IDlcDetector implementations for the DLCs that should be faked and the requested dlc version as a dictionary.</param>
        /// <exception cref="InstanceNameTakenKraken">Thrown if the instance name is already in use.</exception>
        /// <exception cref="NotKSPDirKraken">Thrown by AddInstance() if created instance is not valid, e.g. if a write operation didn't complete for whatever reason.</exception>
        public void FakeInstance(IGame game, string newName, string newPath, GameVersion version,
                                 Dictionary<DLC.IDlcDetector, GameVersion> dlcs = null)
        {
            TxFileManager fileMgr = new TxFileManager();
            using (TransactionScope transaction = CkanTransaction.CreateTransactionScope())
            {
                if (HasInstance(newName))
                {
                    throw new InstanceNameTakenKraken(newName);
                }

                if (!version.InBuildMap(game))
                {
                    throw new BadGameVersionKraken(string.Format(
                        Properties.Resources.GameInstanceFakeBadVersion, game.ShortName, version));
                }
                if (Directory.Exists(newPath) && (Directory.GetFiles(newPath).Length != 0 || Directory.GetDirectories(newPath).Length != 0))
                {
                    throw new BadInstallLocationKraken(Properties.Resources.GameInstanceFakeNotEmpty);
                }

                log.DebugFormat("Creating folder structure and text files at {0} for {1} version {2}", Path.GetFullPath(newPath), game.ShortName, version.ToString());

                // Create a KSP root directory, containing a GameData folder, a buildID.txt/buildID64.txt and a readme.txt
                fileMgr.CreateDirectory(newPath);
                fileMgr.CreateDirectory(Path.Combine(newPath, game.PrimaryModDirectoryRelative));
                game.RebuildSubdirectories(newPath);

                foreach (var anchor in game.InstanceAnchorFiles)
                {
                    fileMgr.WriteAllText(Path.Combine(newPath, anchor),
                                         version.WithoutBuild.ToString());
                }

                // Don't write the buildID.txts if we have no build, otherwise it would be -1.
                if (version.IsBuildDefined && game is KerbalSpaceProgram)
                {
                    foreach (var b in KspBuildIdVersionProvider.buildIDfilenames)
                    {
                        fileMgr.WriteAllText(Path.Combine(newPath, b),
                                             string.Format("build id = {0}", version.Build));
                    }
                }

                // Create the readme.txt WITHOUT build number
                fileMgr.WriteAllText(Path.Combine(newPath, "readme.txt"),
                                     string.Format("Version {0}",
                                                   version.WithoutBuild.ToString()));

                // Create the needed folder structure and the readme.txt for DLCs that should be simulated.
                if (dlcs != null)
                {
                    foreach (KeyValuePair<DLC.IDlcDetector, GameVersion> dlc in dlcs)
                    {
                        DLC.IDlcDetector dlcDetector = dlc.Key;
                        GameVersion dlcVersion = dlc.Value;

                        if (!dlcDetector.AllowedOnBaseVersion(version))
                        {
                            throw new WrongGameVersionKraken(
                                version,
                                string.Format(Properties.Resources.GameInstanceFakeDLCNotAllowed,
                                    game.ShortName,
                                    dlcDetector.ReleaseGameVersion,
                                    dlcDetector.IdentifierBaseName));
                        }

                        string dlcDir = Path.Combine(newPath, dlcDetector.InstallPath());
                        fileMgr.CreateDirectory(dlcDir);
                        fileMgr.WriteAllText(
                            Path.Combine(dlcDir, "readme.txt"),
                            string.Format("Version {0}", dlcVersion));
                    }
                }

                // Add the new instance to the config
                GameInstance new_instance = new GameInstance(game, newPath, newName, User);
                AddInstance(new_instance);
                transaction.Complete();
            }
        }

        /// <summary>
        /// Given a string returns a unused valid instance name by postfixing the string
        /// </summary>
        /// <returns> A unused valid instance name.</returns>
        /// <param name="name">The name to use as a base.</param>
        /// <exception cref="Kraken">Could not find a valid name.</exception>
        public string GetNextValidInstanceName(string name)
        {
            // Check if the current name is valid
            if (InstanceNameIsValid(name))
            {
                return name;
            }

            // Try appending a number to the name
            var validName = Enumerable.Repeat(name, 1000)
                .Select((s, i) => s + " (" + i + ")")
                .FirstOrDefault(InstanceNameIsValid);
            if (validName != null)
            {
                return validName;
            }

            // Check if a name with the current timestamp is valid
            validName = name + " (" + DateTime.Now + ")";

            if (InstanceNameIsValid(validName))
            {
                return validName;
            }

            // Give up
            throw new Kraken(Properties.Resources.GameInstanceNoValidName);
        }

        /// <summary>
        /// Check if the instance name is valid.
        /// </summary>
        /// <returns><c>true</c>, if name is valid, <c>false</c> otherwise.</returns>
        /// <param name="name">Name to check.</param>
        private bool InstanceNameIsValid(string name)
        {
            // Discard null, empty strings and white space only strings.
            // Look for the current name in the list of loaded instances.
            return !string.IsNullOrWhiteSpace(name) && !HasInstance(name);
        }

        /// <summary>
        /// Removes the instance from the config and saves.
        /// </summary>
        public void RemoveInstance(string name)
        {
            instances.Remove(name);
            Configuration.SetRegistryToInstances(instances);
        }

        /// <summary>
        /// Renames an instance in the config and saves.
        /// </summary>
        public void RenameInstance(string from, string to)
        {
            // TODO: What should we do if our target name already exists?
            GameInstance ksp = instances[from];
            instances.Remove(from);
            ksp.Name = to;
            instances.Add(to, ksp);
            Configuration.SetRegistryToInstances(instances);
        }

        /// <summary>
        /// Sets the current instance.
        /// Throws an InvalidKSPInstanceKraken if not found.
        /// </summary>
        public void SetCurrentInstance(string name)
        {
            if (!HasInstance(name))
            {
                throw new InvalidKSPInstanceKraken(name);
            }
            else if (!instances[name].Valid)
            {
                throw new NotKSPDirKraken(instances[name].GameDir());
            }

            // Don't try to Dispose a null CurrentInstance.
            if (CurrentInstance != null && !CurrentInstance.Equals(instances[name]))
            {
                // Dispose of the old registry manager to release the registry
                // (without accidentally locking/loading/etc it).
                RegistryManager.DisposeInstance(CurrentInstance);
            }
            CurrentInstance = instances[name];
        }

        public void SetCurrentInstanceByPath(string path)
        {
            var matchingGames = KnownGames.knownGames
                .Where(g => g.GameInFolder(new DirectoryInfo(path)))
                .ToList();
            switch (matchingGames.Count)
            {
                case 0:
                    throw new NotKSPDirKraken(path);

                case 1:
                    GameInstance ksp = new GameInstance(
                        matchingGames.First(), path, Properties.Resources.GameInstanceByPathName, User);
                    if (ksp.Valid)
                    {
                        CurrentInstance = ksp;
                    }
                    else
                    {
                        throw new NotKSPDirKraken(ksp.GameDir());
                    }
                    break;

                default:
                    // TODO: Prompt user to choose
                    break;
            }
        }

        public GameInstance InstanceAt(string path)
        {
            var matchingGames = KnownGames.knownGames
                .Where(g => g.GameInFolder(new DirectoryInfo(path)))
                .ToList();
            switch (matchingGames.Count)
            {
                case 0:
                    return null;

                case 1:
                    return new GameInstance(
                        matchingGames.First(), path, Properties.Resources.GameInstanceByPathName, User);

                default:
                    // TODO: Prompt user to choose
                    return null;

            }
        }

        /// <summary>
        /// Sets the autostart instance in the config and saves it.
        /// </summary>
        public void SetAutoStart(string name)
        {
            if (!HasInstance(name))
            {
                throw new InvalidKSPInstanceKraken(name);
            }
            else if (!instances[name].Valid)
            {
                throw new NotKSPDirKraken(instances[name].GameDir());
            }
            AutoStartInstance = name;
        }

        public bool HasInstance(string name)
            => instances.ContainsKey(name);

        public void ClearAutoStart()
        {
            Configuration.AutoStartInstance = null;
        }

        private void LoadInstances()
        {
            log.Info("Loading game instances");

            instances.Clear();

            foreach (Tuple<string, string, string> instance in Configuration.GetInstances())
            {
                var name = instance.Item1;
                var path = instance.Item2;
                var gameName = instance.Item3;
                try
                {
                    var game = KnownGames.knownGames.FirstOrDefault(g => g.ShortName == gameName)
                        ?? KnownGames.knownGames[0];
                    log.DebugFormat("Loading {0} from {1}", name, path);
                    // Add unconditionally, sort out invalid instances downstream
                    instances.Add(name, new GameInstance(game, path, name, User));
                }
                catch (Exception exc)
                {
                    // Skip malformed instances (e.g. empty path)
                    log.Error($"Failed to load game instance with name=\"{name}\" path=\"{path}\" game=\"{gameName}\"",
                        exc);
                }
            }
        }

        private void LoadCacheSettings()
        {
            if (!Directory.Exists(Configuration.DownloadCacheDir))
            {
                try
                {
                    Directory.CreateDirectory(Configuration.DownloadCacheDir);
                }
                catch
                {
                    // Can't create the configured directory, try reverting it to the default
                    Configuration.DownloadCacheDir = null;
                    Directory.CreateDirectory(Configuration.DownloadCacheDir);
                }
            }

            if (!TrySetupCache(Configuration.DownloadCacheDir, out string failReason))
            {
                log.ErrorFormat("Cache not found at configured path {0}: {1}", Configuration.DownloadCacheDir, failReason);
                // Fall back to default path to minimize chance of ending up in an invalid state at startup
                TrySetupCache("", out _);
            }
        }

        /// <summary>
        /// Switch to using a download cache in a new location
        /// </summary>
        /// <param name="path">Location of folder for new cache</param>
        /// <param name="failureReason">Contains a human readable failure message if the setup failed</param>
        /// <returns>
        /// true if successful, false otherwise
        /// </returns>
        public bool TrySetupCache(string path, out string failureReason)
        {
            string origPath = Configuration.DownloadCacheDir;
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Configuration.DownloadCacheDir = "";
                    Cache = new NetModuleCache(this, Configuration.DownloadCacheDir);
                }
                else
                {
                    // Make sure we can access it
                    var bytesFree = new DirectoryInfo(path).GetDrive()?.AvailableFreeSpace;
                    Cache = new NetModuleCache(this, path);
                    Configuration.DownloadCacheDir = path;
                }
                failureReason = null;
                return true;
            }
            catch (DirectoryNotFoundKraken)
            {
                Configuration.DownloadCacheDir = origPath;
                failureReason = string.Format(Properties.Resources.GameInstancePathNotFound, path);
                return false;
            }
            catch (Exception ex)
            {
                Configuration.DownloadCacheDir = origPath;
                failureReason = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="GameInstance"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="GameInstance"/>. The <see cref="Dispose"/>
        /// method leaves the <see cref="GameInstance"/> in an unusable state. After calling <see cref="Dispose"/>, you must
        /// release all references to the <see cref="GameInstance"/> so the garbage collector can reclaim the memory that
        /// the <see cref="GameInstance"/> was occupying.</remarks>
        public void Dispose()
        {
            if (Cache != null)
            {
                Cache.Dispose();
                Cache = null;
            }

            // Attempting to dispose of the related RegistryManager object here is a bad idea, it cause loads of failures
        }

        public static bool IsGameInstanceDir(DirectoryInfo path)
            => KnownGames.knownGames.Any(g => g.GameInFolder(path));

        /// <summary>
        /// Tries to determine the game that is installed at the given path
        /// </summary>
        /// <param name="path">A DirectoryInfo of the path to check</param>
        /// <param name="user">IUser object for interaction</param>
        /// <returns>An instance of the matching game or null if the user cancelled</returns>
        /// <exception cref="NotKSPDirKraken">Thrown when no games found</exception>
        public IGame DetermineGame(DirectoryInfo path, IUser user)
        {
            var matchingGames = KnownGames.knownGames.Where(g => g.GameInFolder(path)).ToList();
            switch (matchingGames.Count)
            {
                case 0:
                    throw new NotKSPDirKraken(path.FullName);

                case 1:
                    return matchingGames.First();

                default:
                    // Prompt user to choose
                    int selection = user.RaiseSelectionDialog(
                        string.Format(Properties.Resources.GameInstanceManagerSelectGamePrompt,
                                      Platform.FormatPath(path.FullName)),
                        matchingGames.Select(g => g.ShortName).ToArray());
                    return selection >= 0 ? matchingGames[selection] : null;
            }
        }
    }
}
