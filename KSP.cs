using System;
using System.IO;
using Microsoft.Win32;
using log4net;

/// <summary>
/// Everything for dealing with KSP itself.
/// </summary>

namespace CKAN {
    public class KSP {

        // Where to find KSP relative to Steam's root.
        static readonly string steamKSP = Path.Combine( "SteamApps", "common", "Kerbal Space Program" );

        static readonly ILog log = LogManager.GetLogger (typeof(KSP));

        static string cached_gamedir = null;

        /// <summary>
        /// Finds Steam on the current machine.
        /// </summary>
        /// <returns>The path to steam, or null if not found</returns>
        static string SteamPath() {
            // First check the registry.

            string reg_key = @"HKEY_CURRENT_USER\Software\Valve\Steam";
            string reg_value = @"SteamPath";

            log.DebugFormat ("Checking {0}\\{1} for Steam path", reg_key, reg_value);

            string steam = (string) Microsoft.Win32.Registry.GetValue(reg_key, reg_value, null);

            // If that directory exists, we've found steam!
            if (steam != null && FileSystem.IsDirectory(steam)) {
                log.InfoFormat ("Found Steam at {0}", steam);
                return steam;
            }

            log.Debug ("Couldn't find Steam via registry key, trying other locations...");

            // Not in the registry, or missing file, but that's cool. This should find it on Linux/OSX

            steam = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                ".steam", "steam"
            );

            log.DebugFormat ("Looking for Steam in {0}", steam);

            if (FileSystem.IsDirectory (steam)) {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            log.Info ("Steam not found on this system.");
            return null;
        }

        public static string gameDir() {

            if (cached_gamedir != null) {
                return cached_gamedir;
            }

            // TODO: See if KSP was specified on the command line.

            // See if KSP is in the same dir as we're installed (GH #23)

            // Find the directory our executable is stored in.
            // In Perl, this is just `use FindBin qw($Bin);` Verbose enough, C#?
            string exe_dir = System.IO.Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly().Location);

            log.DebugFormat ("Checking if KSP is in my exe dir: {0}", exe_dir);

            // Checking for a GameData directory probably isn't the best way to
            // detect KSP, but it works. More robust implementations welcome.
            if (FileSystem.IsDirectory (Path.Combine (exe_dir, "GameData"))) {
                log.InfoFormat ("KSP found at {0}", exe_dir);
                return cached_gamedir = exe_dir;
            }

            // TODO: See if we've got it cached in the registry.

            // See if we can find KSP as part of a Steam install.

            string steam = SteamPath ();
            if (steam != null) {
                string ksp_dir = Path.Combine (steam, steamKSP);

                if (FileSystem.IsDirectory(ksp_dir)) {
                    log.InfoFormat ("KSP found at {0}", ksp_dir);
                    return cached_gamedir = ksp_dir;
                }

                log.DebugFormat("Have Steam, but KSP is not at {0}", ksp_dir);

            }

            // Oh noes! We can't find KSP!

            throw new DirectoryNotFoundException ();

        }
    
        public static string gameData() {
            return Path.Combine (gameDir (), "GameData");
        }

        public static string ckanDir() {
            return Path.Combine (gameDir (), "CKAN");
        }

        public static string downloadCacheDir() {
            return Path.Combine (ckanDir (), "downloads");
        }

        public static string ships() {
            return Path.Combine (gameDir (), "Ships");
        }

        /// <summary>
        /// Create the CKAN directory and any supporting files.
        /// </summary>
        public static void init() {
            if (! Directory.Exists (ckanDir ())) {
                Console.WriteLine ("Setting up CKAN for the first time...");
                Console.WriteLine ("Creating {0}", ckanDir ());
                Directory.CreateDirectory (ckanDir ());

                Console.WriteLine ("Scanning for installed mods...");
                scanGameData ();
            }

            if (! Directory.Exists( downloadCacheDir() )) {
                Console.WriteLine ("Creating {0}", downloadCacheDir ());
                Directory.CreateDirectory (downloadCacheDir ());
            }
        }

        public static void scanGameData() {

            // TODO: Get rid of magic paths!
            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            // Forget that we've seen any DLLs, as we're going to refresh them all.
            registry.clear_dlls ();

            // TODO: It would be great to optimise this to skip .git directories and the like.
            // Yes, I keep my GameData in git.

            string[] dllFiles = Directory.GetFiles (gameData(), "*.dll", SearchOption.AllDirectories);

            foreach (string file in dllFiles) {
                // register_dll does the heavy lifting of turning it into a modname
                registry.register_dll (file);
            }

            registry_manager.save();
        }
    }
}