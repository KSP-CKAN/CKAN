using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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

namespace CKAN.AutoUpdateHelper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;

            if (args.Length != 4)
            {
                ReportError("{0}: AutoUpdater.exe pid oldPath newPath [no]launch", Properties.Resources.Usage);
                return ExitBADOPT;
            }

            // Yes, it's a global variable, but we're an ephemeral singleton so :shrug:
            fromGui = (args[3] == "launch");

            int    pid          = int.Parse(args[0]);
            string local_path   = args[1];
            string updated_path = args[2];

            if (!File.Exists(updated_path))
            {
                ReportError(Properties.Resources.DownloadNotFound, updated_path);
                return ExitBADOPT;
            }

            // Wait for CKAN to close
            try
            {
                if (IsOnWindows)
                {
                    // On Unix you can only wait for CHILD processes to exit
                    var process = Process.GetProcessById(Math.Abs(pid));
                    if (!process.HasExited)
                    {
                        process.WaitForExit();
                    }
                }
                else if (pid < 0)
                {
                    // v1.29.2 and earlier will send positive, releases after will send negative
                    // AND redirect stdin so it closes at exit
                    while (Console.ReadLine() != null)
                    {
                        // Do nothing (shouldn't hit this because it doesn't send us anything)
                    }
                }
            }
            catch (ArgumentException)
            {
                // Process already exited, we're fine
            }
            catch (Exception exc)
            {
                ReportError(Properties.Resources.FailedToWait, exc.Message);
                return ExitERROR;
            }

            for (int retry = 0; retry < maxRetries && File.Exists(local_path); ++retry)
            {
                try
                {
                    // Delete the old ckan.exe
                    File.Delete(local_path);
                }
                catch (Exception exc)
                {
                    if (retry == maxRetries - 1)
                    {
                        ReportError(Properties.Resources.FailedToDelete, local_path, exc.Message);
                        if (fromGui)
                        {
                            // Launch the old EXE that we can't delete
                            StartCKAN(local_path);
                        }
                        return ExitERROR;
                    }
                    else
                    {
                        // Double sleep every time, starting at 100 ms, ending at 25 sec
                        Thread.Sleep(100 * (int)Math.Pow(2, retry));
                    }
                }
            }

            // Replace ckan.exe
            File.Move(updated_path, local_path);

            MakeExecutable(local_path);

            if (fromGui)
            {
                StartCKAN(local_path);
            }
            return ExitOK;
        }

        /// <summary>
        /// Run the CKAN EXE at the given path
        /// </summary>
        /// <param name="path">Location of our CKAN EXE</param>
        private static void StartCKAN(string path)
        {
            // Start CKAN
            if (IsOnMono)
            {
                Process.Start("mono", string.Format("\"{0}\"", path));
            }
            else
            {
                Process.Start(path, "--asroot");
            }
        }

        private static void MakeExecutable(string path)
        {
            if (!IsOnWindows)
            {
                // TODO: It would be really lovely (and safer!) to use the native system
                // call here: http://docs.go-mono.com/index.aspx?link=M:Mono.Unix.Native.Syscall.chmod

                string command = string.Format("+x \"{0}\"", path);

                var permsprocess = Process.Start(new ProcessStartInfo("chmod", command)
                {
                    UseShellExecute = false
                });
                permsprocess.WaitForExit();
            }
        }

        /// <summary>
        /// Are we on Mono?
        /// </summary>
        private static bool IsOnMono
            => Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Are we on Windows?
        /// </summary>
        private static bool IsOnWindows
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Display unexpected exceptions to user
        /// </summary>
        /// <param name="sender">Source of unhandled exception</param>
        /// <param name="e">Info about the exception</param>
        private static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            ReportError(Properties.Resources.UnhandledException, e.ExceptionObject);
        }

        /// <summary>
        /// It's nice to tell the user when something goes wrong!
        /// </summary>
        /// <param name="err">Description of the problem that happened</param>
        private static void ReportError(string message, params object[] args)
        {
            string err = string.Format(message, args);
            Console.Error.WriteLine(err);
            if (fromGui)
            {
                // Show a popup in case the console isn't open
                MessageBox.Show(err, Properties.Resources.FatalErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private const  int  maxRetries = 8;
        private static bool fromGui    = false;

        private const int ExitOK     = 0;
        private const int ExitBADOPT = 1;
        private const int ExitERROR  = 2;
    }
}
