#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// The GUI implementation of the IUser interface.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class GUIUser : IUser
    {
        public GUIUser(Main main, Wait wait)
        {
            this.main = main;
            this.wait = wait;
        }

        private readonly Main main;
        private readonly Wait wait;

        /// <summary>
        /// A GUIUser is obviously not headless. Returns false.
        /// </summary>
        public bool Headless => false;


        /// <summary>
        /// Shows a small form with the question.
        /// User can select yes or no (ya dont say).
        /// </summary>
        /// <returns><c>true</c> if user pressed yes, <c>false</c> if no.</returns>
        /// <param name="question">Question.</param>
        public bool RaiseYesNoDialog(string question)
            => main.YesNoDialog(question);

        /// <summary>
        /// Will show a small form with the message and a list to choose from.
        /// </summary>
        /// <returns>The index of the selection in the args array. 0-based!</returns>
        /// <param name="message">Message.</param>
        /// <param name="args">Array of offered options.</param>
        public int RaiseSelectionDialog(string message, params object[] args)
            => main.SelectionDialog(message, args);

        /// <summary>
        /// Shows a message box containing the formatted error message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="args">Arguments to format the message.</param>
        public void RaiseError(string message, params object[] args)
        {
            main.ErrorDialog(message, args);
        }

        /// <summary>
        /// Sets the progress bars and the message box of the current WaitTabPage.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="percent">Progress in percent.</param>
        public void RaiseProgress(string message, int percent)
        {
            wait.SetDescription($"{message} - {percent}%");
            main.SetProgress(percent);
        }

        /// <summary>
        /// Displays the formatted message in the lower StatusStrip.
        /// Removes any newline strings.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="args">Arguments to fromat the message.</param>
        public void RaiseMessage(string message, params object[] args)
        {
            main.AddStatusMessage(message, args);
        }
    }
}
