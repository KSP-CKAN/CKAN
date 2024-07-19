using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Timer = System.Timers.Timer;

namespace CKAN.Extensions
{
    public static class IOExtensions
    {
        /// <summary>
        /// Extension method to get from a directory to its drive.
        /// </summary>
        /// <param name="dir">Any DirectoryInfo object</param>
        /// <returns>The DriveInfo associated with this directory, if any, else null</returns>
        public static DriveInfo GetDrive(this DirectoryInfo dir)
            => new DriveInfo(dir.FullName);

        /// <summary>
        /// A version of Stream.CopyTo with progress updates.
        /// </summary>
        /// <param name="src">Stream from which to copy</param>
        /// <param name="dest">Stream to which to copy</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with count of bytes received</param>
        /// <param name="idleInterval">Maximum timespand to elapse between progress updates, will synthesize extra updates as needed</param>
        public static void CopyTo(this Stream       src,
                                  Stream            dest,
                                  IProgress<long>   progress,
                                  TimeSpan?         idleInterval = null,
                                  CancellationToken cancelToken = default)
        {
            // CopyTo says its default buffer is 81920, but we want more than 1 update for a 100 KiB file
            const int bufSize = 16384;
            var buffer = new byte[bufSize];
            long total = 0;
            var lastProgressTime = DateTime.Now;
            // Sometimes a server just freezes and times out, send extra updates if requested
            Timer timer = null;
            if (idleInterval.HasValue)
            {
                timer = new Timer(idleInterval.Value > minProgressInterval
                                      ? idleInterval.Value.TotalMilliseconds
                                      : minProgressInterval.TotalMilliseconds)
                {
                    AutoReset = true,
                };
                timer.Elapsed += (sender, args) =>
                {
                    progress.Report(total);
                    lastProgressTime = DateTime.Now;
                };
                timer.Start();
            }
            // Make sure we get an initial progress notification at the start
            progress.Report(total);
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
                if (now - lastProgressTime >= minProgressInterval)
                {
                    timer?.Stop();
                    timer?.Start();
                    progress.Report(total);
                    lastProgressTime = now;
                }
            }
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
                timer = null;
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

        private static readonly TimeSpan minProgressInterval = TimeSpan.FromMilliseconds(200);
    }
}
