using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ChinhDo.Transactions.FileManager;
using log4net;

using CKAN.IO;
using CKAN.Extensions;

namespace CKAN
{
    public static class Utilities
    {
        public static readonly string[] AvailableLanguages =
        {
            "en-GB",
            "en-US",
            "de-DE",
            "zh-CN",
            "fr-FR",
            "pt-BR",
            "ru-RU",
            "ja-JP",
            "ko-KR",
            "pl-PL",
            "tr-TR",
            "it-IT",
            "nl-NL",
        };

        /// <summary>
        /// Call a function and take a default action if it throws an exception
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Function to call</param>
        /// <param name="onThrow">Function to call if an exception is thrown</param>
        /// <returns>Return value of the function</returns>
        public static T? DefaultIfThrows<T>(Func<T?>             func,
                                            Func<Exception, T?>? onThrow = null) where T : class
        {
            try
            {
                return func();
            }
            catch (Exception exc)
            {
                return onThrow?.Invoke(exc) ?? default;
            }
        }

        /// <summary>
        /// Extract and rethrow the (first) inner exception if an aggregate exception is thrown
        /// </summary>
        /// <param name="func">Function to call</param>
        /// <returns>Return value of func, unless an exception is thrown</returns>
        public static T WithRethrowInner<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (AggregateException agExc)
            {
                agExc.RethrowInner();
                throw;
            }
        }

        /// <summary>
        /// Copies a directory and its subdirectories as a transaction
        /// </summary>
        /// <param name="sourceDirPath">Source directory path</param>
        /// <param name="destDirPath">Destination directory path</param>
        /// <param name="subFolderRelPathsToSymlink">Relative subdirs that should be symlinked to the originals instead of copied</param>
        /// <param name="subFolderRelPathsToLeaveEmpty">Relative subdirs that should be left empty</param>
        public static void CopyDirectory(string   sourceDirPath,
                                         string   destDirPath,
                                         string[] subFolderRelPathsToSymlink,
                                         string[] subFolderRelPathsToLeaveEmpty)
        {
            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                CopyDirectory(sourceDirPath, destDirPath, new TxFileManager(),
                              subFolderRelPathsToSymlink, subFolderRelPathsToLeaveEmpty);
                transaction.Complete();
            }
        }

        private static void CopyDirectory(string        sourceDirPath,
                                          string        destDirPath,
                                          TxFileManager txFileMgr,
                                          string[]      subFolderRelPathsToSymlink,
                                          string[]      subFolderRelPathsToLeaveEmpty)
        {
            var sourceDir = new DirectoryInfo(sourceDirPath);
            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundKraken(
                    sourceDirPath,
                    $"Source directory {sourceDirPath} does not exist or could not be found.");
            }

            // If the destination directory doesn't exist, create it
            if (!Directory.Exists(destDirPath))
            {
                txFileMgr.CreateDirectory(destDirPath);
            }
            else if (Directory.GetDirectories(destDirPath).Length != 0 || Directory.GetFiles(destDirPath).Length != 0)
            {
                throw new PathErrorKraken(destDirPath, $"Directory not empty: {destDirPath}");
            }

            // Get the files in the directory and copy them to the new location
            foreach (var file in sourceDir.GetFiles())
            {
                if (file.Name is "registry.locked" or "playtime.json")
                {
                    continue;
                }
                InstalledFilesDeduplicator.CreateOrCopy(file,
                                                        Path.Combine(destDirPath, file.Name),
                                                        txFileMgr);
            }

            // Create all first level subdirectories
            foreach (var subdir in sourceDir.GetDirectories())
            {
                var temppath = Path.Combine(destDirPath, subdir.Name);
                // If already a sym link, replicate it in the new location
                if (DirectoryLink.TryGetTarget(subdir.FullName, out string? existingLinkTarget)
                    && existingLinkTarget is not null)
                {
                    DirectoryLink.Create(existingLinkTarget, temppath, txFileMgr);
                }
                else
                {
                    if (subFolderRelPathsToSymlink.Contains(subdir.Name, Platform.PathComparer))
                    {
                        DirectoryLink.Create(subdir.FullName, temppath, txFileMgr);
                    }
                    else
                    {
                        txFileMgr.CreateDirectory(temppath);

                        if (!subFolderRelPathsToLeaveEmpty.Contains(subdir.Name, Platform.PathComparer))
                        {
                            // Copy subdir contents to new location
                            CopyDirectory(subdir.FullName, temppath, txFileMgr,
                                          SubPaths(subdir.Name, subFolderRelPathsToSymlink).ToArray(),
                                          SubPaths(subdir.Name, subFolderRelPathsToLeaveEmpty).ToArray());
                        }
                    }
                }
            }
        }

        // Select only paths within subdir, prune prefixes
        private static IEnumerable<string> SubPaths(string parent, string[] paths)
            => paths.Where(p => p.StartsWith($"{parent}/", Platform.PathComparison))
                    .Select(p => p[(parent.Length + 1)..]);

        /// <summary>
        /// Launch a URL. For YEARS this was done by Process.Start in a
        /// cross-platform way, but Microsoft chose to break that,
        /// so now every .NET app has to write its own custom code for it,
        /// with special code for each platform.
        /// https://github.com/dotnet/corefx/issues/10361
        /// </summary>
        /// <param name="url">URL to launch</param>
        /// <returns>
        /// true if launched, false otherwise
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool ProcessStartURL(string url)
        {
            try
            {
                if (Platform.IsMac)
                {
                    Process.Start("open", $"\"{url}\"");
                    return true;
                }
                else if (Platform.IsUnix)
                {
                    Process.Start("xdg-open", $"\"{url}\"");
                    return true;
                }
                else
                {
                    // Try the old way
                    Process.Start(new ProcessStartInfo(url)
                    {
                        UseShellExecute = true,
                        Verb            = "open"
                    });
                    return true;
                }
            }
            catch (Exception exc)
            {
                log.Error($"Exception for URL {url}", exc);
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        public static void OpenFileBrowser(string location)
        {
            // We need the folder of the file
            // Otherwise the OS would try to open the file in its default application
            if (DirPath(location) is string path)
            {
                ProcessStartURL(path);
            }
        }

        private static string? DirPath(string path)
            => Directory.Exists(path) ? path
             : File.Exists(path) && Path.GetDirectoryName(path) is string parent
                                 && Directory.Exists(parent)
                 ? parent
             : null;

        private static readonly ILog log = LogManager.GetLogger(typeof(Utilities));
    }
}
