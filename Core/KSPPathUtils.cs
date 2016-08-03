using log4net;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CKAN
{
    public class KSPPathUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KSPPathUtils));

        /// <summary>
        /// Finds the KSP path under a Steam Library. Returns null if the folder cannot be located.
        /// </summary>
        /// <param name="steam_path">Steam Library Path</param>
        /// <returns>The KSP path.</returns>
        public static string KSPDirectory(string steam_path)
        {
            // There are several possibilities for the path under Linux.
            // Try with the uppercase version.
            string ksp_path = Path.Combine(steam_path, "SteamApps", "common", "Kerbal Space Program");

            if (Directory.Exists(ksp_path))
            {
                return ksp_path;
            }

            // Try with the lowercase version.
            ksp_path = Path.Combine(steam_path, "steamapps", "common", "Kerbal Space Program");

            if (Directory.Exists(ksp_path))
            {
                return ksp_path;
            }

            return null;
        }

        private static string FindSteamPath()
        {
            using (var registry = new Win32Registry())
            {
                var result = registry.FindSteamPath();
                if (!String.IsNullOrEmpty(result))
                {
                    return result;
                }

                Log.Debug("Couldn't find Steam via registry key, trying other locations...");
            }
            // Not in the registry, or missing file, but that's cool. This should find it on Linux
            var steamPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local",
                "share",
                "Steam"
            );

            Log.DebugFormat("Looking for Steam in {0}", steamPath);

            if (Directory.Exists(steamPath))
            {
                Log.InfoFormat("Found Steam at {0}", steamPath);
                return steamPath;
            }

            // Try an alternative path.
            steamPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".steam",
                "steam"
            );

            Log.DebugFormat("Looking for Steam in {0}", steamPath);

            if (Directory.Exists(steamPath))
            {
                Log.InfoFormat("Found Steam at {0}", steamPath);
                return steamPath;
            }

            // Ok - Perhaps we're running OSX?
            steamPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Path.Combine("Library", "Application Support", "Steam")
                );

            Log.DebugFormat("Looking for Steam in {0}", steamPath);

            if (Directory.Exists(steamPath))
            {
                Log.InfoFormat("Found Steam at {0}", steamPath);
                return steamPath;
            }

            Log.Info("Steam not found on this system.");

            return null;
        }

        /// <summary>
        /// Finds the Steam KSP path. Returns null if the folder cannot be located.
        /// </summary>
        /// <returns>The KSP path.</returns>
        public static string KSPSteamPath()
        {
            string steamDir = null;
            using (var registry = new Win32Registry())
            {
                steamDir = registry.FindSteamPath();
                if (steamDir == null)
                {
                    return null;
                }
            }

            //Default steam library
            string installPath = KSPDirectory(steamDir);
            if (installPath != null)
            {
                return installPath;
            }

            //Attempt to find through config file
            string steamConfig = Path.Combine(steamDir, "config", "config.vdf");
            if (File.Exists(steamConfig))
            {
                Log.InfoFormat("Found Steam config file at {0}", steamConfig);
                var configData = File.ReadAllLines(steamConfig);
                var configInstall = configData.First((sought) => "BaseInstallFolder".Equals(sought));

                // TODO: .Split and direct array access is _fragile_
                // This assumes config file is valid, we just skip it if it looks funny.
                string[] splitLine = configInstall.Split('"');

                if (splitLine.Length > 3)
                {
                    Log.DebugFormat("Found a Steam Library Location at {0}", splitLine[3]);

                    installPath = KSPDirectory(splitLine[3]);
                    if (installPath != null)
                    {
                        Log.InfoFormat("Found a KSP install at {0}", installPath);
                        return installPath;
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

            if (path.Contains('/'))
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

            if (!path.StartsWith(root))
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
            // a trailing slash.
            return path.Remove(0, root.Length + 1);
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