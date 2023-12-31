using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Base class for full screen UIs
    /// </summary>
    public abstract class ConsoleScreen : ScreenContainer, IUser {

        /// <summary>
        /// Initialize a screen.
        /// Sets up the F10 key binding for menus.
        /// </summary>
        protected ConsoleScreen()
        {
            AddTip(
                "F10", MenuTip(),
                () => mainMenu != null
            );
            AddBinding(new ConsoleKeyInfo[] {Keys.F10, Keys.Apps}, (object sender, ConsoleTheme theme) => {
                bool val = true;
                if (mainMenu != null) {
                    DrawSelectedHamburger(theme);

                    val = mainMenu.Run(theme, Console.WindowWidth - 1, 1);

                    DrawBackground(theme);
                }
                return val;
            });
        }

        /// <summary>
        /// Launch a screen and then clean up after it so we can continue using this screen.
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="cs">Subscreen to launch</param>
        /// <param name="newProc">Function to drive the screen, default is normal interaction</param>
        protected void LaunchSubScreen(ConsoleTheme theme, ConsoleScreen cs, Action<ConsoleTheme> newProc = null)
        {
            cs.Run(theme, newProc);
            DrawBackground(theme);
            Draw(theme);
        }

        /// <summary>
        /// Function returning text to be shown at the left edge of the top header bar
        /// </summary>
        protected virtual string LeftHeader()
        {
            return "";
        }

        /// <summary>
        /// Function returning text to be shown in the center of the top header bar
        /// </summary>
        protected virtual string CenterHeader()
        {
            return "";
        }

        /// <summary>
        /// Function returning text to be shown to explain the F10 menu hotkey
        /// </summary>
        protected virtual string MenuTip()
        {
            return Properties.Resources.Menu;
        }

        /// <summary>
        /// Menu to open for F10 from the hamburger icon of this screen
        /// </summary>
        protected ConsolePopupMenu mainMenu = null;

        #region IUser

        /// <summary>
        /// Tell IUser clients that we have the ability to interact with the user
        /// </summary>
        public bool Headless => false;

        // These functions can be implemented the same on all screens,
        // so they are not virtual.

        /// <summary>
        /// Ask the user a yes/no question and capture the answer.
        /// </summary>
        /// <param name="question">Message to display to the user</param>
        /// <returns>
        /// True if the user selected Yes, and false if the user selected No.
        /// </returns>
        public virtual bool RaiseYesNoDialog(string question)
        {
            ConsoleMessageDialog d = new ConsoleMessageDialog(
                string.Join("", messagePieces) + question,
                new List<string>() {
                    Properties.Resources.Yes,
                    Properties.Resources.No
                }
            );
            d.AddBinding(Keys.Y, (object sender, ConsoleTheme theme) => {
                d.PressButton(0);
                return false;
            });
            d.AddBinding(Keys.N, (object sender, ConsoleTheme theme) => {
                d.PressButton(1);
                return false;
            });
            messagePieces.Clear();
            bool val = d.Run(userTheme) == 0;
            DrawBackground(userTheme);
            Draw(userTheme);
            return val;
        }

        /// <summary>
        /// Show a message and let the user choose one of several options
        /// This is only used by the Cmdline client.
        /// The Core algorithms don't call it.
        /// </summary>
        /// <param name="message">Text to show to the user</param>
        /// <param name="args">Array of options for user to choose</param>
        /// <returns>
        /// Index of option chosen by user
        /// </returns>
        public int RaiseSelectionDialog(string message, params object[] args)
        {
            ConsoleMessageDialog d = new ConsoleMessageDialog(
                string.Join("", messagePieces) + message,
                new List<string>(Array.ConvertAll(args, p => p.ToString()))
            );
            messagePieces.Clear();
            int val = d.Run(userTheme);
            DrawBackground(userTheme);
            Draw(userTheme);
            return val;
        }

        /// <summary>
        /// Add an error message to the screen.
        /// It is assumed that the application WILL prompt the user for confirmation.
        /// </summary>
        /// <param name="message">Format string for the message</param>
        /// <param name="args">Values to be interpolated into the format string</param>
        public void RaiseError(string message, params object[] args)
        {
            ConsoleMessageDialog d = new ConsoleMessageDialog(
                string.Join("", messagePieces) + string.Format(message, args),
                new List<string>() { Properties.Resources.OK }
            );
            messagePieces.Clear();
            d.Run(userTheme);
            DrawBackground(userTheme);
            Draw(userTheme);
        }

        /// <summary>
        /// Add a status update message to the screen.
        /// It is assumed that the application will NOT prompt the user for confirmation.
        /// </summary>
        /// <param name="message">Format string for the message</param>
        /// <param name="args">Values to be interpolated into the format string</param>
        public void RaiseMessage(string message, params object[] args)
        {
            Message(message, args);
            Draw(userTheme);
        }

        /// <summary>
        /// Forward RaiseMessage requests along to child classes.
        /// If not overridden, saves the message to be combined with others.
        /// </summary>
        /// <param name="message">Format string for message</param>
        /// <param name="args">Arguments to be plugged into the format string</param>
        protected virtual void Message(string message, params object[] args)
        {
            // Use a message popup if the child class doesn't override
            messagePieces.Add(string.Format(message, args) + "\r\n");
        }

        // Core functions call RaiseMessage multiple times for one operation.
        // For example, they might do:
        //   RaiseMessage("Trying to install:");
        //   RaiseMessage(" * ABC");
        //   RaiseMessage(" * DEF");
        //   RaiseMessage("This will require installing XYZ.");
        //   RaiseMessage("It may also require uninstalling some conflicts.");
        //   RaiseYesNoDialog("Do you want to continue?");
        // We can't show a separate message box for each of those.
        // But there's nothing intrinsic to the API that tells us which strings
        // to combine. So we'll just save them all and then combine them
        // when a function is called that takes input.
        private readonly List<string> messagePieces = new List<string>();

        /// <summary>
        /// Update a user visible progress bar
        /// </summary>
        /// <param name="message">Text to be shown describing the task</param>
        /// <param name="percent">Value 0-100 representing the progress</param>
        public void RaiseProgress(string message, int percent)
        {
            Progress(message, percent);
            Draw(userTheme);
        }

        /// <summary>
        /// Update a user visible progress bar
        /// </summary>
        /// <param name="percent">Value 0-100 representing the progress</param>
        /// <param name="bytesPerSecond">Current download rate</param>
        /// <param name="bytesLeft">Bytes remaining in the downloads</param>
        public void RaiseProgress(int percent, long bytesPerSecond, long bytesLeft)
        {
            var fullMsg = string.Format(CKAN.Properties.Resources.NetAsyncDownloaderProgress,
                                        CkanModule.FmtSize(bytesPerSecond),
                                        CkanModule.FmtSize(bytesLeft));
            Progress(fullMsg, percent);
            Draw(userTheme);
        }

        /// <summary>
        /// Forward a RaiseProgress request to child classes
        /// </summary>
        /// <param name="message">Message to be shown in progress bar</param>
        /// <param name="percent">Value from 0 to 100 representing task completion</param>
        protected virtual void Progress(string message, int percent) { }

        #endregion IUser

        private void DrawSelectedHamburger(ConsoleTheme theme)
        {
            Console.SetCursorPosition(Console.WindowWidth - 3, 0);
            Console.BackgroundColor = theme.MenuSelectedBg;
            Console.ForegroundColor = theme.MenuFg;
            Console.Write(hamburger);
        }

        /// <summary>
        /// Set the whole screen to dark blue and draw the top header bar
        /// </summary>
        protected override void DrawBackground(ConsoleTheme theme)
        {
            // Cheat because our IUser handlers need a theme
            userTheme = theme;

            Console.BackgroundColor = theme.MainBg;
            Console.Clear();

            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = theme.HeaderBg;
            Console.ForegroundColor = theme.HeaderFg;
            Console.Write(LeftCenterRight(
                " " + LeftHeader(),
                CenterHeader(),
                mainMenu != null ? hamburger : "",
                Console.WindowWidth
            ));
        }

        /// <summary>
        /// Clear the screen like we're exiting to DOS
        /// </summary>
        protected override void ClearBackground()
        {
            Console.ResetColor();
            Console.Clear();
        }

        private static string LeftCenterRight(string left, string center, string right, int width)
        {
            // If the combined string is too long, shorten the center
            if (center.Length > width - left.Length - right.Length - 4) {
                center = center.Substring(0, width - left.Length - right.Length - 4);
            }
            // Start with the center centered on the screen
            int leftSideWidth = (width - center.Length) / 2;
            // If there isn't enough room for the left side, shift the center over
            if (leftSideWidth < left.Length + 2) {
                leftSideWidth = left.Length + 2;
            }
            // The right side takes whatever's left
            int rightSideWidth = (width - center.Length) - leftSideWidth;
            return left.PadRight(leftSideWidth) + center + right.PadLeft(rightSideWidth);
        }

        private ConsoleTheme userTheme;
        private static readonly string hamburger = $" {Symbols.hamburger} ";
    }

}
