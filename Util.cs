using System;


namespace CKAN
{
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public static class Util
    {                
        /// <summary>
        /// Invokes an action on the UI thread, or directly if we're
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
            if (Platform.IsWindows)
            {
                ShowWindow(GetConsoleWindow(), 0);
            }
        }
   }
}

namespace CKAN
{
    public static class UtilWithoutWinForm
    {
        public static bool IsInstallable(this GUIMod mod)
        {
            if(mod==null) throw new ArgumentNullException();
            return !(mod.IsAutodetected || mod.IsIncompatible) || (!mod.IsAutodetected && mod.IsInstalled);
        }
    }
}
