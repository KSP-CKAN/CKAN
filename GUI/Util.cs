using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using log4net;

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

        /// <summary>
        /// Returns true if the string could be a valid http address.
        /// DOES NOT ACTUALLY CHECK IF IT EXISTS, just the format.
        /// </summary>
        public static bool CheckURLValid(string source)
        {
            Uri uri_result;
            return Uri.TryCreate(source, UriKind.Absolute, out uri_result)
                && (uri_result.Scheme == Uri.UriSchemeHttp
                 || uri_result.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Open a URL, unless it's "N/A"
        /// </summary>
        /// <param name="url">The URL</param>
        public static void OpenLinkFromLinkLabel(string url)
        {
            if (url == Properties.Resources.ModInfoNSlashA)
            {
                return;
            }

            TryOpenWebPage(url);
        }

        /// <summary>
        /// Tries to open an url using the default application.
        /// If it fails, it tries again by prepending each prefix before the url before it gives up.
        /// </summary>
        public static bool TryOpenWebPage(string url, IEnumerable<string> prefixes = null)
        {
            // Default prefixes to try if not provided
            if (prefixes == null)
            {
                prefixes = new string[] { "https://", "http://" };
            }

            foreach (string fullUrl in new string[] { url }
                .Concat(prefixes.Select(p => p + url).Where(CheckURLValid)))
            {
                if (Utilities.ProcessStartURL(fullUrl))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// React to the user clicking a mouse button on a link.
        /// Opens the URL in browser on left click, presents a
        /// right click menu on right click.
        /// </summary>
        /// <param name="url">The link's URL</param>
        /// <param name="e">The click event</param>
        public static void HandleLinkClicked(string url, LinkLabelLinkClickedEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Util.OpenLinkFromLinkLabel(url);
                    break;

                case MouseButtons.Right:
                    Util.LinkContextMenu(url);
                    break;
            }
        }

        /// <summary>
        /// Show a context menu when the user right clicks a link
        /// </summary>
        /// <param name="url">The URL of the link</param>
        public static void LinkContextMenu(string url)
        {
            ToolStripMenuItem copyLink = new ToolStripMenuItem(Properties.Resources.UtilCopyLink);
            copyLink.Click += new EventHandler((sender, ev) => Clipboard.SetText(url));

            ContextMenuStrip menu = new ContextMenuStrip();
            if (Platform.IsMono)
            {
                menu.Renderer = new FlatToolStripRenderer();
            }
            menu.Items.Add(copyLink);
            menu.Show(Cursor.Position);
        }

        /// <summary>
        /// Find a screen that the given box overlaps
        /// </summary>
        /// <param name="location">Upper left corner of box</param>
        /// <param name="size">Width and height of box</param>
        /// <returns>
        /// The first screen that overlaps the box if any, otherwise null
        /// </returns>
        public static Screen FindScreen(Point location, Size size)
        {
            var rect = new Rectangle(location, size);
            return Screen.AllScreens.FirstOrDefault(sc => sc.WorkingArea.IntersectsWith(rect));
        }

        /// <summary>
        /// Adjust position of a box so it fits entirely on one screen
        /// </summary>
        /// <param name="location">Top left corner of box</param>
        /// <param name="size">Width and height of box</param>
        /// <returns>
        /// Original location if already fully on-screen, otherwise
        /// a position representing sliding it onto the screen
        /// </returns>
        public static Point ClampedLocation(Point location, Size size, Screen screen = null)
        {
            if (screen == null)
            {
                log.DebugFormat("Looking for screen of {0}, {1}", location, size);
                screen = FindScreen(location, size);
            }
            if (screen != null)
            {
                log.DebugFormat("Found screen: {0}", screen.WorkingArea);
                // Slide the whole rectangle fully onto the screen
                if (location.X < screen.WorkingArea.Left)
                    location.X = screen.WorkingArea.Left;
                if (location.Y < screen.WorkingArea.Top)
                    location.Y = screen.WorkingArea.Top;
                if (location.X + size.Width > screen.WorkingArea.Right)
                    location.X = screen.WorkingArea.Right - size.Width;
                if (location.Y + size.Height > screen.WorkingArea.Bottom)
                    location.Y = screen.WorkingArea.Bottom - size.Height;
                log.DebugFormat("Clamped location: {0}", location);
            }
            return location;
        }

        /// <summary>
        /// Adjust position of a box so it fits on one screen with a margin around it
        /// </summary>
        /// <param name="location">Top left corner of box</param>
        /// <param name="size">Width and height of box</param>
        /// <param name="topLeftMargin">Size of space between window and top left edge of screen</param>
        /// <param name="bottomRightMargin">Size of space between window and bottom right edge of screen</param>
        /// <returns>
        /// Original location if already fully on-screen plus margins, otherwise
        /// a position representing sliding it onto the screen
        /// </returns>
        public static Point ClampedLocationWithMargins(Point location, Size size, Size topLeftMargin, Size bottomRightMargin, Screen screen = null)
        {
            // Imagine drawing a larger box around the window, the size of the desired margin.
            // We pass that box to ClampedLocation to make sure it fits on screen,
            // then place our window at an offset within the box
            return ClampedLocation(location - topLeftMargin, size + topLeftMargin + bottomRightMargin, screen) + topLeftMargin;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(Util));
    }
}
