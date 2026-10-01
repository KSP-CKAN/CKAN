using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32.SafeHandles;

// Dedicated Windows-only ckan:// protocol handler.
// ckan.exe is a console app so on Windows it always gets a console window
// popup on launch. Doing that every time a ckan URL opens is ugly and looks dodgy.

// This stub is a WinExe instead, so no popup.
// Called via reg as `ckan-urlhandler.exe <path to ckan.exe> <verb> <pipe name> <url>`. It sends the url to the
// running CKAN over the pipe, or starts CKAN with `<verb> --url <url> --no-handoff`.

// Must not reference anything outside the BCL so that it can be compiled
// with Native AOT for packing into ckan-windows.exe (needed to avoid substantially larger binary).

namespace CKAN.URLHandler
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static int Main(string[] args)
        {
            // Call should only ever come from the URL handler reg entry, so the args are known ahead of time:
            // <path to ckan.exe> <verb> <pipe name> <url>.
            // The verb is whichever UI registered last. CKAN passes its own pipe name so the two can't drift.

            if (args.Length != 4
                || string.IsNullOrWhiteSpace(args[0])
                || string.IsNullOrWhiteSpace(args[1])
                || string.IsNullOrWhiteSpace(args[2])
                || string.IsNullOrWhiteSpace(args[3]))
            {
                return 1;
            }

            var ckanExe = args[0];
            var verb = args[1];
            var pipeName = args[2];
            var url = args[3];

            // A ckan url can't contain a quote. Reject it rather than let it splice arguments onto the ckan.exe command line.
            if (url.IndexOf('"') >= 0)
            {
                return 1;
            }

            return (TrySendToRunningInstance(pipeName, url) || LaunchCKAN(ckanExe, verb, url)) ? 0 : 1;
        }

        // Loosely duplicates URLPipe.TrySend because this project can't reference Core.
        // Necessary to make the url handler as small as possible.
        private static bool TrySendToRunningInstance(string pipeName, string url)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                client.Connect(50);

                // CKAN GUI can't bring its own window to the front. This program inherits that right
                // from the browser's link click. Grant it to CKAN before sending.
                if (GetNamedPipeServerProcessId(client.SafePipeHandle, out var pid))
                {
                    _ = AllowSetForegroundWindow(pid);
                }

                using var writer = new StreamWriter(client) { AutoFlush = true };
                writer.WriteLine(url);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool LaunchCKAN(string ckanExe, string verb, string url)
        {
            try
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = ckanExe,
                    Arguments = $"{verb} --url \"{url}\" --no-handoff", // no-handoff: skip double TrySend.
                    UseShellExecute = false,
                    CreateNoWindow = verb == "gui", // Prevents a console flash.
                });

                // Same foreground rights handoff as the pipe path.
                if (proc != null)
                {
                    _ = AllowSetForegroundWindow((uint)proc.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                _ = MessageBox(IntPtr.Zero, $"Could not launch CKAN to handle the link:\n{ex.Message}", "CKAN", MB_ICONERROR);

                return false;
            }
        }

        private const uint MB_ICONERROR = 0x00000010;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint pid);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint pid);
    }
}
