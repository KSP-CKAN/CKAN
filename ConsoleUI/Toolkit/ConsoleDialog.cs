using System;

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
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="l">Left edge of box casting the shadow</param>
        /// <param name="t">Top edge of box casting the shadow</param>
        /// <param name="r">Right edge of box casting the shadow</param>
        /// <param name="b">Bottom edge of box casting the shadow</param>
        public static void DrawShadow(ConsoleTheme theme, int l, int t, int r, int b)
        {
            int w = r - l + 1;
            if (theme.PopupShadow.HasValue)
            {
                Console.BackgroundColor = theme.PopupShadow.Value;
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
        }

        /// <summary>
        /// Function to call to get the title of the popup.
        /// If non-empty, the value will be drawn centered at the top,
        /// otherwise the border will go all the way across.
        /// </summary>
        protected Func<string> CenterHeader = () => "";

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
        protected override void DrawBackground(ConsoleTheme theme)
        {
            int w = GetRight() - GetLeft() + 1;
            string fullHorizLineDouble = new string(Symbols.horizLineDouble, w - 2);
            string midSpace            = new string(' ',                     w - 2);
            Console.BackgroundColor = theme.PopupBg;
            Console.ForegroundColor = theme.PopupOutlineFg;
            for (int y = GetTop(); y <= GetBottom(); ++y) {
                if (y < 0 || y >= Console.WindowHeight) {
                    continue;
                }
                Console.SetCursorPosition(GetLeft(), y);
                if (y == GetTop()) {
                    // Top row
                    string curTitle = CenterHeader();
                    if (string.IsNullOrEmpty(curTitle)) {
                        Console.Write(Symbols.upperLeftCornerDouble + fullHorizLineDouble + Symbols.upperRightCornerDouble);
                    } else {
                        // Title centered
                        Console.Write(Symbols.upperLeftCornerDouble
                            + ScreenObject.PadCenter($" {curTitle} ", w - 2, Symbols.horizLineDouble)
                            + Symbols.upperRightCornerDouble);
                    }
                } else if (y == GetBottom()) {
                    // Bottom row
                    Console.Write(Symbols.lowerLeftCornerDouble + fullHorizLineDouble + Symbols.lowerRightCornerDouble);
                } else {
                    // Blank lines, mostly padding
                    Console.Write(Symbols.vertLineDouble + midSpace + Symbols.vertLineDouble);
                }
            }
            DrawShadow(theme, GetLeft(), GetTop(), GetRight(), GetBottom());
        }

        private bool validX(int x)
        {
            x = Formatting.ConvertCoord(x, Console.WindowWidth);
            return x >= 0 && x < Console.WindowWidth;
        }
        private bool validY(int y)
        {
            y = Formatting.ConvertCoord(y, Console.WindowHeight);
            return y >= 0 && y < Console.WindowHeight;
        }

        private int left, top, right, bottom;
    }

}
