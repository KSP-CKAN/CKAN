using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CKAN
{

    public static class Util
    {

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

        /// <summary>
        /// Invokes an actin on the UI thread, or directly if we're
        /// on the UI thread.
        /// </summary>
        public static void Invoke<T>(T obj, Action action) where T : Control
        {
            if (obj.InvokeRequired) // if we're not in the UI thread
            {
                // enqueue call on UI thread and wait for it to return
                obj.Invoke(new MethodInvoker(action));
            }
            else
            {
                // we're on the UI thread, execute directly
                action();
            }
        }

        // utility helper to deal with multi-threading and UI
        // async version, doesn't wait for UI thread
        // use with caution, when not sure use blocking Invoke()
        public static void AsyncInvoke<T>(T obj, Action action) where T : Control
        {
            if (obj.InvokeRequired) // if we're not in the UI thread
            {
                // enqueue call on UI thread and continue
                obj.BeginInvoke(new MethodInvoker(action));
            }
            else
            {
                // we're on the UI thread, execute directly
                action();
            }
        }

        // hides the console window on windows
        // useful when running the GUI
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void HideConsoleWindow()
        {
            if (!IsLinux)
            {
                ShowWindow(GetConsoleWindow(), 0);
            }
        }


        public static bool IsInstallable(this GUIMod mod)
        {
            return !(mod == null || mod.IsAutodetected || mod.IsIncompatible);
        }
   }

}
