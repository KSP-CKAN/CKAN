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

        bool RaiseYesNoDialog(string question);

        /// <summary>
        /// Ask the user to select one of the elements of the array.
        /// The output is index 0 based.
        /// To supply a default option, make the first option an integer indicating the index of it.
        /// </summary>
        /// <returns>The index of the item selected from the array or -1 if cancelled</returns>
        int  RaiseSelectionDialog(string message, params object[] args);
        void RaiseError(string message, params object[] args);

        void RaiseProgress(string message, int percent);
        void RaiseProgress(int percent, long bytesPerSecond, long bytesLeft);
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
        public bool Headless => true;

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

        public void RaiseProgress(int percent, long bytesPerSecond, long bytesLeft)
        {
        }

        public void RaiseMessage(string message, params object[] args)
        {
        }
    }
}
