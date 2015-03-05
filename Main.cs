using System.Diagnostics;
using System.IO;

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
            if (args.Length != 3)
            {
                return;
            }

            var pid = int.Parse(args[0]);
            var local_path = args[1];
            var updated_path = args[2];

            // wait for CKAN to close
            Process.GetProcessById(pid).WaitForExit();

            // replace ckan.exe
            File.Move(updated_path, local_path);

            // Start CKANn
            Process.Start(local_path);
        }
    }
}
