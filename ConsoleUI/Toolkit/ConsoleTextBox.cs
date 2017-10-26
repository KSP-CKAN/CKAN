using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Object displaying a long screen in a big box
    /// </summary>
    public class ConsoleTextBox : ScreenObject {

        /// <summary>
        /// Initialize the text box
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="b">Y coordinate of bottom edge</param>
        /// <param name="autoScroll">If true, keep the bottommost row visible, else keep the topmost row visible</param>
        /// <param name="ta">Alignment of the contents</param>
        /// <param name="bgFunc">Function returning the background color for the text</param>
        /// <param name="fgFunc">Function returning the foreground color for the text</param>
        public ConsoleTextBox(
                int l, int t, int r, int b,
                bool autoScroll = true,
                TextAlign ta = TextAlign.Left,
                Func<ConsoleColor> bgFunc = null,
                Func<ConsoleColor> fgFunc = null)
            : base(l, t, r, b)
        {
            scrollToBottom = autoScroll;
            align          = ta;
            getFgColor     = fgFunc;
            getBgColor     = bgFunc;
        }

        /// <summary>
        /// Add a line to the text box
        /// </summary>
        /// <param name="line">String to add</param>
        public void AddLine(string line)
        {
            lines.AddRange(FmtUtils.WordWrap(line, GetRight() - GetLeft() + 1));
        }

        /// <summary>
        /// Draw the text box
        /// </summary>
        /// <param name="focused">Framework parameter not relevant to this object</param>
        public override void Draw(bool focused)
        {
            int l     = GetLeft();
            int w     = GetRight() - l + 1;
            int h     = GetBottom() - GetTop() + 1;
            int index = !scrollToBottom || lines.Count < h
                ? 0
                : lines.Count - h;

            if (getBgColor != null) {
                Console.BackgroundColor = getBgColor();
            } else {
                Console.BackgroundColor = ConsoleTheme.Current.TextBoxBg;
            }
            if (getFgColor != null) {
                Console.ForegroundColor = getFgColor();
            } else {
                Console.ForegroundColor = ConsoleTheme.Current.TextBoxFg;
            }
            for (int y = GetTop(); y <= GetBottom(); ++y, ++index) {
                Console.SetCursorPosition(l, y);
                if (index < lines.Count) {
                    switch (align) {
                        case TextAlign.Left:
                            Console.Write(lines[index].PadRight(w));
                            break;
                        case TextAlign.Center:
                            Console.Write(ScreenObject.PadCenter(lines[index], w));
                            break;
                        case TextAlign.Right:
                            Console.Write(lines[index].PadLeft(w));
                            break;
                    }
                } else {
                    Console.Write("".PadRight(w));
                }
            }

            // FUTURE: Scrollbar, if we need it to be interactive
        }

        /// <summary>
        /// Tell the container we can't receive focus
        /// </summary>
        public override bool Focusable() { return false; }

        private bool         scrollToBottom;
        private TextAlign    align;
        private List<string> lines = new List<string>();
        private Func<ConsoleColor> getBgColor;
        private Func<ConsoleColor> getFgColor;
    }

    /// <summary>
    /// Alignment of text box
    /// </summary>
    public enum TextAlign {
        /// <summary>
        /// Left aligned, padding on right
        /// </summary>
        Left,
        /// <summary>
        /// Centered, padding on both left and right
        /// </summary>
        Center,
        /// <summary>
        /// Right aligned, padding on left
        /// </summary>
        Right
    }

}
