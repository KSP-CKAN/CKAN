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
        /// <param name="taskDescription">Description of the task being done for the header</param>
        /// <param name="initMsg">Starting string to put in the progress bar</param>
        public ProgressScreen(string taskDescription, string initMsg = "")
        {
            progress = new ConsoleProgressBar(
                1, 2, -1,
                () => topMessage,
                () => percent
            );
            messages = new ConsoleTextBox(
                1, 4, -1, -2
            );

            AddObject(progress);
            AddObject(messages);

            topMessage   = initMsg;
            LeftHeader   = () => $"CKAN {Meta.GetVersion()}";
            CenterHeader = () => taskDescription;
        }

        // IUser stuff for managing the progress bar and message box

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

        private ConsoleProgressBar progress;
        private ConsoleTextBox     messages;

        private string topMessage = "";
        private double percent    = 0;
    }

}
