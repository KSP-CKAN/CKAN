using System;

namespace CKAN
{
    /// <summary>
    /// The GUI implementation of the IUser interface.
    /// </summary>
    public class GUIUser : IUser
    {
        /// <summary>
        /// A GUIUser is obviously not headless. Returns false.
        /// </summary>
        public bool Headless
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates if a confirmation prompt should be shown for this type of User.
        /// E.g. don't ask for confirmation prior to installing mods in the GUI,
        /// because it's already done in a different way.
        /// </summary>
        /// <value><c>true</c> if confirm prompt should be shown; <c>false</c> if not.</value>
        public bool ConfirmPrompt
        {
            get { return false; }
        }

        /// <summary>
        /// Shows a small form with the question.
        /// User can select yes or no (ya dont say).
        /// </summary>
        /// <returns><c>true</c> if user pressed yes, <c>false</c> if no.</returns>
        /// <param name="question">Question.</param>
        public bool RaiseYesNoDialog (string question)
        {
            return Main.Instance.YesNoDialog(question);
        }

        /// <summary>
        /// Will show a small form with the message and a list to choose from.
        /// </summary>
        /// <returns>The index of the selection in the args array. 0-based!</returns>
        /// <param name="message">Message.</param>
        /// <param name="args">Array of offered options.</param>
        public int RaiseSelectionDialog(string message, params object[] args)
        {
            return Main.Instance.SelectionDialog(message, args);
        }

        /// <summary>
        /// Shows a message box containing the formatted error message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="args">Arguments to format the message.</param>
        public void RaiseError (string message, params object[] args)
        {
            Main.Instance.ErrorDialog(message, args);
        }

        /// <summary>
        /// Sets the progress bars and the message box of the current WaitTabPage.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="percent">Progress in percent.</param>
        public void RaiseProgress( string message, int percent)
        {
            Main.Instance.SetDescription($"{message} - {percent}%");
            Main.Instance.SetProgress(percent);
        }

        /// <summary>
        /// Displays the formatted message in the lower StatusStrip.
        /// Removes any newline strings.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="args">Arguments to fromat the message.</param>
        public void RaiseMessage (string message, params object[] args)
        {
            Main.Instance.AddStatusMessage(message, args);
        }
    }

}
