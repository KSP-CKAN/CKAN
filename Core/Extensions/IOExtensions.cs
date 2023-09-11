using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace CKAN.Extensions
{
    public static class IOExtensions
    {
        private static bool StringArrayStartsWith(string[] child, string[] parent)
        {
            if (parent.Length > child.Length)
            {
                // Only child is allowed to have extra pieces
                return false;
            }
            var opt = Platform.IsWindows ? StringComparison.InvariantCultureIgnoreCase
                                         : StringComparison.InvariantCulture;
            for (int i = 0; i < parent.Length; ++i)
            {
                if (!parent[i].Equals(child[i], opt))
                {
                    return false;
                }
            }
            return true;
        }

        private static readonly char[] pathDelims = new char[] {Path.DirectorySeparatorChar};

        /// <summary>
        /// Check whether a given path is an ancestor of another
        /// </summary>
        /// <param name="parent">The path to treat as potential ancestor</param>
        /// <param name="child">The path to treat as potential descendant</param>
        /// <returns>true if child is a descendant of parent, false otherwise</returns>
        public static bool IsAncestorOf(this DirectoryInfo parent, DirectoryInfo child)
            => StringArrayStartsWith(
                child.FullName.Split(pathDelims, StringSplitOptions.RemoveEmptyEntries),
                parent.FullName.Split(pathDelims, StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Extension method to fill in the gap of getting from a
        /// directory to its drive in .NET.
        /// Returns the drive with the longest RootDirectory.FullName
        /// that's a prefix of the dir's FullName.
        /// </summary>
        /// <param name="dir">Any DirectoryInfo object</param>
        /// <returns>The DriveInfo associated with this directory, if any, else null</returns>
        public static DriveInfo GetDrive(this DirectoryInfo dir)
            => Platform.IsMono
                // Mono's DriveInfo.GetDrives doesn't return mounted filesystems, so we
                // can't get the drive for a dir on Linux or Mac
                ? null
                : DriveInfo.GetDrives()
                           .Where(dr => dr.IsReady
                                        && dr.DriveType != DriveType.NoRootDirectory
                                        && dr.RootDirectory.IsAncestorOf(dir))
                           .OrderByDescending(dr => dr.RootDirectory.FullName.Length)
                           .FirstOrDefault();

        /// <summary>
        /// A version of Stream.CopyTo with progress updates.
        /// </summary>
        /// <param name="src">Stream from which to copy</param>
        /// <param name="dest">Stream to which to copy</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with count of bytes received</param>
        public static void CopyTo(this Stream src, Stream dest, IProgress<long> progress, CancellationToken cancelToken = default(CancellationToken))
        {
            // CopyTo says its default buffer is 81920, but we want more than 1 update for a 100 KiB file
            const int bufSize = 16384;
            var buffer = new byte[bufSize];
            long total = 0;
            // Make sure we get an initial progress notification at the start
            progress.Report(total);
            var lastProgressTime = DateTime.Now;
            while (true)
            {
                var bytesRead = src.Read(buffer, 0, bufSize);
                if (bytesRead == 0)
                {
                    break;
                }
                dest.Write(buffer, 0, bytesRead);
                total += bytesRead;
                cancelToken.ThrowIfCancellationRequested();
                var now = DateTime.Now;
                if (now - lastProgressTime >= progressInterval)
                {
                    progress.Report(total);
                    lastProgressTime = now;
                }
            }
            // Make sure we get a final progress notification after we're done
            progress.Report(total);
        }

        public static IEnumerable<byte> BytesFromStream(this Stream s)
        {
            int b;
            while ((b = s.ReadByte()) != -1)
            {
                yield return Convert.ToByte(b);
            }
        }

        private static readonly TimeSpan progressInterval = TimeSpan.FromMilliseconds(200);
    }
}
