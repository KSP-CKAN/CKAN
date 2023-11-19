using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Screen object representing a box to be drawn around some other stuff
    /// </summary>
    public class ConsoleFrame : ScreenObject {

        /// <summary>
        /// Initialize a frame
        /// </summary>
        /// <param name="l">X coordinate of left edge of box</param>
        /// <param name="t">Y coordinate of top edge of box</param>
        /// <param name="r">X coordinate of right edge of box</param>
        /// <param name="b">Y coordinate of bottom edge of box</param>
        /// <param name="title">Function returning text to put in middle of top edge of box</param>
        /// <param name="borderColor">Function returning foreground color for border</param>
        /// <param name="dblBorder">If true, draw a double line border, else single line</param>
        public ConsoleFrame(int l, int t, int r, int b, Func<string> title,
                Func<ConsoleTheme, ConsoleColor> borderColor, bool dblBorder = false)
            : base(l, t, r, b)
        {
            getTitle     = title;
            getColor     = borderColor;
            doubleBorder = dblBorder;
        }

        /// <summary>
        /// Draw a frame
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">Framework parameter not relevant to this control</param>
        public override void Draw(ConsoleTheme theme, bool focused)
        {
            int l = GetLeft(), t = GetTop(), r = GetRight(), b = GetBottom();
            int w = r - l + 1;
            string title = getTitle();

            Console.BackgroundColor = theme.MainBg;
            Console.ForegroundColor = getColor(theme);
            Console.SetCursorPosition(l, t);
            Console.Write(doubleBorder ? Symbols.upperLeftCornerDouble  : Symbols.upperLeftCorner);
            if (title.Length > 0) {
                int topLeftSidePad  = (w - 4 - title.Length) / 2;
                int topRightSidePad = (w - 4 - title.Length) - topLeftSidePad;
                if (topLeftSidePad < 0 || topRightSidePad < 0) {
                    topLeftSidePad  = 0;
                    topRightSidePad = 0;
                    title = title.Substring(0, w - 4);
                }
                Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, topLeftSidePad));
                Console.Write($" {title} ");
                Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, topRightSidePad));
            } else {
                Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, w - 2));
            }
            Console.Write(doubleBorder ? Symbols.upperRightCornerDouble : Symbols.upperRightCorner);

            for (int y = t + 1; y <= b - 1; ++y) {
                Console.SetCursorPosition(l, y);
                Console.Write(doubleBorder ? Symbols.vertLineDouble : Symbols.vertLine);
                Console.SetCursorPosition(r, y);
                Console.Write(doubleBorder ? Symbols.vertLineDouble : Symbols.vertLine);
            }

            Console.SetCursorPosition(l, b);
            Console.Write(doubleBorder ? Symbols.lowerLeftCornerDouble  : Symbols.lowerLeftCorner);
            Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, w - 2));
            Console.Write(doubleBorder ? Symbols.lowerRightCornerDouble : Symbols.lowerRightCorner);
        }

        /// <summary>
        /// Tell the container this control can't be focused
        /// </summary>
        public override bool Focusable() { return false; }

        private readonly Func<string>       getTitle;
        private readonly Func<ConsoleTheme, ConsoleColor> getColor;
        private readonly bool               doubleBorder;
    }

}
