namespace CKAN
{
    /// <summary>
    /// This interface holds all methods which communicate with the user in some way.
    /// Every CKAN interface (GUI, cmdline, consoleUI) has an implementation of the IUser interface.
    /// The implementations define HOW we interact with the user.
    /// </summary>
    public interface IUser
    {
        bool Headless { get; }
        bool ConfirmPrompt { get; }

        bool RaiseYesNoDialog(string question);
        int  RaiseSelectionDialog(string message, params object[] args);
        void RaiseError(string message, params object[] args);

        void RaiseProgress(string message, int percent);
        void RaiseMessage(string message, params object[] args);
    }

    /// <summary>
    /// To be used in tests.
    /// Supresses all output.
    /// </summary>
    public class NullUser : IUser
    {
        /// <summary>
        /// NullUser is headless. Variable not used for NullUser.
        /// </summary>
        public bool Headless
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates if a confirmation prompt should be shown for this type of User.
        /// NullUser returns false.
        /// </summary>
        /// <value><c>true</c> if confirm prompt should be shown; <c>false</c> if not.</value>
        public bool ConfirmPrompt
        {
            get { return false; }
        }

        /// <summary>
        /// NullUser returns true.
        /// </summary>
        public bool RaiseYesNoDialog(string question)
        {
            return true;
        }

        /// <summary>
        /// NullUser returns 0.
        /// </summary>
        public int RaiseSelectionDialog(string message, params object[] args)
        {
            return 0;
        }

        public void RaiseError(string message, params object[] args)
        {
        }

        public void RaiseProgress(string message, int percent)
        {
        }

        public void RaiseMessage(string message, params object[] args)
        {
        }
    }
}
