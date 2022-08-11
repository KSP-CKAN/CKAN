using System;
using System.IO;
using System.Diagnostics;
using System.Transactions;
using ChinhDo.Transactions.FileManager;
using log4net;

namespace CKAN
{
    public static class Utilities
    {
        public static readonly string[] AvailableLanguages =
        {
            "en-GB", "en-US", "de-DE", "zh-CN", "fr-FR", "pt-BR", "ru-RU", "ja-JP", "ko-KR"
        };

        /// <summary>
        /// Copies a directory and optionally its subdirectories as a transaction.
        /// </summary>
        /// <param name="sourceDirPath">Source directory path.</param>
        /// <param name="destDirPath">Destination directory path.</param>
        /// <param name="copySubDirs">Copy sub dirs recursively if set to <c>true</c>.</param>
        public static void CopyDirectory(string sourceDirPath, string destDirPath, bool copySubDirs)
        {
            TxFileManager file_transaction = new TxFileManager();
            using (TransactionScope transaction = CkanTransaction.CreateTransactionScope())
            {
                _CopyDirectory(sourceDirPath, destDirPath, copySubDirs, file_transaction);
                transaction.Complete();
            }
        }


        private static void _CopyDirectory(string sourceDirPath, string destDirPath, bool copySubDirs, TxFileManager file_transaction)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirPath);

            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundKraken(
                    sourceDirPath,
                    "Source directory does not exist or could not be found.");
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirPath))
            {
                file_transaction.CreateDirectory(destDirPath);
            }
            else if (Directory.GetDirectories(destDirPath).Length != 0 || Directory.GetFiles(destDirPath).Length != 0)
            {
                throw new PathErrorKraken(destDirPath, "Directory not empty: ");
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = sourceDir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name == "registry.locked")
                {
                    continue;
                }

                else if (file.Name == "playtime.json")
                {
                    continue;
                }

                string temppath = Path.Combine(destDirPath, file.Name);
                file_transaction.Copy(file.FullName, temppath, false);
            }

            // Create all first level subdirectories
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirPath, subdir.Name);
                file_transaction.CreateDirectory(temppath);

                // If copying subdirectories, copy their contents to new location.
                if (copySubDirs)
                {
                    _CopyDirectory(subdir.FullName, temppath, copySubDirs, file_transaction);
                }
            }
        }

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
