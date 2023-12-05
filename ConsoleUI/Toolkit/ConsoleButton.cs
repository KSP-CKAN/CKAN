using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Class representing a button with some text and an action that the user can select and press
    /// </summary>
    public class ConsoleButton : ScreenObject {

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="l">X coordinate of left edge of button</param>
        /// <param name="t">Y coordinate of button</param>
        /// <param name="r">X coordinate of right edge of button</param>
        /// <param name="cap">Text to show on button</param>
        /// <param name="onClick">Function to call if user clicks button</param>
        public ConsoleButton(int l, int t, int r, string cap, Action onClick)
            : base(l, t, r, t)
        {
            caption     = cap;
            choiceEvent = onClick;
            shadowStrip = new string(Symbols.upperHalfBlock, GetRight() - GetLeft() + 1);
        }

        /// <summary>
        /// Draw the button
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">True if button has the focus, false otherwise</param>
        public override void Draw(ConsoleTheme theme, bool focused)
        {
            int w = GetRight() - GetLeft() + 1;

            // Main button text
            Console.SetCursorPosition(GetLeft(), GetTop());
            Console.BackgroundColor = theme.PopupButtonBg;
            Console.ForegroundColor = focused ? theme.PopupButtonSelectedFg : theme.PopupButtonFg;
            Console.Write(PadCenter(caption, w));

            // Right shadow
            if (theme.PopupButtonShadow.HasValue)
            {
                Console.BackgroundColor = theme.PopupBg;
                Console.ForegroundColor = theme.PopupButtonShadow.Value;
                Console.Write(Symbols.lowerHalfBlock);

                // Bottom shadow
                Console.SetCursorPosition(GetLeft() + 1, GetTop() + 1);
                Console.Write(shadowStrip);
            }
        }

        /// <summary>
        /// Tell the container that the button can receive focus
        /// </summary>
        public override bool Focusable() { return true; }

        /// <summary>
        /// Put the cursor at the left edge of the button
        /// </summary>
        public override void PlaceCursor()
        {
            Console.SetCursorPosition(GetLeft(), GetTop());
        }

        /// <summary>
        /// Press the button on space and enter,
        /// move focus with arrows and tab.
        /// </summary>
        public override void OnKeyPress(ConsoleKeyInfo k)
        {
            switch (k.Key) {
                case ConsoleKey.Tab:
                    Blur(!k.Modifiers.HasFlag(ConsoleModifiers.Shift));
                    break;
                case ConsoleKey.Spacebar:
                case ConsoleKey.Enter:
                    choiceEvent();
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.LeftArrow:
                    Blur(false);
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.RightArrow:
                    Blur(true);
                    break;
            }
        }

        private readonly string caption;
        private readonly Action choiceEvent;
        private readonly string shadowStrip;
    }
}
