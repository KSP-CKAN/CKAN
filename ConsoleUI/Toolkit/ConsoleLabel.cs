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
        /// <param name="cf">Function returning the color to use for the label</param>
        public ConsoleLabel(int l, int t, int r, Func<string> lf, Func<ConsoleColor> cf = null)
            : base(l, t, r, t)
        {
            labelFunc = lf;
            colorFunc = cf;
        }

        /// <summary>
        /// Draw the labelFunc
        /// </summary>
        /// <param name="focused">Framework parameter not relevant to this object</param>
        public override void Draw(bool focused)
        {
            int w = GetRight() - GetLeft() + 1;
            Console.SetCursorPosition(GetLeft(), GetTop());
            Console.BackgroundColor = ConsoleTheme.Current.LabelBg;
            if (colorFunc == null) {
                Console.ForegroundColor = ConsoleTheme.Current.LabelFg;
            } else {
                Console.ForegroundColor = colorFunc();
            }
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

        private Func<string>       labelFunc;
        private Func<ConsoleColor> colorFunc;
    }

}
