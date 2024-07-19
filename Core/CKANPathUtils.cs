using System;
using System.IO;
using System.Text.RegularExpressions;

using log4net;

using CKAN.Extensions;

namespace CKAN
{
    public static class CKANPathUtils
    {
        /// <summary>
        /// Path to save CKAN data shared across all game instances
        /// </summary>
        public static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Meta.GetProductName());

        private static readonly ILog log = LogManager.GetLogger(typeof(CKANPathUtils));

        /// <summary>
        /// Normalizes the path by replacing all \ with / and removing any trailing slash.
        /// </summary>
        /// <param name="path">The path to normalize</param>
        /// <returns>The normalized path</returns>
        public static string NormalizePath(string path)
            => path == null    ? null
             : path.Length < 2 ? path.Replace('\\', '/')
             : path.Replace('\\', '/').TrimEnd('/');

        /// <summary>
        /// Gets the last path element. Ex: /a/b/c returns c
        /// </summary>
        /// <returns>The last path element.</returns>
        /// <param name="path">The path to process.</param>
        public static string GetLastPathElement(string path)
            => Regex.Replace(NormalizePath(path), @"^.*/", "");

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

            return Regex.IsMatch(path, "/")
                ? Regex.Replace(path, @"(^.*)/.+", "$1")
                : string.Empty;
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
                throw new PathErrorKraken(path, string.Format(
                    Properties.Resources.PathUtilsNotAbsolute, path));
            }

            if (!path.StartsWith(root, Platform.PathComparison))
            {
                throw new PathErrorKraken(path, string.Format(
                    Properties.Resources.PathUtilsNotInside, path, root));
            }

            // Strip off the root, then remove any slashes at the beginning
            return path.Remove(0, root.Length).TrimStart('/');
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
                    string.Format(Properties.Resources.PathUtilsAlreadyAbsolute, path)
                );
            }

            if (!Path.IsPathRooted(root))
            {
                throw new PathErrorKraken(
                    root,
                    string.Format(Properties.Resources.PathUtilsNotRoot, root)
                );
            }

            // Why normalise it AGAIN? Because Path.Combine can insert
            // the un-prettiest slashes.
            return NormalizePath(Path.Combine(root, path));
        }

        public static void CheckFreeSpace(DirectoryInfo where, long bytesToStore, string errorDescription)
        {
            if (bytesToStore > 0)
            {
                var bytesFree = where.GetDrive()?.AvailableFreeSpace;
                if (bytesFree.HasValue && bytesToStore > bytesFree.Value) {
                    throw new NotEnoughSpaceKraken(errorDescription, where,
                                                   bytesFree.Value, bytesToStore);
                }
                log.DebugFormat("Storing {0} to {1} ({2} free)...",
                                CkanModule.FmtSize(bytesToStore),
                                where.FullName,
                                bytesFree.HasValue ? CkanModule.FmtSize(bytesFree.Value)
                                                   : "unknown bytes");
            }
        }

    }
}
