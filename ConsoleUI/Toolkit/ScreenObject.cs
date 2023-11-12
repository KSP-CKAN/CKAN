using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Base class for UI elements like labels, fields, frames, list boxes, etc.
    /// </summary>
    public abstract class ScreenObject {

        /// <summary>
        /// Initialize the object
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="b">Y coordinate of bottom edge</param>
        protected ScreenObject(int l, int t, int r, int b)
        {
            left   = l;
            top    = t;
            right  = r;
            bottom = b;
        }

        /// <summary>
        /// Add padding to the left and right edges of a string to center it in a given WindowWidth
        /// </summary>
        /// <param name="s">String to center</param>
        /// <param name="w">Width of space to center in</param>
        /// <param name="pad">Character to use for padding</param>
        /// <returns>
        /// {padding}s{padding}
        /// </returns>
        public static string PadCenter(string s, int w, char pad = ' ')
        {
            if (s.Length > w) {
                return s.Substring(0, w);
            } else {
                int lp = (w - s.Length) / 2;
                return FormatExactWidth(s, w - lp, pad).PadLeft(w, pad);
            }
        }

        /// <summary>
        /// Truncate or pad a string to fit a given width exactly
        /// </summary>
        /// <param name="val">String to process</param>
        /// <param name="w">Width to fit</param>
        /// <param name="pad">Character to use for padding</param>
        /// <returns>
        /// val{padding} or substring of val
        /// </returns>
        public static string FormatExactWidth(string val, int w, char pad = ' ')
        {
            return val.PadRight(w, pad).Substring(0, w);
        }

        /// <summary>
        /// Truncate a string if it's longer than the limit
        /// </summary>
        /// <param name="val">String to truncate</param>
        /// <param name="w">Maximum allowed length</param>
        /// <returns>First 'w' characters of 'val', or whole string if short enough</returns>
        public static string TruncateLength(string val, int w)
            => val.Length <= w ? val
                               : val.Substring(0, w);

        /// <summary>
        /// Custom key bindings for this UI element
        /// </summary>
        public Dictionary<ConsoleKeyInfo, ScreenContainer.KeyAction> Bindings =
            new Dictionary<ConsoleKeyInfo, ScreenContainer.KeyAction>();

        /// <summary>
        /// Add a custom key binding
        /// </summary>
        /// <param name="k">Key to bind</param>
        /// <param name="a">Action to bind to key</param>
        public void AddBinding(ConsoleKeyInfo k, ScreenContainer.KeyAction a)
        {
            Bindings.Add(k, a);
        }

        /// <summary>
        /// Add custom key bindings
        /// </summary>
        /// <param name="keys">Keys to bind</param>
        /// <param name="a">Action to bind to key</param>
        public void AddBinding(IEnumerable<ConsoleKeyInfo> keys, ScreenContainer.KeyAction a)
        {
            foreach (ConsoleKeyInfo k in keys) {
                AddBinding(k, a);
            }
        }

        /// <summary>
        /// Tips to show in the footer when this UI element is focused
        /// </summary>
        public List<ScreenTip> Tips = new List<ScreenTip>();

        /// <summary>
        /// Add tip to show in footer
        /// </summary>
        /// <param name="key">Description of the key</param>
        /// <param name="descrip">Description of the action bound to the key</param>
        /// <param name="displayIf">Function returning true to show the tip, false to hide it</param>
        public void AddTip(string key, string descrip, Func<bool> displayIf = null)
        {
            if (displayIf == null) {
                displayIf = () => true;
            }
            Tips.Add(new ScreenTip(key, descrip, displayIf));
        }

        /// <summary>
        /// Draw a scrollbar for scrollable screen objects
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="r">X coordinate of scrollbar</param>
        /// <param name="t">Y coordinate of top of scrollbar</param>
        /// <param name="b">Y coordinate of bottom of scrollbar</param>
        /// <param name="dragRow">Y coordinate of the box indicating how scrolled the bar is</param>
        protected void DrawScrollbar(ConsoleTheme theme, int r, int t, int b, int dragRow)
        {
            Console.BackgroundColor = theme.ScrollBarBg;
            Console.ForegroundColor = theme.ScrollBarFg;
            for (int y = t; y <= b; ++y) {
                Console.SetCursorPosition(r, y);
                if (y <= t) {
                    Console.Write(scrollUp);
                } else if (y == b) {
                    Console.Write(scrollDown);
                } else if (y == dragRow) {
                    Console.Write(scrollDrag);
                } else {
                    Console.Write(scrollBar);
                }
            }
        }

        /// <returns>
        /// X coordinate of left edge of dialog
        /// </returns>
        protected int GetLeft()   { return Formatting.ConvertCoord(left,   Console.WindowWidth);  }
        /// <returns>
        /// Y coordinate of top edge of dialog
        /// </returns>
        protected int GetTop()    { return Formatting.ConvertCoord(top,    Console.WindowHeight); }
        /// <returns>
        /// X coordinate of right edge of dialog
        /// </returns>
        protected int GetRight()  { return Formatting.ConvertCoord(right,  Console.WindowWidth);  }
        /// <returns>
        /// Y coordinate of bottom edge of dialog
        /// </returns>
        protected int GetBottom() { return Formatting.ConvertCoord(bottom, Console.WindowHeight); }

        /// <summary>
        /// Draw the UI element
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">If true, draw with focus, else draw without focused</param>
        public abstract void Draw(ConsoleTheme theme, bool focused);

        /// <summary>
        /// Return whether the UI element can accept focus
        /// </summary>
        public virtual bool Focusable() { return true; }
        /// <summary>
        /// Place focus based on the UI element's positioning
        /// </summary>
        public virtual void PlaceCursor() { }
        /// <summary>
        /// Handle default key bindings for the UI element
        /// </summary>
        public virtual void OnKeyPress(ConsoleKeyInfo k) { }

        /// <summary>
        /// Type for event to notify container that we'd like to lose focus
        /// </summary>
        public delegate void BlurListener(ScreenObject sender, bool forward);
        /// <summary>
        /// Event to notify container that we'd like to lose focus
        /// </summary>
        public event BlurListener OnBlur;
        /// <summary>
        /// Function to fire event to notify container that we'd like to lose focus
        /// </summary>
        protected void Blur(bool forward)
        {
            OnBlur?.Invoke(this, forward);
        }

        private readonly int left, top, right, bottom;

        private static readonly string scrollUp    = "^";
        private static readonly string scrollDown  = "v";
        private static readonly string scrollBar   = Symbols.hashBox;
        private static readonly string scrollDrag  = "*";
    }

}
