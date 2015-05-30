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
 * 
 * Invoked as:
 * AutoUpdate.exe <running CKAN PID> <running CKAN path> <updated CKAN path>
 */

namespace AutoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                return;
            }

            var pid = int.Parse(args[0]);
            var local_path = args[1];
            var updated_path = args[2];
            var launch = args[3];

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
            catch (Exception) {}

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

            if (args[3] == "launch")
            {
                //Start CKAN
                Process.Start(local_path);    
            }
        }

        public static bool IsLinux
        {
            get
            {
                // Magic numbers ahoy! This arcane incantation was found
                // in a Unity help-page, which was found on a scroll,
                // which was found in an urn that dated back to Mono 2.0.
                // It documents singular numbers of great power.
                //
                // "And lo! 'pon the 4, 6, and 128 the penguin shall
                // come, and it infiltrate dominate from the smallest phone to
                // the largest cloud."
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}
