using System;
using System.IO;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{
    public class KSPPathUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(KSPPathUtils));

        /// <summary>
        /// Finds Steam on the current machine.
        /// </summary>
        /// <returns>The path to Steam, or null if not found</returns>
        public static string SteamPath()
        {
            // First check the registry.

            const string reg_key = @"HKEY_CURRENT_USER\Software\Valve\Steam";
            const string reg_value = @"SteamPath";

            log.DebugFormat("Checking {0}\\{1} for Steam path", reg_key, reg_value);

            var steam = (string)Microsoft.Win32.Registry.GetValue(reg_key, reg_value, null);

            // If that directory exists, we've found Steam!
            if (steam != null && Directory.Exists(steam))
            {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            log.Debug("Couldn't find Steam via registry key, trying other locations...");

            // Not in the registry, or missing file, but that's cool. This should find it on Linux
            steam = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local",
                "share",
                "Steam"
            );

            log.DebugFormat("Looking for Steam in {0}", steam);

            if (Directory.Exists(steam))
            {
                log.InfoFormat("Found Steam at {0}", steam);
                return steam;
            }

            // Try an alternative path.
            steam = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".steam",
                "steam"
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

        /// <summary>
        /// Finds the KSP path under a Steam Libary. Returns null if the folder cannot be located.
        /// </summary>
        /// <param name="steamPath">Steam Libary Path</param>
        /// <returns>The KSP path.</returns>
        public static string KSPDirectory(string steamPath)
        {
            // There are several possibilities for the path under Linux.
            // Try with the uppercase version.
            string installPath = Path.Combine(steamPath, "SteamApps", "common", "Kerbal Space Program");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }

            // Try with the lowercase version.
            installPath = Path.Combine(steamPath, "steamapps", "common", "Kerbal Space Program");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }

            return null;

        }

        /// <summary>
        /// Finds the Steam KSP path. Returns null if the folder cannot be located.
        /// </summary>
        /// <returns>The KSP path.</returns>
        public static string KSPSteamPath()
        {
            // Attempt to get the Steam path.
            string steamPath = SteamPath();

            if (steamPath == null)
            {
                return null;
            }

            //Default steam libary
            string installPath = KSPDirectory(steamPath);
            if(installPath != null)
            {
                return installPath;
            }

            //Attempt to find through config file
            string configPath = Path.Combine(steamPath, "config", "config.vdf");
            if (File.Exists(configPath))
            {
                log.InfoFormat("Found Steam config file at {0}", configPath);
                StreamReader reader = new StreamReader(configPath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Found Steam library
                    if (line.Contains("BaseInstallFolder"))
                    {

                        // This assumes config file is valid, we just skip it if it looks funny.
                        string[] split_line = line.Split('"');

                        if (split_line.Length > 3)
                        {
                            log.DebugFormat("Found a Steam Libary Location at {0}", split_line[3]);

                            installPath = KSPDirectory(split_line[3]);
                            if (installPath != null)
                            {
                                log.InfoFormat("Found a KSP install at {0}", installPath);
                                return installPath;
                            }
                        }
                    }
                }
            }

            // Could not locate the folder.
            return null;
        }

        /// <summary>
        /// Normalizes the path by replace any \ with / and removing any trailing slash.
        /// </summary>
        /// <returns>The normalized path.</returns>
        /// <param name="path">The path to normalize.</param>
        public static string NormalizePath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }

        /// <summary>
        /// Gets the last path element. Ex: /a/b/c returns c
        /// </summary>
        /// <returns>The last path element.</returns>
        /// <param name="path">The path to process.</param>
        public static string GetLastPathElement(string path)
        {
            return Regex.Replace(NormalizePath(path), @"^.*/", "");
        }

        /// <summary>
        /// Gets the leading path elements. Ex: /a/b/c returns /a/b
        ///
        /// Returns empty string if there is no leading path. (Eg: "Example.dll" -> "");
        /// </summary>
        /// <returns>The leading path elements.</returns>
        /// <param name="path">The path to process.</param>
        public static string GetLeadingPathElements(string path)
        {
            path = NormalizePath(path);

            if (Regex.IsMatch(path, "/"))
            {
                return Regex.Replace(path, @"(^.*)/.+", "$1");
            }
            return String.Empty;
        }

        /// <summary>
        /// Converts a path to one relative to the root provided.
        /// Please use KSP.ToRelative when working with gamedirs.
        /// Throws a PathErrorKraken if the path is not absolute, not inside the root,
        /// or either argument is null.
        /// </summary>
        public static string ToRelative(string path, string root)
        {
            if (path == null || root == null)
            {
                throw new PathErrorKraken(null, "Null path provided");
            }

            // We have to normalise before we check for rootedness,
            // otherwise backslash separators fail on Linux.

            path = NormalizePath(path);
            root = NormalizePath(root);

            if (!Path.IsPathRooted(path))
            {
                throw new PathErrorKraken(
                    path,
                    String.Format("{0} is not an absolute path", path)
                );
            }

            if (!path.StartsWith(root, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new PathErrorKraken(
                    path,
                    String.Format(
                        "Oh snap. {0} isn't inside {1}",
                        path, root
                    )
                );
            }

            // The +1 here is because root will never have
            // a trailing slash. However, if the strings are
            // the same, it causes an exception.
            return path.Length > root.Length
                ? path.Remove(0, root.Length + 1)
                : "";
        }

        /// <summary>
        /// Returns root/path, but checks that root is absolute,
        /// path is relative, and normalises everything for great justice.
        /// Please use KSP.ToAbsolute if converting from a KSP gamedir.
        /// Throws a PathErrorKraken if anything goes wrong.
        /// </summary>
        public static string ToAbsolute(string path, string root)
        {
            if (path == null || root == null)
            {
                throw new PathErrorKraken(null, "Null path provided");
            }

            path = NormalizePath(path);
            root = NormalizePath(root);

            if (Path.IsPathRooted(path))
            {
                throw new PathErrorKraken(
                    path,
                    String.Format("{0} is already absolute", path)
                );
            }

            if (!Path.IsPathRooted(root))
            {
                throw new PathErrorKraken(
                    root,
                    String.Format("{0} isn't an absolute root", root)
                );
            }

            // Why normalise it AGAIN? Because Path.Combine can insert
            // the un-prettiest slashes.
            return NormalizePath(Path.Combine(root, path));
        }
    }
}
