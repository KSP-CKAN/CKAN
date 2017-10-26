using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Base class for popup dialogs
    /// </summary>
    public abstract class ConsoleDialog : ScreenContainer {

        /// <summary>
        /// Initialize the dialog.
        /// By default sets size and position to middle 50%.
        /// </summary>
        protected ConsoleDialog()
        {
            left   =     Console.WindowWidth  / 4;
            top    =     Console.WindowHeight / 4;
            right  = 3 * Console.WindowWidth  / 4;
            bottom = 3 * Console.WindowHeight / 4;
        }

        /// <summary>
        /// Draw a drop shadow to the right and bottom of a given box
        /// </summary>
        /// <param name="l">Left edge of box casting the shadow</param>
        /// <param name="t">Top edge of box casting the shadow</param>
        /// <param name="r">Right edge of box casting the shadow</param>
        /// <param name="b">Bottom edge of box casting the shadow</param>
        public static void DrawShadow(int l, int t, int r, int b)
        {
            int w = r - l + 1;
            Console.BackgroundColor = ConsoleTheme.Current.PopupShadow;
            if (r < Console.WindowWidth - 2) {
                // Right shadow
                for (int y = t + 1; y <= b; ++y) {
                    if (y >= 0 && y < Console.WindowHeight - 1) {
                        Console.SetCursorPosition(r + 1, y);
                        Console.Write("  ");
                    }
                }
            }
            // Bottom shadow
            if (l + w + 2 > Console.WindowWidth) {
                w = Console.WindowWidth - l - 2;
            }
            if (b < Console.WindowHeight - 1) {
                Console.SetCursorPosition(l + 2, b + 1);
                Console.Write("".PadRight(w));
            }
        }

        /// <returns>
        /// X coordinate of left edge of dialog
        /// </returns>
        protected int GetLeft()   { return FmtUtils.ConvertCoord(left,   Console.WindowWidth);  }
        /// <returns>
        /// Y coordinate of top edge of dialog
        /// </returns>
        protected int GetTop()    { return FmtUtils.ConvertCoord(top,    Console.WindowHeight); }
        /// <returns>
        /// X coordinate of right edge of dialog
        /// </returns>
        protected int GetRight()  { return FmtUtils.ConvertCoord(right,  Console.WindowWidth);  }
        /// <returns>
        /// Y coordinate of bottom edge of dialog
        /// </returns>
        protected int GetBottom() { return FmtUtils.ConvertCoord(bottom, Console.WindowHeight); }

        private bool validX(int x)
        {
            x = FmtUtils.ConvertCoord(x, Console.WindowWidth);
            return x >= 0 && x < Console.WindowWidth;
        }
        private bool validY(int y)
        {
            y = FmtUtils.ConvertCoord(y, Console.WindowHeight);
            return y >= 0 && y < Console.WindowHeight;
        }

        /// <summary>
        /// Set position of dialog
        /// </summary>
        /// <param name="l">X coordinate of left edge of dialog</param>
        /// <param name="t">Y coordinate of top edge of dialog</param>
        /// <param name="r">X coordinate of right edge of dialog</param>
        /// <param name="b">Y coordinate of bottom edge of dialog</param>
        protected void SetDimensions(int l, int t, int r, int b)
        {
            left   = validX(l) ? l :  2;
            top    = validY(t) ? t :  1;
            right  = validX(r) ? r : -2;
            bottom = validY(b) ? b : -1;
        }

        /// <summary>
        /// Draw the outline of the dialog and clear the footer
        /// </summary>
        protected override void DrawBackground()
        {
            int w = GetRight() - GetLeft() + 1;
            string fullHorizLineDouble = new string(Symbols.horizLineDouble, w - 2);
            string midSpace            = new string(' ',                     w - 2);
            Console.BackgroundColor = ConsoleTheme.Current.PopupBg;
            Console.ForegroundColor = ConsoleTheme.Current.PopupOutlineFg;
            for (int y = GetTop(); y <= GetBottom(); ++y) {
                if (y < 0 || y >= Console.WindowHeight) {
                    continue;
                }
                Console.SetCursorPosition(GetLeft(), y);
                if (y == GetTop()) {
                    // Top row
                    Console.Write(Symbols.upperLeftCornerDouble + fullHorizLineDouble + Symbols.upperRightCornerDouble);
                } else if (y == GetBottom()) {
                    // Bottom row
                    Console.Write(Symbols.lowerLeftCornerDouble + fullHorizLineDouble + Symbols.lowerRightCornerDouble);
                } else {
                    // Blank lines, mostly padding
                    Console.Write(Symbols.vertLineDouble + midSpace + Symbols.vertLineDouble);
                }
            }
            DrawShadow(GetLeft(), GetTop(), GetRight(), GetBottom());
        }

        private int left, top, right, bottom;
    }

    /// <summary>
    /// Group of functions for handling screen formatting
    /// </summary>
    public static class FmtUtils {

        /// <summary>
        /// Turn an abstract coordinate into a real coordinate.
        /// This just means that we use positive values to represent offsets from left/top,
        /// and negative values to represent offsets from right/bottom.
        /// </summary>
        /// <param name="val">Coordinate value to convert</param>
        /// <param name="max">Maximum value for the coordinate, used to translate negative values</param>
        /// <returns>
        /// Position represented
        /// </returns>
        public static int ConvertCoord(int val, int max)
        {
            if (val >= 0) {
                return val;
            } else {
                return max + val - 1;
            }
        }

        /// <summary>
        /// Word wrap a long string into separate lines
        /// </summary>
        /// <param name="msg">Long message to wrap</param>
        /// <param name="w">Allowed length of lines</param>
        /// <returns>
        /// List of strings, one per line
        /// </returns>
        public static List<string> WordWrap(string msg, int w)
        {
            List<string> messageLines = new List<string>();
            if (!string.IsNullOrEmpty(msg)) {
                // The string is allowed to contain line breaks.
                string[] hardLines = msg.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);
                foreach (var line in hardLines) {
                    if (string.IsNullOrEmpty(line)) {
                        messageLines.Add("");
                    } else {
                        int used = 0;
                        while (used < line.Length) {
                            while (used < line.Length && line[used] == ' ') {
                                // Skip spaces so lines start with non-spaces
                                ++used;
                            }
                            if (used >= line.Length) {
                                // Ran off the end of the string with spaces, we're done
                                messageLines.Add("");
                                break;
                            }
                            int lineLen;
                            if (used + w >= line.Length) {
                                // We're at the end of the line, use the whole thing
                                lineLen = line.Length - used;
                            } else {
                                // Middle of the line, find a word wrappable chunk
                                for (lineLen = w; lineLen >= 0 && line[used + lineLen] != ' '; --lineLen) { }
                            }
                            if (lineLen < 1) {
                                // Word too long, truncate it
                                lineLen = w;
                            }
                            messageLines.Add(line.Substring(used, lineLen));
                            used += lineLen;
                        }
                    }
                }
            }
            return messageLines;
        }
    }

}
