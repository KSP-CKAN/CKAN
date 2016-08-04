using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * CKAN AUTO-UPDATE TOOL
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *
 * This simple program is used to replace the local ckan.exe with the latest one i.e. auto-update.
 * It is a command-line tool only, meant to be invoked by the main CKAN process and not manually.
 * Argument launch must be one of: launch, nolaunch
 *
 * Invoked as:
 * AutoUpdate.exe <running CKAN PID> <running CKAN path> <updated CKAN path> <launch>
 */

namespace AutoUpdater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                return;
            }

            var pid = int.Parse(args[0]);
            var local_path = args[1];
            var updated_path = args[2];

            if (!File.Exists(updated_path))
            {
                return;
            }

            // wait for CKAN to close
            try
            {
                var process = Process.GetProcessById(pid);

                if (!process.HasExited)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception) { }

            int retries = 8;

            while (File.Exists(local_path))
            {
                try
                {
                    // delete the old ckan.exe
                    File.Delete(local_path);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }

                retries--;
                if (retries == 0)
                {
                    return;
                }
            }

            // replace ckan.exe
            File.Move(updated_path, local_path);

            MakeExecutable(local_path);

            if (args[3] == "launch")
            {
                //Start CKAN
                if (IsOnMono())
                {
                    Process.Start("mono", String.Format("\"{0}\"", local_path));
                }
                else
                {
                    Process.Start(local_path);
                }
            }
        }

        private static void MakeExecutable(string path)
        {
            if (!IsOnWindows())
            {
                // TODO: It would be really lovely (and safer!) to use the native system
                // call here: http://docs.go-mono.com/index.aspx?link=M:Mono.Unix.Native.Syscall.chmod

                string command = string.Format("+x \"{0}\"", path);

                ProcessStartInfo permsinfo = new ProcessStartInfo("chmod", command);
                permsinfo.UseShellExecute = false;
                Process permsprocess = Process.Start(permsinfo);
                permsprocess.WaitForExit();
            }
        }

        /// <summary>
        /// Are we on Mono?
        /// </summary>
        private static bool IsOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// Are we on Windows?
        /// </summary>
        private static bool IsOnWindows()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            return platform != PlatformID.MacOSX &&
                platform != PlatformID.Unix && platform != PlatformID.Xbox;
        }
    }
}