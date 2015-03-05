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

            if (!File.Exists(updated_path))
            {
                return;
            }

            // wait for CKAN to close
            Process.GetProcessById(pid).WaitForExit();

            if (File.Exists(local_path))
            {
                // delete the old ckan.exe
                File.Delete(local_path);
            }
           
            // replace ckan.exe
            File.Move(updated_path, local_path);

            // Start CKAN
            Process.Start(local_path);
        }
    }
}
