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
        /// <param name="b">List of captions for buttons</param>
        public ConsoleMessageDialog(string m, List<string> b)
            : base()
        {
            int l    = GetLeft(),
                r    = GetRight();
            int w    = Console.WindowWidth / 2;
            int btnW = b.Count * buttonWidth + (b.Count - 1) * buttonPadding;
            if (w < btnW + 4) {
                // Widen the window to fit the buttons
                // Buttons will NOT wrap - use ConsoleChoiceDialog
                // if you have many large options.
                w = btnW + 4;
                l = (Console.WindowWidth - w) / 2;
                r = Console.WindowWidth - l;
            }

            List<string> messageLines = FmtUtils.WordWrap(m, w - 4);
            int h = 2 + messageLines.Count + (b.Count > 0 ? 2 : 0) + 2;

            SetDimensions(
                l, (Console.WindowHeight - h) / 2,
                r, (Console.WindowHeight - h) / 2 + h - 1
            );
            int btnRow = GetBottom() - 2;

            ConsoleTextBox tb = new ConsoleTextBox(
                GetLeft() + 2, GetTop() + 2, GetRight() - 2, GetBottom() - 2 - (b.Count > 0 ? 2 : 0),
                false,
                TextAlign.Center,
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            );
            AddObject(tb);
            tb.AddLine(m);

            int btnLeft = (Console.WindowWidth - btnW) / 2;
            for (int i = 0; i < b.Count; ++i) {
                string cap = b[i];
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
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <returns>
        /// Index of button the user pressed
        /// </returns>
        public new int Run(Action process = null)
        {
            base.Run(process);
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
    }

}
