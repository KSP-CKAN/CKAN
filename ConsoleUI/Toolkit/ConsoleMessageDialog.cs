using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Dialog showing a message and buttons the user can press
    /// </summary>
    public class ConsoleMessageDialog : ConsoleDialog {

        /// <summary>
        /// Initialize a dialog
        /// </summary>
        /// <param name="m">Message to show</param>
        /// <param name="btns">List of captions for buttons</param>
        /// <param name="hdr">Function to generate the header</param>
        /// <param name="ta">Alignment of the contents</param>
        /// <param name="vertOffset">Pass non-zero to move popup vertically</param>
        public ConsoleMessageDialog(string m, List<string> btns, Func<string> hdr = null, TextAlign ta = TextAlign.Center, int vertOffset = 0)
            : base()
        {
            int maxLen = Formatting.MaxLineLength(m);
            int w      = Math.Max(minWidth, Math.Min(maxLen + 6, Console.WindowWidth - 4));
            int l      = (Console.WindowWidth - w) / 2;
            int r      = -l;
            if (hdr != null) {
                CenterHeader = hdr;
            }

            int btnW = (btns.Count * buttonWidth) + ((btns.Count - 1) * buttonPadding);
            if (w < btnW + 4) {
                // Widen the window to fit the buttons
                // Buttons will NOT wrap - use ConsoleChoiceDialog
                // if you have many large options.
                w = btnW + 4;
                l = (Console.WindowWidth - w) / 2;
                r = Console.WindowWidth - l;
            }

            List<string> messageLines = Formatting.WordWrap(m, w - 4);
            int h = 2 + messageLines.Count + (btns.Count > 0 ? 2 : 0) + 2;
            if (h > Console.WindowHeight - 4) {
                h = Console.WindowHeight - 4;
            }

            // Calculate vertical position including offset
            int t, b;
            if (vertOffset <= 0) {
                t = ((Console.WindowHeight - h) / 2) + vertOffset;
                if (t < 1) {
                    t = 2;
                }
                b = t + h - 1;
            } else {
                b = ((Console.WindowHeight - h) / 2) + h - 1;
                if (b >= Console.WindowHeight - 1) {
                    b = Console.WindowHeight - 1;
                }
                t = b - h + 1;
            }

            SetDimensions(l, t, r, b);
            int btnRow = GetBottom() - 2;

            ConsoleTextBox tb = new ConsoleTextBox(
                GetLeft() + 2, GetTop() + 2, GetRight() - 2, GetBottom() - 2 - (btns.Count > 0 ? 2 : 0),
                false,
                ta,
                th => th.PopupBg,
                th => th.PopupFg
            );
            AddObject(tb);
            tb.AddLine(m);

            int boxH = GetBottom() - 2 - (btns.Count > 0 ? 2 : 0) - (GetTop() + 2) + 1;

            if (messageLines.Count > boxH) {
                // Scroll
                AddTip(Properties.Resources.CursorKeys, Properties.Resources.Scroll);
                tb.AddScrollBindings(this);
            }

            int btnLeft = (Console.WindowWidth - btnW) / 2;
            for (int i = 0; i < btns.Count; ++i) {
                string cap = btns[i];
                int j = i;
                AddObject(new ConsoleButton(btnLeft, btnRow, btnLeft + buttonWidth - 1, cap, () => {
                    selectedButton = j;
                    Quit();
                }));
                btnLeft += buttonWidth + buttonPadding;
            }
        }

        /// <summary>
        /// Show the dialog and handle its interaction
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <returns>
        /// Index of button the user pressed
        /// </returns>
        public new int Run(ConsoleTheme theme, Action<ConsoleTheme> process = null)
        {
            base.Run(theme, process);
            return selectedButton;
        }

        /// <summary>
        /// Simulate pressing a button, handy for key binding shortcuts
        /// </summary>
        /// <param name="which">Index of button to pressing</param>
        public void PressButton(int which)
        {
            selectedButton = which;
            Quit();
        }

        private       int selectedButton = 0;
        private const int buttonWidth    = 10;
        private const int buttonPadding  = 3;
        private const int minWidth       = 40;
    }

}
