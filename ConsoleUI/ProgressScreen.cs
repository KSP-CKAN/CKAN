using System;
using System.Collections.Generic;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen showing a progress bar and a text box to track progress of things
    /// </summary>
    public class ProgressScreen : ConsoleScreen {

        // The IUser stuff is ONLY used by the mod download/install operations.

        /// <summary>
        /// Initialize the Screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="descrip">Description of the task being done for the header</param>
        /// <param name="initMsg">Starting string to put in the progress bar</param>
        public ProgressScreen(ConsoleTheme theme, string descrip, string initMsg = "")
            : base(theme)
        {
            // A nice frame to take up some of the blank space at the top
            AddObject(new ConsoleDoubleFrame(
                1, 2, -1, -1, 8,
                () => Properties.Resources.ProgressTitle,
                () => Properties.Resources.ProgressMessages,
                // Cheating because our IUser handler needs a theme context
                th => th.NormalFrameFg));
            progress = new ConsoleProgressBar(
                3, 5, -3,
                () => topMessage,
                () => percent);
            messages = new ConsoleTextBox(
                3, 10, -3, -3);

            AddObject(progress);
            AddObject(messages);

            topMessage      = initMsg;
            taskDescription = descrip;
        }

        /// <summary>
        /// Put CKAN 1.25.5 in upper left corner
        /// </summary>
        protected override string LeftHeader()
            => $"{Meta.ProductName} {Meta.GetVersion()}";

        /// <summary>
        /// Put description of what we're doing in top center
        /// </summary>
        protected override string CenterHeader()
            => taskDescription;

        // IUser stuff for managing the progress bar and message box

        /// <summary>
        /// Ask the user a yes/no question and capture the answer.
        /// </summary>
        /// <param name="question">Message to display to the user</param>
        /// <returns>
        /// True if the user selected Yes, and false if the user selected No.
        /// </returns>
        public override bool RaiseYesNoDialog(string question)
        {
            // Show the popup at the top of the screen
            // to overwrite the progress bar instead of the messages
            var d = new ConsoleMessageDialog(
                theme,
                // The installer's questions include embedded newlines for spacing in CmdLine
                question.Trim(),
                new List<string>() {
                    Properties.Resources.Yes,
                    Properties.Resources.No
                },
                null,
                TextAlign.Center,
                -Console.WindowHeight / 2
            );
            d.AddBinding(Keys.Y, (object sender) => {
                d.PressButton(0);
                return false;
            });
            d.AddBinding(Keys.N, (object sender) => {
                d.PressButton(1);
                return false;
            });

            // Scroll messages
            d.AddTip(Properties.Resources.CursorKeys, Properties.Resources.ScrollMessages);
            messages.AddScrollBindings(d, theme, true);

            bool val = d.Run() == 0;
            DrawBackground();
            Draw();
            return val;
        }

        /// <summary>
        /// Redirect messages into the text box
        /// </summary>
        /// <param name="message">Format string to put in the text box</param>
        /// <param name="args">Values to substitute into the message</param>
        protected override void Message(string message, params object[] args)
        {
            messages.AddLine(string.Format(message, args));
        }

        /// <summary>
        /// Redirect progress events into the progress bar
        /// </summary>
        /// <param name="message">String to put in the progress bar</param>
        /// <param name="percent">Value between 0-100 indicating task completion</param>
        protected override void Progress(string message, int percent)
        {
            topMessage   = message;
            this.percent = percent / 100.0;
        }

        private readonly ConsoleProgressBar progress;
        private readonly ConsoleTextBox     messages;

        private          string topMessage      = "";
        private readonly string taskDescription;
        private          double percent         = 0;
    }

}
