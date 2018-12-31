using System;
using System.IO;

namespace CKAN
{
    public static class Utilities
    {
        /// <summary>
        /// Copies a directory and optionally its supdirectories.
        /// </summary>
        /// <param name="sourceDirPath">Source directory path.</param>
        /// <param name="destDirPath">Destination directory path.</param>
        /// <param name="copySubDirs">Copy sub dirs recursively if set to <c>true</c>.</param>
        public static void CopyDirectory(string sourceDirPath, string destDirPath, bool copySubDirs)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirPath);

            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirPath);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }
            else if (Directory.GetDirectories(sourceDirPath).Length != 0 || Directory.GetFiles(sourceDirPath).Length != 0)
            {
                throw new IOException("Directory not empty: "+ sourceDirPath);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = sourceDir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirPath, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo[] dirs = sourceDir.GetDirectories();

                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirPath, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
