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
        public static readonly IUser User = new NullUser();

        public virtual bool Headless
        {
            get { return false; }
        }

        protected virtual void DisplayMessage(string message, params object[] args)
        {
        }

        public bool RaiseYesNoDialog(string question)
        {
            return true;
        }

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
