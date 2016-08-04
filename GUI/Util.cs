using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void HideConsoleWindow()
        {
            if (Platform.IsWindows)
            {
                ShowWindow(GetConsoleWindow(), 0);
            }
        }

        /// <summary>
        /// Returns true if the string could be a valid http address.
        /// DOES NOT ACTUALLY CHECK IF IT EXISTS, just the format.
        /// </summary>
        public static bool CheckURLValid(string source)
        {
            Uri uri_result;
            return Uri.TryCreate(source, UriKind.Absolute, out uri_result) && uri_result.Scheme == Uri.UriSchemeHttp;
        }

        public static void OpenLinkFromLinkLabel(LinkLabel link_label)
        {
            if (link_label.Text == "N/A")
            {
                return;
            }

            TryOpenWebPage(link_label.Text);
        }

        /// <summary>
        /// Tries to open an url using the default application.
        /// If it fails, it tries again by prepending each prefix before the url before it gives up.
        /// </summary>
        public static bool TryOpenWebPage(string url, IEnumerable<string> prefixes = null)
        {
            // Default prefixes to try if not provided
            if (prefixes == null)
                prefixes = new string[] { "http://", "https:// " };

            try // opening the page normally
            {
                Process.Start(url);
                return true; // we did it! return true
            }
            catch (Exception) // something bad happened
            {
                foreach (string prefixed_url in prefixes.Select(p => p + url).Where(CheckURLValid))
                {
                    try // with a new prefix
                    {
                        Process.Start(prefixed_url);
                        return true;
                    }
                    catch (Exception)
                    {
                        // move along to the next prefix
                    }
                }
                // We tried all prefixes, and still no luck.
                return false;
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
            if (mod == null) throw new ArgumentNullException();
            return !(mod.IsAutodetected || mod.IsIncompatible) || (!mod.IsAutodetected && mod.IsInstalled);
        }
    }
}