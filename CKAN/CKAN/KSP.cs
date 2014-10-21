using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{

    public class KSPPathConstants
    {
        public const string CKAN_KEY = @"HKEY_CURRENT_USER\Software\CKAN";
        public const string CKAN_GAMEDIR_VALUE = @"GameDir";
        public const string CKAN_INSTANCES_COUNT_VALUE = @"InstancesCount";
        public static readonly string steamKSP = Path.Combine("SteamApps", "common", "Kerbal Space Program");

        public static void SetRegistryValue<T>(string key, T value)
        {
            Microsoft.Win32.Registry.SetValue(KSPPathConstants.CKAN_KEY, key, value);
        }

        public static T GetRegistryValue<T>(string key, T defaultValue)
        {
            return (T)Microsoft.Win32.Registry.GetValue(KSPPathConstants.CKAN_KEY, key, defaultValue);
        }

    }

    public class KSPPathUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(KSPPathUtils));

        /// <summary>
        ///     Finds Steam on the current machine.
        /// </summary>
        /// <returns>The path to steam, or null if not found</returns>
        public static string SteamPath()
        {
            // First check the registry.

            string reg_key = @"HKEY_CURRENT_USER\Software\Valve\Steam";
            string reg_value = @"SteamPath";

            log.DebugFormat("Checking {0}\\{1} for Steam path", reg_key, reg_value);

            var steam = (string)Microsoft.Win32.Registry.GetValue(reg_key, reg_value, null);

            // If that directory exists, we've found steam!
            if (steam != null && Directory.Exists(steam))
            {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            log.Debug("Couldn't find Steam via registry key, trying other locations...");

            // Not in the registry, or missing file, but that's cool. This should find it on Linux

            steam = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".steam", "steam"
                );

            log.DebugFormat("Looking for Steam in {0}", steam);

            if (Directory.Exists(steam))
            {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            // Ok - Perhaps we're running OSX?

            steam = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Path.Combine("Library", "Application Support", "Steam")
                );

            log.DebugFormat("Looking for Steam in {0}", steam);

            if (Directory.Exists(steam))
            {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            log.Info("Steam not found on this system.");
            return null;
        }
    }

    /// <summary>
    ///     Everything for dealing with KSP itself.
    /// </summary>
    public class KSP
    {
        // Where to find KSP relative to Steam's root.
        private static readonly ILog log = LogManager.GetLogger(typeof(KSP));

        public static Dictionary<string, KSP> Instances = new Dictionary<string, KSP>();
        public static KSP CurrentInstance = null;
        public static string AutoStartInstance = null;

        public static void AddDefaultInstance()
        {
            Instances.Add("Auto-detected instance", new KSP());
        }

        public static void AddInstance(string name, string path)
        {
            var ksp = new KSP();
            ksp.SetGameDir(path);
            Instances.Add(name, ksp);
        }

        public static void InitializeInstance(string name)
        {
            if (!Instances.ContainsKey(name))
            {
                throw new InvalidKSPInstanceException();
            }

            CurrentInstance = Instances[name];
            Instances[name].Init();
        }

        private string cached_gamedir;
        private KSPVersion cached_version;

        public string GameDir()
        {
            // Return cached if found.
            if (cached_gamedir != null)
            {
                return cached_gamedir;
            }

            // Go find and cache it.
            return cached_gamedir = FindGameDir();
        }

        // This can be called to set our GameDir directly.
        // It's primary use it cmdline argument switches.
        public void SetGameDir(string directory)
        {
            if (cached_gamedir != null)
            {
                // Changing gamedir may result in inconsistencies,
                // so we don't allow it.

                log.FatalFormat("Attempt to change gamedir from {0} to {1}", cached_gamedir, directory);
                throw new InvalidOperationException();
            }

            // Verify that we *actually* have a KSP install
            // TODO: Have a better test than just GameData presence.

            if (!IsKspDir(directory))
            {
                log.FatalFormat("Cannot find GameData in {0}", directory);
                throw new DirectoryNotFoundException();
            }

            // All good. Set our gamedir for this session.
            log.InfoFormat("Setting KSP dir to {0} by explicit request", directory);
            cached_gamedir = directory;
        }

        private string FindGameDir()
        {
            // See if KSP is in the same dir as we're installed (GH #23)

            // Find the directory our executable is stored in.
            // In Perl, this is just `use FindBin qw($Bin);` Verbose enough, C#?
            string exe_dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            log.DebugFormat("Checking if KSP is in my exe dir: {0}", exe_dir);

            // Checking for a GameData directory probably isn't the best way to
            // detect KSP, but it works. More robust implementations welcome.
            if (Directory.Exists(Path.Combine(exe_dir, "GameData")))
            {
                log.InfoFormat("KSP found at {0}", exe_dir);
                return exe_dir;
            }

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
        
        public static void LoadInstancesFromRegistry()
        {
            Instances.Clear();

            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\CKAN");
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\CKAN");
            }

            AutoStartInstance = KSPPathConstants.GetRegistryValue(@"KSPAutoStartInstance", "");
            var instanceCount = KSPPathConstants.GetRegistryValue(@"KSPInstanceCount", 0);
         
            for (int i = 0; i < instanceCount; i++)
            {
                var name = KSPPathConstants.GetRegistryValue(@"KSPInstanceName_" + i, "");
                var path = KSPPathConstants.GetRegistryValue(@"KSPInstancePath_" + i, "");

                var ksp = new KSP();
                ksp.SetGameDir(path);
                Instances.Add(name, ksp);
            }
        }

        public static void PopulateRegistryWithInstances()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\CKAN");
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\CKAN");
            }

            KSPPathConstants.SetRegistryValue(@"KSPAutoStartInstance", AutoStartInstance);
            KSPPathConstants.SetRegistryValue(@"KSPInstanceCount", Instances.Count);

            int i = 0;
            foreach (var instance in Instances)
            {
                var name = instance.Key;
                var ksp = instance.Value;

                KSPPathConstants.SetRegistryValue(@"KSPInstanceName_" + i, name);
                KSPPathConstants.SetRegistryValue(@"KSPInstancePath_" + i, ksp.GameDir());
                
                i++;
            }
        }

        // Returns true if we have what looks like a KSP dir.
        private bool IsKspDir(string directory)
        {
            if (!Directory.Exists(Path.Combine(directory, "GameData")))
            {
                log.FatalFormat("Cannot find GameData in {0}", directory);
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

        /// <summary>
        ///     Create the CKAN directory and any supporting files.
        /// </summary>
        public void Init()
        {
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

            if (!Directory.Exists(FilesystemTransaction.TempPath))
            {
                Directory.CreateDirectory(FilesystemTransaction.TempPath);
            }
            else
            {
                var directory = new DirectoryInfo(FilesystemTransaction.TempPath);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
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
            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            // Forget that we've seen any DLLs, as we're going to refresh them all.
            registry.ClearDlls();

            // TODO: It would be great to optimise this to skip .git directories and the like.
            // Yes, I keep my GameData in git.

            string[] dllFiles = Directory.GetFiles(GameData(), "*.dll", SearchOption.AllDirectories);

            foreach (string file in dllFiles)
            {
                // register_dll does the heavy lifting of turning it into a modname
                registry.RegisterDll(file);
            }

            registry_manager.Save();
        }

        public KSPVersion Version()
        {
            if (cached_version != null)
            {
                return cached_version;
            }

            return cached_version = DetectVersion(GameDir());
        }

        private KSPVersion DetectVersion(string path)
        {
            // Slurp our README into memory
            string readme = File.ReadAllText(Path.Combine(path, "readme.txt"));

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

    public class InvalidKSPInstanceException : Exception
    {
    }

}