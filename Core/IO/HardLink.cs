using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
using FileMode = System.IO.FileMode;

using ChinhDo.Transactions.FileManager;
using log4net;

namespace CKAN.IO
{
    /// <summary>
    /// This class provides methods for creating hard links to files.
    /// It also provides methods to retrieve information relevant to hard links,
    /// such as the device identifier, hard link target identifier, and link count of a file.
    /// </summary>
    internal static class HardLink
    {
        /// <summary>
        /// Create a hard link to a file.
        /// This will throw an exception if the hard link cannot be created (e.g., if the paths are on different volumes).
        /// </summary>
        /// <param name="target">The original file</param>
        /// <param name="linkPath">The place where we want to make a link or copy</param>
        /// <exception cref="Kraken">Thrown if can't make a hard link</exception>
        public static void Create(string target, string linkPath)
        {
            log.DebugFormat("Creating hard link from {0} to {1}...", target, linkPath);
            if (!CreateImpl(target, linkPath))
            {
                throw new Kraken(Platform.IsWindows
                    ? $"Failed to create hard link from {target} to {linkPath}: {Marshal.GetLastWin32Error()}"
                    : $"Failed to create hard link from {target} to {linkPath}.");
            }
        }

        /// <summary>
        /// Create a hard link to a file.
        /// If the hard link cannot be created (e.g., if the paths are on different volumes), copy the file instead.
        /// </summary>
        /// <param name="target">The original file</param>
        /// <param name="linkPath">The place where we want to make a link or copy</param>
        /// <param name="file_transaction">Transaction in case we need to roll back</param>
        public static void CreateOrCopy(string        target,
                                        string        linkPath,
                                        TxFileManager file_transaction)
        {
            log.DebugFormat("Creating hard link or copy from {0} to {1}...", target, linkPath);
            if (!CreateImpl(target, linkPath))
            {
                // If we can't create a hard link (e.g., if the paths are on separate volumes), copy instead
                file_transaction.Copy(target, linkPath, false);
            }
        }

        private static bool CreateImpl(string target, string linkPath)
            => Platform.IsWindows ? CreateHardLink(linkPath, target, IntPtr.Zero)
                                  : link(target, linkPath) == 0;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateHardLink(string lpFileName,
                                                  string lpExistingFileName,
                                                  IntPtr lpSecurityAttributes);

        [DllImport("libc")]
        private static extern int link(string oldpath, string newpath);

        /// <summary>
        /// Get the device identifiers of files or directories.
        /// This is a unique identifier for the filesystem.
        /// Files on different devices cannot be hard-linked to each other.
        /// </summary>
        /// <param name="paths">The files' paths</param>
        /// <returns>Sequence of long integers representing the device/volume if found, otherwise null</returns>
        public static IEnumerable<ulong?> GetDeviceIdentifiers(IEnumerable<string> paths)
            => Platform.IsWindows
                   ? paths.Select(path => GetFileInformation(path,
                                                             out BY_HANDLE_FILE_INFORMATION fileInfo)
                                              ? (ulong?)fileInfo.VolumeSerialNumber
                                              : null)
                   : (Platform.IsUnix || Platform.IsMac)
                         ? RunStat(paths, StatArg.DeviceID)
                               .Select(v => v.FirstOrDefault())
                         : Enumerable.Empty<ulong?>();

        /// <summary>
        /// Get the identifiers of files.
        /// This is a unique identifier for the file on the filesystem.
        /// Files with the same identifier are hard links to the same file.
        /// On Windows, it is a combination of the volume serial number and the file index.
        /// On Unix-like systems, it is a combination of the device ID and the inode number.
        /// </summary>
        /// <param name="paths">The files' paths</param>
        /// <returns>Sequence of 128-bit values representing the files if found, otherwise null</returns>
        public static IEnumerable<Guid?> GetFileIdentifiers(IEnumerable<string> paths)
            => Platform.IsWindows
                   ? paths.Select(path => GetFileInformation(path,
                                                             out BY_HANDLE_FILE_INFORMATION fileInfo)
                                              ? (Guid?)new Guid(BitConverter.GetBytes((ulong)fileInfo.VolumeSerialNumber)
                                                          .Concat(BitConverter.GetBytes(fileInfo.FileIndexHigh))
                                                          .Concat(BitConverter.GetBytes(fileInfo.FileIndexLow))
                                                          .ToArray())
                                              : null)
                   : (Platform.IsUnix || Platform.IsMac)
                         ? RunStat(paths, StatArg.DeviceID, StatArg.InodeNumber)
                               .Select(v => (Guid?)new Guid(v.SelectMany(val => BitConverter.GetBytes(val ?? 0L))
                                                             .ToArray()))
                         : Enumerable.Empty<Guid?>();

        /// <summary>
        /// Get the number of hard links to files' contents, including themselves.
        /// This is the number of directory entries that point to the file.
        /// </summary>
        /// <param name="paths">The files' paths</param>
        /// <returns>Sequence of number of links if found, otherwise null</returns>
        public static IEnumerable<ulong?> GetLinkCounts(IEnumerable<string> paths)
            => Platform.IsWindows
                   ? paths.Select(path => GetFileInformation(path,
                                                             out BY_HANDLE_FILE_INFORMATION fileInfo)
                                              ? (ulong?)fileInfo.NumberOfLinks
                                              : null)
                   : (Platform.IsUnix || Platform.IsMac)
                         ? RunStat(paths, StatArg.HardLinkCount)
                               .Select(v => v.FirstOrDefault())
                         : Enumerable.Empty<ulong?>();

        private static bool GetFileInformation(string path,
                                               out BY_HANDLE_FILE_INFORMATION FileInformation)
        {
            var h = CreateFile(path, 0, 0, IntPtr.Zero,
                               FileMode.Open, BackupSemantics, IntPtr.Zero);
            if (!h.IsInvalid)
            {
                var val = GetFileInformationByHandle(h, out FileInformation);
                h.Close();
                return val;
            }
            FileInformation = default;
            return false;
        }

        private enum StatArg
        {
            DeviceID,
            InodeNumber,
            HardLinkCount,
        }

        /// <summary>
        /// Get the device ID, inode number, or hard link count of files
        /// using the stat command on Unix.
        /// We use the stat command because struct stat's byte layout
        /// is not guaranteed to be the same across different Unix-like systems.
        /// It handles multiple file at once to reduce the number of processes we start.
        /// </summary>
        /// <param name="paths">The files to inspect</param>
        /// <param name="what">The info to get</param>
        /// <returns>Values returned by stat if any, else null</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the what arg isn't valid</exception>
        private static IEnumerable<ulong?[]> RunStat(IEnumerable<string> paths,
                                                     params StatArg[]    what)
        {
            var fmt = string.Join(" ", what.Select(w => w switch
            {
                StatArg.DeviceID      => "%d",
                StatArg.InodeNumber   => "%i",
                StatArg.HardLinkCount => "%h",
                _                     => throw new ArgumentOutOfRangeException(nameof(what),
                                                                               what, "Invalid stat arg"),
            }));
            foreach (var pathsString in LimitedStringJoins(paths.Select(p => $"\"{p}\""),
                                                           " ", STAT_ARG_MAX))
            {
                if (Process.Start(new ProcessStartInfo("stat", $"-c \"{fmt}\" {pathsString}")
                                  {
                                      UseShellExecute        = false,
                                      RedirectStandardOutput = true,
                                      RedirectStandardError  = true,
                                      CreateNoWindow         = true,
                                  })
                    is Process proc)
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        yield return proc.StandardOutput
                                         .ReadLine()
                                         ?.Trim()
                                          .Split(' ')
                                          .Select(s => ulong.TryParse(s, out ulong val)
                                                           ? (ulong?)val
                                                           : null)
                                          .ToArray()
                                         ?? Array.Empty<ulong?>();
                    }
                    proc.WaitForExit();
                }
            }
        }

        private static IEnumerable<string> LimitedStringJoins(IEnumerable<string> strings,
                                                              string              separator,
                                                              long                maxLength)
        {
            var current = "";
            foreach (var str in strings)
            {
                if (current.Length + separator.Length + str.Length > maxLength)
                {
                    yield return current;
                    current = "";
                }
                current += current.Length > 0 ? separator + str
                                              : str;
            }
            if (current.Length > 0)
            {
                yield return current;
            }
        }

        private static long STAT_ARG_MAX => sysconf(_SC_ARG_MAX)
                                            - POSIX_HEADROOM
                                            - "-c %d %i %h ".Length;

        /// <summary>
        /// xargs says POSIX requires 2048 bytes of headroom for environment variables.
        /// </summary>
        private const long POSIX_HEADROOM = 2048;

        /// <summary>
        /// POSIX defines this as a C enum without a specific value,
        /// so of course it is different on every platform.
        /// </summary>
        private static readonly int _SC_ARG_MAX = Platform.IsMac ? 1 : 0;

        [DllImport("libc")]
        private static extern long sysconf(int name);


        private const uint BackupSemantics = 0x02000000u;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPTStr)] string    filename,
                                                        [MarshalAs(UnmanagedType.U4)]     uint      access,
                                                        [MarshalAs(UnmanagedType.U4)]     FileShare share,
                                                        IntPtr securityAttributes,
                                                        [MarshalAs(UnmanagedType.U4)]     FileMode  creationDisposition,
                                                        [MarshalAs(UnmanagedType.U4)]     uint      flagsAndAttributes,
                                                        IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle                 hFile,
                                                              out BY_HANDLE_FILE_INFORMATION FileInformation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct BY_HANDLE_FILE_INFORMATION
        {
            public uint     FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint     VolumeSerialNumber;
            public uint     FileSizeHigh;
            public uint     FileSizeLow;
            public uint     NumberOfLinks;
            public uint     FileIndexHigh;
            public uint     FileIndexLow;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(HardLink));
    }
}
