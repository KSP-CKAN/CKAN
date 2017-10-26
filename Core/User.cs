// Communicate with the user (status messages, yes/no questions, etc)
// This class will proxy to either the GUI or cmdline functionality.

using System;

namespace CKAN
{

    public interface IUser
    {
        bool Headless { get; }

        bool RaiseYesNoDialog(string question);
        int  RaiseSelectionDialog(string message, params object[] args);
        void RaiseError(string message, params object[] args);

        void RaiseProgress(string message, int percent);
        void RaiseMessage(string message, params object[] url);
        void RaiseDownloadsCompleted(Uri[] file_urls, string[] file_paths, Exception[] errors);
    }

    //Can be used in tests to supress output or as a base class for other types of user.
    //It supplies no op event handlers so that subclasses can avoid null checks.
    public class NullUser : IUser
    {
        public static readonly IUser User = new NullUser();

        public NullUser() { }

        public virtual bool Headless
        {
            get { return false; }
        }

        protected virtual bool DisplayYesNoDialog(string message)
        {
            return true;
        }

        protected virtual void DisplayMessage(string message, params object[] args)
        {
        }

        protected virtual int DisplaySelectionDialog(string message, params object[] args)
        {
            return 0;
        }

        protected virtual void DisplayError(string message, params object[] args)
        {
        }

        protected virtual void ReportProgress(string format, int percent)
        {
        }

        protected virtual void ReportDownloadsComplete(Uri[] urls, string[] filenames, Exception[] errors)
        {
        }

        public void RaiseMessage(string message, params object[] args)
        {
            DisplayMessage(message, args);
        }

        public void RaiseProgress(string message, int percent)
        {
            ReportProgress(message, percent);
        }

        public bool RaiseYesNoDialog(string question)
        {
            return DisplayYesNoDialog(question);
        }

        public int RaiseSelectionDialog(string message, params object[] args)
        {
            return DisplaySelectionDialog(message, args);
        }

        public void RaiseError(string message, params object[] args)
        {
            DisplayError(message, args);
        }

        public void RaiseDownloadsCompleted(Uri[] file_urls, string[] file_paths, Exception[] errors)
        {
            ReportDownloadsComplete(file_urls, file_paths, errors);
        }
    }
}
