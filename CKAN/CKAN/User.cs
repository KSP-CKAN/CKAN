// Communicate with the user (status messages, yes/no questions, etc)
// This class will proxy to either the GUI or cmdline functionality.

namespace CKAN
{
    public interface IUser
    {
        //bool YesNo(string text = null);
        bool DisplayYesNoDialog(string message);
        void DisplayMessage(string message, params object[] args);
        void DisplayError(string message, params object[] args);
        int WindowWidth { get; }
    }

    public class NullUser : IUser
    {
        public static readonly IUser User = new NullUser();

        public bool DisplayYesNoDialog(string message)
        {
            return true;
        }

        public void DisplayMessage(string message, params object[] args)
        {         
        }

        public void DisplayError(string message, params object[] args)
        {            
        }

        public int WindowWidth
        {
            get { return -1; }
        }
    }
}
