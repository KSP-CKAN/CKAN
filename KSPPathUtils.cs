using System;
using System.IO;
using log4net;
using System.Text.RegularExpressions;

namespace CKAN
{
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
    }
}

