using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using ChinhDo.Transactions.FileManager;
using log4net;

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

        public static T DefaultIfThrows<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Copies a directory and its subdirectories as a transaction
        /// </summary>
        /// <param name="sourceDirPath">Source directory path</param>
        /// <param name="destDirPath">Destination directory path</param>
        /// <param name="subFolderRelPathsToSymlink">Relative subdirs that should be symlinked to the originals instead of copied</param>
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
                                          TxFileManager file_transaction,
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
                file_transaction.CreateDirectory(destDirPath);
            }
            else if (Directory.GetDirectories(destDirPath).Length != 0 || Directory.GetFiles(destDirPath).Length != 0)
            {
                throw new PathErrorKraken(destDirPath, "Directory not empty: ");
            }

            // Get the files in the directory and copy them to the new location
            foreach (var file in sourceDir.GetFiles())
            {
                if (file.Name == "registry.locked")
                {
                    continue;
                }

                else if (file.Name == "playtime.json")
                {
                    continue;
                }

                file_transaction.Copy(file.FullName, Path.Combine(destDirPath, file.Name), false);
            }

            // Create all first level subdirectories
            foreach (var subdir in sourceDir.GetDirectories())
            {
                var temppath = Path.Combine(destDirPath, subdir.Name);
                // If already a sym link, replicate it in the new location
                if (DirectoryLink.TryGetTarget(subdir.FullName, out string existingLinkTarget))
                {
                    DirectoryLink.Create(existingLinkTarget, temppath, file_transaction);
                }
                else
                {
                    if (subFolderRelPathsToSymlink.Contains(subdir.Name, Platform.PathComparer))
                    {
                        DirectoryLink.Create(subdir.FullName, temppath, file_transaction);
                    }
                    else
                    {
                        file_transaction.CreateDirectory(temppath);

                        if (!subFolderRelPathsToLeaveEmpty.Contains(subdir.Name, Platform.PathComparer))
                        {
                            // Copy subdir contents to new location
                            CopyDirectory(subdir.FullName, temppath, file_transaction,
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
                    .Select(p => p.Remove(0, parent.Length + 1));

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

        private static readonly ILog log = LogManager.GetLogger(typeof(Utilities));
    }
}
