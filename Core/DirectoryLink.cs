using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

using ChinhDo.Transactions.FileManager;

namespace CKAN
{
    /// <summary>
    /// Junctions on Windows, symbolic links on Unix
    /// </summary>
    public static class DirectoryLink
    {
        public static void Create(string target, string link, TxFileManager txMgr)
        {
            if (!CreateImpl(target, link, txMgr))
            {
                throw new Kraken(Platform.IsWindows
                    ? $"Failed to create junction at {link}: {Marshal.GetLastWin32Error()}"
                    : $"Failed to create symbolic link at {link}");
            }
        }

        private static bool CreateImpl(string target, string link, TxFileManager txMgr)
            => Platform.IsWindows ? CreateJunction(link, target, txMgr)
                                  : symlink(target, link) == 0;

        [DllImport("libc")]
        private static extern int symlink(string target, string link);

        private static bool CreateJunction(string link, string target, TxFileManager txMgr)
        {
            // A junction is a directory with some extra magic attached
            if (!txMgr.DirectoryExists(link))
            {
                txMgr.CreateDirectory(link);
            }
            using (var h = CreateFile(link, GenericWrite, FileShare.Read | FileShare.Write, IntPtr.Zero,
                                      FileMode.Open, BackupSemantics | OpenReparsePoint, IntPtr.Zero))
            {
                if (!h.IsInvalid)
                {
                    var junctionInfo = ReparseDataBuffer.FromPath(target, out int byteCount);
                    return DeviceIoControl(h, FSCTL_SET_REPARSE_POINT,
                                           ref junctionInfo, byteCount + 20,
                                           null, 0,
                                           out _, IntPtr.Zero);
                }
            }
            return false;
        }

        public static bool TryGetTarget(string link, out string target)
        {
            target = null;
            var fi = new DirectoryInfo(link);
            if (fi.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                if (Platform.IsWindows)
                {
                    var h = CreateFile(link, 0, FileShare.Read, IntPtr.Zero,
                                       FileMode.Open, BackupSemantics | OpenReparsePoint, IntPtr.Zero);
                    if (!h.IsInvalid)
                    {
                        if (DeviceIoControl(h, FSCTL_GET_REPARSE_POINT,
                                            null, 0,
                                            out ReparseDataBuffer junctionInfo, Marshal.SizeOf(typeof(ReparseDataBuffer)),
                                            out _, IntPtr.Zero))
                        {
                            if (junctionInfo.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
                            {
                                target = junctionInfo.PathBuffer.TrimStart("\\\\?\\");
                            }
                        }
                        h.Close();
                    }
                }
                else
                {
                    var bytes = new byte[1024];
                    var result = readlink(link, bytes, bytes.Length);
                    if (result > 0)
                    {
                        target = Encoding.UTF8.GetString(bytes);
                    }
                }
            }
            return !string.IsNullOrEmpty(target);
        }

        public static void Remove(string link)
        {
            if (Platform.IsWindows)
            {
                if (Directory.Exists(link))
                {
                    var h = CreateFile(link, GenericWrite, FileShare.Write, IntPtr.Zero,
                                       FileMode.Open, BackupSemantics | OpenReparsePoint, IntPtr.Zero);
                    if (!h.IsInvalid)
                    {
                        var junctionInfo = ReparseDataBuffer.Empty();
                        if (!DeviceIoControl(h, FSCTL_DELETE_REPARSE_POINT,
                                             ref junctionInfo, 8,
                                             null, 0,
                                             out _, IntPtr.Zero))
                        {
                            throw new Kraken($"Failed to remove junction at {link}: {Marshal.GetLastWin32Error()}");
                        }
                        h.Close();
                        Directory.Delete(link);
                    }
                }
            }
            else
            {
                File.Delete(link);
            }
        }

        private const uint GenericWrite               = 0x40000000u;
        private const uint BackupSemantics            = 0x02000000u;
        private const uint OpenReparsePoint           = 0x00200000u;
        private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003u;
        private const uint FSCTL_SET_REPARSE_POINT    = 0x000900A4u;
        private const uint FSCTL_GET_REPARSE_POINT    = 0x000900A8u;
        private const uint FSCTL_DELETE_REPARSE_POINT = 0x000900ACu;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(SafeFileHandle        hDevice,
                                                   uint                  IoControlCode,
                                                   ref ReparseDataBuffer InBuffer,
                                                   int                   nInBufferSize,
                                                   byte[]                OutBuffer,
                                                   int                   nOutBufferSize,
                                                   out int               pBytesReturned,
                                                   IntPtr                Overlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(SafeFileHandle        hDevice,
                                                   uint                  IoControlCode,
                                                   byte[]                InBuffer,
                                                   int                   nInBufferSize,
                                                   out ReparseDataBuffer OutBuffer,
                                                   int                   nOutBufferSize,
                                                   out int               pBytesReturned,
                                                   IntPtr                Overlapped);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct ReparseDataBuffer
        {
            public           uint   ReparseTag;
            public           ushort ReparseDataLength;
            private readonly ushort Reserved;
            public           ushort SubstituteNameOffset;
            public           ushort SubstituteNameLength;
            public           ushort PrintNameOffset;
            public           ushort PrintNameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8184)]
            public           string PathBuffer;

            public static ReparseDataBuffer Empty()
            {
                return new ReparseDataBuffer
                {
                    ReparseTag           = IO_REPARSE_TAG_MOUNT_POINT,
                    ReparseDataLength    = 0,
                    SubstituteNameOffset = 0,
                    SubstituteNameLength = 0,
                    PrintNameOffset      = 0,
                    PrintNameLength      = 0,
                    PathBuffer           = "",
                };
            }

            public static ReparseDataBuffer FromPath(string target, out int byteCount)
            {
                var fullTarget = $@"\??\{Path.GetFullPath(target)}";
                byteCount = Encoding.Unicode.GetByteCount(fullTarget);
                return new ReparseDataBuffer
                {
                    ReparseTag           = IO_REPARSE_TAG_MOUNT_POINT,
                    ReparseDataLength    = (ushort)(byteCount + 12),
                    SubstituteNameOffset = 0,
                    SubstituteNameLength = (ushort)byteCount,
                    PrintNameOffset      = (ushort)(byteCount + 2),
                    PrintNameLength      = 0,
                    PathBuffer           = fullTarget,
                };
            }
        }

        [DllImport("libc")]
        private static extern int readlink(string link, byte[] buf, int bufsize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPTStr)] string    filename,
                                                        [MarshalAs(UnmanagedType.U4)]     uint      access,
                                                        [MarshalAs(UnmanagedType.U4)]     FileShare share,
                                                        IntPtr securityAttributes,
                                                        [MarshalAs(UnmanagedType.U4)]     FileMode  creationDisposition,
                                                        [MarshalAs(UnmanagedType.U4)]     uint      flagsAndAttributes,
                                                        IntPtr templateFile);

        private static string TrimStart(this string orig, string toRemove)
            => orig.StartsWith(toRemove) ? orig.Remove(0, toRemove.Length)
                                         : orig;

    }
}
