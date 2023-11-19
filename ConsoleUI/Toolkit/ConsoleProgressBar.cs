using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Object representing a progress bar showing completion of tasks
    /// </summary>
    public class ConsoleProgressBar : ScreenObject {

        /// <summary>
        /// Initialize the progress bar
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="cf">Function returning caption of the progress bar</param>
        /// <param name="pf">Function returning percentage of the progress bar</param>
        public ConsoleProgressBar(int l, int t, int r, Func<string> cf, Func<double> pf)
            : base(l, t, r, t)
        {
            captionFunc = cf;
            percentFunc = pf;
        }

        /// <summary>
        /// Draw the progress bar
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">Framework parameter not relevant to this object</param>
        public override void Draw(ConsoleTheme theme, bool focused)
        {
            int l = GetLeft(), t = GetTop();
            int w = GetRight() - l + 1;

            double percent = percentFunc == null ? 0 : percentFunc();
            int highlightWidth = (int)Math.Floor(w * percent);
            if (highlightWidth < 0) {
                highlightWidth = 0;
            } else if (highlightWidth > w) {
                highlightWidth = w;
            }

            // Build one big string representing the whole contents of the bar
            string caption = PadCenter(captionFunc == null ? "" : captionFunc(), w);

            Console.SetCursorPosition(l, t);

            // Draw the highlighted part
            if (highlightWidth > 0) {
                Console.BackgroundColor = theme.ProgressBarHighlightBg;
                Console.ForegroundColor = theme.ProgressBarHighlightFg;
                Console.Write(caption.Substring(0, highlightWidth));
            }

            // Draw the non highlighted part
            Console.BackgroundColor = theme.ProgressBarBg;
            Console.ForegroundColor = theme.ProgressBarFg;
            Console.Write(caption.Substring(highlightWidth));
        }

        /// <summary>
        /// Tell the container we can't receive focus
        /// </summary>
        public override bool Focusable() { return false; }

        private readonly Func<string> captionFunc;
        private readonly Func<double> percentFunc;
    }

}
