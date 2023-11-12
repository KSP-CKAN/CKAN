using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Object representing a simple text label
    /// </summary>
    public class ConsoleLabel : ScreenObject {

        /// <summary>
        /// Initialize a labelFunc
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="lf">Function returning the text to show in the label</param>
        /// <param name="bgFunc">Function returning the background color for the label</param>
        /// <param name="fgFunc">Function returning the foreground color for the label</param>
        public ConsoleLabel(int l, int t, int r, Func<string> lf, Func<ConsoleTheme, ConsoleColor> bgFunc = null, Func<ConsoleTheme, ConsoleColor> fgFunc = null)
            : base(l, t, r, t)
        {
            labelFunc  = lf;
            getBgColor = bgFunc;
            getFgColor = fgFunc;
        }

        /// <summary>
        /// Draw the labelFunc
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">Framework parameter not relevant to this object</param>
        public override void Draw(ConsoleTheme theme, bool focused)
        {
            int w = GetRight() - GetLeft() + 1;
            Console.SetCursorPosition(GetLeft(), GetTop());
            Console.BackgroundColor = getBgColor == null ? theme.LabelBg : getBgColor(theme);
            Console.ForegroundColor = getFgColor == null ? theme.LabelFg : getFgColor(theme);
            try {
                Console.Write(FormatExactWidth(labelFunc(), w));
            } catch (Exception ex) {
                Console.Write(FormatExactWidth(ex.Message,  w));
            }
        }

        /// <summary>
        /// Tell the container we can't take focus
        /// </summary>
        public override bool Focusable() { return false; }

        private readonly Func<string>       labelFunc;
        private readonly Func<ConsoleTheme, ConsoleColor> getBgColor;
        private readonly Func<ConsoleTheme, ConsoleColor> getFgColor;
    }

}
