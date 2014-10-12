namespace CKAN {

    using System;
    using System.IO;
    using Microsoft.Win32;
    using System.Text.RegularExpressions;
    using log4net;

    /// <summary>
    /// Everything for dealing with KSP itself.
    /// </summary>

    public class KSP {

        // Where to find KSP relative to Steam's root.
        static readonly string steamKSP = Path.Combine( "SteamApps", "common", "Kerbal Space Program" );

        static readonly ILog log = LogManager.GetLogger (typeof(KSP));

        static string cached_gamedir = null;
        static string cached_version = null;

        private const string CKAN_KEY = @"HKEY_CURRENT_USER\Software\CKAN";
        private const string CKAN_GAMEDIR_VALUE = @"GameDir";


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
            if (steam != null && Directory.Exists (steam)) {
                log.InfoFormat ("Found Steam at {0}", steam);
                return steam;
            }

            log.Debug ("Couldn't find Steam via registry key, trying other locations...");

            // Not in the registry, or missing file, but that's cool. This should find it on Linux

            steam = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                ".steam", "steam"
            );

            log.DebugFormat ("Looking for Steam in {0}", steam);

            if (Directory.Exists (steam)) {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            // Ok - Perhaps we're running OSX?

            steam = Path.Combine (
                Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                Path.Combine("Library", "Application Support", "Steam")
            );

            log.DebugFormat ("Looking for Steam in {0}", steam);

            if (Directory.Exists (steam)) {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            log.Info ("Steam not found on this system.");
            return null;
        }

        public static string GameDir() {

            // Return cached if found.
            if (cached_gamedir != null) {
                return cached_gamedir;
            }

            // Go find and cache it.
            return cached_gamedir = FindGameDir ();
        }

        // This can be called to set our GameDir directly.
        // It's primary use it cmdline argument switches.
        public static void SetGameDir(string directory) {
            if (cached_gamedir != null) {
                // Changing gamedir may result in inconsistencies,
                // so we don't allow it.

                log.FatalFormat ("Attempt to change gamedir from {0} to {1}", cached_gamedir, directory);
                throw new InvalidOperationException ();
            }

            // Verify that we *actually* have a KSP install
            // TODO: Have a better test than just GameData presence.

            if (!IsKspDir(directory)) {
                log.FatalFormat ("Cannot find GameData in {0}", directory);
                throw new DirectoryNotFoundException ();
            }

            // All good. Set our gamedir for this session.
            log.InfoFormat ("Setting KSP dir to {0} by explicit request", directory);
            cached_gamedir = directory;
        }

        private static string FindGameDir() {

            // See if KSP is in the same dir as we're installed (GH #23)

            // Find the directory our executable is stored in.
            // In Perl, this is just `use FindBin qw($Bin);` Verbose enough, C#?
            string exe_dir = System.IO.Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly().Location);

            log.DebugFormat ("Checking if KSP is in my exe dir: {0}", exe_dir);

            // Checking for a GameData directory probably isn't the best way to
            // detect KSP, but it works. More robust implementations welcome.
            if (Directory.Exists (Path.Combine (exe_dir, "GameData"))) {
                log.InfoFormat ("KSP found at {0}", exe_dir);
                return exe_dir;
            }

            // Check the registry, maybe it's there.
            string registry_dir = FindGamedirRegistry ();

            if (registry_dir != null) {
                return registry_dir;
            }

            // See if we can find KSP as part of a Steam install.

            string steam = SteamPath ();
            if (steam != null) {
                string ksp_dir = Path.Combine (steam, steamKSP);

                if (Directory.Exists (ksp_dir)) {
                    log.InfoFormat ("KSP found at {0}", ksp_dir);
                    return ksp_dir;
                }

                log.DebugFormat("Have Steam, but KSP is not at {0}", ksp_dir);

            }

            // Oh noes! We can't find KSP!

            throw new DirectoryNotFoundException ();

        }

        private static string FindGamedirRegistry() {
            // Check the Windows/Mono registry (GH #28)

            log.DebugFormat ("Checking {0}\\{1} for KSP path", CKAN_KEY, CKAN_GAMEDIR_VALUE);

            string ksp_dir = (string)Microsoft.Win32.Registry.GetValue (CKAN_KEY, CKAN_GAMEDIR_VALUE, null);

            if (ksp_dir != null) {
                log.DebugFormat ("Found KSP dir in {0} via registry", ksp_dir);
                return ksp_dir;
            }

            return null;
        }

        public static void PopulateGamedirRegistry(string gamedir = null) {

            if (gamedir == null) {
                log.Debug ("Registering default gamedir in registry.");
                gamedir = GameDir ();
            }

            if (! IsKspDir (gamedir)) {
                throw new DirectoryNotFoundException ();
            }

            log.DebugFormat ("Registering KSP {0}\\{1} as {2}", CKAN_KEY, CKAN_GAMEDIR_VALUE, gamedir);

            Microsoft.Win32.Registry.SetValue (CKAN_KEY, CKAN_GAMEDIR_VALUE, gamedir);
        }

        // Returns true if we have what looks like a KSP dir.
        private static bool IsKspDir(string directory) {
            if (!Directory.Exists (Path.Combine (directory, "GameData"))) {
                log.FatalFormat ("Cannot find GameData in {0}", directory);
                return false;
            }
            log.DebugFormat ("{0} looks like a GameDir", directory);
            return true;
        }
    
        public static string GameData() {
            return Path.Combine (GameDir (), "GameData");
        }

        public static string CkanDir() {
            return Path.Combine (GameDir (), "CKAN");
        }

        public static string DownloadCacheDir() {
            return Path.Combine (CkanDir (), "downloads");
        }

        public static string Ships() {
            return Path.Combine (GameDir (), "Ships");
        }

        /// <summary>
        /// Create the CKAN directory and any supporting files.
        /// </summary>
        public static void Init() {
            if (! Directory.Exists (CkanDir ())) {
                User.WriteLine ("Setting up CKAN for the first time...");
                User.WriteLine ("Creating {0}", CkanDir ());
                Directory.CreateDirectory (CkanDir ());

                User.WriteLine ("Scanning for installed mods...");
                ScanGameData ();
            }

            if (! Directory.Exists( DownloadCacheDir() )) {
                User.WriteLine ("Creating {0}", DownloadCacheDir ());
                Directory.CreateDirectory (DownloadCacheDir ());
            }

            // If we've got no game in the registry, then store this one.
            // If we *do* have a game there, don't touch it.

            if (FindGamedirRegistry () == null) {
                PopulateGamedirRegistry ();
            }

        }

        public static void CleanCache() {

            log.Debug ("Cleaning cahce directory");

            string[] files = Directory.GetFiles (DownloadCacheDir (), "*", SearchOption.AllDirectories);

            foreach (string file in files) {

                if (Directory.Exists (file)) {
                    log.DebugFormat ("Skipping directory: {0}", file);
                    continue;
                }

                log.DebugFormat ("Deleting {0}", file);
                File.Delete (file);
            }

            return;
        }

        public static void ScanGameData() {

            RegistryManager registry_manager = RegistryManager.Instance();
            Registry registry = registry_manager.registry;

            // Forget that we've seen any DLLs, as we're going to refresh them all.
            registry.ClearDlls ();

            // TODO: It would be great to optimise this to skip .git directories and the like.
            // Yes, I keep my GameData in git.

            string[] dllFiles = Directory.GetFiles (GameData(), "*.dll", SearchOption.AllDirectories);

            foreach (string file in dllFiles) {
                // register_dll does the heavy lifting of turning it into a modname
                registry.RegisterDll (file);
            }

            registry_manager.Save();
        }

        public static string Version() {

            if (cached_version != null) {
                return cached_version;
            }

            return cached_version = DetectVersion (GameDir ());
        }

        private static string DetectVersion(string path) {

            // Slurp our README into memory
            string readme = File.ReadAllText(Path.Combine (path, "readme.txt"));

            // And find the KSP version. Easy! :)
            Match match = Regex.Match (readme, @"^Version\s+(\d+\.\d+\.\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (match.Success) {
                string version = match.Groups [1].Value;
                log.DebugFormat ("Found version {0}", version);
                return version;
            }

            // Oh noes! We couldn't find the version!
            // (Suggestions for better exceptions welcome!)
            log.Error ("Could not find KSP version in readme.txt");
            throw new BadVersionException ();
        }
    }

    public class BadVersionException : Exception { }
}
