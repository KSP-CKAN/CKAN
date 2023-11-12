using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Screen object representing a box to be drawn around some other stuff
    /// with a big divider through the middle splitting it into two sections
    /// </summary>
    public class ConsoleDoubleFrame : ScreenObject {

        /// <summary>
        /// Initialize a frame
        /// </summary>
        /// <param name="l">X coordinate of left edge of box</param>
        /// <param name="t">Y coordinate of top edge of box</param>
        /// <param name="r">X coordinate of right edge of box</param>
        /// <param name="b">Y coordinate of bottom edge of box</param>
        /// <param name="midY">Y coordinate of middle line</param>
        /// <param name="topTitle">Function returning text to put in middle of top edge of box</param>
        /// <param name="midTitle">Function returning text to put in middle of the middle line</param>
        /// <param name="borderColor">Function returning foreground color for border</param>
        /// <param name="dblBorder">If true, draw a double line border, else single line</param>
        public ConsoleDoubleFrame(int l, int t, int r, int b, int midY,
                Func<string> topTitle, Func<string> midTitle,
                Func<ConsoleTheme, ConsoleColor> borderColor, bool dblBorder = false)
            : base(l, t, r, b)
        {
            getTopTitle  = topTitle;
            getMidTitle  = midTitle;
            middleRow    = midY;
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

            Console.BackgroundColor = theme.MainBg;
            Console.ForegroundColor = getColor(theme);
            Console.SetCursorPosition(l, t);
            Console.Write(doubleBorder ? Symbols.upperLeftCornerDouble  : Symbols.upperLeftCorner);
            writeTitleRow(getTopTitle(), w);
            Console.Write(doubleBorder ? Symbols.upperRightCornerDouble : Symbols.upperRightCorner);

            for (int y = t + 1; y <= b - 1; ++y) {
                Console.SetCursorPosition(l, y);
                if (y == middleRow) {
                    Console.Write(doubleBorder ? Symbols.leftTeeDouble : Symbols.leftTee);
                    writeTitleRow(getMidTitle(), w);
                    Console.Write(doubleBorder ? Symbols.rightTeeDouble : Symbols.rightTee);
                } else {
                    Console.Write(doubleBorder ? Symbols.vertLineDouble : Symbols.vertLine);
                    Console.SetCursorPosition(r, y);
                    Console.Write(doubleBorder ? Symbols.vertLineDouble : Symbols.vertLine);
                }
            }

            Console.SetCursorPosition(l, b);
            Console.Write(doubleBorder ? Symbols.lowerLeftCornerDouble  : Symbols.lowerLeftCorner);
            Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, w - 2));
            Console.Write(doubleBorder ? Symbols.lowerRightCornerDouble : Symbols.lowerRightCorner);
        }

        private void writeTitleRow(string title, int w)
        {
            if (title.Length > 0) {
                int leftSidePad  = (w - 4 - title.Length) / 2;
                int rightSidePad = (w - 4 - title.Length) - leftSidePad;
                if (leftSidePad < 0 || rightSidePad < 0) {
                    leftSidePad  = 0;
                    rightSidePad = 0;
                    title = title.Substring(0, w - 4);
                }
                Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, leftSidePad));
                Console.Write($" {title} ");
                Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, rightSidePad));
            } else {
                Console.Write(new string(doubleBorder ? Symbols.horizLineDouble : Symbols.horizLine, w - 2));
            }
        }

        /// <summary>
        /// Tell the container this control can't be focused
        /// </summary>
        public override bool Focusable() { return false; }

        private readonly Func<string>       getTopTitle;
        private readonly Func<string>       getMidTitle;
        private readonly Func<ConsoleTheme, ConsoleColor> getColor;
        private readonly bool               doubleBorder;
        private readonly int                middleRow;
    }

}
