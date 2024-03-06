using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
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
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern int FreeConsole();

        public static void HideConsoleWindow()
        {
            if (Platform.IsWindows)
            {
                FreeConsole();
            }
        }

        /// <summary>
        /// Returns true if the string could be a valid http address.
        /// DOES NOT ACTUALLY CHECK IF IT EXISTS, just the format.
        /// </summary>
        public static bool CheckURLValid(string source)
            => Uri.TryCreate(source, UriKind.Absolute, out Uri uri_result)
                && (uri_result.Scheme == Uri.UriSchemeHttp
                 || uri_result.Scheme == Uri.UriSchemeHttps);

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
                {
                    return true;
                }
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
                    OpenLinkFromLinkLabel(url);
                    break;

                case MouseButtons.Right:
                    LinkContextMenu(url);
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
                {
                    location.X = screen.WorkingArea.Left;
                }

                if (location.Y < screen.WorkingArea.Top)
                {
                    location.Y = screen.WorkingArea.Top;
                }

                if (location.X + size.Width > screen.WorkingArea.Right)
                {
                    location.X = screen.WorkingArea.Right - size.Width;
                }

                if (location.Y + size.Height > screen.WorkingArea.Bottom)
                {
                    location.Y = screen.WorkingArea.Bottom - size.Height;
                }

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

        /// <summary>
        /// Coalesce multiple events from a busy event source into single delayed reactions
        ///
        /// See: https://www.freecodecamp.org/news/javascript-debounce-example/
        ///
        /// Additional convenience features:
        ///   - Ability to do something immediately unconditionally
        ///   - Execute immediately if a condition is met
        ///   - Pass the events to the functions
        /// </summary>
        /// <param name="startFunc">Called immediately when the event is fired, for fast parts of the handling</param>
        /// <param name="immediateFunc">If this returns true for an event, truncate the delay and fire doneFunc immediately</param>
        /// <param name="abortFunc">If this returns true for an event, ignore it completely (e.g. for setting text box contents programmatically)</param>
        /// <param name="doneFunc">Called after timeoutMs milliseconds, or immediately if immediateFunc returns true</param>
        /// <param name="timeoutMs">Number of milliseconds between the last event and when to call doneFunc</param>
        /// <typeparam name="EventT">Event type handled</typeparam>
        /// <returns>A new event handler that wraps the given functions using the timer</returns>
        public static EventHandler<EventT> Debounce<EventT>(
            EventHandler<EventT>       startFunc,
            Func<object, EventT, bool> immediateFunc,
            Func<object, EventT, bool> abortFunc,
            EventHandler<EventT>       doneFunc,
            int timeoutMs = 500)
        {
            // Store the most recent event we received
            object receivedFrom = null;
            EventT received     = default;

            // Set up the timer that will track the delay
            Timer timer = new Timer() { Interval = timeoutMs };
            timer.Tick += (sender, evt) =>
            {
                timer.Stop();
                doneFunc(receivedFrom, received);
            };

            return (object sender, EventT evt) =>
            {
                if (!abortFunc(sender, evt))
                {
                    timer.Stop();
                    startFunc(sender, evt);
                    if (immediateFunc(sender, evt))
                    {
                        doneFunc(sender, evt);
                        receivedFrom = null;
                        received     = default;
                    }
                    else
                    {
                        receivedFrom = sender;
                        received     = evt;
                        timer.Start();
                    }
                }
            };
        }

        public static Color BlendColors(Color[] colors)
            => colors.Length <  1 ? Color.Empty
             : colors.Length == 1 ? colors[0]
             : colors.Aggregate((back, fore) => fore.AlphaBlendWith(1f / colors.Length, back));

        public static Color AlphaBlendWith(this Color c1, float alpha, Color c2)
            => AddColors(c1.MultiplyBy(alpha),
                         c2.MultiplyBy(1f - alpha));

        private static Color MultiplyBy(this Color c, float f)
            => Color.FromArgb((int)(f * c.R),
                              (int)(f * c.G),
                              (int)(f * c.B));

        private static Color AddColors(Color a, Color b)
            => Color.FromArgb(a.R + b.R,
                              a.G + b.G,
                              a.B + b.B);

        /// <summary>
        /// Simple syntactic sugar around Graphics.MeasureString
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="font">The font to be used for the text</param>
        /// <param name="text">String to measure size of</param>
        /// <param name="maxWidth">Number of pixels allowed horizontally</param>
        /// <returns>
        /// Number of pixels needed vertically to fit the string
        /// </returns>
        public static int StringHeight(Graphics g, string text, Font font, int maxWidth)
            => (int)g.MeasureString(text, font, (int)(maxWidth / XScale(g))).Height;

        /// <summary>
        /// Calculate how much vertical space is needed to display a label's text
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="lbl">The label</param>
        /// <returns>
        /// Number of pixels needed vertically to show the label's full text
        /// </returns>
        public static int LabelStringHeight(Graphics g, Label lbl)
            => (int)(YScale(g) * (lbl.Margin.Vertical + lbl.Padding.Vertical
                                  + StringHeight(g, lbl.Text, lbl.Font,
                                                 (lbl.Width - lbl.Margin.Horizontal
                                                            - lbl.Padding.Horizontal))));

        private static float XScale(Graphics g) => g.DpiX / 96f;
        private static float YScale(Graphics g) => g.DpiY / 96f;

        private static readonly ILog log = LogManager.GetLogger(typeof(Util));
    }
}
