using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{
    /// <summary>
    ///     Everything for dealing with KSP itself.
    /// </summary>
    public class KSP
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(KSP));

        private string gamedir;
        private KSPVersion version;
        private NetFileCache _Cache;
        public NetFileCache Cache
        {
            get
            {
                return _Cache;
            }
        }

        public KSP(string directory)
        {
            if (! IsKspDir(directory))
            {
                throw new NotKSPDirKraken(directory);
            }
            
            gamedir = directory;
            Init();
            _Cache = new NetFileCache(DownloadCacheDir());
        }

        public string GameDir()
        {
            return gamedir;
        }

        /// <summary>
        /// Returns the path to our portable version of KSP if ckan.exe is in the same
        /// directory as the game. Otherwise, returns null.
        /// </summary>
        public static string PortableDir()
        {
            // Find the directory our executable is stored in.
            // In Perl, this is just `use FindBin qw($Bin);` Verbose enough, C#?
            string exe_dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            log.DebugFormat("Checking if KSP is in my exe dir: {0}", exe_dir);

            // Checking for a GameData directory probably isn't the best way to
            // detect KSP, but it works. More robust implementations welcome.
            if (IsKspDir(exe_dir))
            {
                log.InfoFormat("KSP found at {0}", exe_dir);
                return exe_dir;
            }

            return null;
        }

        public static string FindGameDir()
        {

            // See if we can find KSP as part of a Steam install.

            string steam = KSPPathUtils.SteamPath();
            if (steam != null)
            {
                string ksp_dir = Path.Combine(steam, KSPPathConstants.steamKSP);

                if (Directory.Exists(ksp_dir))
                {
                    log.InfoFormat("KSP found at {0}", ksp_dir);
                    return ksp_dir;
                }

                log.DebugFormat("Have Steam, but KSP is not at {0}", ksp_dir);
            }

            // Oh noes! We can't find KSP!

            throw new DirectoryNotFoundException();
        }
        


        /// <summary>
        /// Checks if the specified directory looks like a KSP directory.
        /// Returns true if found, false if not.
        /// </summary>
        internal static bool IsKspDir(string directory)
        {
            //first we need to check is directory exists
            if (!Directory.Exists(Path.Combine(directory, "GameData")))
            {
                log.DebugFormat("Cannot find GameData in {0}", directory);
                return false;
            }
            //next we should be able to get game version
            try
            {
                DetectVersion(directory);
            }
            catch
            {
                log.DebugFormat("Cannot detect KSP version in {0}", directory);
                return false;
            }
            log.DebugFormat("{0} looks like a GameDir", directory);
            return true;
        }

        public string GameData()
        {
            return Path.Combine(GameDir(), "GameData");
        }

        public string CkanDir()
        {
            return Path.Combine(GameDir(), "CKAN");
        }

        public string DownloadCacheDir()
        {
            return Path.Combine(CkanDir(), "downloads");
        }

        public string Ships()
        {
            return Path.Combine(GameDir(), "Ships");
        }

        public string Tutorial()
        {
            return Path.Combine(GameDir(), "saves", "training");
        }

        public string TempDir()
        {
            return Path.Combine(CkanDir(), "temp");
        }

        /// <summary>
        ///     Create the CKAN directory and any supporting files.
        /// </summary>
        public void Init()
        {
            log.DebugFormat("Initialising {0}", CkanDir());

            if (! Directory.Exists(CkanDir()))
            {
                User.WriteLine("Setting up CKAN for the first time...");
                User.WriteLine("Creating {0}", CkanDir());
                Directory.CreateDirectory(CkanDir());

                User.WriteLine("Scanning for installed mods...");
                ScanGameData();
            }

            if (! Directory.Exists(DownloadCacheDir()))
            {
                User.WriteLine("Creating {0}", DownloadCacheDir());
                Directory.CreateDirectory(DownloadCacheDir());
            }

            // Clear any temporary files we find. If the directory
            // doesn't exist, then no sweat; FilesystemTransaction
            // will auto-create it as needed.
            // Create our temporary directories, or clear them if they
            // already exist.
            if (Directory.Exists(TempDir()))
            {
                var directory = new DirectoryInfo(TempDir());
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }

            log.DebugFormat("Initialised {0}", CkanDir());
        }

        public void CleanCache()
        {
            log.Debug("Cleaning cahce directory");

            string[] files = Directory.GetFiles(DownloadCacheDir(), "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    log.DebugFormat("Skipping directory: {0}", file);
                    continue;
                }

                log.DebugFormat("Deleting {0}", file);
                File.Delete(file);
            }
        }

        public void ScanGameData()
        {
            RegistryManager registry_manager = RegistryManager.Instance(CkanDir());
            Registry registry = registry_manager.registry;

            // Forget that we've seen any DLLs, as we're going to refresh them all.
            registry.ClearDlls();

            // TODO: It would be great to optimise this to skip .git directories and the like.
            // Yes, I keep my GameData in git.

            string[] dllFiles = Directory.GetFiles(GameData(), "*.dll", SearchOption.AllDirectories);

            foreach (string file in dllFiles)
            {
                var fixedPath = file.Replace('\\', '/');
                // register_dll does the heavy lifting of turning it into a modname
                registry.RegisterDll(fixedPath);
            }

            registry_manager.Save();
        }

        public KSPVersion Version()
        {
            if (version != null)
            {
                return version;
            }

            return version = DetectVersion(GameDir());
        }

        internal static KSPVersion DetectVersion(string path)
        {
            string readme = "";
            try
            {
                // Slurp our README into memory
                readme = File.ReadAllText(Path.Combine(path, "readme.txt"));
            }
            catch
            {
                log.Error("Could not open KSP readme.txt");
                throw new BadVersionException();
            }

            // And find the KSP version. Easy! :)
            Match match = Regex.Match(readme, @"^Version\s+(\d+\.\d+\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (match.Success)
            {
                string version = match.Groups[1].Value;
                log.DebugFormat("Found version {0}", version);
                return new KSPVersion(version);
            }

            // Oh noes! We couldn't find the version!
            // (Suggestions for better exceptions welcome!)
            log.Error("Could not find KSP version in readme.txt");
            throw new BadVersionException();
        }
    }

    public class BadVersionException : Exception
    {
    }
   
}
